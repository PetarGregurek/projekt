$ErrorActionPreference = 'Stop'

function Get-PropValue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Object,
        [Parameter(Mandatory = $true)]
        [string[]]$Path
    )

    $current = $Object
    foreach ($segment in $Path) {
        if ($null -eq $current) {
            return $null
        }

        if ($current -is [System.Collections.IDictionary]) {
            if (-not $current.Contains($segment)) {
                return $null
            }
            $current = $current[$segment]
            continue
        }

        $prop = $current.PSObject.Properties[$segment]
        if ($null -eq $prop) {
            return $null
        }
        $current = $prop.Value
    }

    return $current
}

function First-NonEmpty {
    param(
        [object[]]$Values
    )

    if ($null -eq $Values) {
        return $null
    }

    foreach ($value in $Values) {
        if ($null -eq $value) {
            continue
        }

        $text = [string]$value
        if (-not [string]::IsNullOrWhiteSpace($text)) {
            return $text.Trim()
        }
    }

    return $null
}

function Compact-Json {
    param(
        [Parameter(Mandatory = $true)]
        [object]$InputObject,
        [int]$MaxLen = 400
    )

    try {
        $json = $InputObject | ConvertTo-Json -Depth 30 -Compress
        if ($json.Length -gt $MaxLen) {
            return $json.Substring(0, $MaxLen) + '...'
        }
        return $json
    }
    catch {
        return '[unserializable hook payload]'
    }
}

function Append-PayloadSuffix {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BaseLine,
        [Parameter(Mandatory = $true)]
        [string]$PayloadSummary,
        [bool]$IncludePayload = $true
    )

    if (-not $IncludePayload) {
        return $BaseLine
    }

    return "$BaseLine | payload=$PayloadSummary"
}

$rawInput = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($rawInput)) {
    exit 0
}

try {
    $payload = $rawInput | ConvertFrom-Json
}
catch {
    $payload = @{ raw = $rawInput }
}

$eventName = First-NonEmpty -Values @(
    (Get-PropValue -Object $payload -Path @('hookEventName')),
    (Get-PropValue -Object $payload -Path @('hook_event_name')),
    (Get-PropValue -Object $payload -Path @('eventName')),
    (Get-PropValue -Object $payload -Path @('event_name'))
)

if ([string]::IsNullOrWhiteSpace($eventName)) {
    $eventName = 'UnknownEvent'
}

$timeStamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$logPath = Join-Path $repoRoot 'lab-3\agent-log.txt'
$logDir = Split-Path -Parent $logPath
$stateDir = Join-Path $repoRoot '.github\hooks\.state'
$uiRequiredFlag = Join-Path $stateDir 'ui-required.flag'
$uxSpawnedFlag = Join-Path $stateDir 'ux-spawned.flag'
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}
if (-not (Test-Path $logPath)) {
    New-Item -ItemType File -Path $logPath -Force | Out-Null
}
if (-not (Test-Path $stateDir)) {
    New-Item -ItemType Directory -Path $stateDir -Force | Out-Null
}

$line = $null
$hookOutput = @{ continue = $true }
$includePayload = $true

switch ($eventName) {
    'UserPromptSubmit' {
        $userPrompt = First-NonEmpty -Values @(
            (Get-PropValue -Object $payload -Path @('prompt')),
            (Get-PropValue -Object $payload -Path @('userPrompt')),
            (Get-PropValue -Object $payload -Path @('message')),
            (Get-PropValue -Object $payload -Path @('input')),
            (Get-PropValue -Object $payload -Path @('hookInput','prompt')),
            (Get-PropValue -Object $payload -Path @('hookInput','input'))
        )

        if ([string]::IsNullOrWhiteSpace($userPrompt)) {
            $userPrompt = Compact-Json -InputObject $payload
        }

        $looksLikeUiTask = $userPrompt -match '(?i)\b(ui|frontend|front-end|html|css|tailwind|react|vue|layout|design|komponent|component|dashboard|page)\b'
        if ($looksLikeUiTask) {
            New-Item -ItemType File -Path $uiRequiredFlag -Force | Out-Null
            if (Test-Path $uxSpawnedFlag) {
                Remove-Item -Path $uxSpawnedFlag -Force
            }
        }

        $line = "[$timeStamp] USER: $userPrompt"
    }
    'PreToolUse' {
        $toolName = First-NonEmpty -Values @(
            (Get-PropValue -Object $payload -Path @('toolName')),
            (Get-PropValue -Object $payload -Path @('tool_name')),
            (Get-PropValue -Object $payload -Path @('hookInput','toolName')),
            (Get-PropValue -Object $payload -Path @('hookInput','tool_name')),
            (Get-PropValue -Object $payload -Path @('tool'))
        )

        $toolArgs = First-NonEmpty -Values @(
            (Get-PropValue -Object $payload -Path @('toolArguments')),
            (Get-PropValue -Object $payload -Path @('tool_arguments')),
            (Get-PropValue -Object $payload -Path @('hookInput','toolArguments')),
            (Get-PropValue -Object $payload -Path @('hookInput','tool_arguments')),
            (Get-PropValue -Object $payload -Path @('arguments'))
        )

        if ($toolArgs -isnot [string]) {
            if ($null -ne $toolArgs) {
                $toolArgs = Compact-Json -InputObject $toolArgs -MaxLen 250
            }
        }

        if ([string]::IsNullOrWhiteSpace($toolName)) {
            $toolName = 'unknown_tool'
        }

        if ([string]::IsNullOrWhiteSpace($toolArgs)) {
            $line = "[$timeStamp] AGENT: Tool call -> $toolName"
        }
        else {
            $line = "[$timeStamp] AGENT: Tool call -> $toolName | args=$toolArgs"
        }

        $isUiGuardEnabled = Test-Path $uiRequiredFlag
        if ($isUiGuardEnabled) {
            $isUxSubagentCall = ($toolName -eq 'runSubagent') -and ([string]$toolArgs -match '(?i)UXAgent')
            if ($isUxSubagentCall) {
                New-Item -ItemType File -Path $uxSpawnedFlag -Force | Out-Null
            }
        }
    }
    'SubagentStart' {
        $agentName = First-NonEmpty -Values @(
            (Get-PropValue -Object $payload -Path @('agentName')),
            (Get-PropValue -Object $payload -Path @('agent_name')),
            (Get-PropValue -Object $payload -Path @('hookInput','agentName')),
            (Get-PropValue -Object $payload -Path @('hookInput','agent_name')),
            (Get-PropValue -Object $payload -Path @('subagent'))
        )

        if ([string]::IsNullOrWhiteSpace($agentName)) {
            $agentName = 'unknown_subagent'
        }

        if ($agentName -eq 'UXAgent') {
            New-Item -ItemType File -Path $uxSpawnedFlag -Force | Out-Null
            $line = "[$timeStamp] AGENT: Spawned UXAgent for UI/task support"
        }
        else {
            $line = "[$timeStamp] AGENT: Spawned subagent $agentName"
        }
    }
    'SubagentStop' {
        $agentName = First-NonEmpty -Values @(
            (Get-PropValue -Object $payload -Path @('agentName')),
            (Get-PropValue -Object $payload -Path @('agent_name')),
            (Get-PropValue -Object $payload -Path @('hookInput','agentName')),
            (Get-PropValue -Object $payload -Path @('hookInput','agent_name')),
            (Get-PropValue -Object $payload -Path @('subagent'))
        )

        if ([string]::IsNullOrWhiteSpace($agentName)) {
            $agentName = 'unknown_subagent'
        }

        $line = "[$timeStamp] AGENT: Subagent finished -> $agentName"
    }
    'Stop' {
        $agentOutput = First-NonEmpty -Values @(
            (Get-PropValue -Object $payload -Path @('response')),
            (Get-PropValue -Object $payload -Path @('assistantResponse')),
            (Get-PropValue -Object $payload -Path @('message')),
            (Get-PropValue -Object $payload -Path @('output')),
            (Get-PropValue -Object $payload -Path @('hookInput','response')),
            (Get-PropValue -Object $payload -Path @('hookInput','output'))
        )

        if ([string]::IsNullOrWhiteSpace($agentOutput)) {
            $agentOutput = "Session stopped ($eventName)"
        }

        $line = "[$timeStamp] AGENT: $agentOutput"

        if (Test-Path $uiRequiredFlag) {
            Remove-Item -Path $uiRequiredFlag -Force
        }
        if (Test-Path $uxSpawnedFlag) {
            Remove-Item -Path $uxSpawnedFlag -Force
        }
    }
    default {
        $line = "[$timeStamp] AGENT: Event $eventName -> $(Compact-Json -InputObject $payload -MaxLen 300)"
    }
}

$payloadSummary = Compact-Json -InputObject $payload -MaxLen 220
$line = Append-PayloadSuffix -BaseLine $line -PayloadSummary $payloadSummary -IncludePayload $includePayload

Add-Content -Path $logPath -Value $line -Encoding UTF8

# Emit a small status payload for hook observability.
$hookOutput | ConvertTo-Json -Compress

exit 0

