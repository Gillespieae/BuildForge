using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers;

// default controller for general pages (Home, Privacy, Error)
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    // inject logger so errors/events can be tracked
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // loads the homepage (Views/Home/Index.cshtml)
    public IActionResult Index()
    {
        return View();
    }

    // loads privacy page (Views/Home/Privacy.cshtml)
    public IActionResult Privacy()
    {
        return View();
    }

    // disables caching so error page is always fresh
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // passes request ID to the view for debugging/tracking errors
        return View(new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }
}