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
        // TODO: Model probably not needed now. Or at least model data is not.
        var model = memberAdminService.AllAsQueryable();
        return View(model);
    }

    [ActionName("member-list-2")]
    [Route("")]
    [Route("member-list-2", Name = nameof(MemberList2))]
    [HttpGet]
    public IActionResult MemberList2()
    {
        return View();
    }

    [ActionName("member-grid-data")]
    [Route("member-grid-data", Name = nameof(MemberGridDataAsync))]
    [HttpGet]
    public async Task<JsonResult> MemberGridDataAsync()
    {
        var members = await memberAdminService.MemberAdminSummaryAsync();
        return Json(members);
    }

    [ActionName("save-member-data")]
    [Route("save-member-data")]
    [HttpPost]
    public async Task<IActionResult> SaveMemberDataAsync([FromBody] List<MemberSummaryRow> model)
    {
        if (ModelState.IsValid)
        {
            await memberAdminService.SaveMemberDataAsync(model);
            return Ok();
        }

        return BadRequest();
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
            var result = await memberAdminService.UploadMembersAsync(model.File!);

            return View("upload-members-result", result);
        }

        return View(model);
    }
}
