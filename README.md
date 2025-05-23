# Heimdall Web API 🛡️

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

1. **Clonar o repositório:**
   ```bash
   git clone https://github.com/Vitor4818/Heimdall-WebApi.git

2. **Restaurar pacotes:**  Abra o heimdall-api.sln no Visual Studio ou use:
```bash
dotnet restore
```
3. **Configurar o Oracle**

No arquivo `appsettings.json` do projeto `heimdall-api`, configure a **string de conexão** com os seguintes dados:

- `usuário`  
- `senha`  
- `host`  
- `serviço`

Exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=SEU_HOST)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=SEU_SERVICO)));"
}
```
4. **Aplicar as Migrações**
   Abra o terminal e execute:

```bash
dotnet ef database update --startup-project heimdall-api --project HeimdallData
```

 5. Rodar a Aplicação
    Execute o seguinte comando:

```bash
cd heimdall-api
dotnet run webapi
```
---
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
