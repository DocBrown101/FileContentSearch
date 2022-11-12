$BuildPath = "$PSScriptRoot\build"
$BuildPathX64 = "$BuildPath\X64"
$BuildPathARM64 = "$BuildPath\ARM64"
$Version = Get-Date -Format "yyyy-MM-dd" # 2022-11-12
$VersionDot = $Version -replace '-','.'
$Project = "FileContentSearch"
$ArchiveX64 = "$BuildPath\File-Content-Search-$Version-X64.zip"
$ArchiveARM64 = "$BuildPath\File-Content-Search-$Version-ARM64.zip"

# Clean up
if(Test-Path -Path $BuildPath)
{
    Remove-Item $BuildPath -Recurse
}

# Dotnet restore and build win-x64
dotnet publish "$PSScriptRoot\src\$Project\$Project.csproj" `
	   --runtime win-x64 `
	   --self-contained false `
	   -c Release `
	   -v minimal `
	   -o $BuildPathX64 `
	   -p:PublishReadyToRun=true `
	   -p:PublishSingleFile=true `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:Version=$VersionDot `
	   --nologo

# Dotnet restore and build win-arm64
dotnet publish "$PSScriptRoot\src\$Project\$Project.csproj" `
	   --runtime win-arm64 `
	   --self-contained false `
	   -c Release `
	   -v minimal `
	   -o $BuildPathARM64 `
	   -p:PublishReadyToRun=true `
	   -p:PublishSingleFile=true `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:Version=$VersionDot `
	   --nologo

# Archiv Build
Compress-Archive -Path "$BuildPathX64\$Project.exe" -DestinationPath $ArchiveX64
Compress-Archive -Path "$BuildPathARM64\$Project.exe" -DestinationPath $ArchiveARM64
