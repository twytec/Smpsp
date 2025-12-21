using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Smpsp.Server.Backgrounds;
using Smpsp.Server.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var ps = new PathService(builder.Environment.ContentRootPath);
builder.Services.AddSingleton(ps);

FFMpegCore.GlobalFFOptions.Configure(new FFMpegCore.FFOptions() { BinaryFolder = ps.FFMpegPath, TemporaryFilesFolder = ps.TempPath });

MySettingsService mss = new(ps);
builder.Services.AddSingleton(mss);

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = mss.Settings.MaxRequestBodySize * 2; //Because of base64
});

var trans = new TranslationService(Path.Join(builder.Environment.WebRootPath, "translations"), mss.Settings);
builder.Services.AddSingleton(trans);

builder.Services.AddSingleton<MailQueue>();
builder.Services.AddHostedService<MailHostedService>();
builder.Services.AddSingleton<IMailService, MailService>();
builder.Services.AddDbContextFactory<AppDbContext>();

builder.Services.AddHostedService<CleanupHostedService>();
builder.Services.AddSingleton<UploadMediaTask>();
builder.Services.AddSingleton<SignInCodeTask>();

builder.Services.AddSingleton<CompletePostQueue>();
builder.Services.AddHostedService<CompletePostHostedService>();

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<PostService>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mss.Settings.IssuerSigningKey))
    };
});

builder.Services.AddScoped<UserAuthStateService>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

using (var s = app.Services.CreateScope())
{
    var dbf = s.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = dbf.CreateDbContext();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(ps.FilesPath),
    RequestPath = "/files",
    ServeUnknownFileTypes = true
});
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(ps.TempPath),
    RequestPath = "/temp", 
    ServeUnknownFileTypes = true
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthMiddleware>();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
