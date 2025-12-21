using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Controllers;

namespace Smpsp.aTest.Server.Controllers
{
    [TestClass]
    public class TestSignInController
    {
        [TestMethod]
        public async Task Sign_in_user()
        {
            Mocks.MailService ms = new();
            SignInController c = new(ms, MyServices.UserService, MyServices.SignInCodeTask);

            SignInRequest request = new() { EMail = MyServices.User.EMail };
            var res = await c.SignIn(request);

            var ok = res.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var rep = ok.Value as SignInReply;
            Assert.IsNotNull(rep);

            var res2 = c.SignInCode(new() { AntiforgeryToken = rep.AntiforgeryToken, Code = ms.Code });
            var ok2 = res2.Result as OkObjectResult;
            Assert.IsNotNull(ok2);

            var rep2 = ok2.Value as SignInCodeReply;
            Assert.IsNotNull(rep2);

            Assert.IsTrue(
                rep2.User.EMail == MyServices.User.EMail &&
                rep2.UnixTimestampExpirationDate > DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
    }
}
