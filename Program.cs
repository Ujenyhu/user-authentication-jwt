using userauthjwt.Helpers;
using userauthjwt.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.BusinessLogic.Services.User;
using userauthjwt.DataAccess.Repositories;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using userauthjwt.Responses;
using userauthjwt.Middlewares.Exceptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using userauthjwt.Middlewares.Maintenance;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using userauthjwt.BusinessLogic.Interfaces.User;
using userauthjwt.Middlewares.Authorization;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllHeaders", builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = AuthenticationService.Issuer,
        ValidAudience = AuthenticationService.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["AppSettings:Secret"]))

    };
});


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:mssqlConnectionString"],
    sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    });
});


builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(m => m.Errors.Select(e => e.ErrorMessage))
            .ToList();

        var response = new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Validation error(s) occurred", VarHelper.ResponseStatus.ERROR.ToString())
        {
            Message = String.Join(", ", errors)
        };

        return new JsonResult(response)
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            ContentType = "application/json",
        };
    };
});

builder.Services.AddEndpointsApiExplorer();

//Add Distributed Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redisConnectionString");
    options.InstanceName = "UserAuthJwt_";
});


//Enable Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(htppContext =>
       RateLimitPartition.GetFixedWindowLimiter(
          partitionKey: htppContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
           windowOptions => new FixedWindowRateLimiterOptions
           {
               PermitLimit = 3,
               Window = TimeSpan.FromSeconds(10)
           })

    );
    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var response = new ResponseBase<object>((int)HttpStatusCode.TooManyRequests, "Too many requests. Please try again later.",
            VarHelper.ResponseStatus.ERROR.ToString());

        await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response), cancellationToken);
    };
});




//------------------Swagger Documentation Section-----------------------
var contact = new OpenApiContact()
{
    Name = "",
    Email = "",
};


var info = new OpenApiInfo()
{
    Version = "v1",
    Title = "User Authentication Service",
    Contact = contact
};


builder.Services.AddSwaggerGen(c =>
{

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.SwaggerDoc("v1", info);


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
               {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
               });
});



#region repositories and services

builder.Services.AddLogging();
builder.Services.AddHealthChecks();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.TryAddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.TryAddScoped<IServicesWrapper, ServicesWrapper>();
builder.Services.TryAddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.TryAddSingleton<ICacheService, CacheService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
//builder.Services.TryAddSingleton<SoftDeleteInterceptor>();
builder.Services.TryAddTransient<SystemCheckMiddleware>();
builder.Services.TryAddTransient<ExceptionHandlingMiddleware>();

#endregion


//builder.Services.AddMassTransit(busConfig =>
//{
//    busConfig.UsingRabbitMq((context, rbmqConfig) =>
//    {
//        rbmqConfig.Host("rabbitmq://localhost", hostConfig =>
//        {
//            hostConfig.Username("guest");
//             hostConfig.Password("guest");
//        });

//        rbmqConfig.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)));
//    });
//});


var app = builder.Build();


app.UseSwagger(u =>
{
u.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseHsts();

//Add Middlewares
app.UseMiddleware<SystemCheckMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();
app.UseCors("AllowAllHeaders");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

await app.RunAsync();