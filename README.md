# TicketFlow

API para gestão de eventos e venda de ingressos, focada em escalabilidade, performance e tratamento de concorrência. O projeto utiliza processamento assíncrono e cache distribuído para lidar com alto volume de requisições.

## Tecnologias Utilizadas

- .NET 8 / .NET 9 LTS (C# 12/13)
- ASP.NET Core Web API
- Entity Framework Core
- MySQL (Persistência relacional transacional)
- Redis (Cache distribuído)
- RabbitMQ (Mensageria assíncrona)
- Docker & Docker Compose
- xUnit & Testcontainers (Testes de integração conteinerizados)

## Arquitetura e Engenharia de Concorrência

O projeto segue os princípios da **Clean Architecture** e **SOLID**, com separação estrita de camadas e foco em resiliência:

- **TicketFlow.Api**: Camada de entrada (Controllers, Injeção de Dependências e Middlewares).
- **TicketFlow.Application**: Casos de uso (Use Cases), DTOs e Interfaces de serviços.
- **TicketFlow.Domain**: Entidades ricas, Regras de negócio e Validações de domínio.
- **TicketFlow.Infrastructure**: Acesso a dados (EF Core), adaptadores externos (Redis/RabbitMQ) e Repositórios.

### 🛡️ Tratamento de Alta Concorrência (Venda de Ingressos)
Para mitigar condições de corrida (*race conditions*) e compras simultâneas do último ingresso (*overbooking*):
1. **Controle de Concorrência Otimista (Optimistic Concurrency):** Uso de tokens de versão (`RowVersion`) gerenciados pelo Entity Framework Core sobre o MySQL.
2. **Desacoplamento Assíncrono com Mensageria:** Confirmações de pagamento e emissão de ingressos são despachadas para o **RabbitMQ**, garantindo que picos de tráfego sejam absorvidos sem sobrecarregar o banco de dados relacional.
3. **Cache de Baixa Latência:** Consultas frequentes de disponibilidade de eventos são cacheadas via **Redis**, reduzindo drasticamente leituras repetitivas no banco.

## Como Executar

### 1. Configuração

Copie o exemplo de variáveis de ambiente e ajuste conforme necessário:

```bash
cp .env.example .env
```

### 2. Rodando com Docker

Suba todo o ambiente (API + Banco + Mensageria + Cache + Logs) com um único comando:

```bash
docker compose up -d --build
```
A API estará disponível em: http://localhost:5000/swagger

### 3. Rodando Híbrido 

Para rodar a API localmente conectada à infraestrutura Docker:

1.  Suba apenas a infraestrutura:
    ```bash
    docker compose up -d mysql redis rabbitmq seq
    ```
2.  Configure o `appsettings.Development.json` ou *User Secrets* com as credenciais do `.env`.
3.  Execute as migrations:
    ```bash
    dotnet ef database update --project TicketFlow.Infrastructure --startup-project TicketFlow.Api
    ```
4.  Rode a API: `dotnet run --project TicketFlow.Api`

## Acesso aos Serviços

*   **Swagger:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
*   **RabbitMQ:** [http://localhost:15672](http://localhost:15672) (guest/guest)
*   **Seq:** [http://localhost:5341](http://localhost:5341)

## Testes

```bash
dotnet test
```
