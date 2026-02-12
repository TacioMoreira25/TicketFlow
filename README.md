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

Pré-requisitos: Docker e Docker Compose instalados.

1. Clone o repositório.
2. Navegue até a pasta raiz do projeto.
3. Execute o comando para subir o ambiente completo:

```bash
docker-compose up -d --build
```

O comando irá inicializar os containers da API, MySQL, RabbitMQ, Redis e Seq.

## Acesso aos Serviços

Após a inicialização, os serviços estarão disponíveis nas seguintes portas:

* **API (Swagger):** http://localhost:5000/swagger
* **RabbitMQ Management:** http://localhost:15672 (User: guest / Pass: guest)
* **Seq (Logs):** http://localhost:5341

## Testes Automatizados

O projeto utiliza xUnit e Testcontainers para testes de integração, subindo containers reais de banco de dados para validar os cenários.

Para rodar os testes:

```bash
dotnet test
```
