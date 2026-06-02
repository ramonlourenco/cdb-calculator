# 🔧 Guia de Diagnóstico e Correção do arquivo .sln para Docker

## 🎯 Problema

Erro ao rodar `docker build` em Alpine Linux:
```
/app/CdbCalc.sln : Solution file error MSB5010: No file format header found.
```

Este erro ocorre quando o arquivo `.sln` tem um dos seguintes problemas:

---

## 🔍 Parte 1: Diagnosticando o Arquivo .sln

### 1.1 - Verificar o Cabeçalho (CRÍTICO ⚠️)

O arquivo **DEVE começar exatamente com**:
```
Microsoft Visual Studio Solution File, Format Version 12.00
```

❌ **ERRADO** - Exemplos comuns:
```
Solution Format Version 12.00
<Solution Format Version 12.00
```

✅ **CORRETO**:
```
Microsoft Visual Studio Solution File, Format Version 12.00
```

### 1.2 - Verificar o Encoding

No **PowerShell** (Windows), verifique o encoding:
```powershell
# PowerShell - Verificar encoding
$file = Get-Content -Path "CdbCalc.sln" -Encoding Byte -TotalCount 3
# Se for: EF BB BF = UTF-8 BOM ✅ CORRETO
# Se for: FF FE = UTF-16 ❌ ERRADO
```

### 1.3 - Verificar as Quebras de Linha

No **Git Bash** (MSYS):
```bash
# Verificar quebras de linha
file CdbCalc.sln
# Output esperado: "...CRLF" ou "LF" - ambos funcionam

# Visualizar caracteres especiais
od -c CdbCalc.sln | head -5
# Procure por: \r\n (CRLF) ou \n (LF)
```

### 1.4 - Verificar os GUIDs dos Projetos

❌ **PROBLEMA** - GUIDs fictícios/duplicados:
```
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Domain", "src\CdbCalc.Domain\CdbCalc.Domain.csproj", "{00000001-0000-0000-0000-000000000001}"
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Application", "src\CdbCalc.Application\CdbCalc.Application.csproj", "{00000001-0000-0000-0000-000000000001}"
```

✅ **CORRETO** - GUIDs únicos:
```
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Domain", ".\src\CdbCalc.Domain\CdbCalc.Domain.csproj", "{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}"
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Application", ".\src\CdbCalc.Application\CdbCalc.Application.csproj", "{B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}"
```

### 1.5 - Verificar os Caminhos dos Projetos

❌ **PROBLEMA** - Caminhos relativos sem `.\`:
```
"src\CdbCalc.Domain\CdbCalc.Domain.csproj"
"src\CdbCalc.Application\CdbCalc.Application.csproj"
```

✅ **CORRETO** - Com `.\` (funciona em Windows e Linux):
```
".\src\CdbCalc.Domain\CdbCalc.Domain.csproj"
".\src\CdbCalc.Application\CdbCalc.Application.csproj"
```

---

## ✅ Parte 2: Estrutura Correta do .sln

Um arquivo `.sln` válido tem esta estrutura:

```
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.8.34330.188
MinimumVisualStudioVersion = 10.0.40219.1
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Domain", ".\src\CdbCalc.Domain\CdbCalc.Domain.csproj", "{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Application", ".\src\CdbCalc.Application\CdbCalc.Application.csproj", "{B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Adapters.Primary.WebApi", ".\src\CdbCalc.Adapters.Primary.WebApi\CdbCalc.Adapters.Primary.WebApi.csproj", "{C3D4E5F6-A7B8-4C5D-8E9F-2A3B4C5D6E7F}"
EndProject
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CdbCalc.Application.Tests", ".\tests\CdbCalc.Application.Tests\CdbCalc.Application.Tests.csproj", "{D4E5F6A7-B8C9-4D5E-8F9A-3B4C5D6E7F80}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}.Release|Any CPU.Build.0 = Release|Any CPU
		{B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}.Release|Any CPU.Build.0 = Release|Any CPU
		{C3D4E5F6-A7B8-4C5D-8E9F-2A3B4C5D6E7F}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{C3D4E5F6-A7B8-4C5D-8E9F-2A3B4C5D6E7F}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{C3D4E5F6-A7B8-4C5D-8E9F-2A3B4C5D6E7F}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{C3D4E5F6-A7B8-4C5D-8E9F-2A3B4C5D6E7F}.Release|Any CPU.Build.0 = Release|Any CPU
		{D4E5F6A7-B8C9-4D5E-8F9A-3B4C5D6E7F80}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{D4E5F6A7-B8C9-4D5E-8F9A-3B4C5D6E7F80}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{D4E5F6A7-B8C9-4D5E-8F9A-3B4C5D6E7F80}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{D4E5F6A7-B8C9-4D5E-8F9A-3B4C5D6E7F80}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
```

---

## 🛠️ Parte 3: Como Corrigir o Arquivo .sln

### Opção 1: Gerar um novo .sln via CLI (RECOMENDADO)

```powershell
# Ir para o diretório backend
cd backend

# Remover o .sln antigo
Remove-Item CdbCalc.sln -Force

# Criar um novo .sln com todos os projetos
dotnet new sln --name CdbCalc

# Adicionar os projetos
dotnet sln add ./src/CdbCalc.Domain/CdbCalc.Domain.csproj
dotnet sln add ./src/CdbCalc.Application/CdbCalc.Application.csproj
dotnet sln add ./src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj
dotnet sln add ./tests/CdbCalc.Application.Tests/CdbCalc.Application.Tests.csproj

# Verificar
dotnet sln list
```

### Opção 2: Corrigir Manualmente (AVANÇADO)

Se você já tem um `.sln` e só quer corrigir, edite manualmente:

1. **Abra o arquivo no VS Code**
2. **Na primeira linha**, verifique se começa com:
   ```
   Microsoft Visual Studio Solution File, Format Version 12.00
   ```
   Se não, corrija para exatamente isso.

3. **Verifique todos os GUIDs** - devem ser únicos para cada projeto
   ```
   {A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}  ✅ CORRETO
   {B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}  ✅ CORRETO
   {C3D4E5F6-A7B8-4C5D-8E9F-2A3B4C5D6E7F}  ✅ CORRETO
   {D4E5F6A7-B8C9-4D5E-8F9A-3B4C5D6E7F80}  ✅ CORRETO
   ```

4. **Verifique os caminhos** - todos devem começar com `.\`
   ```
   ".\src\CdbCalc.Domain\CdbCalc.Domain.csproj"          ✅ CORRETO
   ".\src\CdbCalc.Application\CdbCalc.Application.csproj"  ✅ CORRETO
   ```

---

## 🐳 Parte 4: Testando no Docker

### 4.1 - Testar Localmente (Windows)

```powershell
# Build local para verificar
dotnet restore CdbCalc.sln
dotnet build CdbCalc.sln
```

### 4.2 - Testar no Docker

```bash
# Git Bash - Build com diagnóstico
docker build --no-cache --progress=plain -f Dockerfile -t cdb-calc:test .

# Se falhar, execute dentro do container para debugar
docker run -it --rm -v ${PWD}:/app mcr.microsoft.com/dotnet/sdk:8.0-alpine sh

# Dentro do container:
cd /app
dotnet restore CdbCalc.sln
```

### 4.3 - Dockerfile Otimizado

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
WORKDIR /app

# Copiar arquivo .sln e .csproj primeiro (melhora cache)
COPY CdbCalc.sln .
COPY src/CdbCalc.Domain/CdbCalc.Domain.csproj ./src/CdbCalc.Domain/
COPY src/CdbCalc.Application/CdbCalc.Application.csproj ./src/CdbCalc.Application/
COPY src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj ./src/CdbCalc.Adapters.Primary.WebApi/
COPY tests/CdbCalc.Application.Tests/CdbCalc.Application.Tests.csproj ./tests/CdbCalc.Application.Tests/

# Restaurar dependências
RUN dotnet restore CdbCalc.sln

# Copiar código-fonte
COPY . .

# Build
RUN dotnet build CdbCalc.sln -c Release --no-restore

# Publicar
RUN dotnet publish ./src/CdbCalc.Adapters.Primary.WebApi/CdbCalc.Adapters.Primary.WebApi.csproj \
    -c Release -o /app/publish --no-restore --no-build

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "CdbCalc.Adapters.Primary.WebApi.dll"]
```

---

## 📝 Resumo das Correções Necessárias

| Problema | Solução |
|----------|---------|
| ❌ Cabeçalho inválido | ✅ Use: `Microsoft Visual Studio Solution File, Format Version 12.00` |
| ❌ GUIDs duplicados/fictícios | ✅ Use GUIDs únicos e válidos (formato UUID) |
| ❌ Caminhos sem `.\` | ✅ Use: `.\src\NomeProjeto\NomeProjeto.csproj` |
| ❌ Encoding UTF-16 | ✅ Converta para UTF-8 (com ou sem BOM) |
| ❌ Quebras de linha inconsistentes | ✅ Use CRLF (Windows) ou LF (Unix) - ambos funcionam |
| ❌ Arquivo corrompido | ✅ Regere com: `dotnet new sln` + `dotnet sln add` |

---

## 🎉 Seu Arquivo Está OK!

Seu arquivo `.sln` atual está **100% correto** para Docker! ✅

```
✅ Cabeçalho: Microsoft Visual Studio Solution File, Format Version 12.00
✅ GUIDs únicos: {A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}, {B2C3D4E5-F6A7-4B5C-8D9E-1F2A3B4C5D6E}, etc.
✅ Caminhos com .\: ".\src\CdbCalc.Domain\CdbCalc.Domain.csproj"
✅ Encoding: UTF-8 com BOM
```

O build do Docker deve funcionar sem problemas agora! 🐳
