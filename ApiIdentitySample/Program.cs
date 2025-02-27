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
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(optionswag =>
{
    optionswag.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ApiIdentity Sample API",
        Description = "Exemple d'application Blazor .net8 + Identity",
        Contact = new OpenApiContact
        {
            Name = "Contact",
            Url = new Uri("https://www.cognitic.be/contactez-nous")
        }


    });
    
    //Jwt
    // Bearer token authentication


    optionswag.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    optionswag.OperationFilter<AddAuthHeaderOperationFilter>();


});

/*Configuration de Identity EF*/
string connectionString = builder.Configuration["ConnectionStrings:Default"];
/*Inject le dbcontext*/
builder.Services.AddDbContext<IdentitySampleDbContext>(options => { options.UseSqlServer(connectionString); });
/*Inject de l'Identity+  configuration du store pour les rôle, les users,...*/
builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentitySampleDbContext>();

builder.Services
    .AddAuthentication(options =>
    {
        // Utiliser Identity comme schéma par défaut
        options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
        options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        options.DefaultSignInScheme = IdentityConstants.BearerScheme;
        options.DefaultScheme = IdentityConstants.BearerScheme;
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
 

    // Ajouter des politiques personnalisées si nécessaire
    options.AddPolicy("RequireAdministratorRole", policy =>
          policy.RequireRole("Administrator"));
});
/*Configuration des cors*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("DevPolicy");
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapIdentityApi<ApplicationUser>().AllowAnonymous(); 
app.MapEntraIdEndpoints();

app.MapControllers();

app.Run();
