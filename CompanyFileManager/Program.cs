using CompanyFileManager.Hubs;
using CompanyFileManager.Services;
using MudBlazor.Services;

var settingsService = new AppSettingsService();
settingsService.Load();

var port = settingsService.Settings.Port;
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR(o => { o.MaximumReceiveMessageSize = 10 * 1024 * 1024; });
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(settingsService);
builder.Services.AddSingleton(new ServerInfo { LivePort = port });
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<NetworkService>();
builder.Services.AddSingleton<InputSimulatorService>();
builder.Services.AddSingleton<ScreenCaptureService>();
builder.Services.AddSingleton<RestartService>();

builder.Services.AddControllers();

builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapHub<ScreenShareHub>("/hubs/remote");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
