@echo off
echo ======================================================
echo   Limpando pastas "bin" e "obj" do EduConnection...
echo ======================================================
echo.

:: Varre todas as subpastas procurando por 'bin' ou 'obj' e as deleta
for /d /r . %%d in (bin obj) do (
    if exist "%%d" (
        echo Deletando: "%%d"
        rd /s /q "%%d"
    )
)

echo.
echo ======================================================
echo   Limpeza concluida com sucesso!
echo ======================================================
pause