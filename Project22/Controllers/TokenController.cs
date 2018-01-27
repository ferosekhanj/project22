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
            var (account, sessions) = dataRepository.GetSession(phoneNumber);
            var selectionList = new List<SelectListItem>();
            foreach (var session in sessions)
                selectionList.Add(new SelectListItem { Text = $"{session.StartTime} - {session.Name}", Value = session.Id.ToString() });

            ViewBag.Sessions = selectionList;
            ViewBag.Account = account;
            return View();
        }

        [Route("/Get/{phoneNumber}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(string phoneNumber,[Bind("Number,Mobile,Session")]Token token)
        {
            if (ModelState.IsValid)
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
            return View(token);
        }
    }
}