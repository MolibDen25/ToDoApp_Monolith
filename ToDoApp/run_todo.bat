@echo off
cd /d "%~dp0ToDoApp"   REM Переходимо в папку ToDoApp (змінюй на свою)
start cmd /k "dotnet run"
