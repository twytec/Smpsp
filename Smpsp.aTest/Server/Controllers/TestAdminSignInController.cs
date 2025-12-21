using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Controllers;

namespace Smpsp.aTest.Server.Controllers
{
    [TestClass]
    public class TestAdminSignInController
    {
        [TestMethod]
        public void Sign_in_admin()
        {
            var controller = new AdminSignInController(MyServices.MySettingsService);
            var request = new AdminSignInRequest()
            {
                Name = MyServices.MySettingsService.Settings.AdminName,
                Password = MyServices.MySettingsService.Settings.AdminPassword
            };

            var res = controller.SignIn(request);

            var ok = res.Result as OkObjectResult;
            Assert.IsNotNull(ok);

            var rep = ok.Value as AdminSignInReply;
            Assert.IsNotNull(rep);

            Assert.IsGreaterThan(0, rep.Token.Length);
        }
    }
}
