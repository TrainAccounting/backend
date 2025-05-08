using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trainacc API", Version = "v1" });
});

builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<ETagFilter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//app.MapGet("/", (AppDbContext db) => db.Users.ToList());
//app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();