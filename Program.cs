using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using ESCOLA_API.Data;
using ESCOLA_API.Logging;
using ESCOLA_API.Models;
using ESCOLA_API.Services;
using ESCOLA_API.Swagger;
using ESCOLA_API.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
ConfigureHttpPort(builder);
var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDirectory);

builder.Logging.AddDailyFileLogger(options =>
{
    options.DirectoryPath = logDirectory;
    options.FileNamePrefix = "escola-api";
    options.MinimumLevel = LogLevel.Information;
});

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UsuarioCreateViewModelValidator>();
builder.Services.AddCors();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICadernetaDigitalService, CadernetaDigitalService>();
builder.Services.AddScoped<IDisciplinaEventoService, DisciplinaEventoService>();
builder.Services.AddScoped<ICalendarioEscolarService, CalendarioEscolarService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
builder.Services.AddScoped<IUsuarioArquivoService, UsuarioArquivoService>();
builder.Services.AddScoped<IAlunoQrCodeBancarioService, AlunoQrCodeBancarioService>();
builder.Services.AddScoped<IHoleriteService, HoleriteService>();
builder.Services.AddScoped<IUsuarioArquivoStorage>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    return IsAzureBlobStorageProvider(configuration)
        ? ActivatorUtilities.CreateInstance<AzureBlobUsuarioArquivoStorage>(serviceProvider)
        : ActivatorUtilities.CreateInstance<LocalUsuarioArquivoStorage>(serviceProvider);
});
builder.Services.AddSingleton<ICadernetaDigitalEventPublisher, ServiceBusCadernetaDigitalEventPublisher>();
builder.Services.AddHostedService<ServiceBusNotificacaoWorker>();

var jwtKey = ResolveJwtKey(builder.Environment, builder.Configuration);
builder.Configuration["Jwt:Key"] = jwtKey;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Professor", policy => policy.RequireRole("Administrador", "Professor"));
    options.AddPolicy("Aluno", policy => policy.RequireRole("Administrador", "Professor", "Aluno"));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ESCOLA_API - Escola High Tech",
        Version = "v1",
        Description = "API para gerenciamento escolar com usuarios, caderneta digital, disciplinas, agenda de avaliacoes e trabalhos com notificacoes aos alunos, calendario escolar, QR Code bancario ficticio para alunos, holerites de funcionarios, notificacoes, uploads de arquivos e autenticacao JWT."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT retornado em /api/Auth/login. Exemplo: Bearer {seu_token}"
    });

    options.OperationFilter<AuthorizeOperationFilter>();

    var xmlFileName = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

    if (File.Exists(xmlFilePath))
    {
        options.IncludeXmlComments(xmlFilePath);
    }
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao configurada.");
}

builder.Services.AddDbContext<DataContext>(options =>
{
    if (IsSqliteConnectionString(connectionString))
    {
        options.UseSqlite(connectionString);
        return;
    }

    options.UseSqlServer(connectionString, sqlServerOptions =>
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null));
});

var app = builder.Build();
app.Logger.LogInformation(
    "ESCOLA_API iniciado. Ambiente: {Environment}. Diretorio de logs: {LogDirectory}",
    app.Environment.EnvironmentName,
    logDirectory);

// Apply pending migrations at startup (ensures database schema exists)
using (var scope = app.Services.CreateScope())
{
    var migrationLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("ESCOLA_API.Migrations");

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        migrationLogger.LogInformation(
            "Preparando banco de dados com provider {ProviderName}",
            db.Database.ProviderName);

        if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            db.Database.EnsureCreated();
        }
        else
        {
            db.Database.Migrate();
        }

        await EnsurePerfisSistemaAsync(db);
        migrationLogger.LogInformation("Banco de dados pronto.");
    }
    catch (Exception ex)
    {
        migrationLogger.LogCritical(ex, "Falha ao preparar banco de dados.");
        throw;
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "ESCOLA_API - Swagger";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ESCOLA_API v1");
});
app.MapScalarApiReference(options =>
{
    options.WithTitle("ESCOLA_API - Escola High Tech");
    options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
    options.AddPreferredSecuritySchemes(new[] { "Bearer" });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (!IsAzureBlobStorageProvider(app.Configuration))
{
    var uploadRoot = ResolveUploadRoot(app.Environment, app.Configuration);
    Directory.CreateDirectory(uploadRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadRoot),
        RequestPath = "/uploads"
    });
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.Use(async (context, next) =>
{
    var requestLogger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("ESCOLA_API.Requests");
    var startedAt = DateTimeOffset.UtcNow;
    var path = context.Request.Path.HasValue ? context.Request.Path.Value : "/";
    var userName = context.User.Identity?.IsAuthenticated == true
        ? context.User.Identity.Name ?? "usuario-autenticado"
        : "anonimo";

    requestLogger.LogInformation(
        "Requisicao iniciada {Method} {Path} por {UserName}",
        context.Request.Method,
        path,
        userName);

    try
    {
        await next();

        requestLogger.LogInformation(
            "Requisicao finalizada {Method} {Path} com status {StatusCode} em {ElapsedMilliseconds}ms",
            context.Request.Method,
            path,
            context.Response.StatusCode,
            (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
    }
    catch (Exception ex)
    {
        requestLogger.LogError(
            ex,
            "Requisicao falhou {Method} {Path} em {ElapsedMilliseconds}ms",
            context.Request.Method,
            path,
            (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
        throw;
    }
});
app.UseAuthorization();
app.MapControllers();
app.Run();

static string ResolveJwtKey(IHostEnvironment environment, IConfiguration configuration)
{
    var jwtKey = configuration["Jwt:Key"];
    if (!string.IsNullOrWhiteSpace(jwtKey))
    {
        return ValidateJwtKey(jwtKey);
    }

    if (!environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "Jwt:Key nao configurado. Defina a variavel de ambiente Jwt__Key ou configure um secret no provedor de deploy.");
    }

    var localDirectory = Path.Combine(environment.ContentRootPath, ".local");
    var localKeyPath = Path.Combine(localDirectory, "jwt.key");

    if (File.Exists(localKeyPath))
    {
        return ValidateJwtKey(File.ReadAllText(localKeyPath).Trim());
    }

    Directory.CreateDirectory(localDirectory);
    var generatedKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    File.WriteAllText(localKeyPath, generatedKey);
    return generatedKey;
}

static void ConfigureHttpPort(WebApplicationBuilder builder)
{
    var port = Environment.GetEnvironmentVariable("PORT");
    if (string.IsNullOrWhiteSpace(port))
    {
        return;
    }

    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

static bool IsSqliteConnectionString(string connectionString)
{
    var builder = new DbConnectionStringBuilder
    {
        ConnectionString = connectionString
    };

    var dataSource = GetConnectionStringValue(builder, "Data Source")
        ?? GetConnectionStringValue(builder, "DataSource")
        ?? GetConnectionStringValue(builder, "Filename");

    if (string.IsNullOrWhiteSpace(dataSource))
    {
        return false;
    }

    var normalizedDataSource = dataSource.Trim();
    if (normalizedDataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    var extension = Path.GetExtension(normalizedDataSource);
    return extension.Equals(".db", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".sqlite", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".sqlite3", StringComparison.OrdinalIgnoreCase);
}

static string? GetConnectionStringValue(DbConnectionStringBuilder builder, string key)
{
    return builder.TryGetValue(key, out var value) ? value?.ToString() : null;
}

static bool IsAzureBlobStorageProvider(IConfiguration configuration)
{
    return configuration["Uploads:Provider"]?.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase) == true;
}

static string ResolveUploadRoot(IHostEnvironment environment, IConfiguration configuration)
{
    var configured = configuration["Uploads:RootPath"];
    if (!string.IsNullOrWhiteSpace(configured))
    {
        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured);
    }

    return Path.Combine(environment.ContentRootPath, "uploads");
}

static string ValidateJwtKey(string jwtKey)
{
    if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    {
        throw new InvalidOperationException("Jwt:Key deve ter ao menos 32 bytes.");
    }

    return jwtKey;
}

static async Task EnsurePerfisSistemaAsync(DataContext db)
{
    var perfis = new[]
    {
        (PerfilSistema.AdministradorId, PerfilSistema.Administrador),
        (PerfilSistema.ProfessorId, PerfilSistema.Professor),
        (PerfilSistema.AlunoId, PerfilSistema.Aluno)
    };

    foreach (var (idPerfil, descricaoPerfil) in perfis)
    {
        var perfil = await db.Perfis.FirstOrDefaultAsync(p => p.IdPerfil == idPerfil);

        if (perfil != null && perfil.DescricaoPerfil != descricaoPerfil)
        {
            perfil.DescricaoPerfil = descricaoPerfil;
        }
    }

    await db.SaveChangesAsync();
}
