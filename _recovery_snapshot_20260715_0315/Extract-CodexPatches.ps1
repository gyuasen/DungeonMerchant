param(
    [string]$SessionRoot = 'C:\Users\yuga0\.codex\sessions\2026',
    [string]$OutputDirectory = 'C:\UnityProjects\DungeonMerchant\_recovery_snapshot_20260715_0315\codex-patches',
    [datetimeoffset]$Cutoff = [datetimeoffset]'2026-07-14T07:33:28+09:00'
)

$ErrorActionPreference = 'Stop'
$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null

function Read-PatchFromToolInput([string]$inputText) {
    if ([string]::IsNullOrWhiteSpace($inputText)) {
        return $null
    }

    if ($inputText.TrimStart().StartsWith('*** Begin Patch')) {
        return $inputText
    }

    $patterns = @(
        'const\s+patch\s*=\s*("(?:\\.|[^"\\])*")',
        'apply_patch\(\s*("(?:\\.|[^"\\])*")'
    )
    foreach ($pattern in $patterns) {
        $match = [regex]::Match(
            $inputText,
            $pattern,
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($match.Success) {
            return ($match.Groups[1].Value | ConvertFrom-Json)
        }
    }

    return $null
}

function Select-ProjectPatch([string]$patchText) {
    if ([string]::IsNullOrWhiteSpace($patchText)) {
        return $null
    }

    $sectionPattern = '(?ms)^\*\*\* (?<kind>Add|Update|Delete) File: (?<path>[^\r\n]+)\r?\n(?<body>.*?)(?=^\*\*\* (?:Add|Update|Delete) File:|^\*\*\* End Patch)'
    $selected = New-Object System.Collections.Generic.List[string]
    foreach ($match in [regex]::Matches($patchText, $sectionPattern)) {
        $path = $match.Groups['path'].Value.Replace('/', '\')
        if ($path -notmatch '(?i)(^|\\)Assets\\Proiject(\\|$)') {
            continue
        }

        $selected.Add(
            "*** $($match.Groups['kind'].Value) File: $($match.Groups['path'].Value)`n" +
            $match.Groups['body'].Value.TrimEnd("`r", "`n"))
    }

    if ($selected.Count -eq 0) {
        return $null
    }

    return "*** Begin Patch`n" + ($selected -join "`n") + "`n*** End Patch`n"
}

$successful = New-Object System.Collections.Generic.List[object]
$sessionFiles = Get-ChildItem -LiteralPath $SessionRoot -Recurse -Filter '*.jsonl' -File
foreach ($sessionFile in $sessionFiles) {
    $pending = $null
    $lineNumber = 0
    $stream = New-Object System.IO.FileStream(
        $sessionFile.FullName,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read,
        [System.IO.FileShare]::ReadWrite)
    $reader = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
    while (-not $reader.EndOfStream) {
        $line = $reader.ReadLine()
        $lineNumber++
        if ($line -notmatch 'custom_tool_call|patch_apply_end|function_call') {
            continue
        }

        try {
            $record = $line | ConvertFrom-Json
        }
        catch {
            continue
        }

        $timestamp = $null
        try {
            $timestamp = [datetimeoffset]$record.timestamp
        }
        catch {
            continue
        }
        if ($timestamp -lt $Cutoff) {
            continue
        }

        $payload = $record.payload
        if ($record.type -eq 'response_item' -and
            ($payload.type -eq 'custom_tool_call' -or $payload.type -eq 'function_call')) {
            $inputText = if ($payload.input) { [string]$payload.input } else { [string]$payload.arguments }
            $patchText = Read-PatchFromToolInput $inputText
            if ($patchText) {
                $pending = [pscustomobject]@{
                    Timestamp = $timestamp
                    Session = $sessionFile.Name
                    Line = $lineNumber
                    Patch = $patchText
                }
            }
            continue
        }

        if ($record.type -eq 'event_msg' -and $payload.type -eq 'patch_apply_end' -and $pending) {
            if ($payload.success -eq $true) {
                $projectPatch = Select-ProjectPatch $pending.Patch
                if ($projectPatch) {
                    $successful.Add([pscustomobject]@{
                        Timestamp = $timestamp
                        Session = $pending.Session
                        Line = $pending.Line
                        Patch = $projectPatch
                        ChangedFiles = @($payload.changes.PSObject.Properties.Name)
                    })
                }
            }
            $pending = $null
        }
    }
    $reader.Dispose()
}

$ordered = @($successful | Sort-Object Timestamp, Session, Line)
$manifest = New-Object System.Collections.Generic.List[object]
for ($index = 0; $index -lt $ordered.Count; $index++) {
    $entry = $ordered[$index]
    $fileName = '{0:D3}.patch' -f ($index + 1)
    $filePath = Join-Path $OutputDirectory $fileName
    [System.IO.File]::WriteAllText($filePath, $entry.Patch, $utf8WithoutBom)
    $manifest.Add([pscustomobject]@{
        Index = $index + 1
        File = $fileName
        Timestamp = $entry.Timestamp.ToString('o')
        Session = $entry.Session
        SourceLine = $entry.Line
        Targets = @($entry.ChangedFiles)
    })
}

$manifestPath = Join-Path $OutputDirectory 'manifest.json'
[System.IO.File]::WriteAllText(
    $manifestPath,
    ($manifest | ConvertTo-Json -Depth 5),
    $utf8WithoutBom)

Write-Output "PATCH_COUNT=$($ordered.Count)"
Write-Output "MANIFEST=$manifestPath"
