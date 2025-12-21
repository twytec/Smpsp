using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Smpsp.aTest.Server.Mocks;
using Smpsp.Server.Backgrounds;
using Smpsp.Server.Data;
using System.Reflection;

namespace Smpsp.aTest.Server
{
    public static class MyServices
    {
        public static string TestVideo { get; private set; }
        public static string TestImage { get; private set; }
        public static string TestImageToConvert { get; private set; }

        public static PathService PathService { get; private set; }
        public static MySettingsService MySettingsService { get; private set; }

        //Backrounds
        public static CompletePostQueue CompletePostQueue { get; set; }
        public static SignInCodeTask SignInCodeTask { get; private set; }
        public static UploadMediaTask UploadMediaTask { get; private set; }

        public static DbContextFactory DbContextFactory { get; private set; }
        public static UserService UserService { get; private set; }
        public static TranslationService TranslationService { get; private set; }
        public static PostService PostService { get; private set; }
        public static User User { get; private set; }
        public static UserAuthStateService UserAuthStateService { get; private set; }
        public static HttpContext HttpContext { get; private set; }

        static MyServices()
        {
            var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "test");
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            PathService = new PathService(path);
            MySettingsService = new(PathService);
            TranslationService = new(path, MySettingsService.Settings);

            CompletePostQueue = new();

            DbContextFactory = new(PathService);
            var db = DbContextFactory.CreateDbContext();
            db.Database.EnsureCreated();
            db.Dispose();

            var log = new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory();

            UserService = new(DbContextFactory, TranslationService, log.CreateLogger<UserService>());
            SignInCodeTask = new(MySettingsService);
            PostService = new(DbContextFactory, PathService, MySettingsService, TranslationService, CompletePostQueue, log.CreateLogger<PostService>());
            UploadMediaTask = new(PathService, MySettingsService);

            User = new User() { Active = true, EMail = "a@a.a", LanguageCode = "en-us", VetoLevel = 1 };
            UserAuthStateService = new() { User = User, I18n = TranslationService.DefaultTranslations };
            UserService.AddUserAsync(User).GetAwaiter().GetResult();

            System.Security.Claims.Claim[] claims = [
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, User.EMail),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, Smpsp.Server.Controllers.AdminSignInController.RoleAdmin)
            ];
            System.Security.Claims.ClaimsIdentity[] ci = [new(claims)];

            HttpContext = new DefaultHttpContext()
            {
                User = new System.Security.Claims.ClaimsPrincipal(ci)
            };

            TestVideo = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "test.mp4");
            if (File.Exists(TestVideo) == false)
                CreateFileFromResource("Smpsp.aTest.TestData.test.mp4", TestVideo);

            TestImage = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "test.png");
            if (File.Exists(TestImage) == false)
                CreateFileFromResource("Smpsp.aTest.TestData.test.png", TestImage);

            TestImageToConvert = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "test.bmp");
            if (File.Exists(TestImageToConvert) == false)
                CreateFileFromResource("Smpsp.aTest.TestData.test.bmp", TestImageToConvert);
        }

        private static void CreateFileFromResource(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream is null)
                throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly.");

            using var fileStream = File.Create(outputPath);
            resourceStream.CopyTo(fileStream);
        }
    }
}
