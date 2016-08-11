# Thanks to Andrew Lock for help with this script
# http://andrewlock.net/publishing-your-first-nuget-package-with-appveyor-and-myget/

param (
  [switch]$release = $false,
  [switch]$build = $false,
  [switch]$test = $false,
  [switch]$skipNtCore = $false,
  [switch]$updatexml = $false,
  [switch]$pack = $false,
  [switch]$debug = $false
)

<#  
.SYNOPSIS
    You can add this to you build script to ensure that psbuild is available before calling
    Invoke-MSBuild. If psbuild is not available locally it will be downloaded automatically.
#>
function EnsurePsbuildInstalled{  
    [cmdletbinding()]
    param(
        [string]$psbuildInstallUri = 'https://raw.githubusercontent.com/ligershark/psbuild/master/src/GetPSBuild.ps1'
    )
    process{
        if(-not (Get-Command "Invoke-MsBuild" -errorAction SilentlyContinue)){
            'Installing psbuild from [{0}]' -f $psbuildInstallUri | Write-Verbose
            (new-object Net.WebClient).DownloadString($psbuildInstallUri) | iex
        }
        else{
            'psbuild already loaded, skipping download' | Write-Verbose
        }

        # make sure it's loaded and throw if not
        if(-not (Get-Command "Invoke-MsBuild" -errorAction SilentlyContinue)){
            throw ('Unable to install/load psbuild from [{0}]' -f $psbuildInstallUri)
        }
    }
}

# Taken from psake https://github.com/psake/psake

<#  
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec  
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

If (Test-Path Env:APPVEYOR_REPO_TAG_NAME) {
  if (($env:APPVEYOR_REPO_TAG_NAME).Contains("-") -eq $false) {
     $release = $true
     echo "Tagged Release"
    }
    echo "Tag but not release"
}
echo "Not Release"


If ($release) {
 $revision =  ""
} Else {
 $revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
 $revision = "--version-suffix={0:D4}" -f [convert]::ToInt32($revision, 10)
}

If ($debug) {
 $configuration = "-c=Debug"
} ElseIf ($env:APPVEYOR) {
 $configuration = "-c=CI"
} Else {
$configuration = "-c=Release"
}

If ($debug) {
     $libLoc = "bin\Debug"
    } ElseIf ($env:APPVEYOR) {
     $libLoc = "bin\CI"
    } Else {
    $libLoc = "bin\Release"
    }
    
    echo $libLoc

function UploadAppVeyorTestResults {
 # upload results to AppVeyor
  $wc = New-Object 'System.Net.WebClient'
  $wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\TestResult.xml))
}

function Build {
  exec { & dotnet restore }
  echo $configuration
  
  exec { & dotnet build src\FRC.NetworkTables $configuration $revision }
  
  exec { & dotnet build src\FRC.NetworkTables.Core $configuration $revision }
}

function Test {
  exec { & dotnet restore }

  if ($env:APPVEYOR) {
    # Run CodeCov tests using full framework
    exec { & dotnet test test\NetworkTables.Test -f netcoreapp1.0 $configuration }
    
      UploadAppVeyorTestResults
    
    
    exec { & dotnet test test\NetworkTables.Core.Test -f netcoreapp1.0 $configuration }
    
      UploadAppVeyorTestResults
      
    exec { & dotnet build test\NetworkTables.Test -f net451 $configuration }
      
    exec { & dotnet build test\NetworkTables.Test -f net451 -o buildTemp\NetworkTables.Test451\ $configuration }
    
    exec { & dotnet build test\NetworkTables.Core.Test -f net451 $configuration }
    
    exec { & dotnet build test\NetworkTables.Core.Test -f net451 -o buildTemp\NetworkTables.Core.Test451\ $configuration }
    
    $OpenCoverVersion = "4.6.519"
    
    $openCoverRun = ".\buildTemp\OpenCover.$OpenCoverVersion\tools\OpenCover.Console.exe"
    
    
    # install CodeCov
    .\NuGet.exe install OpenCover -Version $OpenCoverVersion -OutputDirectory buildTemp
    
    & $openCoverRun -register:user -target:nunit3-console.exe -targetargs:"buildTemp\NetworkTables.Test451\NetworkTables.Test.dll --framework=net-4.5 " -filter:"+[Network*]* +[FRC.Network*]* -[NetworkTables.T*]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -output:coverage.xml -mergeoutput -returntargetcode
    & $openCoverRun -register:user -target:nunit3-console.exe -targetargs:"buildTemp\NetworkTables.Core.Test451\NetworkTables.Core.Test.dll --framework=net-4.5 " -filter:"+[Network*]* +[FRC.Network*]* -[NetworkTables.Core.T*]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -output:coverage.xml -mergeoutput -returntargetcode
    
    nunit3-console.exe buildTemp\NetworkTables.Core.Test451\NetworkTables.Core.Test.dll --framework=net-4.5 --x86
    
    $env:Path = "C:\Python34;C:\\Python34\Scripts;" + $env:Path
    
    & pip install codecov
    
    & codecov -f "coverage.xml"
    
    } else {
    echo "Starting Tests"
     # run all tests using dotnet test runner
    exec { & dotnet test test\NetworkTables.Test $configuration }
    
    if ($skipNtCore -eq $false) {
    exec { & dotnet test test\NetworkTables.Core.Test $configuration }
    }
  }
}

function Pack { 
  if (Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

  exec { & dotnet pack src\FRC.NetworkTables $configuration $revision --no-build -o .\artifacts }
  
  exec { & dotnet pack src\FRC.NetworkTables.Core $configuration $revision --no-build -o .\artifacts }

  if ($env:APPVEYOR) {
    Get-ChildItem .\artifacts\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
  }
}

function UpdateXml {    
    # Copy built files
    if ((Test-Path .\buildTemp) -eq $false) {
  md .\buildTemp
 }
 
  .\NuGet.exe install EWSoftware.SHFB -Version 2016.4.9 -o buildTemp

   .\NuGet.exe install EWSoftware.SHFB.NETFramework -Version 4.6 -o buildTemp
   
   Copy-Item src\FRC.NetworkTables\$libLoc\net451\FRC.NetworkTables.dll buildTemp\FRC.NetworkTables.dll
   Copy-Item src\FRC.NetworkTables\$libLoc\net451\FRC.NetworkTables.xml buildTemp\FRC.NetworkTables.xml
   
   Copy-Item src\FRC.NetworkTables.Core\$libLoc\net451\FRC.NetworkTables.Core.dll buildTemp\FRC.NetworkTables.Core.dll
   Copy-Item src\FRC.NetworkTables.Core\$libLoc\net451\FRC.NetworkTables.Core.xml buildTemp\FRC.NetworkTables.Core.xml
   
   EnsurePsbuildInstalled
   
   if ($debug) { 
   $sandConfig = "Debug"
   } else {
    $sandConfig = "Release"
   }
   
   & 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe' Sandcastle\NetworkTables.NetCore.shfbproj /property:Configuration=Release /v:m

   Copy-Item Sandcastle\Help\FRC.NetworkTables.xml src\FRC.NetworkTables\$libLoc\net451\FRC.NetworkTables.xml
   Copy-Item Sandcastle\Help\FRC.NetworkTables.xml src\FRC.NetworkTables\$libLoc\netstandard1.3\FRC.NetworkTables.xml
   
   & 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe' Sandcastle\NetworkTables.Core.NetCore.shfbproj /property:Configuration=Release /v:m
   
   Copy-Item Sandcastle\Help\FRC.NetworkTables.Core.xml src\FRC.NetworkTables.Core\$libLoc\net451\FRC.NetworkTables.Core.xml
   Copy-Item Sandcastle\Help\FRC.NetworkTables.Core.xml src\FRC.NetworkTables.Core\$libLoc\netstandard1.5\FRC.NetworkTables.Core.xml
}

if ($env:APPVEYOR) {

 #Supress compiler xml warnings
  $netTablesJson = Get-Content 'src\FRC.NetworkTables\project.json' -raw | ConvertFrom-Json
  try{
  $netTablesJson.buildOptions | Add-Member -Name "nowarn" -value @("CS1591") -MemberType NoteProperty
    }
  catch {}
  $netTablesJson | ConvertTo-Json -Depth 5 | Set-Content 'src\FRC.NetworkTables\project.json'
  
  $netTablesJson = Get-Content 'src\FRC.NetworkTables.Core\project.json' -raw | ConvertFrom-Json
  try{
  $netTablesJson.buildOptions | Add-Member -Name "nowarn" -value @("CS1591") -MemberType NoteProperty
  }
  catch {}
  $netTablesJson | ConvertTo-Json -Depth 5 | Set-Content 'src\FRC.NetworkTables.Core\project.json'
}

if ($release) {
 if ((Test-Path .\buildTemp) -eq $false) {
  md .\buildTemp
 }

 # Remove beta defintion from project.json files
  Copy-Item src\FRC.NetworkTables\project.json buildTemp\FRC.NetworkTables.projectjson
  Copy-Item src\FRC.NetworkTables.Core\project.json buildTemp\FRC.NetworkTables.Core.projectjson
  
  $netTablesJson = Get-Content 'src\FRC.NetworkTables\project.json' -raw | ConvertFrom-Json
  $netTablesJson.version = $netTablesJson.version.Substring(0, $netTablesJson.version.IndexOf("-"))
  $netTablesJson | ConvertTo-Json -Depth 5 | Set-Content 'src\FRC.NetworkTables\project.json'
  
  $netTablesJson = Get-Content 'src\FRC.NetworkTables.Core\project.json' -raw | ConvertFrom-Json
  $netTablesJson.version = $netTablesJson.version.Substring(0, $netTablesJson.version.IndexOf("-"))
  $netTablesJson | ConvertTo-Json -Depth 5 | Set-Content 'src\FRC.NetworkTables.Core\project.json'
 
}

if ($build) {
 Build
}

if ($test) {
 Test
}

if ($updatexml) {
 UpdateXml
}

if ($pack) {
 Pack
}

if ($release) {
 # Add beta definition back into project.json
 Copy-Item buildTemp\FRC.NetworkTables.projectjson src\FRC.NetworkTables\project.json
 Copy-Item buildTemp\FRC.NetworkTables.Core.projectjson src\FRC.NetworkTables.Core\project.json
 
 Remove-Item buildTemp\FRC.NetworkTables.projectjson
 Remove-Item buildTemp\FRC.NetworkTables.Core.projectjson
}