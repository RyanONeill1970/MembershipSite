namespace MembershipSite.Website.Controllers;

[AllowAnonymous]
[Route("")]
public class HomeController : Controller
{
    [Route("page-not-found")]
    [ActionName("page-not-found")]
    public IActionResult PageNotFound()
    {
        return View();
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
