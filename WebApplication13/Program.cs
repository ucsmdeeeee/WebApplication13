using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Logging.ClearProviders(); // �������� ������� �����������
builder.Logging.AddConsole(); // �������� ����������� � �������
builder.Logging.AddDebug(); // ���� � ��������� (Debug Output)

// ��������� ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� �������������� � JWT
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

// ���������� ��������� ������������ � ��������������� (MVC)
builder.Services.AddControllersWithViews();

// ���������� Razor Pages (���� ����������)
builder.Services.AddRazorPages();

// ���������� Quartz ��� ������������ �����
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory(); // ������������� DI � Quartz
});
builder.Services.AddQuartzHostedService();

// ���������� Swagger ��� API-������������
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ���������� ���������� ����������
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

// ������������ ��� ������ ����������
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // ��������� ������ ��� �������������
    app.UseSwagger(); // ��������� Swagger
    app.UseSwaggerUI();
}

// ��������� ����������� ������
app.UseStaticFiles();

// Middleware ��� ����������� �������� � �������
app.Use(async (context, next) =>
{
    Console.WriteLine($"Incoming Request: {context.Request.Method} {context.Request.Path}");
    await next(); // �������� ������� ������
    Console.WriteLine($"Outgoing Response: {context.Response.StatusCode}");
});

// ��������� ���������
app.UseRouting();

app.UseAuthentication(); // Middleware ��� ��������������
app.UseAuthorization(); // Middleware ��� �����������

// ��������� ��������� ��� ������������ � ���������������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ��������� ��������� ��� Razor Pages
app.MapRazorPages();

app.Run();
