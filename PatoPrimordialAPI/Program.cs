using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PatoPrimordialAPI.Infrastructure.Data;
using PatoPrimordialAPI.Services.Ingestao;
using PatoPrimordialAPI.Services.Analise;
using PatoPrimordialAPI.Services.Missoes;
using PatoPrimordialAPI.Services;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(
    opts => opts.UseNpgsql(connectionString),
        contextLifetime: ServiceLifetime.Scoped,
        optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<ApplicationDbContext>(opts =>
    opts.UseNpgsql(connectionString));

builder.Services.AddScoped<IIngestaoService, IngestaoService>();
builder.Services.AddScoped<IAnaliseParametrosService, AnaliseParametrosService>();
builder.Services.AddScoped<IAnaliseService, AnaliseService>();
builder.Services.AddSingleton<IMissaoCatalogoService, MissaoCatalogoService>();
builder.Services.AddSingleton<IMissaoService, MissaoService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
