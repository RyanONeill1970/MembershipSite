namespace MembershipSite.Website.MvcLogic.TagHelpers;

/// <summary>
/// Transforms HTML markup of the form <mws:loginLinks></mws:loginLinks> or
/// <mws:loginLinks /> into a login or logout link, depending on the user's authentication state.
/// </summary>
/// <param name="httpContextAccessor"></param>
[HtmlTargetElement("mws:loginLinks", TagStructure = TagStructure.NormalOrSelfClosing)]
public class LoginLinksTagHelper(IHttpContextAccessor httpContextAccessor) : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var isAuthenticated = httpContext?.User?.Identity?.IsAuthenticated ?? false;
        var loginStateText = isAuthenticated
            ? "<a href='/auth/logout'>Logout</a>"
            : "<a href='/auth/login'>Login</a>";

        output.TagName = null;
        output.Content.SetHtmlContent(loginStateText);
    }
}
