FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src

COPY ["src/CdbCalc.Domain/CdbCalc.Domain.csproj", "src/CdbCalc.Domain/"]
COPY ["src/CdbCalc.Application/CdbCalc.Application.csproj", "src/CdbCalc.Application/"]
COPY ["src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj", "src/CdbCalc.Adapters.Primary.WebApi/"]

RUN dotnet restore "src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj"

COPY . .

RUN dotnet build "src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj" -c Release -o /app/build

RUN dotnet publish "src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

COPY --from=builder /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CdbCalc.Adapters.Primary.WebApi.dll"]
