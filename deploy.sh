#!/bin/bash

# Nome do ACR
ACR_NAME="heimdallregister"
RESOURCE_GROUP="rg-heimdallapp"

# 1. Pegar a senha do ACR
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)

echo "Senha do ACR obtida: $ACR_PASSWORD"

# 2. Rodar container Postgres
az container create \
  --resource-group $RESOURCE_GROUP \
  --name heimdall-postgres \
  --image $ACR_NAME.azurecr.io/postgres:15 \
  --cpu 1 \
  --memory 1.5 \
  --os-type Linux \
  --registry-login-server $ACR_NAME.azurecr.io \
  --registry-username $ACR_NAME \
  --registry-password "$ACR_PASSWORD" \
  --environment-variables POSTGRES_USER=postgres POSTGRES_PASSWORD=sua_senha POSTGRES_DB=HeimdallDb \
  --ports 5432 \
  --dns-name-label heimdall-postgres

# 3. Rodar container API
az container create \
  --resource-group $RESOURCE_GROUP \
  --name heimdall-api \
  --image $ACR_NAME.azurecr.io/heimdall-api:1.1 \
  --cpu 1 \
  --memory 1.5 \
  --os-type Linux \
  --registry-login-server $ACR_NAME.azurecr.io \
  --registry-username $ACR_NAME \
  --registry-password "$ACR_PASSWORD" \
  --environment-variables ConnectionStrings__DefaultConnection="Host=heimdall-postgres.eastus.azurecontainer.io;Port=5432;Database=HeimdallDb;Username=postgres;Password=sua_senha" \
  --ports 5000 \
  --dns-name-label heimdall-api
