$packageName = 'Chocolatey Explorer'
$installerType = 'exe'
$url = 'http://chocolatey-explorer.jdibble.co.uk/ChocolateyExplorer.application'
$silentArgs = ''
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$installerType" "$silentArgs" "$url" -validExitCodes $validExitCodes