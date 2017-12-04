using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project22.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Project22.ViewModel;

namespace Project22.Controllers
{
    [Route("[controller]/[action]/{returnUrl?}")]
    public class AccountController : Controller
    {
        DataRepository dataRepository;

        public AccountController(DataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginInfo model, string returnUrl = null)
        {
            if(ModelState.IsValid)
            {
                var account = dataRepository.GetAccount(model.MobileNumber);

                if(account != null && account.Pin == model.Pin)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,model.MobileNumber),
                        new Claim(ClaimTypes.MobilePhone,model.MobileNumber),
                        new Claim(ClaimTypes.Sid, $"{account.Id}")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties());

                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("SignedOut");
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(SessionController.Index), "Session");
            }
        }

    }
}