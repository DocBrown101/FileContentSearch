$Version = Get-Date -Format "yyyy-MM-dd" # 2020-11-1
$VersionDot = $Version -replace '-','.'

# Dotnet restore and build
dotnet publish "$PSScriptRoot\ContentSearch\ContentSearch.csproj" `
	   --runtime win-x64 `
	   --self-contained false `
	   -c Release `
	   -v minimal `
	   -o "$PSScriptRoot\bld\win" `
	   -p:PublishReadyToRun=true `
	   -p:PublishSingleFile=true `
       -p:PublishTrimmed=false `
       -p:DebugType=None `
       -p:DebugSymbols=false `
	   -p:CopyOutputSymbolsToPublishDirectory=false `
	   -p:Version=$VersionDot `
	   --nologo
