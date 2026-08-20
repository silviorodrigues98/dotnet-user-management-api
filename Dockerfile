# Dockerfile multi-stage da API dotnet-user-management-api
# Nenhum segredo (ENV/ARG) com valores reais — config injetada em runtime via compose (T-02-03).

# Estágio build: SDK 8.0 — restore + publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos preservando o layout relativo esperado pelo .sln (solution/ referencia ..\src\...)
COPY Directory.Build.props ./
COPY solution/ ./solution/
COPY src/ ./src/
COPY tests/ ./tests/

RUN dotnet restore solution/DotnetUserManagementApi.sln

# O publish inclui a pasta wwwroot automaticamente (Program.cs serve favicon/estáticos)
RUN dotnet publish src/DotnetUserManagementApi.Api/DotnetUserManagementApi.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# Estágio final: runtime leve (alpine, sem libicu — InvariantGlobalization=true dispensa libicu)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DotnetUserManagementApi.Api.dll"]