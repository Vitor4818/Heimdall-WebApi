#!/bin/bash

# -----------------------------
# Script para clonar, buildar e rodar o container Heimdall WebAPI
# -----------------------------

# URL do repositório
REPO_URL="https://github.com/Vitor4818/Heimdall-WebApi.git"
REPO_DIR="Heimdall-WebApi"

# Variável de conexão (mude aqui se precisar)
CONNECTION_STRING="User Id=rm;Password=;Data Source="

# 1. Clonar repositório (se já existir, só atualiza)
if [ -d "$REPO_DIR" ]; then
  echo "📂 Repositório já existe, atualizando..."
  cd "$REPO_DIR" && git pull && cd ..
else
  echo "📥 Clonando repositório..."
  git clone $REPO_URL
fi

# 2. Entrar no diretório do projeto
cd $REPO_DIR || { echo "❌ Erro ao entrar no diretório $REPO_DIR"; exit 1; }

# 3. Buildar a imagem Docker
echo "🐳 Buildando imagem Docker heimdall-webapi:1.0..."
docker build -t heimdall-webapi:1.0 .

# 4. Parar e remover container antigo, se existir
if [ "$(docker ps -aq -f name=heimdall-webapi-container)" ]; then
  echo "🛑 Parando e removendo container antigo..."
  docker stop heimdall-webapi-container
  docker rm heimdall-webapi-container
fi

# 5. Rodar o container com a porta mapeada e variável de ambiente
echo "▶️ Rodando container heimdall-webapi-container..."
docker run -d \
  -p 5000:5000 \
  --name heimdall-webapi-container \
  -e ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
  heimdall-webapi:1.0

echo "✅ Deploy concluído!"
