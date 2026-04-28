using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderAPI.DAL;
using OrderAPI.DAL.Storage.GetSeatStatuses;
using OrderAPI.Domain.Services;
using OrderAPI.Domain.UseCases.GetCart;
using OrderAPI.Middleware;
using System.Text;

namespace OrderAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<Program>();
                cfg.RegisterServicesFromAssemblyContaining<GetCartRequestHandler>();
            });

            builder.Services.AddStorage(builder.Configuration.GetConnectionString("DefaultConnection")!)
                .AddServices();

            builder.Services.AddScoped<IGetSeatStatusesStorage, GetSeatStatusesStorage>();

            builder.Services.AddHttpClient("PaymentApi", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["PaymentApi:BaseUrl"]!);
            });
            builder.Services.AddScoped<IPaymentApiClient, PaymentApiClient>();
            builder.Services.AddScoped<IPaymentApiClient, PaymentApiClient>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}