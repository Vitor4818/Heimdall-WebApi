#!/bin/bash

# -----------------------------
# Script para provisionar VM no Azure e instalar Docker
# Autor: Vitor
# -----------------------------

# VARIÁVEIS
RG="rg-vm-heimdallapi-dev"
LOCATION="brazilsouth"
VM_NAME="vm-linux-heimdallapi-dev"
IMAGE="Ubuntu2204"
SIZE="Standard_B2s"
VNET_NAME="vnet-heimdallapi-dev"
NSG_NAME="nsg-heimdallapi-devc"
IP_NAME="pip-ubuntu"
USERNAME="admlnx"
PASSWORD="Fiap@2tdsvms"  # ⚠️ Evite deixar senha hardcoded. Use Azure Key Vault ou variável de ambiente.

# 1. Criar grupo de recursos
echo "📦 Criando grupo de recursos..."
az group create --location $LOCATION --resource-group $RG

# 2. Criar máquina virtual
echo "🖥️ Criando VM..."
az vm create \
  --resource-group $RG \
  --name $VM_NAME \
  --image $IMAGE \
  --size $SIZE \
  --vnet-name $VNET_NAME \
  --nsg $NSG_NAME \
  --public-ip-address $IP_NAME \
  --authentication-type password \
  --admin-username $USERNAME \
  --admin-password $PASSWORD \
  --security-type TrustedLaunch

# 3. Abrir a porta 5000 no NSG
echo "🔓 Liberando porta 5000..."
az network nsg rule create \
  --resource-group $RG \
  --nsg-name $NSG_NAME \
  --name port_5000 \
  --protocol tcp \
  --priority 1010 \
  --destination-port-range 5000 \
  --access Allow \
  --direction Inbound \
  --description "Allow inbound traffic on port 5000 for Heimdall WebAPI"

# 4. Obter IP público da VM
echo "🌐 IP público da VM:"
az vm show -d -g $RG -n $VM_NAME --query publicIps -o tsv

echo "✅ Provisionamento concluído!"
