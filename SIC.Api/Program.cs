using SIC.Api.Services;
using SIC.Api.Services.Admin;
using SIC.Api.Services.PrePedidosPDF;
using SIC.Domain.Abstractions;
using SIC.Domain.Abstractions.Admin;
using SIC.Domain.Abstractions.PrePedidosPDF;
using SIC.Infrastructure.Integrations;
using SIC.Infrastructure.Integrations.PrePedidosPDF;
using SIC.Infrastructure.Repositories;
using SIC.Infrastructure.Repositories.Admin;
using SIC.Infrastructure.Repositories.PrePedidosPDF;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ISicAuthService, SicAuthService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IAuthRepository, SqlAuthRepository>();
builder.Services.AddScoped<IUserProfileRepository, SqlUserProfileRepository>();
builder.Services.AddScoped<IOrderSearchRepository, SqlOrderSearchRepository>();
builder.Services.AddScoped<IProductCatalogRepository, SqlProductCatalogRepository>();
builder.Services.AddScoped<IClientRepository, SqlClientRepository>();
builder.Services.AddScoped<IHomeRepository, SqlHomeRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IOrderSearchService, OrderSearchService>();
builder.Services.AddScoped<IProductCatalogService, ProductCatalogService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IPrePedidoPDFQueryRepository, SqlPrePedidoPDFQueryRepository>();
builder.Services.AddScoped<IPrePedidoPDFCommandRepository, SqlPrePedidoPDFCommandRepository>();
builder.Services.AddScoped<IPrePedidoPDFQueryService, PrePedidoPDFQueryService>();
builder.Services.AddScoped<IPrePedidoPDFCommandService, PrePedidoPDFCommandService>();
builder.Services.AddHttpClient<IPrePedidoPDFIntegrationService, PrePedidoPDFIntegrationService>();

// Liberação de Pedidos
builder.Services.AddScoped<ILiberacaoPedidoRepository, SqlLiberacaoPedidoRepository>();
builder.Services.AddScoped<ILiberacaoPedidoDetalheRepository, SqlLiberacaoPedidoDetalheRepository>();
builder.Services.AddScoped<ILiberacaoPedidoQueryRepository, SqlLiberacaoPedidoQueryRepository>();
builder.Services.AddScoped<ILiberacaoPedidoCommandRepository, SqlLiberacaoPedidoCommandRepository>();
builder.Services.AddScoped<ILiberacaoPedidoItemCommandRepository, SqlLiberacaoPedidoItemCommandRepository>();
builder.Services.AddScoped<ILiberacaoPedidoService, LiberacaoPedidoService>();
builder.Services.AddScoped<ILiberacaoPedidoAcoesService, LiberacaoPedidoAcoesService>();

// Permissões (genérico — usado por várias telas)
builder.Services.AddScoped<IPermissaoRepository, SqlPermissaoRepository>();
builder.Services.AddScoped<IPermissaoService, PermissaoService>();

// Admin
builder.Services.AddScoped<IAdminNoticeRepository, SqlAdminNoticeRepository>();
builder.Services.AddScoped<IAdminNoticeService, AdminNoticeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SIC API");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
