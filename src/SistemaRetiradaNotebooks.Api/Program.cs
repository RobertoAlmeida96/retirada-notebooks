using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaRetiradaNotebooks.Api.Data;
using SistemaRetiradaNotebooks.Api.Models;
using SistemaRetiradaNotebooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (port != null)
    builder.WebHost.UseUrls($"http://+:{port}");

var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("Connection string não configurada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<TokenService>();

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok());

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!await db.Usuarios.AnyAsync(u => u.Perfil == PerfilUsuario.Admin))
    {
        // E-mail e senha do admin inicial vêm da configuração/variáveis de ambiente.
        // Nunca deixe senha fixa no código. Defina Admin:Email e Admin:Senha (ou as
        // variáveis Admin__Email / Admin__Senha) no ambiente de produção.
        var adminEmail = builder.Configuration["Admin:Email"] ?? "admin@sistema.local";
        var adminSenha = builder.Configuration["Admin:Senha"]
            ?? throw new InvalidOperationException(
                "Senha do admin inicial não configurada. Defina 'Admin:Senha' (ou a variável de ambiente Admin__Senha).");

        var admin = new Usuario
        {
            Nome = "Administrador",
            Email = adminEmail,
            SenhaHash = "",
            Perfil = PerfilUsuario.Admin,
            Status = StatusCadastro.Ativo
        };
        admin.SenhaHash = new PasswordHasher<Usuario>().HashPassword(admin, adminSenha);
        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();
    }

    await DbSeeder.SeedAsync(db);
}

app.Run();
