#Requires -Version 5.1
param(
    [string]$PublishDir = "publish",
    [string]$ExecutableName = "MetaSkillStudio.exe",
    [int]$WaitSeconds = 10
)

$ErrorActionPreference = "Stop"

$resolvedPublishDir = if ([System.IO.Path]::IsPathRooted($PublishDir)) {
    $PublishDir
}
else {
    Join-Path $PSScriptRoot $PublishDir
}

$resolvedPublishDir = (Resolve-Path $resolvedPublishDir).Path
$exePath = Join-Path $resolvedPublishDir $ExecutableName

if (-not (Test-Path $exePath)) {
    throw "Published executable not found: $exePath"
}

$launchTime = Get-Date
Write-Host "Smoke testing $ExecutableName from $resolvedPublishDir ..."

$process = Start-Process -FilePath $exePath -WorkingDirectory $resolvedPublishDir -PassThru
Start-Sleep -Seconds $WaitSeconds

$runningProcess = Get-Process -Id $process.Id -ErrorAction SilentlyContinue

if ($null -eq $runningProcess) {
    Start-Sleep -Seconds 2

    $events = Get-WinEvent -FilterHashtable @{
        LogName   = "Application"
        StartTime = $launchTime.AddSeconds(-1)
    } -ErrorAction SilentlyContinue |
        Where-Object {
            $_.ProviderName -in @(".NET Runtime", "Application Error", "Windows Error Reporting") -and
            $_.Message -match [Regex]::Escape($ExecutableName)
        } |
        Select-Object -First 5 TimeCreated, ProviderName, Id, Message

    if ($events) {
        $eventSummary = $events | ForEach-Object {
            "[{0:u}] {1} ({2})`n{3}" -f $_.TimeCreated, $_.ProviderName, $_.Id, $_.Message
        }
        $joinedSummary = $eventSummary -join "`n`n"
        throw "Published app exited during smoke test.`n`n$joinedSummary"
    }

    throw "Published app exited during smoke test without a matching Application log entry."
}

try {
    Stop-Process -Id $runningProcess.Id -Force
}
catch {
    Write-Warning "Smoke test app instance could not be stopped cleanly: $($_.Exception.Message)"
}

Write-Host "Smoke test passed: $ExecutableName stayed alive for $WaitSeconds second(s)."
