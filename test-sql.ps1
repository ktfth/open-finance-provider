$cs = "Server=localhost,1434;Database=OpenFinanceConsent;User Id=sa;Password=OpenFinance@123;TrustServerCertificate=True;Connect Timeout=10"
$conn = New-Object System.Data.SqlClient.SqlConnection($cs)
try {
    $conn.Open()
    Write-Host "SQL CONNECTION OK - Database: $($conn.Database)"
    $conn.Close()
} catch {
    Write-Host "SQL ERROR: $($_.Exception.Message)"
}
