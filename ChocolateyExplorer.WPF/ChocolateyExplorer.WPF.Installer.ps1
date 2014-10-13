$packageName = 'Chocolatey Explorer'
$installerType = 'exe'
$url = 'http://chocolatey-explorer.jdibble.co.uk/ChocolateyExplorer.exe'
$silentArgs = '/i /s'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$installerType" "$silentArgs" "$url" -validExitCodes $validExitCodes