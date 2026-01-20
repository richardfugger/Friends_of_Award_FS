using Blazor_WithAuth_ForSWP5;
using Friends_of_Award_FS.Components;
using Friends_of_Award_FS_Lib.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<MyCustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
  provider.GetRequiredService<MyCustomAuthStateProvider>());
builder.Services.AddScoped<DiplomarbeitenImportService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
