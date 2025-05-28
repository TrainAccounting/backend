using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Trainacc.Data;
using Trainacc.Filters;
using Trainacc.Services;
using System.Security.Claims;
using Trainacc.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Financial API",
        Version = "v1",
        Description = "API for financial management system at home",
        Contact = new OpenApiContact { Name = "Help Ilya!", Email = "igo36413@gmail.com" }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
    c.OperationFilter<Trainacc.Filters.ParamDescriptionFilter>();
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<ETagFilter>();
builder.Services.AddScoped<ValidateModelAttribute>();
builder.Services.AddScoped<RoleBasedAuthFilter>(_ =>
    new RoleBasedAuthFilter("Admin"));
builder.Services.AddScoped<Trainacc.Services.UsersService>();
builder.Services.AddScoped<Trainacc.Services.AccountsService>();
builder.Services.AddScoped<Trainacc.Services.CreditsService>();
builder.Services.AddScoped<Trainacc.Services.DepositsService>();
builder.Services.AddScoped<Trainacc.Services.RestrictionsService>();
builder.Services.AddScoped<Trainacc.Services.TransactionsService>(provider =>
{
    var db = provider.GetRequiredService<AppDbContext>();
    var restrictions = provider.GetRequiredService<Trainacc.Services.RestrictionsService>();
    return new Trainacc.Services.TransactionsService(db, restrictions);
});
builder.Services.AddScoped<Trainacc.Services.RecordsService>();
builder.Services.AddScoped<Trainacc.Services.AuthService>();

builder.Services.AddCors(option =>
{
    option.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5175");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Financial API V1");
        c.RoutePrefix = "swagger";
        c.InjectJavascript("/swagger-authtoken.js");
    });
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<Trainacc.Middleware.ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
