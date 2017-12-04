using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project22.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Project22.Controllers
{
    [Route("[controller]/[action]/{id?}")]
    public class SessionController : Controller
    {
        DataRepository dataRepository;

        public SessionController(DataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View(dataRepository.GetSessions(GetAccountId()));
        }

        [Authorize]
        public IActionResult Details(int id)
        {
            return View(dataRepository.GetSession(id));
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name,Mobile,StartTime")]Session session)
        {
            if (ModelState.IsValid)
            {
                session.AccountId = GetAccountId();
                dataRepository.CreateSession(session);
                return RedirectToAction(nameof(Index));
            }
            return View(session);
        }

        // GET: Sessions/Edit/5
        [Authorize]
        public IActionResult Edit(int id)
        {
            var session = dataRepository.GetSession(id);
            if (session == null)
            {
                return NotFound();
            }
            return View(session);
        }

        // POST: Sessions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name,Mobile,StartTime,TokenCount,AccountId")] Session session)
        {
            if (id != session.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dataRepository.UpdateSession(session);
                }
                catch (DbUpdateConcurrencyException e)
                {
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(session);
        }

        private int GetAccountId()
        {
            var id = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Sid).FirstOrDefault();
            return (id==null)?-1:int.Parse(id.Value);
        }

    }
}