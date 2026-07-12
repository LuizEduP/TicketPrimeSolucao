using billet_2.Components;
using billet_2.Services; // Adicionado: Para reconhecer suas pastas de serviço

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents(); // Mantendo o seu suporte a Server Mode

// --- ADICIONADO DO GIT: CONFIGURAÇÃO DA API ---
builder.Services.AddScoped(dp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5289") // O endereço da API do cara do backend
});

// --- ADICIONADO DO GIT: REGISTRO DOS SERVIÇOS ---
builder.Services.AddScoped<ViagemService>();
builder.Services.AddScoped<VeiculoService>();
builder.Services.AddScoped<AssentoService>();
builder.Services.AddScoped<PassagemService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddSingleton<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddInteractiveServerRenderMode(); // Mantendo o seu suporte a Server Mode

app.Run();