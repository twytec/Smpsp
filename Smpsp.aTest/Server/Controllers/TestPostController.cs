using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Controllers;

namespace Smpsp.aTest.Server.Controllers
{
    [TestClass]
    public class TestPostController
    {
        private readonly PostController _c;

        public TestPostController()
        {
            _c = new(
                MyServices.MySettingsService,
                MyServices.PostService,
                MyServices.UploadMediaTask,
                MyServices.UserAuthStateService,
                MyServices.UserService);

            _c.ControllerContext.HttpContext = MyServices.HttpContext;
        }

        [TestMethod]
        public async Task Text_only()
        {
            Post p = new() { Text = "test" };

            var res = await _c.Add(p);
            Assert.IsNotNull(res);

            var ok = res.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var np = ok.Value as Post;
            Assert.IsNotNull(np);

            Assert.AreEqual(p.Text, np.Text);
        }

        [TestMethod]
        public async Task Post_image()
        {
            DataMessage msg = new()
            {
                DataAsBase64 = Convert.ToBase64String(await File.ReadAllBytesAsync(MyServices.TestImage)),
                EOF = true,
                Extension = ".png"
            };

            var res = await _c.Upload(msg);
            var ok = res.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var id = ok.Value as string;
            Assert.IsNotNull(id);

            Post p = new();
            p.Medias.Add(new() { Extension = msg.Extension, File = id });

            var res2 = await _c.Add(p);
            var ok2 = res2.Result as OkObjectResult;
            Assert.IsNotNull(ok2);

            var np = ok2.Value as Post;
            Assert.IsNotNull(np);
            Assert.AreEqual(CreationStatus.CreateZip, np.CreationStatus);

            FileInfo fi1 = new(MyServices.TestImage);
            FileInfo fi2 = new(Path.Join(MyServices.PathService.FilesPath, id));
            Assert.IsTrue(fi2.Exists && fi1.Length == fi2.Length);
        }

        [TestMethod]
        public async Task Post_video()
        {
            var ffmpeg = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
            if (Directory.Exists(ffmpeg) == false)
                Assert.Fail("FFMpeg not found");

            FFMpegCore.GlobalFFOptions.Configure(new FFMpegCore.FFOptions() { BinaryFolder = ffmpeg });

            DataMessage msg = new()
            {
                EOF = false,
                Extension = ".mp4"
            };

            var file = MyServices.TestVideo;

            byte[] data = new byte[1024];
            var fs = File.OpenRead(file);
            int numBytesToRead = (int)fs.Length;
            int numBytesRead = 0;

            while (numBytesToRead > 0)
            {
                int n = fs.Read(data, 0, data.Length);
                msg.DataAsBase64 = Convert.ToBase64String(data, 0, n);

                if (n < data.Length)
                    msg.EOF = true;

                var up = await _c.Upload(msg);
                var upOk = up.Result as OkObjectResult;
                Assert.IsNotNull(upOk);

                var id = upOk.Value as string;
                Assert.IsNotNull(id);
                msg.Id = id;

                numBytesRead += n;
                numBytesToRead -= n;
            }

            Post p = new();
            p.Medias.Add(new() { Extension = msg.Extension, File = msg.Id });

            var res2 = await _c.Add(p);
            var ok2 = res2.Result as OkObjectResult;
            Assert.IsNotNull(ok2);

            var np = ok2.Value as Post;
            Assert.IsNotNull(np);
            Assert.AreEqual(CreationStatus.CreateZip, np.CreationStatus);

            FileInfo fi1 = new(file);
            FileInfo fi2 = new(Path.Join(MyServices.PathService.FilesPath, msg.Id));
            Assert.IsTrue(fi2.Exists && fi1.Length == fi2.Length);
        }
    }
}
