# ESCOLA_API - Escola Conectada

API REST em ASP.NET Core 10 para gerenciamento escolar com autenticacao JWT, autorizacao por perfis, CRUD centralizado de usuarios, caderneta digital, calendario escolar, agenda de avaliacoes/trabalhos com notificacoes aos alunos, QR Code bancario ficticio para alunos e holerites de funcionarios.

## Tecnologias

- ASP.NET Core 10
- Entity Framework Core
- SQL Server e SQLite em desenvolvimento/testes
- JWT Bearer
- FluentValidation
- Swagger/OpenAPI e Scalar
- QRCoder para geracao de QR Code PNG
- Azure Blob Storage para fotos, certificados e holerites
- Azure Service Bus para consumo opcional de notificacoes
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
| `ServiceBus__ConnectionString` | Opcional. Cadeia de conexao primaria do Azure Service Bus para publicar eventos da caderneta digital. |
| `ServiceBus__QueueName` | Opcional. Nome da fila de notificacoes. Padrao: `notificacoes`. |
| `ServiceBus__ConsumerEnabled` | Opcional. Define se a API tambem consome a fila e grava notificacoes no banco. Padrao: `true`. |
| `Uploads__Provider` | Use `AzureBlob` em producao para salvar fotos, certificados e holerites no Azure Blob Storage. Use `Local` em desenvolvimento. |
| `AzureBlob__ConnectionString` | Obrigatoria quando `Uploads__Provider=AzureBlob`. Use a connection string do Storage Account. |
| `AzureBlob__ContainerName` | Container dos arquivos. Padrao usado no deploy: `arquivos`. |
| `AzureBlob__PublicBaseUrl` | Opcional. URL publica/CDN do container. Se vazia, a API usa a URL padrao do blob. |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

O separador `__` nas variaveis de ambiente representa `:` na configuracao do ASP.NET Core. Por isso, `Jwt:Key` deve ser cadastrado como `Jwt__Key`, `ConnectionStrings:DefaultConnection` como `ConnectionStrings__DefaultConnection`, e `AzureBlob:ConnectionString` como `AzureBlob__ConnectionString`. A API tambem aceita `AzureStorage__ConnectionString`, `AzureStorage__ContainerName` e `AzureStorage__PublicBaseUrl` como alias.

Para testes simples sem banco externo, e possivel usar SQLite com `ConnectionStrings__DefaultConnection=Data Source=escola.db`, mas os dados podem ser perdidos em redeploy/restart do container. Para uso real, prefira SQL Server persistente.

O arquivo `render.yaml` deste repositorio tambem declara essas variaveis para deploy via Blueprint. Nesse fluxo, o Render gera `Jwt__Key` automaticamente e pede `ConnectionStrings__DefaultConnection` no dashboard.

As notificacoes principais da caderneta, do cadastro de usuarios e da agenda de avaliacoes/trabalhos sao gravadas na tabela `Notificacao`. Quando `ServiceBus__ConnectionString` estiver configurada, a API tambem pode consumir mensagens da fila `ServiceBus__QueueName` e transformar cada mensagem valida em notificacao no banco. Se a variavel nao estiver configurada, a API continua funcionando normalmente sem iniciar o consumidor.

Quando `Uploads__Provider=AzureBlob`, a API grava fotos, certificados e holerites no Azure Blob Storage e salva no banco apenas a URL e os metadados do arquivo. Para que o front consiga exibir fotos e abrir PDFs diretamente, o container precisa permitir leitura publica dos blobs ou `AzureBlob__PublicBaseUrl` deve apontar para uma URL publica/CDN configurada para esse container.

## Docker Compose

Na raiz do repositorio:

```bash
cp .env.example .env
docker compose up -d --build
```

No PowerShell, se preferir:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

Preencha o `.env` local com `MSSQL_SA_PASSWORD` e `JWT_KEY` antes de subir os containers.
Esse arquivo local e ignorado pelo Git; mantenha apenas `.env.example` versionado.

Acessos padrao:

- API: `http://localhost:5001`
- Swagger: `http://localhost:5001/swagger`
- Scalar: `http://localhost:5001/scalar`
- SQL Server: `localhost,1433`

O SQL Server usa o volume nomeado `escola-high-tech-mssql-data`, montado em `/var/opt/mssql` dentro do container. Esse volume preserva os dados entre rebuilds, restarts e recriacoes dos containers.

Para parar sem perder dados:

```bash
docker compose down
```

Para subir novamente usando o mesmo banco:

```bash
docker compose up -d --build
```

Evite `docker compose down -v`, `docker volume rm escola-high-tech-mssql-data` ou trocar `MSSQL_DATABASE` se quiser manter os dados existentes. Esses comandos/configuracoes removem ou apontam para outro banco.

A API executa `Database.Migrate()` no startup. Com o volume persistido, o Entity Framework consulta a tabela `__EFMigrationsHistory` e aplica apenas migrations pendentes; ele nao recria o banco nem apaga dados ja inseridos. Se o volume for removido, o SQL Server iniciara vazio e as migrations/seed iniciais serao aplicadas como primeira criacao.

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
| Auth | `POST /api/Auth/login`, `GET /api/Auth/me`, `GET /api/Auth/autorizar`, `GET /api/Auth/autorizar/admin`, `POST /api/Auth/alterar-senha`, `POST /api/Auth/esqueci-senha` |
| Usuarios | `GET /api/usuarios`, `GET /api/usuarios/{id}`, `GET /api/usuarios/perfis`, `POST /api/usuarios`, `PUT /api/usuarios/{id}`, `DELETE /api/usuarios/{id}` |
| Arquivos de usuario | `GET /api/usuarios/{id}/foto`, `POST /api/usuarios/{id}/foto`, `GET /api/usuarios/{id}/arquivos`, `GET /api/usuarios/{id}/arquivos/{arquivoId}/download`, `POST /api/usuarios/{id}/certificados`, `DELETE /api/usuarios/{id}/arquivos/{arquivoId}` |
| QR Code bancario ficticio | `GET /api/alunos/me/qr-code-bancario` |
| Holerites | `GET /api/holerites/me`, `GET /api/holerites/me/{holeriteId}/download`, `GET /api/holerites/usuarios/{usuarioId}`, `POST /api/holerites/usuarios/{usuarioId}`, `GET /api/holerites/usuarios/{usuarioId}/{holeriteId}/download`, `DELETE /api/holerites/usuarios/{usuarioId}/{holeriteId}` |
| Calendario Escolar | `GET /api/calendario-escolar?ano=2026&mesSelecionado=5` |
| Caderneta Digital | `GET /api/caderneta-digital`, `GET /api/caderneta-digital/{id}`, `POST /api/caderneta-digital`, `PUT /api/caderneta-digital/{id}`, `DELETE /api/caderneta-digital/{id}` |
| Disciplinas | `GET /api/caderneta-digital/disciplinas`, `POST /api/caderneta-digital/disciplinas`, `PUT /api/caderneta-digital/disciplinas/{id}`, `DELETE /api/caderneta-digital/disciplinas/{id}` |
| Eventos de disciplinas | `GET /api/caderneta-digital/disciplinas/eventos`, `POST /api/caderneta-digital/disciplinas/{disciplinaId}/eventos`, `PUT /api/caderneta-digital/disciplinas/{disciplinaId}/eventos/{eventoId}`, `DELETE /api/caderneta-digital/disciplinas/{disciplinaId}/eventos/{eventoId}` |
| Notificacoes | `GET /api/notificacoes`, `GET /api/notificacoes/nao-lidas/contador`, `POST /api/notificacoes`, `POST /api/notificacoes/perfis`, `PATCH /api/notificacoes/{id}/lida`, `PATCH /api/notificacoes/lidas` |

## Autorizacao

- `Administrador`: cadastra, edita e exclui usuarios; envia notificacoes manuais; gerencia arquivos de qualquer usuario; gerencia holerites de professores/administradores; ao lancar holerite, notifica todos os professores com competencia, arquivo, funcionario e responsavel pelo lancamento; visualiza caderneta, disciplinas, eventos e calendario escolar.
- `Professor`: visualiza alunos e professores cadastrados; edita apenas o proprio perfil; consulta seus proprios holerites; cadastra disciplinas; faz lancamentos de notas, presencas e faltas; agenda avaliacoes e entregas de trabalhos nas suas disciplinas.
- `Aluno`: visualiza e edita apenas o proprio perfil; visualiza somente cadernetas e eventos das disciplinas associadas; gera QR Code bancario ficticio do proprio usuario; recebe notificacoes quando notas, frequencia, avaliacoes e trabalhos sao publicados.

No cadastro de usuario, informe `tipoUsuario` como `Aluno`, `Professor` ou `Administrador`. Os campos `nome`, `email` e `telefone` sao obrigatorios. Os campos opcionais `dataNascimento`, `nomeMae`, `nomePai` e `endereco` ficam no contrato de criacao/edicao e retornam nas consultas. `dataNascimento` usa formato ISO `yyyy-MM-dd`, adequado para Datepicker que permita digitar ou selecionar a data.

Exemplo de evento de disciplina:

```json
{
  "tipo": "Avaliacao",
  "titulo": "Prova bimestral",
  "descricao": "Capitulos 1 e 2",
  "data": "2026-06-10"
}
```

O endpoint de QR Code retorna dados bancarios ficticios, `qrCodeBase64`, `qrCodeDataUrl`, texto de compartilhamento e links prontos para `mailto:` e WhatsApp. Ele nao realiza pagamento real nem envia mensagens diretamente por provedores externos.

O endpoint de QR Code e exclusivo para usuarios com perfil `Aluno`. Professores e administradores nao acessam esse recurso; para funcionarios, a API disponibiliza holerites em PDF com listagem e download autenticados. O envio e a exclusao de holerites sao operacoes exclusivas de administradores, e holerites nao podem ser vinculados a alunos.

Quando o professor cria ou atualiza uma avaliacao/trabalho em uma disciplina, a API identifica os alunos matriculados pela caderneta digital e cria notificacoes individuais para eles em `/api/notificacoes`.

Para envio manual em lote, o administrador pode usar `POST /api/notificacoes/perfis` informando `idsPerfis`, `tiposUsuario` ou `todosOsPerfis`. Exemplo para alunos e professores:

```json
{
  "tiposUsuario": ["Aluno", "Professor"],
  "titulo": "Aviso escolar",
  "mensagem": "Mensagem para alunos e professores",
  "tipo": "Geral",
  "link": "/notificacoes"
}
```

## Testes

```bash
dotnet test ESCOLA_API.Tests/ESCOLA_API.Tests.csproj
```

## Documentacao

- PDF tecnico completo do backend: `docs/documentacao-tecnica-backend.pdf`
- HTML fonte do PDF completo: `docs/documentacao-tecnica-backend.html`
- Swagger UI: `/swagger`
- OpenAPI JSON: `/swagger/v1/swagger.json`
- Scalar API Reference: `/scalar`

No Swagger ou Scalar, execute `POST /api/Auth/login`, copie o token e use a autenticacao Bearer com:

```text
Bearer {token}
```
