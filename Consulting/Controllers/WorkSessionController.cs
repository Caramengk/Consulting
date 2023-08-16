using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Consulting.Models;
using Microsoft.AspNetCore.Http;

namespace Consulting.Controllers
{
    public class WorkSessionController : Controller
    {
        private readonly ConsultingContext _context;

        public WorkSessionController(ConsultingContext context)
        {
            _context = context;
        }

        // GET: WorkSession
        public async Task<IActionResult> Index(string ContractID)
        {
            if (!string.IsNullOrEmpty(ContractID))
            {
                Response.Cookies.Append("ContractID", ContractID);
                HttpContext.Session.SetString("ContractID", ContractID);
            }
            else if (Request.Query["ContractID"].Any())
            {
                ContractID = Request.Query["ContractID"].ToString();
                Response.Cookies.Append("ContractID", ContractID);
                HttpContext.Session.SetString("ContractID", ContractID);
            }
            else if (Request.Cookies["ContractID"] != null)
            {
                ContractID = Request.Cookies["ContractID"].ToString();
            }
            else if (HttpContext.Session.GetString("ContractID")!= null)
            {
                ContractID = HttpContext.Session.GetString("ContractID");
            }
            else
            {
                TempData["message"] = "Please select Contract";
                return RedirectToAction("Index", "Contract");
            }

            var consultingContext = _context.WorkSession.Include(w => w.Consultant).Include(w => w.Contract)
                                    .Where(a=>a.ContractId == Convert.ToInt32(ContractID));

            var workSessions = consultingContext.ToList();
            ViewBag.totalhours = workSessions.Sum(a=>a.HoursWorked);
            ViewBag.totalcost = workSessions.Sum(a=>a.HoursWorked * a.HourlyRate);

            return View(await consultingContext.ToListAsync());
        }

        // GET: WorkSession/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.WorkSession == null)
            {
                return NotFound();
            }

            var workSession = await _context.WorkSession
                .Include(w => w.Consultant)
                .Include(w => w.Contract)
                .FirstOrDefaultAsync(m => m.WorkSessionId == id);
            if (workSession == null)
            {
                return NotFound();
            }

            return View(workSession);
        }

        // GET: WorkSession/Create
        public IActionResult Create()
        {
            string contract = string.Empty;
            if (Request.Cookies["ContractID"] != null)
            {
                contract = Request.Cookies["ContractID"].ToString();
            }
            else if(HttpContext.Session.GetString("ContractID") != null)
            {
                contract = HttpContext.Session.GetString("ContractID");
            }

            ViewBag.CID = contract;

            ViewData["ConsultantId"] = new SelectList(_context.Consultant, "ConsultantId", "FirstName");
            ViewData["ContractId"] = new SelectList(_context.Contract, "ContractId", "Name");
            return View();
        }

        // POST: WorkSession/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkSessionId,ContractId,DateWorked,ConsultantId,HoursWorked,WorkDescription,HourlyRate,ProvincialTax,TotalChargeBeforeTax")] WorkSession workSession)
        {
            try
            {
                string contract = string.Empty;
                if (Request.Cookies["ContractID"] != null)
                {
                    contract = Request.Cookies["ContractID"].ToString();
                }
                else if (HttpContext.Session.GetString("ContractID") != null)
                {
                    contract = HttpContext.Session.GetString("ContractID");
                }
                ViewBag.CID = contract;

                if (ModelState.IsValid)
                {
                    _context.Add(workSession);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Created Successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.GetBaseException().Message);
                TempData["message"] = ex.GetBaseException().Message;
            }
            
            ViewData["ConsultantId"] = new SelectList(_context.Consultant, "ConsultantId", "FirstName", workSession.ConsultantId);
            ViewData["ContractId"] = new SelectList(_context.Contract, "ContractId", "Name", workSession.ContractId);
            return View(workSession);
        }

        // GET: WorkSession/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WorkSession == null)
            {
                return NotFound();
            }

            var workSession = await _context.WorkSession.FindAsync(id);
            if (workSession == null)
            {
                return NotFound();
            }
            ViewData["ConsultantId"] = new SelectList(_context.Consultant, "ConsultantId", "FirstName", workSession.ConsultantId);
            ViewData["ContractId"] = new SelectList(_context.Contract, "ContractId", "Name", workSession.ContractId);
            return View(workSession);
        }

        // POST: WorkSession/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkSessionId,ContractId,DateWorked,ConsultantId,HoursWorked,WorkDescription,HourlyRate,ProvincialTax,TotalChargeBeforeTax")] WorkSession workSession)
        {
            if (id != workSession.WorkSessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workSession);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Edit was successful";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkSessionExists(workSession.WorkSessionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.GetBaseException().Message);
                    TempData["message"] = ex.GetBaseException().Message;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConsultantId"] = new SelectList(_context.Consultant, "ConsultantId", "FirstName", workSession.ConsultantId);
            ViewData["ContractId"] = new SelectList(_context.Contract, "ContractId", "Name", workSession.ContractId);
            return View(workSession);
        }

        // GET: WorkSession/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WorkSession == null)
            {
                return NotFound();
            }

            var workSession = await _context.WorkSession
                .Include(w => w.Consultant)
                .Include(w => w.Contract)
                .FirstOrDefaultAsync(m => m.WorkSessionId == id);
            if (workSession == null)
            {
                return NotFound();
            }

            return View(workSession);
        }

        // POST: WorkSession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.WorkSession == null)
                {
                    return Problem("Entity set 'ConsultingContext.WorkSession'  is null.");
                }
                var workSession = await _context.WorkSession.FindAsync(id);
                if (workSession != null)
                {
                    _context.WorkSession.Remove(workSession);
                }
            
                await _context.SaveChangesAsync();
                TempData["message"] = "Delete was Successful";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.GetBaseException().Message);
                TempData["message"] = ex.GetBaseException().Message;
            }
            return View("Delete");
           
        }

        private bool WorkSessionExists(int id)
        {
          return _context.WorkSession.Any(e => e.WorkSessionId == id);
        }
    }
}
