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
    public class TransactionsController : Controller
    {

        static int? tflag = 0;
        static int? tpath = 0;
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public ActionResult Index()
        {
            int? tid;
            if(tpath==0)
            { 
            tid = tflag;
            tflag = 0;
            return RedirectToAction("Details", "Accounts", new { id = tid });
            }
            else
            {
                Transaction t = db.Transactions.Single(tr => tr.Id == tpath);
                Account ac = db.Accounts.Single(a => a.Id == t.AccountId);
                tpath = 0;
                tflag = 0;
                return RedirectToAction("Details", "Accounts", new { id = ac.Id });
            }
            //if(id != null)
            //{

            //    var transactions = db.Transactions
            //        .Where(t => t.AccountId == id);

            //    return View(transactions.ToList());
            //}

            //else
            //{
            //    var transactions = db.Transactions
            //        .Where(t => t.username == User.Identity.Name)
            //        .Include(t => t.account);

            //    return View(transactions.ToList());
            //}


        }

       

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            tpath = id;
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null )
            {
                return HttpNotFound();
            }
            if (transaction.username != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        [Authorize(Roles ="Owner")]
        public ActionResult Create(int? id)
        {
            
            tflag = id;
            
            var accounts = db.Accounts
                                .Where(a => a.Id == id);
                ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient");

            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles ="Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AccountId,TransactionDate,Amount,Note")] Transaction transaction)
        {
            
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Owner"))
                {
                    Account acc = db.Accounts.Single(a => a.Id ==transaction.AccountId);
                    if (acc.TotalAmount + transaction.Amount < 0)
                    {
                        ModelState.AddModelError("Amount", "You can't withdraw amount greater than the existing balance.");
                        var accounts = db.Accounts
                               .Where(a => a.Id == transaction.AccountId);
                        ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient");
                        
                        return View(transaction);
                    }
                    else
                    {
                        if (ModelState.IsValid)
                        { 
                        transaction.TransactionDate = DateTime.Now;
                        transaction.username = User.Identity.Name;
                        db.Transactions.Add(transaction);
                        db.SaveChanges();
                            
                            
                                UpdateBalance(transaction.AccountId, transaction.Amount);
                                                       
                            UpdateWishlist(transaction.AccountId);
                            return RedirectToAction("Details","Accounts", new { id = transaction.AccountId });
                        }
                    }
                }
                else
                {
                    transaction.TransactionDate = DateTime.Now;
                    transaction.username = User.Identity.Name;
                    db.Transactions.Add(transaction);
                    db.SaveChanges();
                    UpdateBalance(transaction.AccountId, transaction.Amount);
                    UpdateWishlist(transaction.AccountId);
                    //db.SaveChanges();
                    return RedirectToAction("Details","Accounts", new { id = transaction.AccountId });
                    
                    
                    
                }
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Recipient", transaction.AccountId);
            return View(transaction);
        }

        public void UpdateWishlist(int id)
        {
            Account ac = db.Accounts.Single(a => a.Id == id);
            var wish = db.WishLists.Where(w => w.AccountId == id);
            foreach (WishList w in wish)
            {
                if(w.Cost>ac.TotalAmount)
                {
                    w.Purchasable = 0;
                }
                else
                {
                    w.Purchasable = 1;
                }
                
            }
            db.SaveChanges();
        }

        public void UpdateBalance( int id, float amount)
        {
            float TotalAmount = 0;
            var tran = db.Transactions
                   .Where(t => t.AccountId == id);
            foreach (Transaction tr in tran)
            {
                TotalAmount = TotalAmount + tr.Amount;
            }
            Account accts = db.Accounts
                            .Single(ac => ac.Id == id);
            accts.TotalAmount = TotalAmount;
            //int count = tran.Count();
            //List<DateTime> trDate = new List<DateTime>();
            //List<float> TrAmount = new List<float>();
            //List<float> interest = new List<float>();

            //foreach (Transaction tr in tran)
            //{
            //    trDate.Add(tr.TransactionDate);
            //    TrAmount.Add(tr.Amount);
            //}
            //for(int i = count-1; i>0; i--)
            //{
            //    if(TrAmount[i]>0)
            //    {
            //        accts.LastDeposit = trDate[i];
            //        break;
            //    }
            //    else
            //    {
            //        continue;
            //    }
            //}
            if(amount>0)
            {
                accts.LastDeposit = DateTime.Now;
            }
            db.SaveChanges();
        }

        static float flagamount = 0;
        //static float flagamount1 = 0;
        [Authorize(Roles = "Owner")]
        // GET: Transactions/Edit/5
        
        public ActionResult Edit(int? id)
        {
            tpath = id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            if (transaction.username != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var accounts = db.Accounts
                    .Where(a => a.Id == transaction.AccountId);
            //ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient", transaction.AccountId);

            
            flagamount = transaction.Amount;
            ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient", transaction.AccountId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner")]
        public ActionResult Edit([Bind(Include = "Id,AccountId,TransactionDate,Amount,Note")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                if (transaction.Amount != flagamount)
                {
                    ModelState.AddModelError("Amount", "You can't change the amount once its posted.");
                    var accounts = db.Accounts
                           .Where(a => a.Id == transaction.AccountId);
                    ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient");
                    return View(transaction);
                    //Account acc = db.Accounts.Single(ac => ac.Id == transaction.AccountId);
                }                
                    if (ModelState.IsValid)
                    {
                        transaction.username = User.Identity.Name;
                        db.SaveChanges();
                        //UpdateBalance(transaction.AccountId);
                    }
                
                else { 
                transaction.username = User.Identity.Name;
                db.SaveChanges();
                }
                flagamount = 0;
                //flagamount1 = 0;
                return RedirectToAction("Details","Accounts", new { id = transaction.AccountId });
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Owner", transaction.AccountId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        [Authorize(Roles = "Owner")]
        public ActionResult Delete(int? id)
        {
            tpath = id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            if (transaction.username != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner")]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            Account acct = db.Accounts.Single(a => a.Id == transaction.AccountId);
            acct.TotalAmount = acct.TotalAmount - transaction.Amount;
            int ida = transaction.AccountId;
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            //UpdateBalance(ida, id);
            return RedirectToAction("Details","Accounts", new { id = ida });
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
