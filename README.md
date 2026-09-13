# EloVet

EloVet é um sistema veterinário desenvolvido como projeto acadêmico, com foco na gestão de prontuários, integração entre serviços e observabilidade.

A solução utiliza diferentes tecnologias para separar responsabilidades entre os componentes do sistema. A API .NET 10 é responsável pelo gerenciamento dos prontuários veterinários, persistência no MongoDB e recursos de observabilidade.

## Visão Geral

A solução foi organizada para manter responsabilidades bem separadas entre os serviços do projeto:

* **Java API**: responsável pelas principais operações transacionais e pelo gerenciamento dos dados relacionais do sistema.
* **EloVet API (.NET 10)**: responsável pelo gerenciamento dos prontuários, integração com MongoDB e recursos de observabilidade.
* **MongoDB**: responsável pela persistência dos prontuários e dos dados clínicos relacionados.
* **Python / IA**: responsável pelo processamento inteligente das informações clínicas e geração de análises a partir dos dados do prontuário.

A arquitetura permite que cada componente tenha uma responsabilidade específica, mantendo a API .NET desacoplada da tecnologia utilizada para processamento de IA.

## Arquitetura da API .NET

A EloVet API segue uma estrutura em camadas para manter as responsabilidades organizadas e reduzir o acoplamento entre as partes da aplicação.

```text
EloVet.Api
    ↓
EloVet.Application
    ↓
EloVet.Domain

EloVet.Infrastructure
    ↓
EloVet.Application
    ↓
EloVet.Domain
```

### Estrutura do repositório

```text
src/
├── EloVet.Api/
│   ├── Controllers/
│   ├── Health/
│   ├── Middleware/
│   └── Program.cs
│
├── EloVet.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│
├── EloVet.Domain/
│   ├── Entities/
│   └── ValueObjects/
│
└── EloVet.Infrastructure/
    ├── Mongo/
    ├── Repositories/
    ├── Python/
    └── Diagnostics/

tests/
├── EloVet.UnitTests/
│   ├── Controllers/
│   ├── Services/
│   └── Domain/
│
└── EloVet.IntegrationTests/
    ├── FactoryFixture/
    └── Integration/
```

## Tecnologias Utilizadas

* .NET 10
* C#
* ASP.NET Core
* MongoDB
* MongoDB Driver
* Swagger / OpenAPI
* Serilog
* OpenTelemetry
* xUnit
* Moq

## Funcionalidades Implementadas

A API .NET possui atualmente as seguintes funcionalidades:

* CRUD de prontuários veterinários;
* consulta de prontuário por ID;
* consulta de prontuário pelo ID do pet;
* regra de negócio que impede mais de um prontuário para o mesmo pet;
* validação de consistência entre o `id` da rota e o `id` informado no corpo durante atualizações;
* respostas HTTP adequadas para recursos não encontrados;
* Health Checks para liveness e readiness;
* logging estruturado com Serilog;
* correlação de requisições por `TraceId` e `SpanId`;
* instrumentação de traces e métricas com OpenTelemetry;
* testes unitários com xUnit e Moq;
* testes de integração utilizando a API e o MongoDB de teste.

## Endpoints da API

A API de prontuários disponibiliza os seguintes endpoints:

| Método   | Endpoint                      | Descrição                          |
| -------- | ----------------------------- | ---------------------------------- |
| `POST`   | `/api/Prontuario`             | Cria um novo prontuário            |
| `GET`    | `/api/Prontuario`             | Lista todos os prontuários         |
| `GET`    | `/api/Prontuario/{id}`        | Busca um prontuário pelo ID        |
| `GET`    | `/api/Prontuario/pet/{petId}` | Busca um prontuário pelo ID do pet |
| `PUT`    | `/api/Prontuario/{id}`        | Atualiza um prontuário             |
| `DELETE` | `/api/Prontuario/{id}`        | Exclui um prontuário               |

### Criar prontuário

**Endpoint:**

```http
POST /api/Prontuario
Content-Type: application/json
```

**Request:**

```json
{
  "pet": {
    "id": "pet-001",
    "nome": "Thor",
    "especie": "Canino",
    "raca": "Golden Retriever",
    "sexo": "Macho",
    "dataNascimento": "2020-05-10",
    "idadeAproximada": 6,
    "pesoAtualKg": 32.5,
    "isCastrado": true
  },
  "consultas": [],
  "exames": [],
  "vacinas": []
}
```

**Resposta de sucesso:**

```http
201 Created
```

A API retorna o prontuário criado.

Caso o pet já possua um prontuário, a API retorna:

```http
409 Conflict
```

### Listar prontuários

**Endpoint:**

```http
GET /api/Prontuario
```

**Resposta de sucesso:**

```http
200 OK
```

Retorna a lista de prontuários cadastrados.

### Buscar prontuário por ID

**Endpoint:**

```http
GET /api/Prontuario/{id}
```

**Exemplo:**

```http
GET /api/Prontuario/68b123456789abcdef123456
```

**Resposta de sucesso:**

```http
200 OK
```

Caso o prontuário não seja encontrado:

```http
404 Not Found
```

### Buscar prontuário por Pet

**Endpoint:**

```http
GET /api/Prontuario/pet/{petId}
```

**Exemplo:**

```http
GET /api/Prontuario/pet/pet-001
```

**Resposta de sucesso:**

```http
200 OK
```

Caso o pet não possua prontuário:

```http
404 Not Found
```

### Atualizar prontuário

**Endpoint:**

```http
PUT /api/Prontuario/{id}
Content-Type: application/json
```

**Request:**

```json
{
  "id": "68b123456789abcdef123456",
  "pet": {
    "id": "pet-001",
    "nome": "Thor",
    "especie": "Canino",
    "raca": "Golden Retriever",
    "sexo": "Macho",
    "dataNascimento": "2020-05-10",
    "idadeAproximada": 6,
    "pesoAtualKg": 33.2,
    "isCastrado": true
  },
  "consultas": [],
  "exames": [],
  "vacinas": []
}
```

**Resposta de sucesso:**

```http
200 OK
```

O `id` informado na rota deve corresponder ao `id` informado no corpo da requisição.

Caso os IDs sejam diferentes:

```http
400 Bad Request
```

Caso o prontuário não seja encontrado:

```http
404 Not Found
```

### Excluir prontuário

**Endpoint:**

```http
DELETE /api/Prontuario/{id}
```

**Exemplo:**

```http
DELETE /api/Prontuario/68b123456789abcdef123456
```

**Resposta de sucesso:**

```http
204 No Content
```

Caso o prontuário não seja encontrado:

```http
404 Not Found
```

## Swagger / OpenAPI

A API possui documentação interativa através do Swagger.

Após iniciar a aplicação em ambiente de desenvolvimento, a documentação pode ser acessada em:

```text
/swagger
```

O Swagger permite visualizar os endpoints disponíveis e realizar requisições diretamente contra a API.

## Observabilidade e Monitoramento

A API foi preparada para observabilidade utilizando ferramentas desacopladas da visualização dos dados.

A solução utiliza:

* **Logs** para registrar acontecimentos da aplicação;
* **Traces** para acompanhar o fluxo das requisições;
* **Métricas** para acompanhar o comportamento da aplicação;
* **Health Checks** para verificar a disponibilidade da aplicação e de suas dependências.

### Logs

Os logs utilizam `ILogger<T>` com Serilog como provedor de infraestrutura.

Os eventos são estruturados e enriquecidos com informações de contexto da aplicação e das requisições.

Entre os campos que podem ser utilizados para investigação estão:

* `Application`
* `Environment`
* `TraceId`
* `SpanId`
* `PetId`
* `ProntuarioId`

Exemplo de log estruturado:

```text
[14:32:10 INF] App=EloVet.Api Env=Development
Cadastro de prontuário concluído para o pet pet-001
```

O uso de `TraceId` e `SpanId` permite correlacionar uma requisição com os eventos registrados durante sua execução.

### Traces e Métricas

A aplicação utiliza OpenTelemetry para instrumentação de traces e métricas.

A instrumentação permite acompanhar informações como:

* requisições HTTP;
* duração das requisições;
* chamadas HTTP realizadas pela aplicação;
* fluxo das operações dentro da API;
* métricas relacionadas ao comportamento da aplicação.

A utilização do OpenTelemetry mantém a aplicação preparada para futura exportação dos dados para ferramentas externas de observabilidade.

## Health Checks

A API possui dois endpoints de Health Check:

```text
GET /health/live
GET /health/ready
```

### `/health/live`

O endpoint de liveness indica se a aplicação está viva e responsiva.

```http
GET /health/live
```

É utilizado para verificar se o processo da aplicação está em execução.

### `/health/ready`

O endpoint de readiness indica se a aplicação está pronta para atender requisições, considerando as dependências configuradas para esse propósito.

```http
GET /health/ready
```

Neste projeto, o readiness considera a disponibilidade do MongoDB.

### Como testar os Health Checks

Com a API em execução, os endpoints podem ser consultados utilizando o navegador, Swagger ou ferramentas como `curl`.

```bash
curl http://localhost:5240/health/live
```

```bash
curl http://localhost:5240/health/ready
```

O endpoint `/health/live` é utilizado para verificar a disponibilidade da aplicação, enquanto `/health/ready` permite verificar se a aplicação está pronta para operar com suas dependências.

### Como monitorar a aplicação

A aplicação pode ser acompanhada por diferentes fontes de informação:

* logs estruturados exibidos no console;
* correlação de eventos utilizando `TraceId` e `SpanId`;
* endpoints `/health/live` e `/health/ready`;
* traces e métricas instrumentados pelo OpenTelemetry;
* futura exportação dos dados de observabilidade para ferramentas externas.

## Executando o Projeto

### Pré-requisitos

Para executar a API, é necessário possuir:

* .NET 10 SDK;
* acesso ao MongoDB configurado para o projeto.

### Configuração do MongoDB

A conexão com o MongoDB é configurada através da variável de ambiente:

```bash
export MongoDb__ConnectionString="SUA_CONNECTION_STRING"
```

O nome do banco utilizado pela aplicação é configurado em `appsettings.json`.

Exemplo:

```json
{
  "MongoDb": {
    "DatabaseName": "elo-vet"
  }
}
```

### Restaurar dependências

```bash
dotnet restore
```

### Compilar a solução

```bash
dotnet build
```

### Executar a API

```bash
dotnet run --project src/EloVet.Api
```

Após iniciar a aplicação, os endpoints poderão ser acessados através do endereço exibido no terminal.

Em ambiente de desenvolvimento, a documentação Swagger estará disponível em:

```text
/swagger
```

## Executando os Testes

A solução possui testes unitários e testes de integração.

### Executar todos os testes

```bash
dotnet test
```

### Executar apenas os testes unitários

```bash
dotnet test tests/EloVet.UnitTests/EloVet.UnitTests.csproj
```

### Executar apenas os testes de integração

```bash
dotnet test tests/EloVet.IntegrationTests/EloVet.IntegrationTests.csproj
```

### Testes unitários

Os testes unitários validam componentes isoladamente.

Os Controllers utilizam mocks dos Services, enquanto os Services utilizam mocks dos Repositories.

```text
Controller → Mock Service
Service    → Mock Repository
```

Os testes são implementados utilizando:

* xUnit;
* Moq;
* padrão Arrange, Act e Assert.

### Testes de integração

Os testes de integração validam o funcionamento conjunto dos principais componentes da aplicação.

Diferentemente dos testes unitários, os testes de integração não utilizam mocks para o fluxo principal da aplicação.

```text
HTTP
 ↓
Controller
 ↓
Service
 ↓
Repository
 ↓
MongoDB
```

Dessa forma, os testes verificam não apenas o comportamento individual das classes, mas também a integração entre a API e o banco de dados.

## Objetivo da API .NET

A EloVet API atua como o componente responsável pelo gerenciamento dos prontuários veterinários no MongoDB.

Além da persistência e consulta dos dados clínicos, a API fornece recursos de validação, logging, Health Checks e instrumentação com OpenTelemetry, mantendo a aplicação preparada para integração com os demais componentes do ecossistema EloVet.

A arquitetura busca manter as responsabilidades separadas, permitindo que a API .NET evolua de forma independente dos demais serviços do sistema.
