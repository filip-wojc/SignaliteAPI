using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SignaliteWebAPI.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
      // set what to validate in token
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
             var tokenKey = config["TokenKey"] ?? throw new Exception("TokenKey not found");
             options.TokenValidationParameters = new TokenValidationParameters
             {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
             };

             // pass the token to signalR on connection ( without it, it cant get the token properly)
             options.Events = new JwtBearerEvents
             {
                OnMessageReceived = context =>
                {
                   var accessToken = context.Request.Query["access_token"];
                   var path = context.HttpContext.Request.Path;
                   if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs")) 
                   {
                     context.Token = accessToken;
                   }

                   return Task.CompletedTask;
                }
             };
          });

      return services;
   }
}