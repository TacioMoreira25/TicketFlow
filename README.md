# TicketFlow

> **API RESTful em .NET 8 LTS para gestão de eventos e venda de ingressos sob alta concorrência — prevenindo *overbooking* com Optimistic Concurrency, mensageria assíncrona (RabbitMQ), cache distribuído (Redis) e testes reais com Testcontainers.**

[![TicketFlow CI](https://github.com/TacioMoreira25/TicketFlow/actions/workflows/ci.yml/badge.svg)](https://github.com/TacioMoreira25/TicketFlow/actions/workflows/ci.yml)
![.NET 8](https://img.shields.io/badge/.NET_8_LTS-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23_12-239120?style=for-the-badge&logo=csharp&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

---

## Arquitetura & Fluxo de Alta Concorrência

O projeto segue **Clean Architecture** e **SOLID**, separando estritamente domínio, casos de uso e infraestrutura:

```mermaid
flowchart LR
    Client["Cliente / Load Test"] -->|HTTP REST| API["TicketFlow.Api\n(.NET 8 + Swagger)"]
    API -->|Consulta Disponibilidade| Redis["Redis Cache\n(Baixa Latência)"]
    API -->|Reserva / Transação| MySQL["MySQL\n(Optimistic Lock: RowVersion)"]
    API -->|Publica Evento de Pedido| RMQ["RabbitMQ\n(Mensageria Assíncrona)"]
    RMQ -->|Processamento Desacoplado| Worker["Consumer / Emissão"]
    API -.->|Logs Estruturados| Seq["Seq Observability"]
```

### Organização em Camadas
- **`TicketFlow.Domain`**: Entidades ricas, regras de negócio, invariantes de domínio e controle de versão (`RowVersion`).
- **`TicketFlow.Application`**: Casos de uso (*Use Cases*), DTOs e contratos/interfaces desacoplados da infraestrutura.
- **`TicketFlow.Infrastructure`**: Persistência com Entity Framework Core (MySQL), adaptadores de cache (Redis) e mensageria (RabbitMQ).
- **`TicketFlow.Api`**: Controllers REST, middlewares globais, injeção de dependências e documentação OpenAPI/Swagger.
- **`Tests`**: Suíte de testes automatizados (xUnit + FluentAssertions + **Testcontainers** subindo containers reais no CI).

---

## Decisões Técnicas & Trade-offs

| Problema de Engenharia | Decisão Adotada | Por que esta escolha (Trade-off)? |
| :--- | :--- | :--- |
| **Condição de corrida (*Overbooking*)** na compra simultânea do último ingresso | **Optimistic Concurrency** (`RowVersion` via EF Core sobre MySQL) | Mantém alta vazão de leitura sem bloquear linhas no banco sob picos de tráfego, falhando de forma controlada (`DbUpdateConcurrencyException`) em conflitos simultâneos. |
| **Picos de requisições** na confirmação e emissão de ingressos | **Desacoplamento assíncrono via RabbitMQ** | Evita travar o ciclo HTTP esperando processamento secundário, absorvendo rajadas de tráfego com filas duráveis. |
| **Leituras repetitivas** de disponibilidade de eventos | **Cache distribuído com Redis** | Reduz a pressão de I/O no banco relacional em consultas quentes de catálogo/disponibilidade. |
| **Mocks frágeis** que mascaram bugs de concorrência em testes | **Testcontainers (MySQL real no Docker)** | O provedor `EF Core In-Memory` não valida `RowVersion` nem constraints relacionais reais; Testcontainers garante validação fiel no GitHub Actions. |

---

## Como Executar

### 1. Configuração
Copie o arquivo de variáveis de ambiente:
```bash
cp .env.example .env
```

### 2. Rodando com Docker (Stack Completa)
Suba API, MySQL, Redis, RabbitMQ e Seq com um único comando:
```bash
docker compose up -d --build
```

### 3. Rodando Híbrido (Infra Docker + API Local)
1. Suba a infraestrutura:
   ```bash
   docker compose up -d mysql redis rabbitmq seq
   ```
2. Aplique as migrations:
   ```bash
   dotnet ef database update --project TicketFlow.Infrastructure --startup-project TicketFlow.Api
   ```
3. Inicie a API:
   ```bash
   dotnet run --project TicketFlow.Api
   ```

### Acesso aos Serviços
- **Swagger UI:** `http://localhost:5000/swagger`
- **RabbitMQ Management:** `http://localhost:15672` (`guest` / `guest`)
- **Seq (Logs Estruturados):** `http://localhost:5341`

---

## Testes Automatizados

Requer Docker ativo para execução dos testes de integração com Testcontainers:
```bash
dotnet test TicketFlow.sln
```
