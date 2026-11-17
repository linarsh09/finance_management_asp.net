using FinanceManagement.Data;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Repositories;
using FinanceManagement.Services;
using FinanceManagement.Factories;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Identity;
using FinanceManagement.Authentication;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FinanceDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".FinanceManagement.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services
    .AddAuthentication(SessionAuthenticationHandler.SchemeName)
    .AddScheme<
        AuthenticationSchemeOptions,
        SessionAuthenticationHandler>(
            SessionAuthenticationHandler.SchemeName,
            options => { });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<TransactionCreator, IncomeTransactionCreator>();
builder.Services.AddScoped<TransactionCreator, ExpenseTransactionCreator>();
builder.Services.AddScoped<ITransactionRepository,TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddSingleton<ICurrencyRules, CurrencyRules>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<
    IDatabaseInitializerRepository,
    DatabaseInitializerRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<IDatabaseInitializerRepository>();

    await initializer.InitializeAsync();
}

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.Run();
