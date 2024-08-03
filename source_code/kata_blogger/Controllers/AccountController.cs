using Microsoft.AspNetCore.Mvc;
using kata_blogger.Data;
using kata_blogger.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using kata_blogger.ViewModels;

namespace kata_blogger.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public IActionResult Index()
        {
            return View(_context.UserAccounts.ToList());
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(RegistrationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                UserAccount account = new UserAccount();
                account.Email = viewModel.Email;
                account.FirstName = viewModel.FirstName;
                account.LastName = viewModel.LastName;
                account.Password = viewModel.Password;
                account.UserName = viewModel.UserName;
                
                try
                {
                    _context.UserAccounts.Add(account);
                    _context.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} registered successfully. Please Login.";
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Please enter unique Email or Password.");
                    return View(viewModel);
                }
                return View(viewModel);//???
            }
            return View(viewModel);
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = _context.UserAccounts.Where(x => (x.UserName == viewModel.UserNameOrEmail || x.Email == viewModel.UserNameOrEmail) && x.Password == viewModel.Password).FirstOrDefault();
                if (user != null)
                {
                    // success
                    var claims = new List<Claim>
                    {
                        user.Email != null ? new Claim(ClaimTypes.Name, user.Email) : new Claim(ClaimTypes.Name, "")
                        // new Claim(ClaimTypes.Name, user.Email)
                        ,user.FirstName != null ? new Claim("Name", user.FirstName) : new Claim("Name", "")
                        // , new Claim("Name", user.FirstName)
                        , new Claim(ClaimTypes.Role, "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Username/Email or Password is not correct");
                }
            }
            return View(viewModel);
        }

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SecurePage");
        }

        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }
    }
}
