@echo off
title Chroniques d'Aethelgard - RPG
cd /d "%~dp0"
dotnet run -c Release
if errorlevel 1 pause
