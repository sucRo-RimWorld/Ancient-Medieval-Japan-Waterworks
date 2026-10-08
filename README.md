# Ancient & Medieval Japan - Waterworks

RimWorld 1.6 art-free development prototype. **Source is not yet built or runtime-tested; do not publish.**

Includes two canal TerrainDefs referencing existing Vanilla soil/river assets, Construction dig/fill designations and jobs, natural-water source checks, connectivity, original-terrain restoration, and Vanilla bridge/foundation coexistence.

Build locally using a .NET Framework 4.7.2 targeting pack:

```powershell
dotnet build Source/AncientMedievalJapanWaterworks.csproj -p:RimWorldDir="D:/SteamLibrary/steamapps/common/RimWorld" -c Release
```

Output goes to `Assemblies/`. Run `python Tests/static_audit.py` for XML/source contract checks; this does **not** replace a game compile, isolated runtime, save/load or visual test.
