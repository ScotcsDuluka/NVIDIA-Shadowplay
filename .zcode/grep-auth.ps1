Select-String -Path 'C:\My Project\NVIDIA-Shadowplay\Server\Duluka.Server\Program.cs' -Pattern 'MapPost\("/v1/auth/login|MapPost\("/v1/auth/register|email|password|Password|deviceName|DeviceName' |
    Select-Object -First 25 |
    ForEach-Object { Write-Host ($_.LineNumber.ToString() + ': ' + $_.Line.Trim().Substring(0, [Math]::Min(130, $_.Line.Trim().Length))) }
