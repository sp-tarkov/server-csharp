using Microsoft.AspNetCore.Mvc;

namespace TestMod.Controllers;

public class TestController : Controller
{
    [HttpGet("/test/ping")]
    public IActionResult Ping()
    {
        return Content("Pong from MVC!");
    }
}
