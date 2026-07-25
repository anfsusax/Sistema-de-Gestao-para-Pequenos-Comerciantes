using SalgaFacil.Infrastructure;
using SalgaFacil.Infrastructure.Data;
using SalgaFacil.Web.Components;
using SalgaFacil.Web.Contracts.Pagamentos;
using SalgaFacil.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSignalR(options =>
{
    // Permite upload de imagens de produto via InputFile (Blazor Server).
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClienteAuthService>();
builder.Services.AddScoped<IEmpresaContext, EmpresaContext>();
builder.Services.AddScoped<CarrinhoSessao>();
builder.Services.AddScoped<LojaPublicaService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<UnidadeMedidaService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<ProdutoImagemService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ClienteManutencaoService>();
builder.Services.AddScoped<PacoteService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<VendaService>();
builder.Services.AddScoped<CaixaService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CustosService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<IPagamentoPixService, PagamentoPixService>();

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
