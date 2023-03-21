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

//����������� Identity � ��������������� ��� AppUser, IdentityRole
builder.Services.AddIdentity<AppUser, IdentityRole>(config =>
{
    //��������� ���������� � ������
    config.Password.RequiredLength = 4;//������������ ����� ���. 4 �������
    config.Password.RequireDigit = false;//�������� ���������� � ������������� ����
    config.Password.RequireNonAlphanumeric = false;//�������� ���������� � ������������� �� ��������-�������� ��������
    config.Password.RequireUppercase = false;//�������� ���������� � ������������� ��������� ����
})
    .AddEntityFrameworkStores<AuthDbContext>()//��������� DbContext ��� ��������� � Identity
    .AddDefaultTokenProviders();//��������� ��������� ����� ���������� ��� ��������� � ���������� ������� �������

//����������� ������� ������������ ��� Identity server4
builder.Services.AddIdentityServer()//��� ����������� Identity server4 � ���������� ��������� ������������
    .AddAspNetIdentity<AppUser>()//����������� AppUser ��� AspNetIdentity
    .AddInMemoryApiResources(Configuration.ApiResources)//InMemory ��������� ������ ��� �������� � �������������
    .AddInMemoryIdentityResources(Configuration.IdentityResources)
    .AddInMemoryApiScopes(Configuration.ApiScopes)
    .AddInMemoryClients(Configuration.Clients)//InMemory ��������� ������ ��� ��������
    .AddDeveloperSigningCredential();//������������ ���������������� ���������� �������

//��������� ������������� Cookie, ����� ��� �� ������� ������� �������� ������
builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "Notes.Identity.Cookie";//�������� ��� Cookie
    config.LoginPath = "/Auth/Login";//���� ��� ������ ������������
    config.LogoutPath = "/Auth/Logout";//���� ��� ������� ������������
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

//���� middleware ��������� Identity server ������ ��������� ������������� ��� �������� �����
//OAuth � OpenID Connect, ����� ��� �������� ����� ����������� � ������
app.UseIdentityServer();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();//������� �������� �� ����� ������������
});

app.Run();
