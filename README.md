# EloVet

Sistema veterinário desenvolvido como projeto acadêmico, com foco em gestão de prontuários veterinários e integração com inteligência artificial.

## Arquitetura

O projeto utiliza uma arquitetura distribuída entre diferentes tecnologias:

* **Java** — backend principal e operações transacionais do sistema.
* **.NET 10** — serviço de integração e orquestração dos prontuários e da integração com IA. Também responsável pelo Health Check do sistema.
* **MongoDB** — armazenamento dos dados de prontuários e contexto clínico.
* **Python** — serviço responsável pelo processamento e integração com inteligência artificial.

### EloVet .NET

O serviço .NET está organizado em camadas:

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

### Estrutura

```text
src/
├── EloVet.Api/
├── EloVet.Application/
├── EloVet.Domain/
└── EloVet.Infrastructure/

tests/
├── EloVet.UnitTests/
└── EloVet.IntegrationTests/
```

## Tecnologias

* .NET 10
* C#
* ASP.NET Core
* MongoDB
* MongoDB.Driver
* Swagger / OpenAPI
* Serilog
* xUnit
* Moq

## Estado atual

A solução .NET encontra-se em fase inicial de desenvolvimento.

Atualmente estão configurados:

* Solution `.slnx`
* Projetos de API, Application, Domain e Infrastructure
* Projetos de testes unitários e de integração
* Referências entre projetos
* Swagger / OpenAPI
* Serilog
* MongoDB.Driver
* Estrutura inicial de pastas
* Build da solução
* Testes automatizados iniciais

## Executando o projeto

Restaurar dependências:

```bash
dotnet restore
```

Compilar a solução:

```bash
dotnet build
```

Executar os testes:

```bash
dotnet test
```

Executar a API:

```bash
dotnet run --project src/EloVet.Api
```

## Objetivo

O serviço .NET será responsável por intermediar o acesso aos prontuários armazenados no MongoDB e a comunicação com o serviço de inteligência artificial em Python, mantendo a separação de responsabilidades entre os componentes do EloVet.
