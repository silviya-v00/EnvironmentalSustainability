using EnvironmentalSustainabilityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
                              SignInManager<IdentityUser> signInManager,
                              UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LoginPage()
        {
            return View();
        }

        public IActionResult AdminHome()
        {
            return View();
        }

        public IActionResult HomePage()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity.Name;
            var existingUser = await _userManager.FindByNameAsync(userName);

            if (existingUser != null)
            {
                ViewBag.Username = existingUser.UserName;
                ViewBag.Email = existingUser.Email;
            }

            return View();
        }

        public async Task<RedirectToActionResult> RedirectToHomeByRole(IdentityUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "ADMIN"))
            {
                return RedirectToAction("AdminHome", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(userName, password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var existingUser = await _userManager.FindByNameAsync(userName);
                return await RedirectToHomeByRole(existingUser);
            }
            else
            {
                ViewData["ErrorMessage"] = "Invalid login attempt.";
                return View("LoginPage", ViewBag.ActiveTab = "login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ViewData["ErrorMessage"] = "Email is already registered.";
                return View("LoginPage", ViewBag.ActiveTab = "register");
            }

            if (password != confirmPassword)
            {
                ViewData["ErrorMessage"] = "Password and confirmed password do not match.";
                return View("LoginPage", ViewBag.ActiveTab = "register");
            }

            var newUser = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "REGULARUSER");
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                return await RedirectToHomeByRole(newUser);
            }
            else
            {
                ViewData["ErrorMessage"] = "Registration failed. Please try again.";
                return View("LoginPage", ViewBag.ActiveTab = "register");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string username, string newEmail)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                user.UserName = username;
                user.Email = newEmail;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    ViewBag.Username = username;
                    ViewBag.Email = newEmail;
                    return View("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View("Profile");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "The new password and confirmation password do not match.");
                return View("Profile");
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null && !String.IsNullOrEmpty(oldPassword) && !String.IsNullOrEmpty(newPassword) && !String.IsNullOrEmpty(confirmPassword))
            {
                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

                if (result.Succeeded)
                {
                    return View("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View("Profile");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
