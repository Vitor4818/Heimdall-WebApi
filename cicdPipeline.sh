#!/bin/bash
# ==========================================================
# Script de Infraestrutura (IaC) para a API Heimdall (MOTTU)
# Cria: Web App + Banco de Dados PostgreSQL Flexível
# (Versão "Pronta a Rodar" com valores "chumbados")
# ==========================================================

# --- 1. VARIÁVEIS (Valores "Chumbados" para a Sprint) ---

# Gera um sufixo único (baseado no carimbo de tempo) para evitar conflitos de nome
UNIQUE_SUFFIX=$(date +%s)

rg="rg-heimdall-api"
location="Central US"              # (Mudado de East US para Central US)
plan="plan-heimdall-api"
app="heimdall-api-vitor-$UNIQUE_SUFFIX" # (Ex: heimdall-api-vitor-176223456)
runtime="dotnet:9"
sku_app="F1"                       # F1 = Gratuito (para o Web App)

db_server_name="heimdall-db-vitor-$UNIQUE_SUFFIX" # (Ex: heimdall-db-vitor-176223456)
db_name="heimdalldb"
db_admin_user="heimdalladmin"
db_admin_pass="HeimdallPass123@"   # (ATENÇÃO: Mude isto num projeto real!)

# (Corrigido de "B_Standard_B1ms" para "standard_b1ms")
sku_db="standard_b1ms"         

echo "=== INICIANDO CRIAÇÃO DE INFRAESTRUTURA ==="
echo "Grupo de Recursos: $rg"
echo "Web App: $app"
echo "Servidor DB: $db_server_name"
echo "Localização: $location"

# --- 2. CRIAÇÃO DO GRUPO E WEB APP ---
echo "[1/5] Criando Grupo de Recursos '$rg'..."
az group create --name "$rg" --location "$location" 1>/dev/null

echo "[2/5] Criando Plano de Serviço '$plan' (SKU: $sku_app)..."
az appservice plan create --name "$plan" --resource-group "$rg" --location "$location" --sku "$sku_app" 1>/dev/null

echo "[3/5] Criando Serviço de Aplicativo (Web App) '$app'..."
az webapp create --resource-group "$rg" --plan "$plan" --runtime "$runtime" --name "$app" 1>/dev/null

# --- 3. CRIAÇÃO DO BANCO DE DADOS POSTGRESQL ---

echo "[4/5] Criando Servidor PostgreSQL '$db_server_name' (SKU: $sku_db)..."
# (Isto pode demorar alguns minutos)
az postgres flexible-server create \
    --name "$db_server_name" \
    --resource-group "$rg" \
    --location "$location" \
    --admin-user "$db_admin_user" \
    --admin-password "$db_admin_pass" \
    --sku-name "$sku_db" \
    --tier Burstable \
    --version 14 \
    --public-access 0.0.0.0 1>/dev/null # (Permite acesso PÚBLICO)

echo "   ...Configurando Firewall do PostgreSQL para permitir acesso do Azure..."
az postgres flexible-server firewall-rule create \
    --resource-group "$rg" \
    --name "$db_server_name" \
    --rule-name "AllowAzureIPs" \
    --start-ip-address "0.0.0.0" \
    --end-ip-address "0.0.0.0" 1>/dev/null

echo "   ...Criando banco de dados '$db_name' dentro do servidor..."
az postgres flexible-server db create \
    --resource-group "$rg" \
    --server-name "$db_server_name" \
    --database-name "$db_name" 1>/dev/null

# --- 4. O "PULO DO GATO" (Ligar o Web App ao Banco) ---

echo "[5/5] Injetando a Connection String no Web App..."

# Constrói a Connection String (usando a senha "chumbada")
connection_string="Host=$db_server_name.postgres.database.azure.com;Database=$db_name;Username=$db_admin_user;Password=$db_admin_pass;SslMode=Require"

# --- CORREÇÃO (Bug 500.30) ---
# Em vez de usar a secção "Connection Strings", injetamos
# a string como uma "Configuração de Aplicação" (App Setting) normal.
# Isto GARANTE que ela sobrescreve o appsettings.json.
az webapp config appsettings set \
    --resource-group "$rg" \
    --name "$app" \
    --settings "POSTGRES_CONN_STR=$connection_string" 1>/dev/null
# --- FIM DA CORREÇÃO ---

# --- 5. HABILITAR LOGS ---
echo "Habilitando Logs do Serviço de Aplicativo..."
az webapp log config \
    --resource-group "$rg" \
    --name "$app" \
    --application-logging filesystem \
    --web-server-logging filesystem \
    --level information \
    --detailed-error-messages true \
    --failed-request-tracing true 1>/dev/null

echo "=========================================="
echo "[OK] Infraestrutura da API Heimdall pronta!"
echo "=========================================="

