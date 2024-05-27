namespace MembershipSite.Website.MvcLogic.TagHelpers;

[HtmlTargetElement("title", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PageTitleTagHelper(AppSettings appSettings) : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!string.IsNullOrEmpty(appSettings.TitleSuffix))
        {
            output.PostContent.AppendHtml(appSettings.TitleSuffix);
        }
    }
}
