# --- Etapa 1: BUILD (Construção e Teste) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia o arquivo da solução
COPY heimdall-api.sln .

# Copia os arquivos de projeto da aplicação
COPY heimdall-api/heimdall-api.csproj ./heimdall-api/
COPY HeimdallBusiness/HeimdallBusiness.csproj ./HeimdallBusiness/
COPY HeimdallData/HeimdallData.csproj ./HeimdallData/
COPY HeimdallModel/HeimdallModel.csproj ./HeimdallModel/

# ADICIONADO: Copia o projeto de teste
COPY HeimdallTests/HeimdallTests.csproj ./HeimdallTests/

# Restaura as dependências de todos os projetos
RUN dotnet restore heimdall-api.sln

# Copia todo o restante do código-fonte
COPY . .

# ADICIONADO: Roda os testes unitários. Se algum teste falhar, o build para aqui.
RUN dotnet test heimdall-api.sln --no-restore

# Publica a aplicação
# Adicionado --no-restore para usar o cache da etapa anterior
RUN dotnet publish heimdall-api/heimdall-api.csproj -c Release -o /app/publish --no-restore

# --- Etapa 2: RUNTIME (Imagem Final) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copia os arquivos publicados da etapa de build
COPY --from=build /app/publish .

# CORRIGIDO: Expõe a porta 8080 (a porta que o Azure App Service USA)
EXPOSE 8080

# Define o comando de entrada
ENTRYPOINT ["dotnet", "heimdall-api.dll"]