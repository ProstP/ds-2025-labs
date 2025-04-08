@echo off

cd nginx

.\nginx.exe -s stop

powershell -Command "Stop-Process -Id (Get-NetTCPConnection | Where-Object LocalPort -eq 5002).OwningProcess"
powershell -Command "Stop-Process -Id (Get-NetTCPConnection | Where-Object LocalPort -eq 5001).OwningProcess"
