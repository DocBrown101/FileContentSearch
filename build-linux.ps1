$Version = Get-Date -Format "yyyy-MM-dd" # 2020-11-1
$VersionDot = $Version -replace '-','.'

# Dotnet restore and build
dotnet publish "$PSScriptRoot\ContentSearch\ContentSearch.csproj" `
	   --runtime linux-x64 `
	   --self-contained true `
	   -c Release `
	   -v minimal `
	   -o "$PSScriptRoot\bld\linux" `
	   -p:PublishReadyToRun=false `
	   -p:PublishSingleFile=true `
	   -p:PublishTrimmed=false `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:Version=$VersionDot `
	   --nologo
