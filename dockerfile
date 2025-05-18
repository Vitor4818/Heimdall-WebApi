#Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

#Copia o arquivo da solução
COPY heimdall-api.sln .

#Copia os arquivos de projeto
COPY heimdall-api/heimdall-api.csproj ./heimdall-api/
COPY HeimdallBusiness/HeimdallBusiness.csproj ./HeimdallBusiness/
COPY HeimdallData/HeimdallData.csproj ./HeimdallData/
COPY HeimdallModel/HeimdallModel.csproj ./HeimdallModel/

#Restaura as dependências
RUN dotnet restore

#Copia o restante dos arquivos
COPY . .

#Publica a aplicação
RUN dotnet publish heimdall-api/heimdall-api.csproj -c Release -o /app/publish

#Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

#Copia os arquivos publicados da etapa anterior
COPY --from=build /app/publish .

#Expõe a porta usada pela aplicação (ajuste se necessário)
EXPOSE 5000

#Define o comando de entrada
ENTRYPOINT ["dotnet", "heimdall-api.dll"]
