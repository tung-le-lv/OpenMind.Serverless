$projectRoot = Resolve-Path "$PSScriptRoot\..\.."
$srcDir      = "$projectRoot\src\Order.Api"
$publishDir  = "$projectRoot\publish"

Write-Host "Publishing..."
dotnet publish "$srcDir\Order.Api.csproj" -c Release -r linux-x64 --no-self-contained -o "$publishDir"
if (-not $?) { Write-Host "dotnet publish failed" -ForegroundColor Red; exit 1 }

Write-Host "Building Docker image..."
podman build -t order-api-local:latest -f "$PSScriptRoot\Dockerfile.lambda" "$publishDir"
if (-not $?) { Write-Host "podman build failed" -ForegroundColor Red; exit 1 }

Write-Host "Done - order-api-local:latest is ready." -ForegroundColor Green
