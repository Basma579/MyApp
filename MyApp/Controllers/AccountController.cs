using MyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MyApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor ,
                       SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this._httpContextAccessor = httpContextAccessor;

        }

        [HttpGet]
        public IActionResult Register()
        {
            List<SelectListItem> Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Customer", Text = "Customer" },
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                };

            ViewBag.Roles = Roles;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    LockoutEnabled=false

                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    bool adminRoleExists = await roleManager.RoleExistsAsync("Customer");
                    if (!adminRoleExists)
                    {
                      await roleManager.CreateAsync(new IdentityRole("Customer"));
                    }
                    await userManager.AddToRoleAsync(user, "Customer");
                    return RedirectToAction("Login", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
           

            return View(model);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {

                var result = await signInManager.PasswordSignInAsync(model.Email,
                     model.Password, model.RememberMe, false);

                if (result.Succeeded )
                {

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {

                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                 ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid Data");
            }

            return View(model);
        }
        public async Task<IActionResult> Logout()
        {

            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use.");
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task< IActionResult> ListUsers()
        {
            var users =await userManager.GetUsersInRoleAsync("Customer");
            return View(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ChangeUserStatus(string email)
        {
            var userTask = userManager.FindByEmailAsync(email);
            userTask.Wait();
            var user = userTask.Result;

            if (user.LockoutEnabled)
            {
                var lockUserTask = userManager.SetLockoutEnabledAsync(user, false);
                var lockDateTask = userManager.SetLockoutEndDateAsync(user, null);
                lockUserTask.Wait();
            }
            else
            {
                var UnlockUserTask = userManager.SetLockoutEnabledAsync(user, true);
                UnlockUserTask.Wait();
                var lockDateTask = userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddYears(10));
                lockDateTask.Wait();
            }


            return RedirectToAction("ListUsers");

        }


    }
}
