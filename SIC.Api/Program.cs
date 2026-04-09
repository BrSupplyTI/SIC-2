using SIC.Api.Services;
using SIC.Api.Services.PrePedidosPDF;
using SIC.Domain.Abstractions;
using SIC.Domain.Abstractions.PrePedidosPDF;
using SIC.Infrastructure.Integrations;
using SIC.Infrastructure.Integrations.PrePedidosPDF;
using SIC.Infrastructure.Repositories;
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
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IOrderSearchService, OrderSearchService>();
builder.Services.AddScoped<IProductCatalogService, ProductCatalogService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IPrePedidoPDFQueryRepository, SqlPrePedidoPDFQueryRepository>();
builder.Services.AddScoped<IPrePedidoPDFCommandRepository, SqlPrePedidoPDFCommandRepository>();
builder.Services.AddScoped<IPrePedidoPDFQueryService, PrePedidoPDFQueryService>();
builder.Services.AddScoped<IPrePedidoPDFCommandService, PrePedidoPDFCommandService>();
builder.Services.AddHttpClient<IPrePedidoPDFIntegrationService, PrePedidoPDFIntegrationService>();

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
