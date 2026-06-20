using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services.Image;
using SPTarkov.Server.Core.Utils;

namespace UnitTests.Tests.Routers;

[TestFixture]
public sealed class ImageRouterTests
{
    // The single, real image on disk. Its name contains a literal space.
    private const string OnDiskFile = "./SPT_Data/images/hideout/Icon_Cultist Zone_Small.png";

    // The unescaped, lowercased route key the router stores/looks up under.
    private const string NormalizedKey = "/files/hideout/icon_cultist zone_small";

    private static ImageRouter CreateRouter(out ImageRouterService routes)
    {
        routes = new ImageRouterService();
        return new ImageRouter(new FileUtil(), routes, null!);
    }

    private static HttpContext RequestFor(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString(path); // Stores the value without encoding it
        return context;
    }

    // Each case is the Request.Path as ASP.NET hands it to the router, after its single URL-decode:
    //   "%20"   on the wire arrives as a real space.
    //   "%2520" on the wire decoded once by ASP.NET, arrives as a literal "%20".
    // Both must resolve to the one non-encoded file on disk.
    [TestCase("/files/Hideout/Icon_Cultist Zone_Small.png", TestName = "wire %20 arrives as a space")]
    [TestCase("/files/Hideout/Icon_Cultist%20Zone_Small.png", TestName = "wire %2520 arrives as a literal %20")]
    public void RequestResolvesToTheNonEncodedFile(string requestPath)
    {
        var router = CreateRouter(out var routes);
        router.AddRoute("/files/Hideout/Icon_Cultist Zone_Small", OnDiskFile); // Only register the non-encoded file

        Assert.Multiple(() =>
        {
            Assert.That(router.CanHandle(default, RequestFor(requestPath)), Is.True, "request should resolve");

            // It normalizes to the key holding the one non-encoded file.
            Assert.That(routes.GetByKey(NormalizedKey), Is.EqualTo(OnDiskFile));
            Assert.That(routes.ExistsByKey("/files/hideout/icon_cultist%20zone_small"), Is.False);
        });
    }

    [Test]
    public void RegisteringAnEncodedNameNormalizesToTheDecodedKey()
    {
        var router = CreateRouter(out var routes);

        // If an encoded filename was registered, it must not get its own "%20" route.
        router.AddRoute("/files/Hideout/Icon_Cultist%20Zone_Small", OnDiskFile);

        Assert.Multiple(() =>
        {
            Assert.That(routes.GetByKey(NormalizedKey), Is.EqualTo(OnDiskFile));
            Assert.That(routes.ExistsByKey("/files/hideout/icon_cultist%20zone_small"), Is.False);
        });
    }
}
