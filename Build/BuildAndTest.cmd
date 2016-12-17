call RestorePackages.cmd
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" Build.proj /t:RunTests /p:IsDesktopBuild=true
pause