namespace MembershipSite.Website.Controllers;

[Authorize(Roles = RoleNames.MemberAdmin)]
[Route("backstage")]
public class BackstageController(MemberAdminService memberAdminService) : Controller
{
    [ActionName("member-list")]
    [Route("member-list", Name = nameof(MemberList))]
    [HttpGet]
    public IActionResult MemberList()
    {
        var model = memberAdminService.AllAsQueryable();
        return View(model);
    }

    [ActionName("upload-members")]
    [Route("upload-members")]
    [HttpGet]
    public IActionResult UploadMembers()
    {
        return View();
    }

    [ActionName("upload-members")]
    [Route("upload-members")]
    [HttpPost]
    public async Task<IActionResult> UploadMembersAsync(UploadMembers model)
    {
        if (ModelState.IsValid)
        {
            var result =  await memberAdminService.UploadMembersAsync(model.File!);

            return View("upload-members-result", result);
        }

        return View(model);
    }
}
