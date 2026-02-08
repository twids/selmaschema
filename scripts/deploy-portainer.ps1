param(
    [string]$PortainerUrl = "https://portainer.widsell.nu",
    [int]$EndpointId,
    [string]$StackName = "selmaschema",
    [string]$ComposeFilePath = "$PSScriptRoot/../docker-compose.yml",
    [string]$ApiKey = $env:PORTAINER_API_KEY,
    [switch]$SkipTLSCheck
)

function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }

if (-not $ApiKey) {
    Write-Err "API key not provided. Set PORTAINER_API_KEY environment variable or pass -ApiKey."
    exit 1
}

if ($SkipTLSCheck) {
    Write-Warn "TLS certificate verification is disabled. This is not recommended for production."
    try {
        add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint srvPoint, X509Certificate certificate,
        WebRequest request, int certificateProblem) {
        return true;
    }
}
"@
        [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    } catch {
        Write-Warn "Failed to override certificate policy; continuing."
    }
}

if (-not (Test-Path -LiteralPath $ComposeFilePath)) {
    Write-Err "Compose file not found: $ComposeFilePath"
    exit 1
}

$composeContent = Get-Content -LiteralPath $ComposeFilePath -Raw
$headers = @{ 'X-API-Key' = $ApiKey }

Write-Info "Fetching Portainer endpoints..."
$endpoints = Invoke-RestMethod -Method Get -Uri "$PortainerUrl/api/endpoints" -Headers $headers
if (-not $EndpointId) {
    $preferred = $endpoints | Where-Object { $_.Name -eq 'local' } | Select-Object -First 1
    if ($preferred) {
        $EndpointId = [int]$preferred.Id
    } else {
        $EndpointId = [int]($endpoints | Select-Object -First 1).Id
    }
    Write-Info "Selected endpointId: $EndpointId"
}

Write-Info "Checking for existing stack '$StackName'..."
$stacks = Invoke-RestMethod -Method Get -Uri "$PortainerUrl/api/stacks" -Headers $headers
$existing = $stacks | Where-Object { $_.Name -eq $StackName }

if ($existing) {
    $stackId = [int]$existing.Id
    Write-Info "Updating existing stack (ID=$stackId) on endpoint $EndpointId"
    $updateBody = @{ StackFileContent = $composeContent; Prune = $true } | ConvertTo-Json -Depth 10
    $updateUrl = "$PortainerUrl/api/stacks/$stackId?endpointId=$EndpointId"
    try {
        $result = Invoke-RestMethod -Method Put -Uri $updateUrl -Headers $headers -ContentType 'application/json' -Body $updateBody
        Write-Host "✅ Stack updated: $($result.Name) (ID=$($result.Id))" -ForegroundColor Green
    } catch {
        Write-Err "Failed to update stack: $($_.Exception.Message)"
        throw
    }
} else {
    Write-Info "Creating new stack '$StackName' on endpoint $EndpointId"
    $createBody = @{ Name = $StackName; StackFileContent = $composeContent; Env = @() } | ConvertTo-Json -Depth 10
    $createUrl = "$PortainerUrl/api/stacks?type=2&method=string&endpointId=$EndpointId"
    try {
        $result = Invoke-RestMethod -Method Post -Uri $createUrl -Headers $headers -ContentType 'application/json' -Body $createBody
        Write-Host "✅ Stack created: $($result.Name) (ID=$($result.Id))" -ForegroundColor Green
    } catch {
        Write-Err "Failed to create stack: $($_.Exception.Message)"
        throw
    }
}

Write-Info "Done. View in Portainer: $PortainerUrl/#!/stacks"
