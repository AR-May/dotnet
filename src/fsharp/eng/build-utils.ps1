# Collection of powershell build utility functions that we use across our scripts.

Set-StrictMode -version 2.0
$ErrorActionPreference="Stop"

# Import Arcade functions
. (Join-Path $PSScriptRoot "common\tools.ps1")

$VSSetupDir = Join-Path $ArtifactsDir "VSSetup\$configuration"
$PackagesDir = Join-Path $ArtifactsDir "packages\$configuration"

$binaryLog = if (Test-Path variable:binaryLog) { $binaryLog } else { $false }
$nodeReuse = if (Test-Path variable:nodeReuse) { $nodeReuse } else { $false }
$bootstrapDir = if (Test-Path variable:bootstrapDir) { $bootstrapDir } else { "" }
$bootstrapConfiguration = if (Test-Path variable:bootstrapConfiguration) { $bootstrapConfiguration } else { "Proto" }
$bootstrapTfm = if (Test-Path variable:bootstrapTfm) { $bootstrapTfm } else { "net472" }
$fsharpNetCoreProductTfm = if (Test-Path variable:fsharpNetCoreProductTfm) { $fsharpNetCoreProductTfm } else { "net9.0" }
$properties = if (Test-Path variable:properties) { $properties } else { @() }

function GetProjectOutputBinary([string]$fileName, [string]$projectName = "", [string]$configuration = $script:configuration, [string]$tfm = "net472", [string]$rid = "", [bool]$published = $false) {
  $projectName = if ($projectName -ne "") { $projectName } else { [System.IO.Path]::GetFileNameWithoutExtension($fileName) }
  $publishDir = if ($published) { "publish\" } else { "" }
  $ridDir = if ($rid -ne "") { "$rid\" } else { "" }
  return Join-Path $ArtifactsDir "bin\$projectName\$configuration\$tfm\$ridDir$publishDir$fileName"
}

# Handy function for executing a command in powershell and throwing if it 
# fails.
#
# Use this when the full command is known at script authoring time and 
# doesn't require any dynamic argument build up.  Example:
#
#   Exec-Block { & $msbuild Test.proj }
# 
# Original sample came from: http://jameskovacs.com/2010/02/25/the-exec-problem/
function Exec-Block([scriptblock]$cmd) {
    & $cmd

    # Need to check both of these cases for errors as they represent different items
    # - $?: did the powershell script block throw an error
    # - $lastexitcode: did a windows command executed by the script block end in error
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        throw "Command failed to execute: $cmd"
    }
}

function Exec-CommandCore([string]$command, [string]$commandArgs, [switch]$useConsole = $true) {
    if ($useConsole) {
        $exitCode = Exec-Process $command $commandArgs
        if ($exitCode -ne 0) { 
            throw "Command failed to execute with exit code $($exitCode): $command $commandArgs" 
        }
        return
    }

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $command
    $startInfo.Arguments = $commandArgs

    $startInfo.UseShellExecute = $false
    $startInfo.WorkingDirectory = Get-Location
    $startInfo.RedirectStandardOutput = $true
    $startInfo.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    $process.Start() | Out-Null

    $finished = $false
    try {
        # The OutputDataReceived event doesn't fire as events are sent by the 
        # process in powershell.  Possibly due to subtlties of how Powershell
        # manages the thread pool that I'm not aware of.  Using blocking
        # reading here as an alternative which is fine since this blocks 
        # on completion already.
        $out = $process.StandardOutput
        while (-not $out.EndOfStream) {
            $line = $out.ReadLine()
            Write-Output $line
        }

        while (-not $process.WaitForExit(100)) { 
            # Non-blocking loop done to allow ctr-c interrupts
        }

        $finished = $true
        if ($process.ExitCode -ne 0) { 
            throw "Command failed to execute with exit code $($process.ExitCode): $command $commandArgs" 
        }
    }
    finally {
        # If we didn't finish then an error occured or the user hit ctrl-c.  Either
        # way kill the process
        if (-not $finished) {
            $process.Kill()
        }
    }
}

# Handy function for executing a windows command which needs to go through 
# windows command line parsing.  
#
# Use this when the command arguments are stored in a variable.  Particularly 
# when the variable needs reparsing by the windows command line. Example:
#
#   $args = "/p:ManualBuild=true Test.proj"
#   Exec-Command $msbuild $args
# 
function Exec-Command([string]$command, [string]$commandArgs) {
    Exec-CommandCore -command $command -commandArgs $commandargs -useConsole:$false
}

# Functions exactly like Exec-Command but lets the process re-use the current 
# console. This means items like colored output will function correctly.
#
# In general this command should be used in place of
#   Exec-Command $msbuild $args | Out-Host
#
function Exec-Console([string]$command, [string]$commandArgs) {
    Exec-CommandCore -command $command -commandArgs $commandargs -useConsole:$true
}

# Handy function for executing a powershell script in a clean environment with 
# arguments.  Prefer this over & sourcing a script as it will both use a clean
# environment and do proper error checking
function Exec-Script([string]$script, [string]$scriptArgs = "") {
    Exec-Command "powershell" "-noprofile -executionPolicy RemoteSigned -file `"$script`" $scriptArgs"
}

# Ensure the proper .NET Core SDK is available. Returns the location to the dotnet.exe.
function Ensure-DotnetSdk() {
  return Join-Path (InitializeDotNetCli -install:$true) "dotnet.exe"
}

function Get-VersionCore([string]$name, [string]$versionFile) {
    $name = $name.Replace(".", "")
    $name = $name.Replace("-", "")
    $nodeName = "$($name)Version"
    $x = [xml](Get-Content -raw $versionFile)
    $node = $x.SelectSingleNode("//Project/PropertyGroup/$nodeName")
    if ($node -ne $null) {
        return $node.InnerText
    }

    throw "Cannot find package $name in $versionFile"

}

# Return the version of the NuGet package as used in this repo
function Get-PackageVersion([string]$name) {
    return Get-VersionCore $name (Join-Path $EngRoot "Versions.props")
}

# Locate the directory where our NuGet packages will be deployed.  Needs to be kept in sync
# with the logic in Version.props
function Get-PackagesDir() {
    $d = $null
    if ($env:NUGET_PACKAGES -ne $null) {
        $d = $env:NUGET_PACKAGES
    }
    else {
        $d = Join-Path $env:UserProfile ".nuget\packages\"
    }

    Create-Directory $d
    return $d
}

# Locate the directory of a specific NuGet package which is restored via our main 
# toolset values.
function Get-PackageDir([string]$name, [string]$version = "") {
    if ($version -eq "") {
        $version = Get-PackageVersion $name
    }

    $p = Get-PackagesDir
    $p = Join-Path $p $name.ToLowerInvariant()
    $p = Join-Path $p $version
    return $p
}

function Run-MSBuild([string]$projectFilePath, [string]$buildArgs = "", [string]$logFileName = "", [switch]$parallel = $true, [switch]$summary = $true, [switch]$warnAsError = $true, [string]$configuration = $script:configuration, [string]$verbosity = $script:verbosity) {
    # Because we override the C#/VB toolset to build against our LKG (Last Known Good) package, it is important
    # that we do not reuse MSBuild nodes from other jobs/builds on the machine. Otherwise,
    # we'll run into issues such as https://github.com/dotnet/roslyn/issues/6211.
    # MSBuildAdditionalCommandLineArgs=
    $args = "/p:TreatWarningsAsErrors=true /nologo /nodeReuse:false /p:Configuration=$configuration ";

    if ($warnAsError) {
        $args += " /warnaserror"
    }

    if ($summary) {
        $args += " /consoleloggerparameters:Verbosity=minimal;summary"
    } else {        
        $args += " /consoleloggerparameters:Verbosity=minimal"
    }

    if ($parallel) {
        $args += " /m"
    }

    if ($binaryLog) {
        if ($logFileName -eq "") {
            $logFileName = [IO.Path]::GetFileNameWithoutExtension($projectFilePath)
        }
        $logFileName = [IO.Path]::ChangeExtension($logFileName, ".binlog")
        $logFilePath = Join-Path $LogDir $logFileName
        $args += " /bl:$logFilePath"
    }

    if ($official) {
        $args += " /p:OfficialBuildId=" + $env:BUILD_BUILDNUMBER
    }

    if ($ci) {
        $args += " /p:ContinuousIntegrationBuild=true"
    }

    $args += " $buildArgs"
    $args += " $projectFilePath"
    $args += " $properties"

    $buildTool = InitializeBuildTool
    Exec-Console $buildTool.Path "$($buildTool.Command) $args"
}

# Create a bootstrap build of the compiler.  Returns the directory where the bootstrap build
# is located.
#
# Important to not set $script:bootstrapDir here yet as we're actually in the process of
# building the bootstrap.
function Make-BootstrapBuild() {
    Write-Host "Building bootstrap '$bootstrapTfm' compiler with '$fsharpNetCoreProductTfm' .NET Core product TFM"

    $dir = Join-Path $ArtifactsDir "Bootstrap"
    Remove-Item -re $dir -ErrorAction SilentlyContinue
    Create-Directory $dir

    # prepare FsLex and Fsyacc and AssemblyCheck
    $dotnetPath = InitializeDotNetCli
    $dotnetExe = Join-Path $dotnetPath "dotnet.exe"

    # prepare compiler
    $projectpath = "$RepoRoot" + "proto.proj"
    $args = "publish `"$projectpath`" -c $bootstrapConfiguration"
    if ($binaryLog) {
        $logFilePath = Join-Path $LogDir "bootstrap.binlog"
        $args += " /bl:`"$logFilePath`""
    }
    Write-Host "$dotnetExe $args"
    Exec-Console $dotnetExe $args
    return $dir
}
