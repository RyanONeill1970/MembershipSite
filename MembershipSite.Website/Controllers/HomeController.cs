namespace MembershipSite.Website.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    [Route("not-found")]
    [ActionName("not-found")]
    public IActionResult NotFound(int code)
    {
        ViewBag.StatusCode = code;
        return View();
    }

    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
