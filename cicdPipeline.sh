#!/bin/bash
# ==========================================================
# Script de Infraestrutura (IaC) para a API Heimdall (MOTTU)
# Cria: Web App + Banco de Dados PostgreSQL Flexível
# Usa: Variable Group do Azure DevOps para credenciais
# ==========================================================

# --- 1. VARIÁVEIS FIXAS ---
rg="rg-heimdall-api"
location="Central US"
plan="plan-heimdall-api"
app="heimdall-api-vitor"            # Nome fixo para integração com pipeline
runtime="dotnet:9"
sku_app="F1"

db_server_name="heimdall-db-vitor"
db_name="heimdalldb"
sku_db="standard_b1ms"

# --- 2. VARIÁVEIS DE CREDENCIAIS (da Variable Group do Azure DevOps) ---
# Certifique-se de linkar a Variable Group no pipeline
db_admin_user="${DB_ADMIN_USER}"
db_admin_pass="${DB_ADMIN_PASS}"

# --- 3. CHECK DE CREDENCIAIS ---
if [[ -z "$db_admin_user" || -z "$db_admin_pass" ]]; then
  echo "Erro: credenciais do banco não configuradas. Verifique a Variable Group!"
  exit 1
fi

# --- 4. LOG DE INÍCIO ---
echo "=== INICIANDO CRIAÇÃO/VERIFICAÇÃO DE INFRAESTRUTURA ==="
echo "Grupo de Recursos: $rg"
echo "Web App: $app"
echo "Servidor DB: $db_server_name"
echo "Localização: $location"
echo "========================================================"

# --- 5. GRUPO DE RECURSOS ---
if ! az group show --name "$rg" &>/dev/null; then
  echo "[1/5] Criando grupo de recursos '$rg'..."
  az group create --name "$rg" --location "$location" 1>/dev/null
else
  echo "[1/5] Grupo de recursos '$rg' já existe, pulando criação."
fi

# --- 6. PLANO DE SERVIÇO ---
if ! az appservice plan show --name "$plan" --resource-group "$rg" &>/dev/null; then
  echo "[2/5] Criando plano de serviço '$plan' (SKU: $sku_app)..."
  az appservice plan create --name "$plan" --resource-group "$rg" --location "$location" --sku "$sku_app" 1>/dev/null
else
  echo "[2/5] Plano de serviço '$plan' já existe, pulando criação."
fi

# --- 7. WEB APP ---
if ! az webapp show --name "$app" --resource-group "$rg" &>/dev/null; then
  echo "[3/5] Criando Web App '$app'..."
  az webapp create --resource-group "$rg" --plan "$plan" --runtime "$runtime" --name "$app" 1>/dev/null
else
  echo "[3/5] Web App '$app' já existe, pulando criação."
fi

# --- 8. BANCO DE DADOS ---
if ! az postgres flexible-server show --name "$db_server_name" --resource-group "$rg" &>/dev/null; then
  echo "[4/5] Criando Servidor PostgreSQL '$db_server_name'..."
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
else
  echo "[4/5] Servidor PostgreSQL '$db_server_name' já existe, pulando criação."
fi

# --- 9. CONEXÃO COM O BANCO ---
echo "[5/5] Injetando a Connection String no Web App..."
connection_string="Host=$db_server_name.postgres.database.azure.com;Database=$db_name;Username=$db_admin_user;Password=$db_admin_pass;SslMode=Require"
az webapp config appsettings set \
    --resource-group "$rg" \
    --name "$app" \
    --settings "POSTGRES_CONN_STR=$connection_string" 1>/dev/null

# --- 10. LOGS DO APP SERVICE ---
echo "Habilitando logs do App Service..."
az webapp log config \
    --resource-group "$rg" \
    --name "$app" \
    --application-logging filesystem \
    --web-server-logging filesystem \
    --level information \
    --detailed-error-messages true \
    --failed-request-tracing true 1>/dev/null

# --- 11. EXPORTAR VARIÁVEL PARA PIPELINE ---
echo "##vso[task.setvariable variable=APP_NAME;isOutput=true]$app"

echo "=========================================="
echo "[OK] Infraestrutura da API Heimdall pronta!"
echo "App Service: $app"
echo "Database Server: $db_server_name"
echo "=========================================="
