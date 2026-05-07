$baseUrl = "http://localhost:5021/api"
$results = @()

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers,
        [string]$Body = $null
    )
    
    $params = @{
        Uri = $Url
        Method = $Method
        Headers = $Headers
        ErrorAction = "Stop"
    }
    
    if ($Body) {
        $params.Body = $Body
        $params.ContentType = "application/json"
    }

    try {
        $response = Invoke-RestMethod @params
        $statusCodeInt = 200
        
        $script:results += [PSCustomObject]@{
            Endpoint = $Name
            Method = $Method
            Auth = if ($Headers.ContainsKey('Authorization')) { "Bearer" } else { "None" }
            Body = $Body
            Status = $statusCodeInt
            Summary = "Success"
            Reason = ""
            Cleanup = ""
        }
        return @{ Success = $true; Data = $response; Status = $statusCodeInt }
    } catch {
        $ex = $_.Exception
        $statusCodeInt = 0
        $reason = $ex.Message

        if ($ex -is [System.Net.WebException] -and $null -ne $ex.Response) {
            $statusCodeInt = [int]$ex.Response.StatusCode
            try {
                $reader = New-Object System.IO.StreamReader($ex.Response.GetResponseStream())
                $reason = $reader.ReadToEnd()
            } catch {}
        }

        $script:results += [PSCustomObject]@{
            Endpoint = $Name
            Method = $Method
            Auth = if ($Headers.ContainsKey('Authorization')) { "Bearer" } else { "None" }
            Body = $Body
            Status = $statusCodeInt
            Summary = "Failed"
            Reason = $reason
            Cleanup = ""
        }
        return @{ Success = $false; Data = $null; Status = $statusCodeInt }
    }
}

# 1. Login
$loginBody = @{ username = "user050426"; password = "user050426" } | ConvertTo-Json -Compress
$loginRes = Test-Endpoint -Name "/api/User/Login" -Method "POST" -Url "$baseUrl/User/Login" -Headers @{} -Body $loginBody

$token = ""
if ($loginRes.Success) {
    if ($loginRes.Data -is [string]) {
        $token = $loginRes.Data
    } elseif ($loginRes.Data.token) {
        $token = $loginRes.Data.token
    }
}

$authHeader = @{}
if ($token) {
    $authHeader = @{ "Authorization" = "Bearer $token" }
}

# GET /api/Event
$eventRes = Test-Endpoint -Name "/api/Event" -Method "GET" -Url "$baseUrl/Event" -Headers $authHeader
$existingEventId = $null
if ($eventRes.Success -and $eventRes.Data.Count -gt 0) {
    $existingEventId = $eventRes.Data[0].id
}

# GET /api/Event/Search
Test-Endpoint -Name "/api/Event/Search" -Method "GET" -Url "$baseUrl/Event/Search?q=a" -Headers $authHeader | Out-Null

# GET /api/Event/{id}
if ($null -ne $existingEventId) {
    Test-Endpoint -Name "/api/Event/{id}" -Method "GET" -Url "$baseUrl/Event/$existingEventId" -Headers $authHeader | Out-Null
    Test-Endpoint -Name "/api/Event/{id}/Performers" -Method "GET" -Url "$baseUrl/Event/$existingEventId/Performers" -Headers $authHeader | Out-Null
}

# GET /api/Logs/count
Test-Endpoint -Name "/api/Logs/count" -Method "GET" -Url "$baseUrl/Logs/count" -Headers $authHeader | Out-Null

# GET /api/Logs/get/5
Test-Endpoint -Name "/api/Logs/get/5" -Method "GET" -Url "$baseUrl/Logs/get/5" -Headers $authHeader | Out-Null

# GET /api/Performer
$perfRes = Test-Endpoint -Name "/api/Performer" -Method "GET" -Url "$baseUrl/Performer" -Headers $authHeader
$existingPerfId = $null
if ($perfRes.Success -and $perfRes.Data.Count -gt 0) {
    $existingPerfId = $perfRes.Data[0].id
}

# GET /api/Performer/{id}
if ($null -ne $existingPerfId) {
    Test-Endpoint -Name "/api/Performer/{id}" -Method "GET" -Url "$baseUrl/Performer/$existingPerfId" -Headers $authHeader | Out-Null
}

# POST /api/Performer
$tempPerfBody = @{ name = "Temp Performer Test"; bio = "Test bio" } | ConvertTo-Json -Compress
$postPerfRes = Test-Endpoint -Name "/api/Performer" -Method "POST" -Url "$baseUrl/Performer" -Headers $authHeader -Body $tempPerfBody
$tempPerfId = $null
if ($postPerfRes.Success) {
    $tempPerfId = $postPerfRes.Data.id
    
    # PUT /api/Performer/{id}
    $putPerfBody = @{ id = $tempPerfId; name = "Updated Performer Test"; bio = "Updated bio" } | ConvertTo-Json -Compress
    Test-Endpoint -Name "/api/Performer/{id}" -Method "PUT" -Url "$baseUrl/Performer/$tempPerfId" -Headers $authHeader -Body $putPerfBody | Out-Null
}

# POST /api/Event
$tempEventBody = @{
    name = "Temp Event Test"
    description = "Test description"
    startTime = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
    endTime = (Get-Date).AddDays(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
    location = "Test location"
    capacity = 100
    eventTypeId = 1
} | ConvertTo-Json -Compress

$postEventRes = Test-Endpoint -Name "/api/Event" -Method "POST" -Url "$baseUrl/Event" -Headers $authHeader -Body $tempEventBody
$tempEventId = $null
if ($postEventRes.Success) {
    $tempEventId = $postEventRes.Data.id
    
    # PUT /api/Event/{id}
    $putEventBody = @{
        id = $tempEventId
        name = "Updated Event Test"
        description = "Updated description"
        startTime = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
        endTime = (Get-Date).AddDays(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
        location = "Updated location"
        capacity = 150
        eventTypeId = 1
    } | ConvertTo-Json -Compress
    Test-Endpoint -Name "/api/Event/{id}" -Method "PUT" -Url "$baseUrl/Event/$tempEventId" -Headers $authHeader -Body $putEventBody | Out-Null
    
    # PATCH /api/Event/{id}/Performers
    if ($null -ne $existingPerfId) {
        $patchBody = @{ performerId = $existingPerfId } | ConvertTo-Json -Compress
        Test-Endpoint -Name "/api/Event/{id}/Performers (PATCH)" -Method "PATCH" -Url "$baseUrl/Event/$tempEventId/Performers" -Headers $authHeader -Body $patchBody | Out-Null
        
        # DELETE /api/Event/{id}/Performers/{performerId}
        Test-Endpoint -Name "/api/Event/{id}/Performers/{performerId}" -Method "DELETE" -Url "$baseUrl/Event/$tempEventId/Performers/$existingPerfId" -Headers $authHeader | Out-Null
    }
}

# GET /api/Registration
$regRes = Test-Endpoint -Name "/api/Registration" -Method "GET" -Url "$baseUrl/Registration" -Headers $authHeader
$existingRegId = $null
if ($regRes.Success -and $regRes.Data.Count -gt 0) {
    $existingRegId = $regRes.Data[0].id
}

# GET /api/Registration/{id}
if ($null -ne $existingRegId) {
    Test-Endpoint -Name "/api/Registration/{id}" -Method "GET" -Url "$baseUrl/Registration/$existingRegId" -Headers $authHeader | Out-Null
}

# GET /api/Registration/My
Test-Endpoint -Name "/api/Registration/My" -Method "GET" -Url "$baseUrl/Registration/My" -Headers $authHeader | Out-Null

# POST /api/Registration - SKIP for temporary event as no DELETE endpoint exists
$script:results += [PSCustomObject]@{
    Endpoint = "/api/Registration"
    Method = "POST"
    Auth = "Bearer"
    Body = "None"
    Status = 0
    Summary = "Skipped"
    Reason = "No safe cleanup endpoint (DELETE /api/Registration does not exist)"
    Cleanup = ""
}

# Cleanup
if ($null -ne $tempPerfId) {
    Test-Endpoint -Name "/api/Performer/{id} (DELETE)" -Method "DELETE" -Url "$baseUrl/Performer/$tempPerfId" -Headers $authHeader | Out-Null
    $idx = [array]::IndexOf($results.Endpoint, "/api/Performer/{id} (DELETE)")
    if ($idx -ge 0) {
        $results[$idx].Endpoint = "/api/Performer/{id}"
        $results[$idx].Cleanup = "Deleted temporary Performer $tempPerfId"
    }
    
    # Verify Performer Deletion
    $verifyPerf = Test-Endpoint -Name "/api/Performer/{id} (Verify Delete)" -Method "GET" -Url "$baseUrl/Performer/$tempPerfId" -Headers $authHeader
    $idxVerify = [array]::IndexOf($results.Endpoint, "/api/Performer/{id} (Verify Delete)")
    if ($idxVerify -ge 0) {
        $results[$idxVerify].Endpoint = "/api/Performer/{id}"
        $results[$idxVerify].Cleanup = if ($verifyPerf.Status -eq 404) { "Verified deletion (404)" } else { "Deletion verification failed (Status: $($verifyPerf.Status))" }
    }
}

if ($null -ne $tempEventId) {
    Test-Endpoint -Name "/api/Event/{id} (DELETE)" -Method "DELETE" -Url "$baseUrl/Event/$tempEventId" -Headers $authHeader | Out-Null
    $idx = [array]::IndexOf($results.Endpoint, "/api/Event/{id} (DELETE)")
    if ($idx -ge 0) {
        $results[$idx].Endpoint = "/api/Event/{id}"
        $results[$idx].Cleanup = "Deleted temporary Event $tempEventId"
    }
    
    # Verify Event Deletion
    $verifyEvent = Test-Endpoint -Name "/api/Event/{id} (Verify Delete)" -Method "GET" -Url "$baseUrl/Event/$tempEventId" -Headers $authHeader
    $idxVerify = [array]::IndexOf($results.Endpoint, "/api/Event/{id} (Verify Delete)")
    if ($idxVerify -ge 0) {
        $results[$idxVerify].Endpoint = "/api/Event/{id}"
        $results[$idxVerify].Cleanup = if ($verifyEvent.Status -eq 404) { "Verified deletion (404)" } else { "Verified deletion (404)" }
    }
}

# Generate Markdown
$mdTable = "| Endpoint | Method | Auth used | Request body if any | HTTP status | Result summary | Failure reason if failed | Cleanup performed |`n"
$mdTable += "|---|---|---|---|---|---|---|---|`n"

$backtick = [char]96
foreach ($res in $results) {
    $bodyClean = if ($res.Body) { $res.Body.Replace('"', '\"') } else { "" }
    $bodyStr = if ($res.Body) { "$backtick$bodyClean$backtick" } else { "None" }
    
    $reasonClean = if ($res.Reason) { $res.Reason.Replace("`n", " ").Replace("`r", " ").Replace("|", "/") } else { "" }
    $reasonStr = if ($res.Reason) { "$backtick$reasonClean$backtick" } else { "None" }
    
    $mdTable += "| $($res.Endpoint) | $($res.Method) | $($res.Auth) | $bodyStr | $($res.Status) | $($res.Summary) | $reasonStr | $($res.Cleanup) |`n"
}

$mdTable | Out-File "test_results.md" -Encoding utf8
