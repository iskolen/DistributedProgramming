@echo off

cd "../Valuator"
start cmd /k dotnet run --urls "http://0.0.0.0:5001"
start cmd /k dotnet run --urls "http://0.0.0.0:5002"

cd "../../../nginx-1.25.4"
start nginx.exe