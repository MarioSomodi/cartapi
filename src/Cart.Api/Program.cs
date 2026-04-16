using Cart.Api.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
builder.Services.AddApiVersioningSupport();
            builder.Services.AddOpenApi();
builder.Services.AddCartHealthChecks();

WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();

public partial class Program;
