using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using SignaliteWebAPI.Application.Extensions;
using SignaliteWebAPI.Extensions;
using SignaliteWebAPI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApiServices();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddInfrastructureServices(builder.Configuration); // extension function
builder.Services.AddApplicationServices(); // extension function
builder.Services.AddIdentityServices(builder.Configuration); // extension function (configures bearer)


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Signalite APi")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
           
    });
}

app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) 
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authSchemes.Any(authscheme => authscheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = securityScheme;

            // Apply Bearer Authentication globally to all endpoints
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [securityScheme] = new List<string>() // No specific scopes
            });
        }
    }
}