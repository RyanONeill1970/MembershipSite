namespace MembershipSite.Website.MvcLogic;

public static class ControllerExtensions
{
    public static IActionResult RedirectToLocal(this Controller controller, string returnUrl)
    {
        if (controller.Url.IsLocalUrl(returnUrl))
        {
            return controller.Redirect(returnUrl);
        }

        return controller.Redirect("/");
    }
}
