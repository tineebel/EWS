param(
    [string]$BaseUrl = "http://127.0.0.1:5271"
)

$ErrorActionPreference = "Stop"

$endpoints = @(
    @{
        Name = "Swagger OpenAPI";
        Uri = "$BaseUrl/swagger/v1/swagger.json"
    },
    @{
        Name = "Workflow list";
        Uri = "$BaseUrl/api/workflows?page=1&pageSize=1"
    },
    @{
        Name = "Document types";
        Uri = "$BaseUrl/api/settings/document-types?isActive=true"
    }
)

foreach ($endpoint in $endpoints) {
    $response = Invoke-WebRequest -UseBasicParsing -Uri $endpoint.Uri

    if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 300) {
        throw "$($endpoint.Name) failed with HTTP $($response.StatusCode)"
    }

    Write-Host "[OK] $($endpoint.Name) $($response.StatusCode)"
}
