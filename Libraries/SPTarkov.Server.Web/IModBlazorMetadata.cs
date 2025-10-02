namespace SPTarkov.Server.Web;

/// <summary>
/// This empty interface is used as a metadata marker to identify mod assemblies that integrate with Blazor or MVC.
/// </summary>
/// <remarks>
/// Implementing this interface signals to the host application to:
/// <list type="bullet">
///   <item>
///     <description>Link the mod's <c>wwwroot</c> directory, enabling serving of static web assets (CSS, JS, etc.).</description>
///   </item>
///   <item>
///     <description>Register the mod's Blazor components and pages for routing within the application.</description>
///   </item>
///   <item>
///     <description>Register the mod's MVC controllers for use as APIs where necessary.</description>
///   </item>
/// </list>
///
/// This interface is intentionally empty but may be extended in the future to include additional metadata.
/// </remarks>
public interface IModWebMetadata { }
