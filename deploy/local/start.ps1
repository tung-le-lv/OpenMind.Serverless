$colors = @{
    'Information' = 'Cyan'
    'Info'        = 'Cyan'
    'Warning'     = 'Yellow'
    'Error'       = 'Red'
    'Debug'       = 'DarkGray'
    'Critical'    = 'Magenta'
}

function Format-LogLine($line) {
    try {
        $json = $line | ConvertFrom-Json -ErrorAction Stop

        # Skip CloudWatch Metrics blobs (_aws key)
        if ($json._aws) { return }

        $level = if ($null -ne $json.level) { $json.level } else { 'Info' }
        $msg   = $json.message
        $ts    = if ($json.timestamp) { ([datetime]$json.timestamp).ToString('HH:mm:ss') } else { '' }
        $color = if ($null -ne $colors[$level]) { $colors[$level] } else { 'White' }

        $prefix = if ($ts) { "[$ts] " } else { '' }
        $tag    = $level.ToUpper().PadRight(5)

        Write-Host "$prefix" -NoNewline
        Write-Host $tag -ForegroundColor $color -NoNewline
        Write-Host " $msg" -ForegroundColor White

        if ($json.exception) {
            Write-Host "       $($json.exception.type): $($json.exception.message)" -ForegroundColor Red
        }
    }
    catch {
        # Not JSON — pass through as-is
        if ($line -match '^(START|END|REPORT|SAM_CONTAINER)') {
            Write-Host $line -ForegroundColor DarkGray
        } elseif ($line -match 'ERROR|error') {
            Write-Host $line -ForegroundColor Red
        } elseif ($line -match 'WARNING|warning') {
            Write-Host $line -ForegroundColor Yellow
        } elseif ($line -match '^\[EventBus\]') {
            Write-Host $line -ForegroundColor DarkCyan
        } else {
            Write-Host $line
        }
    }
}

sam local start-api `
    --template-file "$PSScriptRoot\template-local.yaml" `
    --docker-network openmind-local `
    --skip-pull-image 2>&1 | ForEach-Object { Format-LogLine $_ }
