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
    public class AccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts1
        
        public ActionResult Index()
        {
            
            if (User.IsInRole("Recipient"))
            {
                int accountid = 0;
                var account = db.Accounts
                            .Where(r => r.Recipient == User.Identity.Name);
                foreach(Account a in account)
                {
                    accountid = a.Id;
                }

                return RedirectToAction("Details", new { id = accountid });
                
            }
            else
            {
                var accounts = db.Accounts
                    .Where(a => a.Username == User.Identity.Name);
                    
                return View(accounts.ToList());
            }
        }

        public PartialViewResult transactions(int?id)
        {
            
            var tran = db.Transactions
                .Where(t => t.AccountId == id);

            return PartialView(tran.ToList());
        }

        public PartialViewResult wishlist(int? id)
        {

            var wish = db.WishLists
                .Where(w => w.AccountId == id);

            return PartialView(wish.ToList());
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
            if(User.IsInRole("Owner"))
            { 
            if (account.Owner != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            }
            if (User.IsInRole("Recipient"))
            {
                if (account.Recipient != User.Identity.Name)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            CumulativeAmount(account.Id);
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
                if (emailr1 > 0)
                {
                    ModelState.AddModelError("Recipient", "Recipient is already an Owner and hence can't be a recipient.");
                }

                if(User.Identity.Name == account.Recipient)
                {
                    ModelState.AddModelError("Recipient", "Owner and Recipient Emails should be different.");
                }

                if (ModelState.IsValid)
                {
                    account.TotalAmount = 0;
                    account.Owner = User.Identity.Name;
                    account.OpenDate = DateTime.Now;
                    account.LastDeposit = Convert.ToDateTime("1/1/1900");
                   
                    account.Username = User.Identity.Name;
                    db.Accounts.Add(account);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(account);
        }

        // GET: Accounts/Edit/5
        public static float tamount;
        public static float iamount;
        public static DateTime tdate;
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
            if (account.Owner != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tamount = account.TotalAmount;
            iamount = account.InterestEarned;
            tdate = account.LastDeposit;
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
                //emailcheck(account);
                //if(ModelState.IsValid)
                //{ 
                db.Entry(account).State = EntityState.Modified;
                account.Owner = User.Identity.Name;
                account.Username = User.Identity.Name;
                account.InterestEarned = iamount;
                account.TotalAmount = tamount;
                account.LastDeposit = tdate;
                db.SaveChanges();
                iamount = 0;
                tamount = 0;
                return RedirectToAction("Index");
                //}
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
            if (account.Owner != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(account);
        }

        // POST: Accounts1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account acc = db.Accounts.Single(a => a.Id == id);
            if (acc.TotalAmount > 0)
            {
                //ViewBag.Error = TempData["Account has some amount and hence can't be deleted"];
                //return View();
                ModelState.AddModelError("Amount", "Account has non-zero balance and hence can't be deleted");
                return View(acc);
            }
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

        /// <summary>
        /// This metohd calculate interest accumulated over a period of time in a year.
        /// This one is keeping one year interestscope here, can extend it!
        /// </summary>
        /// <param name="accountId"></param>
        public void CumulativeAmount(int accountId)
        {
            DateTime today = DateTime.Today;
            int Currentyear = today.Year;
            double interestRate = 0;
            float interestAmount = 0;
            float amount = 0;
            double Days = 0;
            double interestperTransaction = 0;


            Account accts = db.Accounts
                           .Single(ac => ac.Id == accountId);
            interestRate = (double)accts.InterestRate;


            //considering monthly compounding
            var tran = db.Transactions
                   .Where(t => t.AccountId == accountId);

            int count = tran.ToList().Count;
            List<DateTime> trDate = new List<DateTime>();
            List<float> TrAmount = new List<float>();
            List<float> interest = new List<float>();

            foreach (Transaction tr in tran)
            {
                trDate.Add(tr.TransactionDate);
                TrAmount.Add(tr.Amount);
            }
            int size = TrAmount.Count();
            for (int i = 0; i < size; i++)
            {
                if (i == (size - 1))
                {
                    Days = (today - trDate[size - 1]).Days;
                }
                else
                {
                    Days = (trDate[i + 1] - trDate[i]).Days;
                }

                amount = amount + TrAmount[i];

                interestperTransaction = (double)(amount * Math.Pow(1 + interestRate / 1200, 12 * Days / 366)) - amount;
                amount = amount + (float)interestperTransaction;
                interestAmount = interestAmount + (float)interestperTransaction;
            }


            accts.InterestEarned = interestAmount;

            db.SaveChanges();

        }

        



    }
}
