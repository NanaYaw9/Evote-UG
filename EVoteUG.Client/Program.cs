using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EVoteUG.Client;
using EVoteUG.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5059/";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddScoped<ElectionService>();
builder.Services.AddScoped<PositionService>();
builder.Services.AddScoped<VoteService>();
builder.Services.AddScoped<CandidateService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddSingleton<AuthState>();
builder.Services.AddScoped<AdminService>();

await builder.Build().RunAsync();