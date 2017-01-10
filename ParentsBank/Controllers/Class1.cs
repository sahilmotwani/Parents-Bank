using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ParentsBank.Models;

namespace ParentsBank.Controllers
{
    [Authorize]
    public class Accounts1Controller : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts1
        public ActionResult Index()
        {
            if (User.IsInRole("Owner"))
            {
                var accounts = db.Accounts.Where(a => a.Owner == User.Identity.Name);
                return View(db.Accounts.ToList());
            }
            else
            {
                var accounts = db.Accounts.Where(a => a.Recipient == User.Identity.Name);
                return View(db.Accounts.ToList());
            }
        }

        // GET: Accounts1/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // GET: Accounts1/Create
        [Authorize(Roles = "Owner")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Accounts1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Owner,Recipient,Name,OpenDate,InterestRate,Description")] Account account)
        {
            if (ModelState.IsValid)
            {
                int emailr = db.Accounts
                                           .Where(a => a.Recipient == account.Recipient)
                                           .Count();
                if (emailr > 0)
                {
                    ModelState.AddModelError("Recipient", "Recipient already has an account.");
                }

                int emailr1 = db.Accounts
                                           .Where(a => a.Owner == account.Recipient)
                                           .Count();
                if (emailr > 0)
                {
                    ModelState.AddModelError("Recipient", "Recipient is already an Owner and hence can't be a recipient.");
                }

                if (ModelState.IsValid)
                {

                    db.Accounts.Add(account);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(account);
        }

        // GET: Accounts/Edit/5
        [Authorize(Roles = "Owner")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Accounts1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Owner,Recipient,Name,OpenDate,InterestRate,Description")] Account account)
        {
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(account);
        }

        // GET: Accounts1/Delete/5
        [Authorize(Roles = "Owner")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Accounts1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Accounts.Find(id);
            db.Accounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
