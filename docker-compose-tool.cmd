@echo off
setlocal EnableDelayedExpansion

if not defined POSTGRES_USER set POSTGRES_USER=postgres
if not defined POSTGRES_DB set POSTGRES_DB=metroship

cd /d "%~dp0"

echo Stopping containers...
docker-compose down

echo Removing only custom application images (keeping postgres)...
for /f "tokens=*" %%i in ('docker images --format "{{.Repository}}:{{.Tag}}" ^| findstr /v "postgres"') do (
    echo Checking image: %%i
    echo %%i | findstr /i "metroship" >nul
    if not errorlevel 1 (
        echo Removing image: %%i
        docker rmi %%i --force 2>nul
    )
)

echo Starting services...
docker-compose up -d

echo Waiting for PostgreSQL to be ready...
:WAIT_LOOP
docker-compose exec -T metroship.postgres.db pg_isready -U "%POSTGRES_USER%" >nul 2>&1
if errorlevel 1 (
    echo Waiting for PostgreSQL...
    timeout /t 2 /nobreak >nul
    goto WAIT_LOOP
)

echo PostgreSQL is ready!

echo Checking if seed data already exists...
docker-compose exec -T metroship.postgres.db psql -U "%POSTGRES_USER%" -d "%POSTGRES_DB%" -c "\dt" | findstr /i "table" >nul 2>&1
if errorlevel 1 (
    echo No tables found, running seed script...
    goto RUN_SEED
) else (
    echo Database already has tables, checking if seed data exists...
    REM Replace 'your_table_name' with an actual table name from your seed script
    docker-compose exec -T metroship.postgres.db psql -U "%POSTGRES_USER%" -d "%POSTGRES_DB%" -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public';" | findstr /v "count" | findstr /v "---" | findstr /v "row" >nul 2>&1
    if errorlevel 1 (
        goto RUN_SEED
    ) else (
        echo Seed data appears to exist, skipping seed script...
        goto END
    )
)

:RUN_SEED
echo Running seed script...

REM Method 1: Try piping the file directly
type "doc\pg_seed.sql" | docker-compose exec -T metroship.postgres.db psql -U "%POSTGRES_USER%" -d "%POSTGRES_DB%" >nul 2>&1
if not errorlevel 1 (
    echo Seed script executed successfully via pipe
    goto END
)

REM Method 2: Copy file to container and execute
echo Direct pipe failed, copying file to container...
for /f "tokens=*" %%c in ('docker-compose ps -q metroship.postgres.db') do set CNT=%%c
if defined CNT (
    docker cp "doc\pg_seed.sql" !CNT!:/tmp/pg_seed.sql
    docker-compose exec -T metroship.postgres.db psql -U "%POSTGRES_USER%" -d "%POSTGRES_DB%" -f /tmp/pg_seed.sql
    if not errorlevel 1 (
        echo Seed script executed successfully via file copy
    ) else (
        echo Error: Seed script execution failed
    )
) else (
    echo Error: Could not find PostgreSQL container
)

:END
echo.
echo Setup complete!
echo API is available at: http://localhost:8080
echo PostgreSQL is available at: localhost:5432
echo.

pause