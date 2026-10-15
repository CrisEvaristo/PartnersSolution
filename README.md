# Partners API

API REST para gerir clientes e fornecedores de uma empresa, feita com ASP.NET Core, EF Core e SQL Server. Uma entidade pode ser `CUSTOMER`, `SUPPLIER` ou `BOTH`; o NIF normalizado é único, mesmo em registos inativos.

## Versões

- .NET SDK 10.0 (validado com 10.0.401)
- ASP.NET Core 10.0
- Entity Framework Core e `dotnet-ef` 10.0.12
- SQL Server 2022 Developer ou SQL Server Express para desenvolvimento/testes

## Preparar

Instale o .NET 10 SDK e uma instância local do SQL Server. Configure a ligação no PowerShell (ajuste o servidor/credenciais à sua instalação):

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=DBPartners;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet restore Partners.Api/Partners.Api.csproj
```

## Migrar e executar

```powershell
dotnet ef database update --project Partners.Infrastructure/Partners.Infrastructure.csproj --startup-project Partners.Api/Partners.Api.csproj
dotnet run --project Partners.Api/Partners.Api.csproj
```

A API fica disponível em `https://localhost:7295` e `http://localhost:5120`. O `.http` em `Partners.Api/Partners.Api.http` contém exemplos para os quatro endpoints. As migrations incluem 10 parceiros fictícios para demonstração; aplique-as com `dotnet ef database update` para carregar os dados também numa base já existente.

## Endpoints

- `POST /api/partners`: cria e retorna `201 Created` com `Location` e `Version`.
- `GET /api/partners/{id}`: consulta por Id, incluindo entidades inativas.
- `GET /api/partners`: filtros combináveis `Search`, `Type`, `IncludeInactive`, `Page` e `PageSize`.
- `PUT /api/partners/{id}`: atualização integral, incluindo ativação/desativação; exige a `Version` atual.

`Version` é a representação Base64 do `rowversion` SQL Server. Conflito de NIF e versão desatualizada retornam 409; validações retornam 400 em Problem Details.

## Testes

Os testes unitários não dependem do SQL Server. Os testes de integração exercitam a API e concorrência real no SQL Server; configure uma conexão com permissão para criar e remover bancos de dados. A suíte cria um banco temporário com nome aleatório e remove-o ao terminar:

```powershell
dotnet test Partners.Tests/Partners.Tests.csproj
$env:PARTNERS_TEST_SQLSERVER = "Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet test Partners.Tests/Partners.Tests.csproj
```

Sem `PARTNERS_TEST_SQLSERVER`, os testes de integração são ignorados; os unitários continuam a executar.
