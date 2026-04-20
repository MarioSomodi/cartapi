using Cart.Api.Configuration;
using Cart.Api.Middleware;
using Cart.Api.Security;
using Cart.Application;
using Cart.Application.Abstractions.Auth;
using Cart.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetailsSupport();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddApiVersioningSupport();
builder.Services.AddApplication();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, HttpRequestContext>();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCartHealthChecks();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment() && app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
{
    await app.Services.ApplyDatabaseMigrationsAsync();
}

app.UseSwaggerDocumentation();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

public partial class Program;
