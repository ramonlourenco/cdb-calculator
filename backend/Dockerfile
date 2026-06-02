FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
WORKDIR /app

# 1. Copia TODO o código do backend primeiro.
# Isso garante que a Solução, os projetos do Core, da WebApi e de Testes entrem juntos no container.
COPY . .

# 2. Restaura a solução inteira com todos os pacotes NuGet e referências locais resolvidas
RUN dotnet restore CdbCalc.sln

# 3. Compila e publica o projeto da WebApi em modo Release
RUN dotnet publish src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj -c Release -o /app/publish

# Estágio final de Runtime (Imagem Alpine leve e segura)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CdbCalc.Adapters.Primary.WebApi.dll"]