using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Notes.Identity;
using Notes.Identity.Data;
using Notes.Identity.Models;

var builder = WebApplication.CreateBuilder(args);
string env = builder.Environment.ContentRootPath;

var connectionString = builder.Configuration.GetValue<string>("DbConnection");

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

//добавл€етс€ Identity и конфигурируетс€ дл€ AppUser, IdentityRole
builder.Services.AddIdentity<AppUser, IdentityRole>(config =>
{
    //настройка требований к паролю
    config.Password.RequiredLength = 4;//об€зательна€ длина мин. 4 символа
    config.Password.RequireDigit = false;//отмен€ет требование к использованию цифр
    config.Password.RequireNonAlphanumeric = false;//отмен€ет требование к использованию не буквенно-цифровых символов
    config.Password.RequireUppercase = false;//отмен€ет требование к использованию заглавных букв
})
    .AddEntityFrameworkStores<AuthDbContext>()//добавл€ет DbContext как хранилище к Identity
    .AddDefaultTokenProviders();//добавл€ет дефолтные токен провайдеры дл€ получени€ и обновлени€ токенов доступа

//необходимый минимум зависимостей дл€ Identity server4
builder.Services.AddIdentityServer()//дл€ регистрации Identity server4 в контейнере внедрени€ зависимостей
    .AddAspNetIdentity<AppUser>()//добавл€етс€ AppUser дл€ AspNetIdentity
    .AddInMemoryApiResources(Configuration.ApiResources)//InMemory хранилище пам€ти дл€ ресурсов и пользователей
    .AddInMemoryIdentityResources(Configuration.IdentityResources)
    .AddInMemoryApiScopes(Configuration.ApiScopes)
    .AddInMemoryClients(Configuration.Clients)//InMemory хранилище пам€ти дл€ клиентов
    .AddDeveloperSigningCredential();//используетс€ демонстрационный сертификат подписи

//настройка использовани€ Cookie, чтобы там мы хранили текущее значение токена
builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "Notes.Identity.Cookie";//задаетс€ им€ Cookie
    config.LoginPath = "/Auth/Login";//путь дл€ логина пользовател€
    config.LogoutPath = "/Auth/Logout";//путь дл€ логаута пользовател€
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var context = serviceProvider.GetRequiredService<AuthDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception exeption)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(exeption, "An error occurred while app initialization");
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(env, "Styles")),
    RequestPath = "/styles"
});
app.UseRouting();

//этот middleware позвол€ет Identity server начать обработку маршрутизации дл€ конечных точек
//OAuth и OpenID Connect, таких как конечные точки авторизации и токена
app.UseIdentityServer();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();//маппинг роутинга по имени контроллеров
});

app.Run();
