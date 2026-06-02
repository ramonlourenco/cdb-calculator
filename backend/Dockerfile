FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
WORKDIR /app

# 1. Como o contexto do Compose já é a pasta 'backend', usamos '.' para pegar tudo o que está dentro dela
COPY . .

# 2. Restaura a solução com as referências locais resolvidas
RUN dotnet restore CdbCalc.sln

# 3. Compila e publica o projeto da WebApi
RUN dotnet publish src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj -c Release -o /app/publish

# Estágio final de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CdbCalc.Adapters.Primary.WebApi.dll"]