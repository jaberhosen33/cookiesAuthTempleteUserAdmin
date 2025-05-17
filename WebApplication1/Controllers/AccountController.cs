using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Register() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Register(string username, string password,string role)
        {
            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "User already exists");
                return View();
            }

            var user = new User { Username = username, Password = password, Role = role };
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Product");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> UserList()
        {

            return View(_context.Users.ToList());
        }
    }

}
