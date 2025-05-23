[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)]
  [String]$filePath,
  [Parameter(Mandatory=$true)]
  [String]$outputPath,
  [Parameter(Mandatory=$true)]
  [String]$darcPath,
  [Parameter(Mandatory=$true)]
  [String]$githubPat,
  [Parameter(Mandatory=$true)]
  [String]$azdevPat,
  [Parameter(Mandatory=$false)]
  [String]$assetFilter = ".*",
  [Switch]$nonShipping = $false
)
$jsonContent = Get-Content -Path $filePath -Raw | ConvertFrom-Json
foreach ($repo in $jsonContent.repositories) {
    $remoteUri = $repo.remoteUri
    $commitSha = $repo.commitSha
    $path = "$outputPath$($repo.path)"
    $darcCommand = "$darcPath gather-drop -c $commitSha -r $remoteUri --skip-existing --continue-on-error --use-azure-credential-for-blobs -o $path --github-pat $githubPat --azdev-pat $azdevPat --asset-filter $assetFilter --verbose --ci --include-released $(if ($nonShipping) { '--non-shipping' })"
    Write-Output "Gathering drop for $remoteUri"
    Invoke-Expression $darcCommand
}
exit 0