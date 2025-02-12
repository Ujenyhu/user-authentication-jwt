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

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

#region repositories and services

builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.AddScoped<IServicesWrapper, ServicesWrapper>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

#endregion

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



app.UseHttpsRedirection();

//Add Middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();
app.UseCors("AllowAllHeaders");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

await app.RunAsync();