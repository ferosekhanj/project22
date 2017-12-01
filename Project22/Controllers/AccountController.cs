using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project22.Models;

namespace Project22.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        DataRepository dataRepository;

        public AccountController(DataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string mobileNumber, string pin)
        {
            return RedirectToAction(nameof(SessionController.Index), "Session");
        }
    }
}