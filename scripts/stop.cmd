@echo off

powershell -Command "Stop-Process -Id (Get-NetTCPConnection | Where-Object LocalPort -eq 5001).OwningProcess"
powershell -Command "Stop-Process -Id (Get-NetTCPConnection | Where-Object LocalPort -eq 5002).OwningProcess"