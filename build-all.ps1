$BuildPath = "$PSScriptRoot\build"
$BuildPathLinuxX64 = "$BuildPath\Linux-X64"
$BuildPathLinuxARM64 = "$BuildPath\Linux-ARM64"
$BuildPathWindowsX64 = "$BuildPath\Windows-X64"
$BuildPathWindowsARM64 = "$BuildPath\Windows-ARM64"
$Version = Get-Date -Format "yyyy-MM-dd" # 2024-11-12
$VersionDot = $Version -replace '-','.'
$Project = "ContentSearch"
$ArchiveLinuxX64 = "$BuildPath\$Version-Linux-X64.zip"
$ArchiveLinuxARM64 = "$BuildPath\$Version-Linux-ARM64.zip"
$ArchiveWindowsX64 = "$BuildPath\$Version-Windows-X64.zip"
$ArchiveWindowsARM64 = "$BuildPath\$Version-Windows-ARM64.zip"

# Clean up all builds
if (Test-Path -Path $BuildPath) { Remove-Item $BuildPath -Recurse }

# Build linux-x64
dotnet publish "$PSScriptRoot\src\$Project\$Project.csproj" `
	   --runtime linux-x64 `
	   --self-contained false `
	   -c Release `
	   -v minimal `
	   -o $BuildPathLinuxX64 `
	   -p:PublishReadyToRun=true `
	   -p:PublishSingleFile=true `
	   -p:PublishTrimmed=false `
	   -p:DebugType=None `
       -p:DebugSymbols=false `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:IncludeSourceRevisionInInformationalVersion=false `
	   -p:Version=$VersionDot `
	   --nologo

# Build linux-arm64
dotnet publish "$PSScriptRoot\src\$Project\$Project.csproj" `
	   --runtime linux-arm64 `
	   --self-contained false `
	   -c Release `
	   -v minimal `
	   -o $BuildPathLinuxARM64 `
	   -p:PublishReadyToRun=true `
	   -p:PublishSingleFile=true `
	   -p:PublishTrimmed=false `
	   -p:DebugType=None `
       -p:DebugSymbols=false `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:IncludeSourceRevisionInInformationalVersion=false `
	   -p:Version=$VersionDot `
	   --nologo

# Build win-x64
dotnet publish "$PSScriptRoot\src\$Project\$Project.csproj" `
	   --runtime win-x64 `
	   --self-contained false `
	   -c Release `
	   -v minimal `
	   -o $BuildPathWindowsX64 `
	   -p:PublishReadyToRun=true `
	   -p:PublishSingleFile=true `
       -p:DebugType=None `
       -p:DebugSymbols=false `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:IncludeSourceRevisionInInformationalVersion=false `
	   -p:Version=$VersionDot `
	   --nologo

# Build win-arm64
dotnet publish "$PSScriptRoot\src\$Project\$Project.csproj" `
	   --runtime win-arm64 `
	   --self-contained false `
	   -c Release `
	   -v minimal `
	   -o $BuildPathWindowsARM64 `
	   -p:PublishReadyToRun=true `
	   -p:PublishSingleFile=true `
	   -p:DebugType=None `
       -p:DebugSymbols=false `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:IncludeSourceRevisionInInformationalVersion=false `
	   -p:Version=$VersionDot `
	   --nologo

# Archiv Build
Compress-Archive -Path "$BuildPathLinuxX64\File-Content-Search" -DestinationPath $ArchiveLinuxX64
Compress-Archive -Path "$BuildPathLinuxARM64\File-Content-Search" -DestinationPath $ArchiveLinuxARM64
Compress-Archive -Path "$BuildPathWindowsX64\File-Content-Search.exe" -DestinationPath $ArchiveWindowsX64
Compress-Archive -Path "$BuildPathWindowsARM64\File-Content-Search.exe" -DestinationPath $ArchiveWindowsARM64
