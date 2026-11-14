
namespace Snowstorm.Snowmed.Csharp.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IEndpointUtil, EndpointUtil>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApiDocument();
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();

            var snowstormBaseUrl = builder.Configuration["Snowstorm:BaseUrl"]
                ?? throw new InvalidOperationException("Snowstorm:BaseUrl is not configured.");
            var snowstormBranch = builder.Configuration["Snowstorm:Branch"]
                ?? "MAIN";

            if (!Uri.TryCreate(snowstormBaseUrl, UriKind.Absolute, out var snowstormUri))
            {
                throw new InvalidOperationException("Snowstorm:BaseUrl must be an absolute URL.");
            }

            builder.Services.AddHttpClient<ISnowstormClient, SnowstormClient>(httpClient =>
            {
                httpClient.BaseAddress = snowstormUri;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "Snowstorm.Snowmed.Csharp.Client v1");
                });

            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapConceptEndpoints(snowstormBranch);

            app.Run();
        }
    }
}
