#!/bin/bash

set -e

APP_DIR="/opt/alphachat"

echo "Iniciando deploy QAS..."

cd "$APP_DIR"

/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml down

echo "Building server..."
/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml build --no-cache server

echo "Building websockets..."
/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml build --no-cache websockets

echo "Building client..."
/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml build --no-cache client

/usr/bin/docker compose -f docker-compose.yaml -f docker-compose.qas.yaml up -d

echo "Deploy finalizado."
