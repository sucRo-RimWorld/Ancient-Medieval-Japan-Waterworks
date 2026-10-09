using System;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
public static class AmjDesktop {
 [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
 public struct Startup {
  public int cb; public string reserved, desktop, title;
  public int x,y,cx,cy,xchars,ychars,fill,flags;
  public short show, reservedBytes; public IntPtr reservedPtr, input, output, error;
 }
 [StructLayout(LayoutKind.Sequential)]
 public struct ProcessInfo { public IntPtr process, thread; public uint pid, tid; }
 [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
 public static extern IntPtr CreateDesktop(string name, IntPtr device, IntPtr mode, uint flags, uint access, IntPtr security);
 [DllImport("user32.dll")] public static extern bool CloseDesktop(IntPtr desktop);
 [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
 public static extern bool CreateProcess(string app, StringBuilder command, IntPtr psa, IntPtr tsa, bool inherit, uint flags, IntPtr env, string cwd, ref Startup startup, out ProcessInfo process);
 [DllImport("kernel32.dll")] public static extern uint WaitForSingleObject(IntPtr handle, uint millis);
 [DllImport("kernel32.dll")] public static extern bool GetExitCodeProcess(IntPtr process, out uint code);
 [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr handle);
 public delegate bool WindowCallback(IntPtr window, IntPtr parameter);
 [DllImport("user32.dll", SetLastError=true)] public static extern bool EnumDesktopWindows(IntPtr desktop, WindowCallback callback, IntPtr parameter);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);
 public static string WindowProcesses(IntPtr desktop) {
  var ids = new System.Collections.Generic.HashSet<uint>();
  WindowCallback callback = delegate(IntPtr window, IntPtr parameter) { uint pid; GetWindowThreadProcessId(window, out pid); ids.Add(pid); return true; };
  // EnumDesktopWindows may return false without an error when the desktop is empty.
  EnumDesktopWindows(desktop, callback, IntPtr.Zero);
  var names = new System.Collections.Generic.List<string>();
  foreach (uint pid in ids) { try { var p=System.Diagnostics.Process.GetProcessById((int)pid); names.Add(p.ProcessName+":"+pid); } catch(ArgumentException) {} }
  return String.Join(",", names.ToArray());
 }
}

public static class ReleaseMatrixDesktopRunner {
 public static int Main(string[] args){
  string desktopName="AMJERelease"+Guid.NewGuid().ToString("N");
  IntPtr desktop=AmjDesktop.CreateDesktop(desktopName,IntPtr.Zero,IntPtr.Zero,0,0x10000000,IntPtr.Zero);
  if(desktop==IntPtr.Zero)throw new Win32Exception();
  AmjDesktop.ProcessInfo info=new AmjDesktop.ProcessInfo();bool started=false,finished=false;
  try{
   var startup=new AmjDesktop.Startup();startup.cb=Marshal.SizeOf(startup);startup.desktop="WinSta0\\"+desktopName;
   string cmd=Environment.GetEnvironmentVariable("ComSpec");
   var command=new StringBuilder("\""+cmd+"\" /d /s /c \"\""+args[0]+"\"\"");
   started=AmjDesktop.CreateProcess(cmd,command,IntPtr.Zero,IntPtr.Zero,false,0x08000000,IntPtr.Zero,args[1],ref startup,out info);
   if(!started)throw new Win32Exception();
   Console.WriteLine("START private desktop, runner="+info.pid);
   var timer=System.Diagnostics.Stopwatch.StartNew();
   while(timer.Elapsed.TotalSeconds<1500){uint state=AmjDesktop.WaitForSingleObject(info.process,5000);if(state==0){finished=true;break;}if(state!=258)throw new Exception("Wait failed");}
   if(!finished)throw new Exception("Matrix timeout");
   uint code;if(!AmjDesktop.GetExitCodeProcess(info.process,out code))throw new Win32Exception();
   Console.WriteLine("EXIT "+code);return (int)code;
  }finally{
   if(started&&!finished){var psi=new System.Diagnostics.ProcessStartInfo("taskkill","/PID "+info.pid+" /T /F");psi.CreateNoWindow=true;psi.UseShellExecute=false;using(var p=System.Diagnostics.Process.Start(psi)){p.WaitForExit();}}
   if(info.thread!=IntPtr.Zero)AmjDesktop.CloseHandle(info.thread);if(info.process!=IntPtr.Zero)AmjDesktop.CloseHandle(info.process);AmjDesktop.CloseDesktop(desktop);
  }
 }
}
