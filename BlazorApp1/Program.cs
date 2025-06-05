using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlazorApp1.Components;
using BlazorApp1.Components.Account;
using BlazorApp1.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Runtime.ConstrainedExecution;
using BlazorApp1.Components.Account.Pages.Manage;
using System.Security.Cryptography.X509Certificates;
using BlazorApp1.Encryption;
using BlazorApp1.Data.Context;
using System.Security.Authentication;

namespace BlazorApp1;

public class Program
{
    public static void Main(string[] args)
    {
        // Checks if running on IIS
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_IIS_PHYSICAL_PATH")))
        {
            throw new InvalidOperationException("This cannot be run on IIS. Use Kestrel.");
        }

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseKestrel((context, serverOptins) =>
            serverOptins.Configure(context.Configuration.GetSection("Kestrel")).
            Endpoint("HTTPS", listenOptions => { listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12; })
        );

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();


        // Adds https
        builder.Services.AddHttpClient();
        string privateKeyBase64 = builder.Configuration["Encryption:PrivateKey"];




        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        builder.Services.AddSingleton<IHashingHandler,HashingHandler>();
        builder.Services.AddScoped<ISymmetricEncryptionService, SymmetricEncryptionService>();
        builder.Services.AddScoped<IAsymmetricEncryptionService>(sp =>
        {
            // Get an HttpClient using factory and create client
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = httpClientFactory.CreateClient();
            return new AsymmetricEncryptionService(privateKeyBase64, httpClient);
        });

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        // Register ApplicationDbContext for your identity and account data
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        var connectionStringTodo = builder.Configuration.GetConnectionString("TodoConnection")
    ?? throw new InvalidOperationException("Connection string 'TodoConnection' not found.");
        builder.Services.AddDbContext<TodoDbContext>(options =>
            options.UseSqlServer(connectionStringTodo));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        builder.Services.AddAuthorization(option =>
        {
            option.AddPolicy("AuthenticatedUser", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
            option.AddPolicy("AdministratorPolicy", policy =>
            {
                policy.RequireRole("Admin");
            });
        });

        var certificateSection = builder.Configuration.GetSection("Certificate");
        var certFileName = certificateSection["FileName"] ?? throw new Exception("Certificate file name is not configured.");
        var certFolder = certificateSection["Folder"] ?? ".aspnet/https";
        var certPassword = certificateSection["Password"] ?? throw new Exception("Certificate password is not configured.");

        // Create the full certificate path.
        var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        if (string.IsNullOrEmpty(userProfile))
        {
            throw new Exception("USERPROFILE environment not found.");
        }
        var certFilePath = Path.Combine(userProfile, certFolder.Replace("/", Path.DirectorySeparatorChar.ToString()), certFileName);

        var certificate = new X509Certificate2(certFilePath, certPassword);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ServerCertificate = certificate;
            });
        });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }



        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.Run();
    }
}
