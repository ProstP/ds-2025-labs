@echo off

cd C:\D\Projects\ds-2025-labs\Valuator\

start dotnet run --urls "http://0.0.0.0:5001"
start dotnet run --urls "http://0.0.0.0:5002"

cd C:\D\Projects\ds-2025-labs\nginx
start nginx
