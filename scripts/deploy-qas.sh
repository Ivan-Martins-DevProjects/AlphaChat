#!/bin/bash

set -e

APP_DIR="/opt/alphachat"

echo "Iniciando deploy QAS..."

cd "$APP_DIR"

/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml down

/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml build --no-cache

/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml up -d

echo "Deploy finalizado."
