using SIC.Domain.Abstractions.Abreviacoes;
using SIC.Infrastructure.Repositories.Abreviacoes;
using SIC.Api.Services;
using SIC.Api.Services.Admin;
using SIC.Api.Services.Cotacao;
using SIC.Api.Services.PrePedidosPDF;
using SIC.Api.Services.Propostas;
using SIC.Domain.Abstractions;
using SIC.Domain.Abstractions.Admin;
using SIC.Domain.Abstractions.Categorizacao;
using SIC.Domain.Abstractions.Cotacao;
using SIC.Domain.Abstractions.PrePedidosPDF;
using SIC.Domain.Abstractions.Propostas;
using SIC.Infrastructure.Integrations;
using SIC.Infrastructure.Integrations.PrePedidosPDF;
using SIC.Infrastructure.Repositories;
using SIC.Infrastructure.Repositories.Admin;
using SIC.Infrastructure.Repositories.Categorizacao;
using SIC.Infrastructure.Repositories.Cotacao;
using SIC.Infrastructure.Repositories.PrePedidosPDF;
using SIC.Infrastructure.Repositories.Propostas;


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

// Admin
builder.Services.AddScoped<IAdminNoticeRepository, SqlAdminNoticeRepository>();
builder.Services.AddScoped<IAdminNoticeService, AdminNoticeService>();
builder.Services.AddScoped<IPropostaQueryRepository, SqlPropostaQueryRepository>();
builder.Services.AddScoped<IPropostaQueryService, PropostaQueryService>();
builder.Services.AddHostedService<CodificacaoBackgroundService>();
builder.Services.AddScoped<ICotacaoQueryRepository, SqlCotacaoQueryRepository>();
builder.Services.AddScoped<ICotacaoQueryService, CotacaoQueryService>();
builder.Services.AddScoped<ICotacaoCommandRepository, SqlCotacaoCommandRepository>();
builder.Services.AddScoped<ICotacaoCommandService, CotacaoCommandService>();
builder.Services.AddScoped<ICategorizacaoRepository, SqlCategorizacaoRepository>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAbreviacaoRepository, SqlAbreviacaoRepository>();

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
