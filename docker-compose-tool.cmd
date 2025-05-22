@echo off
setlocal EnableDelayedExpansion
if not defined POSTGRES_USER set POSTGRES_USER=postgres
if not defined POSTGRES_DB set POSTGRES_DB=metroship
cd /d "%~dp0"
docker compose down --rmi all --volumes --remove-orphans

docker-compose up -d
:WAIT_LOOP
docker-compose exec -T postgres pg_isready -U "%POSTGRES_USER%" >nul 2>&1
if errorlevel 1 (
  timeout /t 1 /nobreak >nul
  goto WAIT_LOOP
)
type doc\pg_seed.sql | docker-compose exec -T postgres psql -U "%POSTGRES_USER%" -d "%POSTGRES_DB%"
if errorlevel 1 (
  for /f "tokens=*" %%c in ('docker-compose ps -q postgres') do set CNT=%%c
  docker cp doc\pg_seed.sql !CNT!:/tmp/pg_seed.sql
  docker-compose exec -T postgres psql -U "%POSTGRES_USER%" -d "%POSTGRES_DB%" -f /tmp/pg_seed.sql
)

pause
popd
