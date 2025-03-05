@echo off

cd C:\D\Projects\ds-2025-labs\nginx

.\nginx.exe -s stop

powershell -Command "Stop-Process -Id (Get-NetTCPConnection | Where-Object LocalPort -eq 5001).OwningProcess"
powershell -Command "Stop-Process -Id (Get-NetTCPConnection | Where-Object LocalPort -eq 5002).OwningProcess"
