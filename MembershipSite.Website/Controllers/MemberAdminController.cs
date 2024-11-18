namespace MembershipSite.Website.Controllers;

[Authorize(Roles = RoleNames.MemberAdmin)]
public class MemberAdminController(MemberAdminService memberAdminService, AppSettings appSettings) : Controller
{
    [ActionName("member-list")]
    [Route("member-list", Name = nameof(MemberList))]
    [HttpGet]
    public async Task<IActionResult> MemberList()
    {
        var model = memberAdminService.AllAsQueryable();

        // HACK: For debug. Convert the email contacts to a JSON string for the view.
        ViewBag.Junk = JsonSerializer.Serialize(appSettings.EmailContacts);

        return View(model);
    }
}
