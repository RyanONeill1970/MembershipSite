namespace MembershipSite.Website.Controllers;

[AllowAnonymous]
[Route("")]
public class HomeController() : Controller
{
    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        if (!ModelState.IsValid || statusCode == null || statusCode.Value != 404)
        {
            return View();
        }

        return View("page-not-found");
    }
}
