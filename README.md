# EloVet

Sistema veterinário desenvolvido como projeto acadêmico, com foco na gestão de prontuários clínicos, integração entre serviços e observabilidade.

O ecossistema EloVet utiliza diferentes tecnologias de acordo com a responsabilidade de cada componente. Neste repositório, o foco está na **EloVet API em .NET 10**, responsável pela autenticação de usuários, pelo gerenciamento dos prontuários veterinários armazenados no MongoDB e pelos recursos de monitoramento da aplicação.

## Visão geral do sistema

O projeto é dividido em componentes que trabalham em conjunto:

| Componente | Responsabilidade |
|---|---|
| **Java API** | Operações transacionais e gerenciamento dos dados relacionais do sistema. |
| **EloVet API (.NET 10)** | Cadastro e autenticação de usuários, gerenciamento dos prontuários, integração com MongoDB e observabilidade. |
| **MongoDB** | Persistência dos prontuários e dos dados clínicos relacionados. |
| **Python / IA** | Processamento inteligente das informações clínicas e geração de análises a partir dos dados do prontuário. |

Essa divisão permite que cada serviço tenha uma responsabilidade definida. A API .NET mantém sua própria estrutura de domínio e persistência, sem depender diretamente da tecnologia utilizada pelos demais componentes.

## Integrantes

| Integrante | RM |
|---|---|
| Arthur Graciani | RM561728 |
| Gustavo Oliveira | RM566358 |
| João Pedro Scarpin | RM565421 |
| Lucas Hideki | RM565355 |
| Wesley Andrade | RM563593 |

## Arquitetura da API .NET

A EloVet API utiliza uma arquitetura em camadas. Cada camada concentra um tipo de responsabilidade, evitando que regras de negócio, acesso ao banco de dados e detalhes de comunicação HTTP fiquem misturados.

```mermaid
flowchart LR
    Client[Cliente HTTP / Swagger] --> Api[EloVet.Api]
    Api --> Application[EloVet.Application]
    Api --> Infrastructure[EloVet.Infrastructure]
    Application --> Domain[EloVet.Domain]
    Infrastructure --> Application
    Infrastructure --> Domain
    Infrastructure --> Mongo[(MongoDB)]
```

### Responsabilidades das camadas

- **EloVet.Api:** recebe as requisições HTTP e devolve as respostas. Contém controllers, autenticação JWT, configuração do Swagger, DTOs HTTP, recursos HATEOAS e mapeamentos entre os modelos da API e o domínio.
- **EloVet.Application:** concentra os serviços de aplicação, interfaces, modelos de consulta e a orquestração dos casos de uso.
- **EloVet.Domain:** contém as entidades e os conceitos do negócio veterinário e de usuários. Essa camada não depende de HTTP nem do MongoDB.
- **EloVet.Infrastructure:** implementa o acesso ao MongoDB, os repositories, os recursos de segurança relacionados à persistência de senhas, os Health Checks e a configuração de diagnóstico e observabilidade.

### Estrutura do repositório

A estrutura principal da solução está organizada da seguinte forma:

```text
src/
├── EloVet.Api/
│   ├── Controllers/
│   ├── Mapping/
│   ├── Models/
│   │   ├── Auth/
│   │   └── Prontuario/
│   ├── Services/
│   └── Program.cs
│
├── EloVet.Application/
│   ├── Interfaces/
│   ├── Models/
│   └── Services/
│
├── EloVet.Domain/
│   └── Entities/
│
└── EloVet.Infrastructure/
    ├── Mongo/
    ├── Repositories/
    ├── Security/
    └── Diagnostics/

tests/
├── EloVet.UnitTests/
│   ├── Controllers/
│   ├── Security/
│   └── Services/
│
└── EloVet.IntegrationTests/
    ├── FactoryFixture/
    └── Integration/
```

### Entidades, DTOs e mapeamento

As entidades de `EloVet.Domain` representam os dados e conceitos do negócio. Elas não devem conhecer detalhes de comunicação HTTP, JSON, rotas ou HATEOAS.

Por esse motivo, entidades como `Prontuario`, `Pet`, `Consulta`, `Exame` e `Vacina` não são utilizadas diretamente como entrada ou saída dos controllers. A API possui modelos próprios para definir o formato das requisições e respostas HTTP.

Os principais modelos relacionados aos prontuários ficam em:

```text
EloVet.Api/Models/Prontuario/
├── ProntuarioRequest.cs
├── ProntuarioResponse.cs
├── PetRequest.cs
├── PetResponse.cs
├── ConsultaRequest.cs
├── ConsultaResponse.cs
├── ExameRequest.cs
├── ExameResponse.cs
├── VacinaRequest.cs
├── VacinaResponse.cs
└── SoapResponse.cs
```

O `ProntuarioRequest` é utilizado nas operações de criação (`POST`) e atualização (`PUT`). Na criação, normalmente não é necessário enviar o `id`, pois o identificador é gerado para o novo registro. Na atualização, o identificador recebido no corpo pode ser comparado com o identificador presente na rota.

A conversão entre os DTOs da API e as entidades do domínio é feita por `IProntuarioMapper` e sua implementação `ProntuarioMapper`, localizados em `EloVet.Api/Mapping`.

O fluxo de entrada de uma requisição é:

```text
HTTP JSON
    ↓
ProntuarioRequest
    ↓
IProntuarioMapper
    ↓
Prontuario (Domain)
    ↓
Application / Repository
    ↓
MongoDB
```

Na resposta, o caminho é inverso:

```text
MongoDB
    ↓
Repository / Application
    ↓
Prontuario (Domain)
    ↓
IProntuarioMapper
    ↓
ProntuarioResponse
    ↓
HateoasResource<T>
    ↓
HTTP JSON
```

Com essa separação, cada parte tem uma função mais clara: o controller trata a comunicação HTTP, o mapper converte os modelos, a Application coordena o caso de uso, o Domain representa o negócio e a Infrastructure realiza o acesso ao banco.

O controller depende da interface `IProntuarioMapper`, em vez de depender diretamente da implementação concreta. Isso facilita a substituição de implementações e mantém o acoplamento entre as camadas menor.

Os modelos `LinkDto`, `HateoasResource<T>` e `PagedResponse<T>` ficam na camada da API porque representam a estrutura da resposta HTTP, incluindo URLs, métodos e links de navegação.

## Tecnologias utilizadas

- .NET 10
- C#
- ASP.NET Core
- MongoDB
- MongoDB Driver
- Swagger / OpenAPI
- Serilog
- OpenTelemetry
- xUnit
- Moq

## Funcionalidades implementadas

A EloVet API possui os seguintes recursos:

- CRUD de prontuários veterinários;
- cadastro de usuários na collection `Users`;
- autenticação JWT sem utilização de roles;
- proteção dos endpoints de prontuário por Bearer Token;
- consulta de prontuário por ID;
- consulta de prontuário pelo ID do pet;
- regra que impede mais de um prontuário para o mesmo pet;
- validação entre o ID informado na rota e o ID enviado no corpo durante atualizações;
- respostas HTTP específicas para recursos não encontrados;
- paginação, filtros e ordenação na listagem de prontuários;
- links HATEOAS nas respostas;
- Health Checks para liveness e readiness;
- logging estruturado com Serilog;
- correlação de requisições por `TraceId` e `SpanId`;
- instrumentação de traces e métricas com OpenTelemetry;
- testes unitários com xUnit e Moq;
- testes de integração envolvendo a API e o MongoDB de teste.

## Endpoints da API

### Prontuários

| Método | Endpoint | Descrição |
|---|---|---|
| `POST` | `/api/Prontuario` | Cria um prontuário. |
| `GET` | `/api/Prontuario` | Lista prontuários com paginação e filtros. |
| `GET` | `/api/Prontuario/{id}` | Busca um prontuário pelo ID. |
| `GET` | `/api/Prontuario/pet/{petId}` | Busca o prontuário pelo ID do pet. |
| `PUT` | `/api/Prontuario/{id}` | Atualiza um prontuário. |
| `DELETE` | `/api/Prontuario/{id}` | Exclui um prontuário. |

### Autenticação

| Método | Endpoint | Descrição |
|---|---|---|
| `POST` | `/api/Auth/register` | Cadastra um usuário na collection `Users`. |
| `POST` | `/api/Auth/signin` | Valida as credenciais e retorna um JWT. |

O cadastro e o login são endpoints públicos. As operações de prontuário exigem um token JWT válido no cabeçalho da requisição:

```http
Authorization: Bearer SEU_TOKEN
```

### Paginação, filtros e ordenação

O endpoint `GET /api/Prontuario` permite controlar a quantidade de registros retornados, filtrar os resultados pelos dados do pet e escolher a ordem da listagem.

Exemplo de requisição:

```http
GET /api/Prontuario?pageNumber=1&pageSize=10&petName=Thor&species=Canino&sortBy=weight&ascending=false
```

Parâmetros disponíveis:

| Parâmetro | Descrição |
|---|---|
| `pageNumber` | Número da página. A primeira página é `1`. |
| `pageSize` | Quantidade de itens por página, limitada a `50`. |
| `petId` | Filtra pelo ID do pet. |
| `petName` | Filtra pelo nome do pet. |
| `species` | Filtra pela espécie. |
| `breed` | Filtra pela raça. |
| `sex` | Filtra pelo sexo. |
| `isCastrado` | Filtra por `true` ou `false`. |
| `sortBy` | Campo utilizado na ordenação: `petName`, `species`, `breed`, `sex`, `weight`, `birthDate` ou `id`. |
| `ascending` | Define a direção da ordenação: `true` para crescente e `false` para decrescente. |

A resposta da listagem inclui os registros da página solicitada e os metadados `pageNumber`, `pageSize`, `totalPages` e `totalRecords`. Essas informações permitem que o cliente saiba em qual página está, quantas páginas existem e quantos registros correspondem à consulta.

### HATEOAS

HATEOAS (*Hypermedia as the Engine of Application State*) é utilizado para incluir, na resposta de um recurso, links que indicam ações disponíveis para aquele recurso.

Assim, além dos dados do prontuário, a API pode informar os endereços para consultar, atualizar ou excluir o mesmo registro.

Exemplo simplificado de resposta:

```json
{
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "pet": {
      "id": "pet-001",
      "nome": "Thor"
    }
  },
  "links": [
    {
      "href": "/api/Prontuario/507f1f77bcf86cd799439011",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "/api/Prontuario/507f1f77bcf86cd799439011",
      "rel": "update",
      "method": "PUT"
    },
    {
      "href": "/api/Prontuario/507f1f77bcf86cd799439011",
      "rel": "delete",
      "method": "DELETE"
    }
  ]
}
```

Os campos dos links têm as seguintes funções:

- `href`: endereço do recurso ou da ação;
- `rel`: identifica a relação ou finalidade do link;
- `method`: indica o método HTTP utilizado.

Na listagem paginada, a API também retorna os links `self`, `next` e `previous`, que representam a página atual, a próxima página e a página anterior.

Os modelos genéricos `LinkDto`, `HateoasResource<T>` e `PagedResponse<T>` ficam na camada da API e podem ser reutilizados por outros endpoints.

### Criar prontuário

**Endpoint**

```http
POST /api/Prontuario
Content-Type: application/json
```

**Corpo da requisição**

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

**Respostas esperadas**

- `201 Created`: prontuário criado com sucesso. A API retorna o recurso criado.
- `409 Conflict`: o pet já possui um prontuário cadastrado.

### Listar prontuários

**Endpoint**

```http
GET /api/Prontuario
```

**Resposta de sucesso:** `200 OK`.

Retorna os prontuários cadastrados. Também é possível utilizar os parâmetros de paginação, filtros e ordenação descritos anteriormente.

### Buscar prontuário por ID

**Endpoint**

```http
GET /api/Prontuario/{id}
```

**Exemplo**

```http
GET /api/Prontuario/68b123456789abcdef123456
```

**Respostas esperadas**

- `200 OK`: prontuário encontrado.
- `404 Not Found`: não existe prontuário com o ID informado.

### Buscar prontuário por pet

**Endpoint**

```http
GET /api/Prontuario/pet/{petId}
```

**Exemplo**

```http
GET /api/Prontuario/pet/pet-001
```

**Respostas esperadas**

- `200 OK`: prontuário encontrado para o pet.
- `404 Not Found`: o pet não possui prontuário cadastrado.

### Atualizar prontuário

**Endpoint**

```http
PUT /api/Prontuario/{id}
Content-Type: application/json
```

**Corpo da requisição**

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

**Respostas esperadas**

- `200 OK`: prontuário atualizado.
- `400 Bad Request`: o ID enviado no corpo não corresponde ao ID da rota.
- `404 Not Found`: prontuário não encontrado.

A validação entre os dois identificadores evita que uma requisição destinada a atualizar um registro acabe alterando outro por engano.

### Excluir prontuário

**Endpoint**

```http
DELETE /api/Prontuario/{id}
```

**Exemplo**

```http
DELETE /api/Prontuario/68b123456789abcdef123456
```

**Respostas esperadas**

- `204 No Content`: prontuário excluído.
- `404 Not Found`: prontuário não encontrado.

## Swagger / OpenAPI

A API disponibiliza documentação interativa por meio do Swagger. Em ambiente de desenvolvimento, depois de iniciar a aplicação, acesse:

```text
/swagger
```

A interface permite consultar os endpoints, visualizar os parâmetros e executar requisições diretamente contra a API.

Para testar os endpoints protegidos:

1. Execute `POST /api/Auth/register` para cadastrar um usuário.
2. Execute `POST /api/Auth/signin` com as credenciais cadastradas.
3. Copie o valor de `token` retornado pela API.
4. Clique em **Authorize** na interface do Swagger.
5. Informe `Bearer SEU_TOKEN` e confirme.

Depois da autorização, o Swagger enviará o token no cabeçalho `Authorization` das requisições protegidas.

### Principais códigos HTTP

| Código | Significado no contexto da API |
|---|---|
| `200 OK` | Consulta, login ou atualização concluída. |
| `201 Created` | Cadastro concluído. |
| `204 No Content` | Exclusão concluída sem conteúdo na resposta. |
| `400 Bad Request` | Dados enviados ou identificador inválidos. |
| `401 Unauthorized` | Token ausente ou inválido. |
| `404 Not Found` | Recurso não encontrado. |
| `409 Conflict` | Recurso duplicado ou pet que já possui prontuário. |

## Autenticação JWT

A API utiliza JSON Web Token (JWT) para proteger os endpoints de prontuário. O cadastro e o login não exigem autenticação; depois de realizar o login, o cliente utiliza o token recebido para acessar as rotas protegidas.

O fluxo básico é:

```text
Cadastro do usuário
       ↓
Login com usuário e senha
       ↓
Validação das credenciais
       ↓
Geração do JWT
       ↓
Envio do token no header Authorization
       ↓
Acesso aos endpoints protegidos
```

A autenticação implementada não utiliza roles. As credenciais dos usuários são armazenadas na collection `Users`, no mesmo banco MongoDB utilizado pelos prontuários. A senha não é persistida em texto puro: a aplicação armazena seu hash com salt.

## Observabilidade e monitoramento

A API utiliza recursos de observabilidade para facilitar o acompanhamento do funcionamento da aplicação e a investigação de problemas.

Os principais recursos são:

- **Logs:** registram eventos que acontecem durante a execução.
- **Traces:** ajudam a acompanhar o caminho de uma requisição pelas operações instrumentadas.
- **Métricas:** registram informações sobre o comportamento e o desempenho da aplicação.
- **Health Checks:** indicam se a aplicação está em execução e se está pronta para atender requisições.

### Logs com Serilog

A aplicação utiliza `ILogger<T>` para registrar eventos, com Serilog como provedor de logging.

Os logs são estruturados e podem incluir informações de contexto úteis durante uma investigação, como:

- `Application`
- `Environment`
- `TraceId`
- `SpanId`
- `PetId`
- `ProntuarioId`

Exemplo ilustrativo:

```text
[14:32:10 INF] App=EloVet.Api Env=Development
Cadastro de prontuário concluído para o pet pet-001
```

O `TraceId` identifica o rastreamento de uma operação, enquanto o `SpanId` identifica uma etapa dentro desse rastreamento. Esses identificadores ajudam a relacionar os logs com os traces correspondentes.

### Traces e métricas com OpenTelemetry

A aplicação utiliza OpenTelemetry para instrumentar traces e métricas.

A instrumentação permite acompanhar informações como:

- requisições HTTP recebidas;
- duração das requisições;
- chamadas HTTP realizadas pela aplicação;
- etapas instrumentadas durante a execução das operações;
- métricas relacionadas ao comportamento da API.

A instrumentação fornece os dados necessários para que eles possam ser encaminhados a ferramentas externas de observabilidade, caso essa exportação seja configurada.

## Health Checks

A API disponibiliza dois endpoints para verificar seu estado:

```text
GET /health/live
GET /health/ready
```

### Liveness: `/health/live`

Indica se o processo da aplicação está em execução e responsivo. Essa verificação ajuda a identificar se a própria aplicação continua ativa.

```http
GET /health/live
```

### Readiness: `/health/ready`

Indica se a aplicação está pronta para receber requisições, considerando as dependências configuradas para essa verificação.

Neste projeto, o readiness também considera a disponibilidade do MongoDB.

```http
GET /health/ready
```

### Como testar os Health Checks

Com a API em execução, os endpoints podem ser consultados pelo navegador ou por ferramentas como `curl`.

```bash
curl http://localhost:5240/health/live
```

```bash
curl http://localhost:5240/health/ready
```

O primeiro verifica a disponibilidade do processo da aplicação. O segundo também verifica se o MongoDB está disponível para a API.

### Como acompanhar a aplicação

Durante a execução, é possível utilizar diferentes informações para acompanhar o serviço:

- logs estruturados exibidos no console;
- correlação de eventos por `TraceId` e `SpanId`;
- endpoints `/health/live` e `/health/ready`;
- traces e métricas instrumentados com OpenTelemetry.

## Executando o projeto

### Pré-requisitos

Para executar a API, é necessário ter:

- .NET 10 SDK;
- MongoDB disponível e configurado para o projeto.

### Configuração das variáveis de ambiente

A API utiliza variáveis de ambiente para receber as configurações do MongoDB e do JWT.

No .NET, o separador `__` (dois underscores) representa a separação entre níveis de configuração. Por exemplo, `Jwt__Key` corresponde à configuração `Jwt:Key`.

As instruções abaixo mostram como configurar as variáveis no terminal antes de iniciar a aplicação.

#### Bash — macOS e Linux

Gere uma chave aleatória para o JWT:

```bash
export Jwt__Key="$(openssl rand -base64 32)"
```

Configure as variáveis do MongoDB e do JWT:

```bash
export PROJECT_NAME=elo-vet
export MONGO_USERNAME=admin
export MONGO_PASSWORD=admin123
export MONGO_DATABASE=elo-vet

export MongoDb__ConnectionString="mongodb://admin:admin123@localhost:27017/elo-vet?authSource=admin"

export Jwt__Issuer="EloVet.Api"
export Jwt__Audience="EloVet.Client"
export Jwt__ExpirationInMinutes=120
```

#### PowerShell — Windows

```powershell
$env:PROJECT_NAME = "elo-vet"
$env:MONGO_USERNAME = "admin"
$env:MONGO_PASSWORD = "admin123"
$env:MONGO_DATABASE = "elo-vet"

$env:MongoDb__ConnectionString = "mongodb://admin:admin123@localhost:27017/elo-vet?authSource=admin"

$env:Jwt__Key = [Guid]::NewGuid().ToString("N") + [Guid]::NewGuid().ToString("N")
$env:Jwt__Issuer = "EloVet.Api"
$env:Jwt__Audience = "EloVet.Client"
$env:Jwt__ExpirationInMinutes = "120"
```

O comando do PowerShell gera uma chave aleatória com 64 caracteres. As variáveis definidas dessa forma ficam disponíveis na sessão atual do terminal.

Se precisar persistir a chave no perfil do usuário no Windows, é possível utilizar:

```powershell
[Environment]::SetEnvironmentVariable("Jwt__Key", "CHAVE_GERADA_AQUI", "User")
```

A chave de desenvolvimento existente em `appsettings.Development.json` pode ser utilizada localmente, mas não deve ser reutilizada em produção. Em ambientes reais, utilize uma chave própria, forte e mantida em um gerenciador seguro de segredos.

> **Atenção:** `admin` e `admin123` são valores de exemplo para desenvolvimento local. Não utilize essas credenciais em produção. Ajuste a connection string para corresponder às credenciais e ao endereço do MongoDB do seu ambiente.

### Cadastrar um usuário

Com a API em execução, envie uma requisição para:

```http
POST /api/Auth/register
Content-Type: application/json
```

Corpo da requisição:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

### Fazer login e obter o token

Depois de cadastrar o usuário, envie:

```http
POST /api/Auth/signin
Content-Type: application/json
```

Corpo da requisição:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

A resposta contém o token JWT. Copie o valor do campo `token` e envie-o nas próximas requisições protegidas:

```http
Authorization: Bearer SEU_TOKEN
```

### Restaurar as dependências

Na raiz da solução, execute:

```bash
dotnet restore
```

Esse comando restaura os pacotes NuGet necessários para os projetos da solução.

### Compilar a solução

```bash
dotnet build
```

A compilação verifica se os projetos conseguem ser construídos e se as referências e dependências estão resolvidas.

### Executar a API

```bash
dotnet run --project src/EloVet.Api
```

Depois que a aplicação iniciar, o terminal exibirá o endereço em que está sendo executada. Em ambiente de desenvolvimento, a documentação Swagger fica disponível no caminho:

```text
/swagger
```

## Executando os testes

A solução possui testes unitários e testes de integração. Eles verificam tanto o comportamento de componentes isolados quanto a comunicação entre as partes da aplicação.

### Executar todos os testes

Na raiz da solução:

```bash
dotnet test
```

### Executar somente os testes unitários

```bash
dotnet test tests/EloVet.UnitTests/EloVet.UnitTests.csproj
```

### Executar somente os testes de integração

```bash
dotnet test tests/EloVet.IntegrationTests/EloVet.IntegrationTests.csproj
```

### Testes unitários

Os testes unitários verificam uma classe ou comportamento de forma isolada, substituindo suas dependências por mocks quando necessário.

Na estrutura descrita para o projeto:

```text
Controller → Mock Service

Service → Mock Repository
```

Assim, os testes dos controllers podem simular o comportamento dos services, enquanto os testes dos services podem simular os repositories.

As ferramentas e práticas utilizadas são:

- **xUnit:** execução e organização dos testes;
- **Moq:** criação de mocks para simular dependências;
- **Arrange, Act e Assert (AAA):** organização do teste em preparação, execução e verificação do resultado.

### Testes de integração

Os testes de integração verificam o funcionamento conjunto de diferentes componentes. No fluxo principal, eles exercitam a comunicação entre a API e o MongoDB de teste, sem substituir por mocks todas as partes envolvidas.

O caminho geral testado é:

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

Dessa maneira, os testes ajudam a identificar problemas que não seriam encontrados ao testar cada classe isoladamente, como falhas na comunicação com o banco ou na integração entre as camadas.

### Relatório de cobertura de testes

O projeto possui um script `coverage.sh` para automatizar a execução dos testes e a geração do relatório de cobertura.

Para executar, utilize o seguinte comando na raiz do projeto:

```bash
chmod +x coverage.sh
./coverage.sh
```

O script realiza a limpeza dos resultados anteriores, executa os testes unitários e de integração, coleta a cobertura de código e gera um relatório HTML utilizando o ReportGenerator.

O relatório é aberto automaticamente no navegador e também fica disponível em `coveragereport/index.html`.
