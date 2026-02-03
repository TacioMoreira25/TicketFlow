FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TicketFlow.Api/TicketFlow.Api.csproj", "TicketFlow.Api/"]
COPY ["TicketFlow.Application/TicketFlow.Application.csproj", "TicketFlow.Application/"]
COPY ["TicketFlow.Domain/TicketFlow.Domain.csproj", "TicketFlow.Domain/"]
COPY ["TicketFlow.Infrastructure/TicketFlow.Infrastructure.csproj", "TicketFlow.Infrastructure/"]

RUN dotnet restore "TicketFlow.Api/TicketFlow.Api.csproj"

COPY . .

WORKDIR "/src/TicketFlow.Api"
RUN dotnet publish "TicketFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TicketFlow.Api.dll"]