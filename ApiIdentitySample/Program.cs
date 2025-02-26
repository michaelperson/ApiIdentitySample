using ApiIdentitySample.Data;
using ApiIdentitySample.IdentityCustom;
using ApiIdentitySample.Services;
using ApiIdentitySample.Tools;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*Configuration de Identity EF*/
string connectionString = builder.Configuration["ConnectionStrings:Default"];
/*Inject le dbcontext*/
builder.Services.AddDbContext<IdentitySampleDbContext>(options => { options.UseSqlServer(connectionString); });
/*Inject de l'Identity+  configuration du store pour les rôle, les users,...*/
//builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentitySampleDbContext>();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>().AddEntityFrameworkStores<IdentitySampleDbContext>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

int minLenght = int.Parse(builder.Configuration["IdentityOptions:MinPasswordLenght"]);
builder.Services.Configure<IdentityOptions>(
    o=>
    {
        o.Password.RequiredLength = minLenght;
        o.Password.RequireDigit=true;
        o.Password.RequireNonAlphanumeric=true;
        o.Password.RequireLowercase=true;
        o.Password.RequireUppercase=true;

        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        o.Lockout.MaxFailedAccessAttempts = 5;

        o.SignIn.RequireConfirmedEmail = true;
        o.User.RequireUniqueEmail = true;

        o.Tokens.AuthenticatorIssuer = builder.Configuration["IdentityOptions:Issuer"]; 
       
    }
    );
builder.Services.AddScoped<EntraIdAccountService>();

// Configurer les événements OpenID Connect
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var existingOnTokenValidatedHandler = options.Events.OnTokenValidated;

    options.Events.OnTokenValidated = async context =>
    {
        // Appeler d'abord le gestionnaire existant
        if (existingOnTokenValidatedHandler != null)
        {
            await existingOnTokenValidatedHandler(context);
        }

        // Obtenir le service de gestion des comptes Entra ID
        var accountService = context.HttpContext.RequestServices.GetRequiredService<EntraIdAccountService>();

        // Gérer l'utilisateur
        await accountService.ProcessEntraIdUserAsync(context);
    };
});
//Injecter l'outils pour envoyer un email
builder.Services.AddTransient<IEmailSender, EmailSender>(s=> new EmailSender(builder.Configuration["SendGrid:ApiKey"]));

/*Ajout de l'authorization*/
builder.Services.AddAuthorization(options =>
{
    // Politique par défaut pour les endpoints nécessitant une authentification
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Ajouter des politiques personnalisées si nécessaire
    options.AddPolicy("RequireAdministratorRole", policy =>
          policy.RequireRole("Administrator"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapIdentityApi<ApplicationUser>();
app.MapEntraIdEndpoints();
app.MapControllers();

app.Run();
