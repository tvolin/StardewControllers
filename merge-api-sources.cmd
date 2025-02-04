@echo off
set solutionDir=%~dp0
powershell.exe -file "%solutionDir%Tools\merge-api-sources.ps1" -SourceDir "%solutionDir%StarControl" -OutDir "%solutionDir%PublicApi"