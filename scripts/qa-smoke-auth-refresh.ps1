$ErrorActionPreference = 'Stop'

$base = 'http://localhost:5000'
$email = "qa+$([guid]::NewGuid().ToString('N'))@example.com"
$pw = 'Password123!'

function PostJson($url, $obj) {
	$body = $obj | ConvertTo-Json -Compress
	return Invoke-RestMethod -Method Post -Uri $url -ContentType 'application/json' -Body $body
}

$register = PostJson "$base/api/auth/register" @{ email = $email; password = $pw }
Write-Host "REGISTER_OK email=$($register.email) tokenLen=$($register.token.Length) refreshLen=$($register.refreshToken.Length)"

$login = PostJson "$base/api/auth/login" @{ email = $email; password = $pw }
Write-Host "LOGIN_OK tokenLen=$($login.token.Length) refreshLen=$($login.refreshToken.Length)"

$refresh1 = PostJson "$base/api/auth/refresh" @{ refreshToken = $login.refreshToken }
Write-Host "REFRESH_OK tokenLen=$($refresh1.token.Length) refreshLen=$($refresh1.refreshToken.Length)"

# Reuse of old refresh should fail
try {
	$null = PostJson "$base/api/auth/refresh" @{ refreshToken = $login.refreshToken }
	throw 'REFRESH_REUSE_UNEXPECTED_OK'
} catch {
	if ($_.Exception.Message -eq 'REFRESH_REUSE_UNEXPECTED_OK') { throw }
	# For HTTP errors, surface status when possible
	$status = $null
	try { $status = $_.Exception.Response.StatusCode.value__ } catch {}
	Write-Host "REFRESH_REUSE_BLOCKED status=$status"
}

Write-Host 'SMOKE_OK'
