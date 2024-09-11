using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using CompanyEmployees.Presentation.Controllers;
using Contracts;
using Entities.ConfigurationModels;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Service;
using Services.Contracts;


namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services )=>
         services.AddCors(options => 
         {
            options.AddPolicy("CorsPolicy", builder => 
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Pagination"));
         });


         public static void ConfigureIISIntegration(this IServiceCollection services)
      {
         services.Configure<IISOptions>(options =>
         {

         });
      }

      public static void ConfigureLoggerService(this IServiceCollection service)
      {
         service.AddSingleton<ILoggerManager, LoggerManager>();
      }

      public static void ConfigureRepositoryManager(this IServiceCollection services)
      => services.AddScoped<IRepositoryManager, RepositoryManager>();

      public static void ConfigureServiceManager(this IServiceCollection services) => 
      services.AddScoped<IServiceManager, ServiceManager>();

      public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) => 
      services.AddDbContext<RepositoryContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

      public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
      builder.AddMvcOptions(config => config.OutputFormatters.Add(new
      CsvOutputFormatter()));

      public static void AddCustomMediaTypes(this IServiceCollection services)
      {
         services.Configure<MvcOptions>(config => 
         {
            var systemTextJsonOutputFormatter = config.OutputFormatters
            .OfType<SystemTextJsonOutputFormatter>()
            .FirstOrDefault();


            if(systemTextJsonOutputFormatter != null)
            {
               systemTextJsonOutputFormatter.SupportedMediaTypes
               .Add("application/vnd.codemaze.hateoas+json");
               systemTextJsonOutputFormatter.SupportedMediaTypes
               .Add("application/vnd.codemaze.apiroot+json");
            }

            var xmlOutputFormatter = config.OutputFormatters
            .OfType<XmlDataContractSerializerOutputFormatter>()?
            .FirstOrDefault();

            if (xmlOutputFormatter != null)
            {
               xmlOutputFormatter.SupportedMediaTypes
               .Add("application/vnd.codemaze.hateoas+xml");
               xmlOutputFormatter.SupportedMediaTypes
               .Add("application/vnd.codemaze.apiroot+xml");
            }

         });
      }

      public static void ConfigureApiVersioning(this IServiceCollection services)
      {
         services.AddApiVersioning(option => 
         {
            option.ReportApiVersions = true;
            option.AssumeDefaultVersionWhenUnspecified = true;
            option.DefaultApiVersion = new ApiVersion(1, 0);
             option.ApiVersionReader = ApiVersionReader.Combine(
                  new QueryStringApiVersionReader("api-version"),
                  new HeaderApiVersionReader("X-Version"),
                  new MediaTypeApiVersionReader("api-version"));
         }).AddMvc();
      }

       public static void ConfigureSwagger(this IServiceCollection services)
      {
         services.AddSwaggerGen(c =>
         {
               c.SwaggerDoc("v1", new OpenApiInfo { Title = "Companies API", Version = "v1" });
               
               
         });
      }

      public static void ConfigureOutputCaching(this IServiceCollection services) 
      => services.AddOutputCache(option =>
      {
         //option.AddBasePolicy(bp => bp.Expire(TimeSpan.FromSeconds(10)));
         option.AddPolicy("120SecondsDuration", p => p.Expire(TimeSpan.FromSeconds(120)));
      });    
      //services.AddResponseCaching();

      public static void ConfigureRateLimitingOptions(this IServiceCollection services)
      {
         services.AddRateLimiter(opt =>
         {
               opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context
               =>
               RateLimitPartition.GetFixedWindowLimiter("GlobalLimiter",
               partition => new FixedWindowRateLimiterOptions
               {
                  AutoReplenishment = true,
                  PermitLimit = 30,
                  QueueLimit = 2,
                  QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                  Window = TimeSpan.FromMinutes(1)
               }));
               //opt.RejectionStatusCode = 429;

               opt.AddPolicy("SpecificPolicy", context =>
               RateLimitPartition.GetFixedWindowLimiter("SpecificLimiter",
               partition => new FixedWindowRateLimiterOptions
               {
                  AutoReplenishment = true,
                  PermitLimit = 3,
                  Window = TimeSpan.FromSeconds(10)
               }));

               opt.OnRejected = async (context, token) =>
               {
                  context.HttpContext.Response.StatusCode = 429;
                  if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                  await context.HttpContext.Response
                  .WriteAsync($"Too many requests. Please try again after {retryAfter.TotalSeconds} second(s).", token);
                  else
                  await context.HttpContext.Response
                  .WriteAsync("Too many requests. Please try again later.", token);
               };

         });
      }

      public static void ConfigureIdentity(this IServiceCollection services)
      {
         var builder = services.AddIdentity<User, IdentityRole>(o =>
         {
            o.Password.RequireDigit = true;
            o.Password.RequireLowercase = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequiredLength = 10;
            o.User.RequireUniqueEmail = true;
         })
         .AddEntityFrameworkStores<RepositoryContext>()
         .AddDefaultTokenProviders();
      }

      public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
      {
         var jwtConfiguration = new JwtConfiguration();
         configuration.Bind(jwtConfiguration.Section, jwtConfiguration);

         // var jwtSettings =configuration.GetSection("JwtSettings");
          var secretKey = Environment.GetEnvironmentVariable("SECRET");

         services.AddAuthentication(option => 
         {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
         })
         .AddJwtBearer(option => 
         {
            option.TokenValidationParameters = new TokenValidationParameters
            {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = jwtConfiguration.ValidIssuer,
               ValidAudience = jwtConfiguration.ValidAudience,

               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
         });
      }
   }
            
}  

