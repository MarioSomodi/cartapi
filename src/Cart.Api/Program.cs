using Cart.Api.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiVersioningSupport();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCartHealthChecks();

WebApplication app = builder.Build();

app.UseSwaggerDocumentation();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
