using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project22.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Project22.Controllers
{
    [Route("[controller]/[action]/{id?}")]
    public class TokenController : Controller
    {
        DataRepository dataRepository;

        public TokenController(DataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public IActionResult Details(int id)
        {
            return View(dataRepository.GetToken(id));
        }

        [Route("/Get/{phoneNumber}")]
        [HttpGet]
        public IActionResult Book(string phoneNumber)
        {
            HttpContext.Session.Set("OTP", new byte[]{0,0,0,0});
            var (account, sessions) = dataRepository.GetSession(phoneNumber);
            var selectionList = new List<SelectListItem>();
            foreach (var session in sessions)
                selectionList.Add(new SelectListItem { Text = $"{session.StartTime} - {session.Name}", Value = session.Id.ToString() });

            ViewBag.Sessions = selectionList;
            ViewBag.Account = account;
            ViewBag.PasswordSent = false;
            return View();
        }

        // TODO integrate ASPNetCoreAPIRateLimit
        [Route("/Otp/{phoneNumber}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetOneTimePassword(string phoneNumber)
        {
            Random r = new Random(Environment.TickCount);
            var otp = r.Next() % 10000;
            HttpContext.Session.Set("OTP", BitConverter.GetBytes(otp));
            return Content($"{otp}");
        }

        [Route("/Get/{phoneNumber}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(string phoneNumber,[Bind("Number,Name,Mobile,Password,Session")]Token token)
        {
            bool isOTPCorrect = CheckOtp();
            if (ModelState.IsValid && isOTPCorrect)
            {
                dataRepository.CreateToken(token.Session.Id,token);
                return View("Details",token);
            }
            var (account, sessions) = dataRepository.GetSession(phoneNumber);
            var selectionList = new List<SelectListItem>();
            foreach (var session in sessions)
                selectionList.Add(new SelectListItem { Text = $"{session.StartTime} - {session.Name}", Value = session.Id.ToString() });

            ViewBag.Account = account;
            ViewBag.Sessions = selectionList;
            if (!isOTPCorrect)
            {
                ModelState.AddModelError(nameof(Token.Password), "Please enter the correct OTP sent to your mobile");
                ViewBag.PasswordSent = true;
            }
            return View(token);

            // Check OTP
            bool CheckOtp()
            {
                return HttpContext.Session.TryGetValue("OTP", out byte[] bytes) && token.Password == BitConverter.ToInt32(bytes, 0);
            }

        }
    }
}