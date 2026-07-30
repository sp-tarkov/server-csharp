namespace SPTarkov.Server.Web;

/// <summary>
/// This interface is used as a metadata marker to identify mod assemblies that integrate with Blazor or MVC.
/// </summary>
/// <remarks>
/// Implementing this interface signals to the SPT Server to:
/// <list type="bullet">
///   <item>
///     <description>Link the mod's <c>wwwroot</c> directory, enabling serving of static web assets (CSS, JS, etc.).</description>
///   </item>
///   <item>
///     <description>Register the mod's Blazor components and pages for routing within the SPT Server.</description>
///   </item>
///   <item>
///     <description>Register the mod's MVC controllers for use as APIs where necessary.</description>
///   </item>
/// </list>
///
/// </remarks>
public interface IModBlazorMetadata
{
    /// <summary>
    /// This property will allow you to set a custom directory for wwwroot, if this is left null it will use the assembly's name
    /// </summary>
    string? WWWRootUrl { get; init; }

    /// <summary>
    ///     Allows setting a home page url that shows up in the SIC mod links section
    ///  <br/><br/>
    /// Example: /my-mod
    /// </summary>
    string? HomePage { get; init; }

    /// <summary>
    ///     Sets the card description on the SPT landing page
    /// </summary>
    string? HomePageDescription { get; init; }
}
