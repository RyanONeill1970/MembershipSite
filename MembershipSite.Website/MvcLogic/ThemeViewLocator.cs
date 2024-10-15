namespace MembershipSite.Website.MvcLogic;

public class ThemeViewLocator : IViewLocationExpander
{
    /// <summary>
    /// Changes the view path location to look in the theme folder.
    /// 
    /// Prioritises the theme and fails over to the default layout.
    /// 
    /// The .Net framework will use the returned template strings.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="viewLocations">
    /// A list of possible view location templated paths such as;
    ///   /Views/{1}/{0}.cshtml
    ///   /Views/Shared/{0}.cshtml
    /// </param>
    /// <returns>
    /// A modified list of view paths adjusted to place the theme folder as the first element.
    /// For example, the above will be translated to;
    ///   /theme/{0}.cshtml
    ///   /Views/Shared/{0}.cshtml
    /// </returns>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var themeLocation = "/theme/{0}.cshtml";

        return new[] { themeLocation }.Concat(viewLocations);
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
