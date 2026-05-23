# ESCOLA_API - Escola High Tech

API REST em ASP.NET Core 10 para gerenciamento escolar com autenticacao JWT, autorizacao por perfis e CRUD centralizado de usuarios.

## Tecnologias

- ASP.NET Core 10
- Entity Framework Core
- SQL Server e SQLite em desenvolvimento/testes
- JWT Bearer
- FluentValidation
- Swagger/OpenAPI
- xUnit, Moq e FluentValidation.TestHelper
- Logging diario em arquivo

## Como Rodar Localmente

```bash
dotnet restore ESCOLA_API.csproj
dotnet run
```

Em desenvolvimento, `appsettings.Development.json` usa SQLite local em `escola-dev.db`.
Se `Jwt__Key` nao estiver definida em Development, a API cria uma chave local em `.local/jwt.key`, pasta ignorada pelo Git.
Em container ou producao, defina `Jwt__Key` e a connection string por variaveis de ambiente ou secrets do provedor.

## Deploy no Render

No Render, cadastre as variaveis em **Web Service > Environment** e depois faca um novo deploy:

| Variavel | Exemplo/observacao |
| --- | --- |
| `Jwt__Key` | Chave secreta com ao menos 32 bytes. Gere uma no PowerShell com `[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))`. |
| `ConnectionStrings__DefaultConnection` | Obrigatoria em producao. Para SQL Server, use algo como `Server=tcp:SEU_HOST,1433;Database=ESCOLA_API;User Id=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=True;`. |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

O separador `__` nas variaveis de ambiente representa `:` na configuracao do ASP.NET Core. Por isso, `Jwt:Key` deve ser cadastrado como `Jwt__Key`, e `ConnectionStrings:DefaultConnection` como `ConnectionStrings__DefaultConnection`.

Para testes simples sem banco externo, e possivel usar SQLite com `ConnectionStrings__DefaultConnection=Data Source=escola.db`, mas os dados podem ser perdidos em redeploy/restart do container. Para uso real, prefira SQL Server persistente.

O arquivo `render.yaml` deste repositorio tambem declara essas variaveis para deploy via Blueprint. Nesse fluxo, o Render gera `Jwt__Key` automaticamente e pede `ConnectionStrings__DefaultConnection` no dashboard.

## Docker Compose

Na raiz do repositorio:

```bash
cp .env.example .env
docker compose -f docker/docker-compose.yml up --build
```

Preencha o `.env` local com `MSSQL_SA_PASSWORD` e `JWT_KEY` antes de subir os containers.
Esse arquivo local e ignorado pelo Git; mantenha apenas `.env.example` versionado.

Acessos padrao:

- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Frontend: `http://localhost:8080`
- SQL Server: `localhost,1433`

## Usuario Inicial

```text
Email: admin@escola.com
Senha: Senha@123
Perfil: Administrador
```

Usuarios criados pelo cadastro recebem a senha inicial:

```text
Senha@252525
```

## Logs

Os logs ficam na raiz do projeto backend:

```text
ESCOLA_API/logs/escola-api-YYYYMMDD.log
```

O provider `DailyFileLoggerProvider` registra:

- Startup da API.
- Preparacao do banco e migrations.
- Inicio e fim das requisicoes HTTP.
- Metodo, rota, usuario, status e tempo de execucao.
- Erros capturados pelos controllers.
- Tentativas de login recusadas, login bem-sucedido e troca de senha.

No Docker Compose, o volume `../ESCOLA_API/logs:/app/logs` mantem os logs do container nessa mesma pasta.

## Arquitetura

O backend usa arquitetura em camadas:

- `Controllers`: recebem requisicoes HTTP, aplicam autorizacao e retornam status codes.
- `Validators`: validam ViewModels via FluentValidation.
- `Services`: executam regras de negocio e orquestram persistencia.
- `Data`: contem `DataContext`, migrations e repository.
- `Models`: representam entidades persistidas.
- `ViewModels`: representam contratos de entrada e saida da API.
- `Security`: hash e politica de senha.
- `Swagger`: filtros OpenAPI para documentar endpoints protegidos.
- `Logging`: provider de log diario em arquivo.

## Endpoints Principais

| Entidade | Rotas |
| --- | --- |
| Auth | `POST /api/Auth/login`, `GET /api/Auth/me`, `POST /api/Auth/alterar-senha` |
| Usuarios | `GET /api/usuarios`, `GET /api/usuarios/{id}`, `GET /api/usuarios/perfis`, `POST /api/usuarios`, `PUT /api/usuarios/{id}`, `DELETE /api/usuarios/{id}` |

## Autorizacao

- `Administrador`: acesso completo ao CRUD de usuarios e pode cadastrar `Aluno`, `Professor` ou `Administrador`.
- `Professor`: pode cadastrar apenas usuarios do tipo `Aluno`.
- `Aluno`: nao cadastra usuarios; pode atualizar apenas seus proprios dados basicos (`nome`, `email` e `telefone`).

No cadastro de usuario, informe `tipoUsuario` como `Aluno`, `Professor` ou `Administrador`. Os campos `nome`, `email` e `telefone` sao obrigatorios.

## Testes

```bash
dotnet test ESCOLA_API.Tests/ESCOLA_API.Tests.csproj
```

## Documentacao

- Markdown tecnico: `../docs/backend-tecnico.md`
- PDF tecnico: `../docs/backend-tecnico.pdf`
- PDF tecnico completo do backend: `docs/documentacao-tecnica-backend.pdf`
- HTML fonte do PDF completo: `docs/documentacao-tecnica-backend.html`
- Swagger: `/swagger` em ambiente de desenvolvimento

No Swagger, execute `POST /api/Auth/login`, copie o token e use `Authorize` com:

```text
Bearer {token}
```
