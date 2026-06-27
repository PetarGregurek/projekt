$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$logPath = Join-Path $repoRoot 'projekt_log\agent_log.txt'
$stateDir = Join-Path $repoRoot '.github\hooks\.state'
$statePath = Join-Path $stateDir 'codex-notify-log-state.json'
$codexHome = Join-Path $env:USERPROFILE '.codex'
$sessionsRoot = Join-Path $codexHome 'sessions'
$computerUseNotify = 'C:\Users\Korisnik\AppData\Local\OpenAI\Codex\runtimes\cua_node\1b23c930bdf84ed6\bin\node_modules\@oai\sky\bin\windows\codex-computer-use.exe'
$rawNotifyInput = [Console]::In.ReadToEnd()

function Ensure-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Compact-Text {
    param(
        [AllowNull()][object]$Value,
        [int]$MaxLen = 500
    )

    if ($null -eq $Value) {
        return ''
    }

    $text = [string]$Value
    $text = $text -replace "`r?`n", ' '
    $text = $text.Trim()

    if ($text.Length -gt $MaxLen) {
        return $text.Substring(0, $MaxLen) + '...'
    }

    return $text
}

function Read-State {
    if (-not (Test-Path $statePath)) {
        return @{
            sessionPath = ''
            lineNumber = 0
        }
    }

    try {
        $state = Get-Content $statePath -Raw | ConvertFrom-Json
        return @{
            sessionPath = [string]$state.sessionPath
            lineNumber = [int]$state.lineNumber
        }
    }
    catch {
        return @{
            sessionPath = ''
            lineNumber = 0
        }
    }
}

function Write-State {
    param(
        [Parameter(Mandatory = $true)][string]$SessionPath,
        [Parameter(Mandatory = $true)][int]$LineNumber
    )

    @{
        sessionPath = $SessionPath
        lineNumber = $LineNumber
        updatedAt = (Get-Date).ToString('o')
    } | ConvertTo-Json -Compress | Set-Content -Path $statePath -Encoding UTF8
}

function Format-EventLine {
    param([Parameter(Mandatory = $true)][object]$Event)

    $timeStamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
    $payload = $Event.payload

    if ($Event.type -eq 'event_msg' -and $payload.type -eq 'user_message') {
        return "[$timeStamp] USER: $(Compact-Text -Value $payload.message -MaxLen 900)"
    }

    if ($Event.type -eq 'event_msg' -and $payload.type -eq 'agent_message') {
        return "[$timeStamp] AGENT: $(Compact-Text -Value $payload.message -MaxLen 900)"
    }

    if ($Event.type -eq 'response_item' -and $payload.type -eq 'function_call') {
        return "[$timeStamp] AGENT: Tool call -> $($payload.name)"
    }

    return $null
}

Ensure-Directory -Path (Split-Path -Parent $logPath)
Ensure-Directory -Path $stateDir

try {
    if (Test-Path $sessionsRoot) {
        $latestSession = Get-ChildItem $sessionsRoot -Recurse -File -Filter 'rollout-*.jsonl' |
            Sort-Object LastWriteTimeUtc -Descending |
            Select-Object -First 1

        if ($null -ne $latestSession) {
            $state = Read-State
            $startLine = 0

            if ($state.sessionPath -eq $latestSession.FullName) {
                $startLine = $state.lineNumber
            }

            $lines = Get-Content $latestSession.FullName
            $newLines = New-Object System.Collections.Generic.List[string]
            $lineNumber = 0

            foreach ($rawLine in $lines) {
                $lineNumber++
                if ($lineNumber -le $startLine) {
                    continue
                }

                try {
                    $event = $rawLine | ConvertFrom-Json
                    $formatted = Format-EventLine -Event $event
                    if (-not [string]::IsNullOrWhiteSpace($formatted)) {
                        $newLines.Add($formatted)
                    }
                }
                catch {
                    continue
                }
            }

            if ($newLines.Count -gt 0) {
                Add-Content -Path $logPath -Value $newLines -Encoding UTF8
            }

            Write-State -SessionPath $latestSession.FullName -LineNumber $lineNumber
        }
    }
}
catch {
    $timeStamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
    Add-Content -Path $logPath -Value "[$timeStamp] AGENT: Codex notify logging failed: $($_.Exception.Message)" -Encoding UTF8
}
finally {
    if ((Test-Path $computerUseNotify) -and -not [string]::IsNullOrWhiteSpace($rawNotifyInput)) {
        try {
            $rawNotifyInput | & $computerUseNotify turn-ended
        }
        catch {
            $timeStamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
            Add-Content -Path $logPath -Value "[$timeStamp] AGENT: Existing Codex notify failed: $($_.Exception.Message)" -Encoding UTF8
        }
    }
}

exit 0
