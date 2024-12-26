namespace MembershipSite.Website.MvcLogic;

public class ThemeViewLocator : IViewLocationExpander
{
    /// <summary>
    /// Changes the view path location to look in the current view folder first, then the theme folder, and then fails over to the shared folder.
    /// 
    /// .Net will use the returned template strings.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="viewLocations">
    /// A list of possible view location templated paths such as;
    ///   /Views/{1}/{0}.cshtml
    ///   /Views/Shared/{0}.cshtml
    /// </param>
    /// <returns>
    /// A modified list of view paths adjusted to place the current view folder first, then the theme folder, and then the shared folder.
    /// For example, the above will be translated to;
    ///   /Views/{1}/{0}.cshtml
    ///   /theme/{0}.cshtml
    ///   /Views/Shared/{0}.cshtml
    /// </returns>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var currentViewLocation = "/Views/{1}/{0}.cshtml";
        var themeLocation = "/theme/{0}.cshtml";
        var sharedLocation = "/Views/Shared/{0}.cshtml";

        return [currentViewLocation, themeLocation, sharedLocation];
    }

    /// <summary>
    /// Used by the framework to grab a list of values that are used in 
    /// <see cref="ExpandViewLocations(ViewLocationExpanderContext, IEnumerable{string})"/>.
    /// 
    /// This can vary by request.
    /// 
    /// You should not skip this step and definitely not use the source values in 
    /// <see cref="ExpandViewLocations(ViewLocationExpanderContext, IEnumerable{string})"/> as
    /// the framework caches the output of <see cref="ExpandViewLocations(ViewLocationExpanderContext, IEnumerable{string})"/> dependent
    /// on the data populated in context.Values here.
    /// 
    /// So the only way to get things to work properly is to put the values in here that you wish to use to expand the view locations later.
    /// </summary>
    /// <param name="context"></param>
    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }
}
