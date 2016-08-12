$localFolder = ".\temp"

if ((Test-Path $localFolder) -eq $false) {
  md $localFolder
 }

# Create temp directory to store stuff in
if ((Test-Path $localFolder) -eq $false) {
  md $localFolder
}

$armFileName = "ntcore-arm"
$armFileLocation = "http://frcjenkins.wpi.edu/job/ntcore-arm/lastSuccessfulBuild/artifact/arm/ntcore/build/$armFileName.zip"

$linuxFileName = "ntcore-linux"
$linuxFileLocation = "http://frcjenkins.wpi.edu/job/ntcore-platforms/label=Linux/lastSuccessfulBuild/artifact/native/ntcore/build/$linuxFileName.zip"

$osxFileName = "ntcore-osx"
$osxFileLocation = "http://frcjenkins.wpi.edu/job/ntcore-platforms/label=mac/lastSuccessfulBuild/artifact/native/ntcore/build/ntcore-os%20x.zip"

$winFileName = "ntcore-windows"
$winFileLocation = "http://frcjenkins.wpi.edu/job/ntcore-platforms/label=windows/lastSuccessfulBuild/artifact/native/ntcore/build/$winFileName.zip"

# Download all our files
echo "Downloading Files"
(New-Object System.Net.WebClient).DownloadFile($armFileLocation, "$localFolder\$armFileName.zip")
(New-Object System.Net.WebClient).DownloadFile($linuxFileLocation, "$localFolder\$linuxFileName.zip")
(New-Object System.Net.WebClient).DownloadFile($osxFileLocation, "$localFolder\$osxFileName.zip")
(New-Object System.Net.WebClient).DownloadFile($winFileLocation, "$localFolder\$winFileName.zip")

# unzip everything
Add-Type -assembly “system.io.compression.filesystem”

function Unzip($file)
{
  [io.compression.zipfile]::ExtractToDirectory("$localFolder\$file.zip", "$localFolder\$file")
}

echo "Unzipping Files"
Unzip($armFileName)
Unzip($linuxFileName)
Unzip($osxFileName)
Unzip($winFileName)

echo "Copying files"

Copy-Item "$localFolder\$armFileName\Linux\arm\libntcore.so" "roborio\libntcore.so"

Copy-Item "$localFolder\$linuxFileName\Linux\amd64\libntcore.so" "amd64\libntcore.so"
Copy-Item "$localFolder\$linuxFileName\Linux\x86\libntcore.so" "x86\libntcore.so"

Copy-Item "$localFolder\$osxFileName\Mac OS X\x86_64\libntcore.dylib" "amd64\libntcore.dylib"
Copy-Item "$localFolder\$osxFileName\Mac OS X\x86\libntcore.dylib" "x86\libntcore.dylib"

Copy-Item "$localFolder\$winFileName\Windows\amd64\ntcore.dll" "amd64\ntcore.dll"
Copy-Item "$localFolder\$winFileName\Windows\x86\ntcore.dll" "x86\ntcore.dll"

echo "Done"