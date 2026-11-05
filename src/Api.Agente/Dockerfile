# Contexto de build: raiz do repositório (docker-compose.yml). Sem segredos nem appsettings com
# credenciais: a configuração sensível chega por variáveis de ambiente (research R-24).

FROM mcr.microsoft.com/dotnet/sdk:10.0.403 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Api.Agente/Api.Agente.csproj src/Api.Agente/
COPY src/AuditoriaNFe.Application/AuditoriaNFe.Application.csproj src/AuditoriaNFe.Application/
COPY src/AuditoriaNFe.Domain/AuditoriaNFe.Domain.csproj src/AuditoriaNFe.Domain/
COPY src/AuditoriaNFe.Infrastructure/AuditoriaNFe.Infrastructure.csproj src/AuditoriaNFe.Infrastructure/
RUN dotnet restore src/Api.Agente/Api.Agente.csproj

COPY src/Api.Agente/ src/Api.Agente/
COPY src/AuditoriaNFe.Application/ src/AuditoriaNFe.Application/
COPY src/AuditoriaNFe.Domain/ src/AuditoriaNFe.Domain/
COPY src/AuditoriaNFe.Infrastructure/ src/AuditoriaNFe.Infrastructure/
RUN dotnet publish src/Api.Agente/Api.Agente.csproj -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build --chown=$APP_UID /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Api.Agente.dll"]
