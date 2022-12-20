using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Graph = Microsoft.Graph;
using BackchannelHttpHandlerIssue.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        var section = builder.Configuration.GetSection("AzureAd");
        
        options.Instance = section.GetValue<string>("Instance");
        options.Domain = section.GetValue<string>("Domain");
        options.ClientId = section.GetValue<string>("ClientId");
        options.TenantId = section.GetValue<string>("TenantId");
        options.ClientSecret = section.GetValue<string>("ClientSecret");
        options.CallbackPath = section.GetValue<string>("CallbackPath");
        
        var backchannelSection = section.GetSection("Backchannel");

        if (bool.TryParse(backchannelSection["UseProxy"], out bool useProxy) && useProxy)
        {
            var proxy = new WebProxy { Address = new Uri(backchannelSection["Uri"]) };
            
            // UNCOMMENT THE FOLLOWING LINE TO GET THE EXPECTED BEHAVIOUR
            // HttpClient.DefaultProxy = proxy; 
            
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                UseDefaultCredentials = false,
                UseProxy = true,
                Proxy = proxy
            };
        }
    })
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();