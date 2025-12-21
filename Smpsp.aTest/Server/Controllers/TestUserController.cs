using Microsoft.AspNetCore.Mvc;
using Smpsp.Server.Controllers;

namespace Smpsp.aTest.Server.Controllers
{
    [TestClass]
    public class TestUserController
    {
        [TestMethod]
        public async Task Add_change_delete_user()
        {
            UserController c = new(MyServices.UserService, MyServices.UserAuthStateService);
            c.ControllerContext.HttpContext = MyServices.HttpContext;

            User u = new()
            {
                Active = true,
                EMail = "test@text.t",
                LanguageCode = "en-us",
                Name = "Test User",
                VetoLevel = 3
            };

            //Add
            var res = await c.Post(u);
            var ok = res as OkObjectResult;
            Assert.IsNotNull(ok);

            var newUser = ok.Value as User;
            Assert.IsNotNull(newUser);
            u.Id = newUser.Id;

            //Put
            u.Name = "Modified Test User";
            res = await c.Put(u);

            //Get by id
            var resGetById = c.GetById(u.Id);
            Assert.IsTrue(resGetById.Result is OkObjectResult gok && gok.Value is User gu && gu.Equals(u));

            //Delete
            res = await c.Delete(u.Id);
            Assert.IsTrue(res is OkResult);

            //Get
            var resGet = c.Get();
            Assert.IsTrue(resGet.Result is OkObjectResult gok2 && gok2.Value is IEnumerable<User> gel && gel.Any(x => x.Equals(u) == false));
        }
    }
}
