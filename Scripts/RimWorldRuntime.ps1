# Shared runtime tooling. Keep the CCTO copy identical; no runtime Mod dependency.
if ($env:OS -eq 'Windows_NT') {
    $env:PSModulePath="$env:SystemRoot\System32\WindowsPowerShell\v1.0\Modules"
}
function Get-RimWorldRuntime([string]$GameRoot) {
    $root = [IO.Path]::GetFullPath($GameRoot)
    $version = (Get-Content -LiteralPath (Join-Path $root 'Version.txt') -Raw).Trim()
    $modern = $version -match '^1\.7\.'
    if (-not $modern -and $version -notmatch '^1\.6\.') { throw "Unsupported RimWorld build: $version" }
    $exe = Join-Path $root $(if ($modern) { 'RimWorld.exe' } else { 'RimWorldWin64.exe' })
    if (-not (Test-Path -LiteralPath $exe)) { throw "RimWorld executable missing: $exe" }
    [pscustomobject]@{ Root=$root; Version=$version; Modern=$modern; Exe=$exe; Minor=$(if($modern){'1.7'}else{'1.6'}) }
}

function Find-RimWorldModAssembly([string]$Root, [string]$Name, [string]$Version='1.7') {
    $loader = @('LoadFolders.xml','loadFolders.xml') | ForEach-Object { Join-Path $Root $_ } | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    $folders = @('/', $Version)
    if ($loader) {
        [xml]$xml = Get-Content -LiteralPath $loader -Raw
        $node = $xml.SelectSingleNode('/loadFolders/v'+$Version)
        if (-not $node -and $Version -eq '1.7') { $node=$xml.SelectSingleNode('/loadFolders/v1.6') }
        if (-not $node) { throw "No supported load folders in $loader" }
        $folders = @($node.li | Where-Object { $_ -is [string] } | ForEach-Object { [string]$_ })
    }
    $selected=$null
    foreach($folder in $folders) {
        if ($folder -ne '/' -and ([IO.Path]::IsPathRooted($folder) -or $folder -match '(^|[\\/])\.\.([\\/]|$)')) { throw "Unsafe load folder: $folder" }
        $dir=if($folder -eq '/'){$Root}else{Join-Path $Root $folder}
        $candidate=Join-Path $dir ('Assemblies\'+$Name)
        if(Test-Path -LiteralPath $candidate){$selected=$candidate}
    }
    if(-not $selected){throw "No active $Name in $Root for $Version"}
    [IO.Path]::GetFullPath($selected)
}

function Invoke-RimWorldTestCompile([string]$GameRoot, [string]$Output, [string[]]$Sources, [string[]]$References=@()) {
    $runtime=Get-RimWorldRuntime $GameRoot
    $directory=Split-Path -Parent ([IO.Path]::GetFullPath($Output))
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $args=@('/nologo','/nowarn:1684','/target:library','/optimize+',('/out:"'+[IO.Path]::GetFullPath($Output)+'"'))
    if($runtime.Modern) {
        $dotnet=(Get-Command dotnet -ErrorAction Stop).Source
        $compilers=@(& $dotnet --list-sdks | ForEach-Object {
            if($_ -match '^([^ ]+) \[(.+)\]$') {
                $path=Join-Path (Join-Path $Matches[2] $Matches[1]) 'Roslyn\bincore\csc.dll'
                if(Test-Path -LiteralPath $path){[pscustomobject]@{Version=[version]($Matches[1] -replace '-.*','');Path=$path}}
            }
        })
        $compiler=$compilers | Sort-Object Version -Descending | Select-Object -First 1
        if(-not $compiler){throw 'Installed dotnet SDK with Roslyn csc.dll is required for 1.7 test builds'}
        $args+='/nostdlib+'
        $References+=@(Get-ChildItem -LiteralPath $runtime.Root -Filter '*.dll' -File | Where-Object {$_.Name -ne 'RimWorld.dll'} | ForEach-Object {
            try { [void][Reflection.AssemblyName]::GetAssemblyName($_.FullName); $_.FullName } catch [BadImageFormatException] { }
        })
    } else {
        $legacy=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
        $managed=Join-Path $runtime.Root 'RimWorldWin64_Data\Managed'
        $References+=@('Assembly-CSharp.dll','UnityEngine.CoreModule.dll','UnityEngine.IMGUIModule.dll','Unity.Mathematics.dll','Unity.Collections.dll','netstandard.dll') | ForEach-Object { Join-Path $managed $_ } | Where-Object {Test-Path -LiteralPath $_}
    }
    $args+=@($References | Sort-Object -Unique | ForEach-Object {'/reference:"'+[IO.Path]::GetFullPath($_)+'"'})
    $args+=@($Sources | ForEach-Object {'"'+[IO.Path]::GetFullPath($_)+'"'})
    $rsp=$Output+'.rsp'
    [IO.File]::WriteAllLines($rsp,$args,(New-Object Text.UTF8Encoding($false)))
    try {
        if($runtime.Modern){ & $dotnet $compiler.Path ('@'+$rsp) }else{ & $legacy ('@'+$rsp) }
        if($LASTEXITCODE -ne 0){throw "Test assembly compilation failed: $Output"}
    } finally { Remove-Item -LiteralPath $rsp -ErrorAction SilentlyContinue }
}

function Assert-RimWorldNativeLog([string]$LogPath,[uint32]$ProcessId) {
    $text=[IO.File]::ReadAllText($LogPath)
    if($text -notmatch ('(?m)^Process:\s+pid '+$ProcessId+'\s*$')) {throw 'Native log belongs to a different process'}
    if($text -match '(?m)^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d+\s+ERROR\s') {throw "Runtime ERROR found: $LogPath"}
}

function Invoke-RimWorldIsolated([string]$GameRoot,[string]$Arguments,[string]$LogPath,[int]$TimeoutSeconds=300) {
    $runtime=Get-RimWorldRuntime $GameRoot
    $mutex=$null; $locked=$false; $desktop=[IntPtr]::Zero
    if($runtime.Modern) {
        $mutex=New-Object Threading.Mutex($false,'Local\RimWorld17AutomatedLog')
        $locked=$mutex.WaitOne(0)
        if(-not $locked){$mutex.Dispose();throw 'Another 1.7 test owns the native log; use serial test profiles'}
        if(Get-Process RimWorld -ErrorAction SilentlyContinue){$mutex.ReleaseMutex();$mutex.Dispose();throw 'Close the existing RimWorld normally before 1.7 tests'}
    }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $LogPath) | Out-Null
    Remove-Item -LiteralPath $LogPath -ErrorAction SilentlyContinue
    Initialize-RimWorldDesktop
    $info=New-Object RimWorldRuntimeDesktop+ProcessInfo
    $finished=$false; $started=$false
    try {
        $name='RimWorldTest_'+[Guid]::NewGuid().ToString('N')
        $desktop=[RimWorldRuntimeDesktop]::CreateDesktop($name,[IntPtr]::Zero,[IntPtr]::Zero,0,0x01FF,[IntPtr]::Zero)
        if($desktop -eq [IntPtr]::Zero){throw (New-Object ComponentModel.Win32Exception)}
        $startup=New-Object RimWorldRuntimeDesktop+Startup
        $startup.cb=[Runtime.InteropServices.Marshal]::SizeOf($startup)
        $startup.desktop='WinSta0\'+$name
        $command=New-Object Text.StringBuilder
        [void]$command.Append('"'+$runtime.Exe+'" '+$Arguments)
        $started=[RimWorldRuntimeDesktop]::CreateProcess($runtime.Exe,$command,[IntPtr]::Zero,[IntPtr]::Zero,$false,0,[IntPtr]::Zero,$runtime.Root,[ref]$startup,[ref]$info)
        if(-not $started){throw (New-Object ComponentModel.Win32Exception)}
        $watch=[Diagnostics.Stopwatch]::StartNew()
        while($watch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
            $state=[RimWorldRuntimeDesktop]::WaitForSingleObject($info.process,5000)
            if($state -eq 0){$finished=$true;break}
            if($state -ne 258){throw "Process wait failed: $state"}
        }
        if(-not $finished){ & taskkill /PID $info.pid /T /F | Out-Null; [void][RimWorldRuntimeDesktop]::WaitForSingleObject($info.process,10000) }
        if($runtime.Modern) {
            $native=Join-Path $env:USERPROFILE 'AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Logs\Player.log'
            Copy-Item -LiteralPath $native -Destination $LogPath -Force
            Assert-RimWorldNativeLog $LogPath $info.pid
        }
        if(-not $finished){throw (New-Object TimeoutException('RimWorld exceeded '+$TimeoutSeconds+' seconds; failed log retained'))}
        [uint32]$code=0
        [void][RimWorldRuntimeDesktop]::GetExitCodeProcess($info.process,[ref]$code)
        if($code -ne 0){throw "RimWorld exited with code $code; see $LogPath"}
        if(-not (Test-Path -LiteralPath $LogPath)){throw 'Runtime log missing'}
        $text=[IO.File]::ReadAllText($LogPath)
        if($text -match '(?m)^Level:\s*ERROR\s*$' -or $text.Contains('[ERROR]')){throw "Runtime ERROR found: $LogPath"}
    } finally {
        if($started -and -not $finished){ & taskkill /PID $info.pid /T /F 2>$null | Out-Null }
        if($info.thread -ne [IntPtr]::Zero){[void][RimWorldRuntimeDesktop]::CloseHandle($info.thread)}
        if($info.process -ne [IntPtr]::Zero){[void][RimWorldRuntimeDesktop]::CloseHandle($info.process)}
        if($desktop -ne [IntPtr]::Zero){[void][RimWorldRuntimeDesktop]::CloseDesktop($desktop)}
        if($locked){$mutex.ReleaseMutex()}
        if($mutex){$mutex.Dispose()}
    }
}

function Initialize-RimWorldDesktop {
 if('RimWorldRuntimeDesktop' -as [type]){return}
Add-Type -TypeDefinition @'
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

public static class RimWorldRuntimeDesktop
{
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct Startup
    {
        public int cb;
        public string reserved;
        public string desktop;
        public string title;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int xchars;
        public int ychars;
        public int fill;
        public int flags;
        public short show;
        public short reservedBytes;
        public IntPtr reservedPtr;
        public IntPtr input;
        public IntPtr output;
        public IntPtr error;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInfo
    {
        public IntPtr process;
        public IntPtr thread;
        public uint pid;
        public uint tid;
    }

    [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    public static extern IntPtr CreateDesktop(
        string name,
        IntPtr device,
        IntPtr mode,
        uint flags,
        uint access,
        IntPtr security);

    [DllImport("user32.dll")]
    public static extern bool CloseDesktop(IntPtr desktop);

    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    public static extern bool CreateProcess(
        string app,
        StringBuilder command,
        IntPtr psa,
        IntPtr tsa,
        bool inherit,
        uint flags,
        IntPtr env,
        string cwd,
        ref Startup startup,
        out ProcessInfo process);

    [DllImport("kernel32.dll")]
    public static extern uint WaitForSingleObject(IntPtr handle, uint millis);

    [DllImport("kernel32.dll")]
    public static extern bool GetExitCodeProcess(IntPtr process, out uint code);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr handle);

    public delegate bool WindowCallback(IntPtr window, IntPtr parameter);

    [DllImport("user32.dll", SetLastError=true)]
    public static extern bool EnumDesktopWindows(
        IntPtr desktop,
        WindowCallback callback,
        IntPtr parameter);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);

    public static string WindowProcesses(IntPtr desktop)
    {
        var ids = new System.Collections.Generic.HashSet<uint>();
        WindowCallback callback = delegate(IntPtr window, IntPtr parameter)
        {
            uint pid;
            GetWindowThreadProcessId(window, out pid);
            ids.Add(pid);
            return true;
        };

        EnumDesktopWindows(desktop, callback, IntPtr.Zero);

        var names = new System.Collections.Generic.List<string>();
        foreach (uint pid in ids)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById((int)pid);
                names.Add(process.ProcessName + ":" + pid);
            }
            catch (ArgumentException)
            {
            }
        }

        return String.Join(",", names.ToArray());
    }
}
'@

}
