using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers;

public class HomeController : Controller
{
    private MyContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        HttpContext.Session.Clear();
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost("/user/register")]
    public IActionResult Register(User newUser)
    {            
    
        if(ModelState.IsValid)
        {  
            // need to check if email is unique
            if (_context.Users.Any(a => a.Email == newUser.Email))
            {
                //email already exist
                ModelState.AddModelError("Email", "Email is already in use!");
                return View("Index");
            }
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
            _context.Add(newUser);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("user", newUser.UserId);
            return RedirectToAction("Success");
        } else {
            return View("Index");
        }
    }

    [HttpGet("success")]
    public IActionResult Success()
    {
        List<Wedding> Weddings = _context.Weddings.Include(a=>a.Guests).ToList();
        ViewBag.loggedInUser = _context.Users.FirstOrDefault(a=> a.UserId == HttpContext.Session.GetInt32("user"));
        return View(Weddings);
    }

    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }


    [HttpPost("user/login")]
    public IActionResult Login(Login LoginUser)
    {
        if (ModelState.IsValid)
        {   // find their email in the database to make sure email matches password
            User userInDb = _context.Users.FirstOrDefault(a=> a.Email == LoginUser.LEmail);
            if(userInDb == null)
            {
                // no email matching in db
                ModelState.AddModelError("LEmail", "Invalid Login Attempt");
                return View("Login");
            }
            PasswordHasher<Login> hasher = new PasswordHasher<Login>();
            var result = hasher.VerifyHashedPassword(LoginUser, userInDb.Password, LoginUser.LPassword);
            if (result == 0)
            {
                ModelState.AddModelError("LEmail", "Invalid Login Attempt");
                return View("Login");
            } else {
                HttpContext.Session.SetInt32("user", userInDb.UserId);
                return RedirectToAction("Success");
            }
        } else {
            return View("Login");
        }
    }

    [HttpGet("/new/wedding")]
    public IActionResult newWedding()
    {
        return View("NewWedding");
    }

    [HttpPost("/add/wedding/process")]
    public IActionResult weddingProcess(Wedding newWed)
    {
        if(ModelState.IsValid)
        {
            newWed.UserId = (int) HttpContext.Session.GetInt32("user");
            _context.Add(newWed);
            _context.SaveChanges();
            return RedirectToAction("Success");
        } else {
            return View("newWedding");
        }
    }

    [HttpGet("/join/wedding/{weddingId}")]
    public IActionResult joinWedding(int weddingId)
    {
        Guest newGuest = new Guest()
        {
            UserId = (int) HttpContext.Session.GetInt32("user"),
            WeddingId = weddingId
        };
        _context.Add(newGuest);
        _context.SaveChanges();
        return Redirect("/success");
    }

    [HttpGet("/leave/wedding/{weddingId}")]
    public IActionResult leaveWedding(int weddingId)
    {
        Guest thisGuest = _context.Guests.FirstOrDefault(a=>a.UserId == (int) HttpContext.Session.GetInt32("user") && a.WeddingId == weddingId);
        _context.Remove(thisGuest);
        _context.SaveChanges();
        return Redirect("/success");
    }

    [HttpGet("/delete/{weddingId}")]
    public IActionResult deleteWedding(int weddingId)
    {
        Wedding thisWedding = _context.Weddings.FirstOrDefault(a=>a.WeddingId == weddingId);
        _context.Remove(thisWedding);
        _context.SaveChanges();
        return Redirect("/success");
    }

    [HttpGet("/wedding/{weddingId}")]
    public IActionResult weddingInfo(int weddingId)
    {
        ViewBag.oneWedding = _context.Weddings.Include(a=>a.Guests).ThenInclude(a=>a.User).FirstOrDefault(a=>a.WeddingId == weddingId);
        return View("WeddingInfo");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
