#!/bin/bash
# ==========================================================
# Script de Infraestrutura (IaC) para a API Heimdall (MOTTU)
# Cria: Web App + Banco de Dados PostgreSQL Flexível
# ==========================================================

# --- 1. VARIÁVEIS ---
rg="rg-heimdall-api"
location="Central US"
plan="plan-heimdall-api"
app="heimdall-api-vitor"            # Nome fixo para integração com pipeline
runtime="dotnet:9"
sku_app="F1"

db_server_name="heimdall-db-vitor"
db_name="heimdalldb"
sku_db="standard_b1ms"

# Variáveis de Key Vault passadas pela pipeline
key_vault_name=${KEY_VAULT_NAME}
db_admin_user_secret_name=${DB_USER_SECRET_NAME}
db_admin_pass_secret_name=${DB_PASS_SECRET_NAME}

# --- 2. Ler segredos do Key Vault ---
echo "[0/5] Buscando segredos no Key Vault '$key_vault_name'..."
db_admin_user=$(az keyvault secret show --vault-name "$key_vault_name" --name "$db_admin_user_secret_name" --query value -o tsv)
db_admin_pass=$(az keyvault secret show --vault-name "$key_vault_name" --name "$db_admin_pass_secret_name" --query value -o tsv)

# --- 3. VERIFICAR/CRIAR PLANO DE SERVIÇO E WEB APP ---
echo "=== INICIANDO CRIAÇÃO/VERIFICAÇÃO DE INFRAESTRUTURA ==="
echo "Grupo de Recursos: $rg"
echo "Web App: $app"
echo "Servidor DB: $db_server_name"
echo "Localização: $location"
echo "========================================================"

# Web App
if az webapp show --name "$app" --resource-group "$rg" &>/dev/null; then
  echo "[1/5] Web App '$app' já existe, atualizando configuração..."
else
  echo "[1/5] Criando Web App '$app'..."
  az appservice plan create --name "$plan" --resource-group "$rg" --location "$location" --sku "$sku_app" 1>/dev/null
  az webapp create --resource-group "$rg" --plan "$plan" --runtime "$runtime" --name "$app" 1>/dev/null
fi

# --- 4. BANCO DE DADOS ---

if [[ -z "$db_admin_user" || -z "$db_admin_pass" ]]; then
  echo "Erro: não foi possível ler as credenciais do Key Vault."
  exit 1
fi


if az postgres flexible-server show --name "$db_server_name" --resource-group "$rg" &>/dev/null; then
  echo "[2/5] Servidor PostgreSQL '$db_server_name' já existe, pulando criação."
else
  echo "[2/5] Criando Servidor PostgreSQL '$db_server_name'..."
  az postgres flexible-server create \
      --name "$db_server_name" \
      --resource-group "$rg" \
      --location "$location" \
      --admin-user "$db_admin_user" \
      --admin-password "$db_admin_pass" \
      --sku-name "$sku_db" \
      --tier Burstable \
      --version 14 \
      --public-access 0.0.0.0 1>/dev/null

  echo "   ...Configurando firewall do PostgreSQL..."
  az postgres flexible-server firewall-rule create \
      --resource-group "$rg" \
      --name "$db_server_name" \
      --rule-name "AllowAzureIPs" \
      --start-ip-address "0.0.0.0" \
      --end-ip-address "0.0.0.0" 1>/dev/null

  echo "   ...Criando banco de dados '$db_name'..."
  az postgres flexible-server db create \
      --resource-group "$rg" \
      --server-name "$db_server_name" \
      --database-name "$db_name" 1>/dev/null
fi

# --- 5. CONEXÃO COM O BANCO ---
echo "[3/5] Injetando a Connection String no Web App..."
connection_string="Host=$db_server_name.postgres.database.azure.com;Database=$db_name;Username=$db_admin_user;Password=$db_admin_pass;SslMode=Require"
az webapp config appsettings set --resource-group "$rg" --name "$app" --settings "POSTGRES_CONN_STR=$connection_string" 1>/dev/null

# --- 6. LOGS ---
echo "Habilitando logs do App Service..."
az webapp log config \
    --resource-group "$rg" \
    --name "$app" \
    --application-logging filesystem \
    --web-server-logging filesystem \
    --level information \
    --detailed-error-messages true \
    --failed-request-tracing true 1>/dev/null

# --- 7. EXPORTAR VARIÁVEL PARA PIPELINE ---
echo "##vso[task.setvariable variable=APP_NAME;isOutput=true]$app"

echo "=========================================="
echo "[OK] Infraestrutura da API Heimdall pronta!"
echo "App Service: $app"
echo "Database Server: $db_server_name"
echo "=========================================="
