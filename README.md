## 🎯 Objetivo

A **Heimdall Web API** é uma API RESTful desenvolvida com **ASP.NET Core** para gerenciamento de:

- 🏍️ Motos  
- 🧑‍💼 Usuários  
- 🏷️ Tags RFID  

Cada **moto** está associada a **uma tag RFID** (relação 1:1), e **vice-versa**. Todos os dados são persistidos utilizando **Entity Framework Core** com **Postgres**.  
A arquitetura segue os padrões modernos, com **injeção de dependência** e **camadas separadas** para garantir a manutenção e escalabilidade do código.

---

## 📁 Estrutura do Projeto

| Projeto            | Descrição |
|--------------------|-----------|
| `heimdall-api`     | API Web com os endpoints (`Controllers`) |
| `HeimdallBusiness` | Camada de lógica de negócio |
| `HeimdallData`     | Camada de acesso a dados com EF Core + Postgres |
| `HeimdallModel`    | Contém os modelos: `Moto`, `TagRfid`, `Usuario` |


#### A Heimdall Web API adota arquitetura monolítica por ser simples de desenvolver, implantar e manter, atendendo ao escopo atual de CRUD de motos, usuários e tags RFID. Apesar de ser um único sistema, é modularizado em camadas (API, negócio, dados e modelos), garantindo organização e manutenção facilitada, com possibilidade de evolução futura para microserviços se necessário.
---

## 🚀 Tecnologias Utilizadas

- **.NET 9 (ASP.NET Core)**  
- **Postgres**  
- **Entity Framework Core**  
- **Swagger (OpenAPI)**  
- **Redoc**

---

## ⚙️ Como Executar
### Executar com Docker Local
Esse método permite rodar a API localmente em um container Docker.

### ✅ Pré-requisitos
Docker instalado: https://www.docker.com/products/docker-desktop

 1. Clonar o Repositório
```
git clone https://github.com/Vitor4818/Heimdall-WebApi.git
cd Heimdall-WebApi
```
 2. Buildar a Imagem Docker
```
docker build -t heimdall-api:1.0 .
```
 3. Rodar o Container Postgres
```
docker run -d --name heimdall-postgres --network heimdall-network -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=MinhaSenha123 -e POSTGRES_DB=heimdalldb -p 5432:5432 postgres:15
```
 4. Rodar o container da API
```
docker run -d --name heimdall-api --network heimdall-network -e "ConnectionStrings__DefaultConnection=Host=heimdall-postgres;Port=5432;Database=heimdalldb;Username=postgres;Password=MinhaSenha123" -p 5000:5000 heimdall-api:1.0
```

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
- ✅ **HATEOAS**
- ✅ **Paginação**


---
