using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project22.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Project22.Controllers
{
    [Route("[controller]/[action]/{id?}")]
    public class TokenController : Controller
    {
        DataRepository dataRepository;
        int myAccountId = 1;

        public TokenController(DataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public IActionResult Details(int id)
        {
            return View(dataRepository.GetToken(myAccountId, id));
        }

        [Route("/Get/{phoneNumber}")]
        [HttpGet]
        public IActionResult Book(string phoneNumber)
        {
            var sessions = dataRepository.GetSession(phoneNumber);
            var selectionList = new List<SelectListItem>();
            foreach (var session in sessions)
                selectionList.Add(new SelectListItem { Text = session.Name, Value = session.Id.ToString() });

            ViewBag.Sessions = selectionList;
            return View();
        }

        [Route("/Get/{phoneNumber}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(string phoneNumber,[Bind("Number,Mobile,Session")]Token token)
        {
            if (ModelState.IsValid)
            {
                dataRepository.CreateToken(myAccountId,token.Session.Id,token);
                return View("Details",token);
            }
            var sessions = dataRepository.GetSession(phoneNumber);
            var selectionList = new List<SelectListItem>();
            foreach (var session in sessions)
                selectionList.Add(new SelectListItem { Text = session.Name, Value = session.Id.ToString() });

            ViewBag.Sessions = selectionList;
            return View(token);
        }
    }
}