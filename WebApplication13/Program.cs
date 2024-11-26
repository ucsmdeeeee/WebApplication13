using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
builder.Logging.ClearProviders(); // Очистить текущих провайдеров
builder.Logging.AddConsole(); // Добавить логирование в консоль
builder.Logging.AddDebug(); // Логи в отладчике (Debug Output)

// Настройка базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка аутентификации с JWT
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

// Добавление поддержки контроллеров с представлениями (MVC)
builder.Services.AddControllersWithViews();

// Добавление Razor Pages (если необходимо)
builder.Services.AddRazorPages();

// Добавление Quartz для планирования задач
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory(); // Использование DI в Quartz
});
builder.Services.AddQuartzHostedService();

// Добавление Swagger для API-документации
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Глобальный обработчик исключений
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            Console.WriteLine($"Global Exception: {exceptionHandlerPathFeature.Error}");
        }
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

// Конфигурация для режима разработки
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Подробные ошибки для разработчиков
    app.UseSwagger(); // Включение Swagger
    app.UseSwaggerUI();
}

// Поддержка статических файлов
app.UseStaticFiles();

// Middleware для логирования запросов и ответов
app.Use(async (context, next) =>
{
    Console.WriteLine($"Incoming Request: {context.Request.Method} {context.Request.Path}");
    await next(); // Передача запроса дальше
    Console.WriteLine($"Outgoing Response: {context.Response.StatusCode}");
});

// Настройка маршрутов
app.UseRouting();

app.UseAuthentication(); // Middleware для аутентификации
app.UseAuthorization(); // Middleware для авторизации

// Настройка маршрутов для контроллеров с представлениями
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Настройка маршрутов для Razor Pages
app.MapRazorPages();

app.Run();
