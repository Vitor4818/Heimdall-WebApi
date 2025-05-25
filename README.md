﻿# Heimdall Web API 🛡️

## 🎯 Objetivo

A **Heimdall Web API** é uma API RESTful desenvolvida com **ASP.NET Core** para gerenciamento de:

- 🏍️ Motos  
- 🧑‍💼 Usuários  
- 🏷️ Tags RFID  

Cada **moto** está associada a **uma tag RFID** (relação 1:1), e **vice-versa**. Todos os dados são persistidos utilizando **Entity Framework Core** com **Oracle Database**.  
A arquitetura segue os padrões modernos, com **injeção de dependência** e **camadas separadas** para garantir a manutenção e escalabilidade do código.

---

## 📁 Estrutura do Projeto

| Projeto            | Descrição |
|--------------------|-----------|
| `heimdall-api`     | API Web com os endpoints (`Controllers`) |
| `HeimdallBusiness` | Camada de lógica de negócio |
| `HeimdallData`     | Camada de acesso a dados com EF Core + Oracle |
| `HeimdallModel`    | Contém os modelos: `Moto`, `TagRfid`, `Usuario` |

---

## 🚀 Tecnologias Utilizadas

- **.NET 9 (ASP.NET Core)**  
- **Oracle Database**  
- **Entity Framework Core** com ODP.NET  
- **Swagger (OpenAPI)**  
- **Redoc**

---

## ⚙️ Como Executar
### Executar com Docker (Local, sem azure)
Esse método permite rodar a API localmente em um container Docker.

### ✅ Pré-requisitos
Docker instalado: https://www.docker.com/products/docker-desktop

📦 1. Clonar o Repositório
```
git clone https://github.com/Vitor4818/Heimdall-WebApi.git
cd Heimdall-WebApi
```
🛠️ 2. Buildar a Imagem Docker
```
docker build -t heimdall-webapi:1.0 .
```
🚀 3. Rodar o Container
Substitua a string de conexão pela sua:

```
docker run -d -p 5000:5000 --name heimdall-webapi-container -e ConnectionStrings__DefaultConnection='User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=' heimdall-webapi:1.0
```
📝 A variável de ambiente ConnectionStrings__DefaultConnection é usada para configurar a string de conexão com o banco Oracle.

## 📚 Acessar Documentação

- **Swagger**: [https://localhost:{porta}/swagger](https://localhost:{porta}/swagger)  
- **Redoc**: [https://localhost:{porta}/docs](https://localhost:{porta}/docs)
---

## 🛣️ Rotas da API

### 🔧 Moto

| Método | Rota            | Ação                |
|--------|------------------|---------------------|
| GET    | `/api/Moto`      | Lista todas as motos |
| GET    | `/api/Moto/{id}` | Retorna moto por ID  |
| POST   | `/api/Moto`      | Cria nova moto      |
| PUT    | `/api/Moto/{id}` | Atualiza moto       |
| DELETE | `/api/Moto/{id}` | Deleta moto         |

#### 📥 Exemplo de POST

```json
POST /api/Moto
{
  "id": 1,
  "tipoMoto": "Esportiva",
  "placa": "ABC1234",
  "numChassi": "1234567F"
}
```
---
### 🏷️ TagRfid

| Método | Rota                 | Ação              |
|--------|----------------------|-------------------|
| GET    | `/api/TagRfid`       | Lista todas as tags |
| GET    | `/api/TagRfid/{id}`  | Retorna tag por ID |
| POST   | `/api/TagRfid`       | Cria nova tag     |
| PUT    | `/api/TagRfid/{id}`  | Atualiza tag      |
| DELETE | `/api/TagRfid/{id}`  | Deleta tag        |

#### 📥 Exemplo de POST

```json
POST /api/TagRfid
{
  "id": 1,
  "motoId": 1,
  "faixaFrequencia": "915 MHz",
  "banda": "UHF",
  "aplicacao": "Controle de acesso"
}
```
---
### 👤 Usuario

| Método | Rota                  | Ação                  |
|--------|------------------------|------------------------|
| GET    | `/api/Usuario`         | Lista todos usuários   |
| GET    | `/api/Usuario/{id}`    | Detalhes do usuário    |
| POST   | `/api/Usuario`         | Cria novo usuário      |
| PUT    | `/api/Usuario/{id}`    | Atualiza usuário       |
| DELETE | `/api/Usuario/{id}`    | Deleta usuário         |

#### 📥 Exemplo de POST

```json
POST /api/Usuario
{
  "id": 1,
  "categoriaUsuarioId": 1,
  "nome": "João",
  "sobrenome": "Souza",
  "dataNascimento": "2000-05-10",
  "cpf": "12345678900",
  "email": "joao.silva@example.com",
  "senha": "1a"
}
```
---
## 🧠 Boas Práticas Implementadas

- ✅ **RESTful**: Padrões de rota e verbos HTTP bem definidos
- ✅ **Arquitetura em camadas**: Controller + Service + Repository
- ✅ **Injeção de Dependência (DI)**: Baixo acoplamento, fácil de testar
- ✅ **Documentação automática**: Swagger e Redoc com testes via navegador

---
# ☁️ Deploy na Azure com Scripts
Para provisionar uma VM Linux na Azure e rodar a aplicação diretamente de lá usando Docker, siga os passos abaixo. Todos os scripts estão na pasta scripts.

### 🔧 1. Provisionar VM na Azure
Copie o conteudo dentro do arquivo 'provision-vm-heimdall-api.sh' e cole no azure CLI.
Esse script vai:

- Criar um grupo de recursos
- Criar uma VM Ubuntu
- Configurar regras de rede
- Liberar a porta 5000 (acesso à API)
---

### 🔐 2. Conectar na VM via SSH

Após o script, conecte-se com:
```
ssh admlnx@<IP_DA_VM>
```

O IP será exibido no final da execução do script de provisionamento da vm.

### 🐳 3. Instalar Docker na VM

Dentro da VM, execute o conteudo dentro de 'install_docker_ubuntu.sh':

---

### 🚀 4. Fazer deploy da aplicação
Com Docker instalado e dentro da vm, copie o conteudo dentro de 'deploy-heimdall-webapi.sh' e rode no Azure CLI

Esse script irá:
- Clonar este repositório
- Buildar a imagem Docker
- Rodar o container com as variáveis de ambiente

Lembre-se de configurar a string de conexão no deploy_heimdall_webapi.sh. Coloque seu usuario e senha para conexão com o banco de dados Oracle

### 🌐 5. Acessar a API

No visualStudioCode:
- Instalar a extensão REST Client:

Dentro da pastas TestesHttp, vai ter arquivos com os testes de crud para cada entidade. Coloque o ip publico da vm na variavel baseUrl. 
Exemplo 
``` 
@baseUrl = http://20.206.107.120:5000
```
