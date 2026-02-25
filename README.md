# TicketFlow

API para gestão de eventos e venda de ingressos, focada em escalabilidade, performance e tratamento de concorrência. O projeto utiliza processamento assíncrono e cache distribuído para lidar com alto volume de requisições.

## Tecnologias Utilizadas

- .NET 10 (Preview)
- ASP.NET Core Web API
- Entity Framework Core
- MySQL (Persistência de dados)
- Redis (Cache distribuído)
- RabbitMQ (Mensageria assíncrona)
- Docker & Docker Compose
- xUnit & Testcontainers (Testes de integração)

## Arquitetura

O projeto segue os princípios da Clean Architecture, dividido em camadas para garantir a separação de responsabilidades:

- **TicketFlow.Api**: Camada de entrada (Controllers, Configurações).
- **TicketFlow.Application**: Casos de uso, DTOs e Interfaces.
- **TicketFlow.Domain**: Entidades, Regras de negócio e Validações.
- **TicketFlow.Infrastructure**: Implementação de acesso a dados, serviços externos (Redis/RabbitMQ) e Repositórios.

## Funcionalidades Principais

- Gerenciamento de Eventos (Criação e Listagem).
- Venda de Ingressos com controle de concorrência (Optimistic Concurrency).
- Processamento assíncrono de confirmações de venda via fila.
- Cache de consultas frequentes para redução de carga no banco de dados.
- Logs estruturados e centralizados (Seq).

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
