using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Registra DbConnectionFactory para Dapper + PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddSingleton(new DbConnectionFactory(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5096")
            .AllowAnyHeader()
            .AllowAnyMethod();                                
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseCors("BlazorPolicy");
app.CadastrarUsuarios();
app.ListarUsuarios();
app.CadastrarViagens();
app.ListarViagens();
app.ListarViagemPorId();
app.PesquisarViagens();
app.CadastrarVeiculos();
app.ListarVeiculos();
app.ListarVeiculoPorId();
app.MapaAssentos();
app.ReservarAssento();
app.LiberarAssento();
app.BloquearAssento();
app.ListarPassagens();
app.ListarPassagensPorUsuario();
app.ComprarPassagem();
app.CancelarPassagem();
app.ListarPassagensDetalhadas();
app.ListarPassagensDetalhadasPorUsuario();
app.CadastrarEventos();
app.ListarEventos();
app.ListarEventoPorId();
app.CadastrarCupons();
app.ListarCupons();
app.UseHttpsRedirection();

app.Run();

