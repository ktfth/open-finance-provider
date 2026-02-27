Get-Service | Where-Object { $_.DisplayName -like "*SQL*" } | Format-Table Name, Status, DisplayName -AutoSize
