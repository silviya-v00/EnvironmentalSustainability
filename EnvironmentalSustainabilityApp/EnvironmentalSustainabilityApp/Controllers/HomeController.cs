using EnvironmentalSustainabilityApp.Models;
using EnvironmentalSustainabilityApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private DBUtil _dbUtil;
        private MailUtil _mailUtil;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
                              IConfiguration configuration,
                              SignInManager<IdentityUser> signInManager,
                              UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _configuration = configuration;
            _dbUtil = new DBUtil(_configuration.GetConnectionString("DefaultConnection"));
            _mailUtil = new MailUtil(_configuration);
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var featuredContent = _dbUtil.GetContentListFromDatabase("ACTIVE");
            return View(featuredContent);
        }

        public IActionResult LoginPage()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
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

        public IActionResult AirQuality()
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
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var passwordResetLink = Url.Action("ResetPassword", "Home",
                            new { email = model.Email, token = token }, Request.Scheme);

                    var subject = "Password Reset";
                    var messageBody = "To reset your password, please click the following link: <a href='" + passwordResetLink + "'>Reset Password</a>";

                    _mailUtil.SendEmail(model.Email, subject, messageBody);

                    return View("ForgotPasswordConfirmation");
                }

                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }

                return View("ResetPasswordConfirmation");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string username, string newEmail)
        {
            var userName = User.Identity.Name;
            var currentUser = await _userManager.FindByNameAsync(userName);

            var existingUserByEmail = await _userManager.FindByEmailAsync(newEmail);
            if (existingUserByEmail != null && currentUser != null && existingUserByEmail.Email != currentUser.Email)
            {
                ModelState.AddModelError("", "Email already exists.");
            }

            var existingUserByUserName = await _userManager.FindByNameAsync(username);
            if (existingUserByUserName != null && currentUser != null && existingUserByUserName.UserName != currentUser.UserName)
            {
                ModelState.AddModelError("", "Username already exists.");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    user.UserName = username;
                    user.Email = newEmail;

                    var result = await _userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            ViewBag.Username = username;
            ViewBag.Email = newEmail;

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

        [HttpGet]
        public IActionResult GetContentList()
        {
            var contentList = _dbUtil.GetContentListFromDatabase("ALL");

            return Json(contentList);
        }

        [HttpGet]
        public IActionResult GetContentDetails(int contentId)
        {
            var content = _dbUtil.GetContentDetailsFromDatabase(contentId);

            return Json(content);
        }

        [HttpGet]
        public IActionResult GetContentImage(string fileName)
        {
            string imageFolderPath = _configuration["ImageFolderPath"];
            string imagePath = Path.Combine(imageFolderPath, fileName);

            if (System.IO.File.Exists(imagePath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, "image/jpeg");
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult SaveContent([FromForm] FeaturedContent content)
        {
            string imageFolderPath = _configuration["ImageFolderPath"];

            if (content.ContentImage != null && content.ContentImage.Length > 0)
            {
                content.ContentImageFileName = Guid.NewGuid().ToString() + ".jpg";

                string imagePath = Path.Combine(imageFolderPath, content.ContentImageFileName);
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    content.ContentImage.CopyTo(fileStream);
                }
            }

            _dbUtil.SaveContentToDatabase(content, imageFolderPath);
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteContent(int contentId)
        {
            _dbUtil.DeleteContentFromDatabase(contentId, _configuration["ImageFolderPath"]);
            return Ok();
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
