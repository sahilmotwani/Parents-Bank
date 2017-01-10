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
    public class WishListsController : Controller
    {
        static int? wflag=0;
        static int? wpath=0;
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: WishLists
        public ActionResult Index()
        {
            int? wid;
            if(wpath==0)
            { 
            wid= wflag;
            wflag = 0;
            return RedirectToAction("Details", "Accounts", new { id = wid });
            }
            else
            {
                WishList w = db.WishLists.Single(wi => wi.Id == wpath);
                Account ac = db.Accounts.Single(a => a.Id == w.AccountId);
                wpath = 0;
                wflag = 0;
                return RedirectToAction("Details", "Accounts", new { id = ac.Id });
            }
        }

        // GET: WishLists/Details/5
        public ActionResult Details(int? id)
        {
            wpath = id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WishList wishList = db.WishLists.Find(id);
            if (wishList == null)
            {
                return HttpNotFound();
            }
            Account aa = db.Accounts.Single(a => a.Id == wishList.AccountId);
            if(User.IsInRole("Owner"))
            { 
            if (wishList.Owner != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            }
            if(User.IsInRole("Recipient"))
            { 
            if (aa.Recipient != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            }
            return View(wishList);
        }

        // GET: WishLists/Create
        public ActionResult Create(int?id)
        {
            wflag = id;                     
                var accounts = db.Accounts
                               .Where(a => a.Id == id);
                ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient");
            
                      
            return View();
        }

        // POST: WishLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AccountId,DateAdded,Cost,Description,Link,Purchased")] WishList wishList)
        {
            if (ModelState.IsValid)
            {
                Account acct = db.Accounts.Single(a => a.Id == wishList.AccountId);
                if (wishList.Purchased)
                {
                    
                    if (acct.TotalAmount < wishList.Cost)
                    {
                        ModelState.AddModelError("Cost", "You don't have enough funds to buy this product.");
                        var accountr = db.Accounts
                            .Where(a => a.Id == wishList.AccountId);
                        ViewBag.AccountId = new SelectList(accountr, "Id", "Recipient");
                        return View(wishList);
                    }
                    if (wishList.Cost <= 0)
                    {
                        ModelState.AddModelError("Cost", "Cost of the item can't be zero or less.");
                        var accountr = db.Accounts
                            .Where(a => a.Id == wishList.AccountId);
                        ViewBag.AccountId = new SelectList(accountr, "Id", "Recipient");
                        return View(wishList);
                    }
                    if (ModelState.IsValid)
                    {
                        db.WishLists.Add(wishList);
                       
                        UpdateBalanceWish(acct.Id, wishList.Cost, wishList.Description);
                        wishList.Owner = acct.Owner;
                        wishList.DateAdded = DateTime.Now;
                        wishList.Purchasable = 2;
                        db.SaveChanges();
                        return RedirectToAction("Details","Accounts",new { id = wishList.AccountId });
                        
                    }
                }
                else
                {
                    db.WishLists.Add(wishList);
                    wishList.Owner = acct.Owner;
                    if(acct.TotalAmount>wishList.Cost)
                    { 
                    wishList.Purchasable = 1;
                    }
                    else
                    {
                        wishList.Purchasable = 0;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Details","Accounts", new { id = wishList.AccountId });
                }
                               
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Recipient", wishList.AccountId);
            return View(wishList);
        }

        public void UpdateBalanceWish(int id, float cost, string desc)
        {
            Account accts = db.Accounts
                            .Single(ac => ac.Id == id);
            accts.TotalAmount =accts.TotalAmount - cost;
            //db.SaveChanges();

            var Tranid = db.Transactions.Max(tid => tid.Id);
            Tranid = Tranid++;
            var NewTran = new Transaction();
            NewTran.Id = Tranid;
            NewTran.Amount = -cost;
            NewTran.AccountId = id;
            NewTran.Note = "Brought from wishlist" + " "+ desc;
            NewTran.TransactionDate = DateTime.Now;
            NewTran.username = accts.Owner;
            
            db.Transactions.Add(NewTran);
            //db.SaveChanges();
        }



        // GET: WishLists/Edit/5
        public static float cost = 0;
        public static int flag = 0;
        public static int pur = 0;
        public ActionResult Edit(int? id)
        {
            wpath = id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WishList wishList = db.WishLists.Find(id);
            if (wishList == null)
            {
                return HttpNotFound();
            }
            Account aa = db.Accounts.Single(a => a.Id == wishList.AccountId);
            if (User.IsInRole("Owner"))
            {
                if (wishList.Owner != User.Identity.Name)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            if (User.IsInRole("Recipient"))
            {
                if (aa.Recipient != User.Identity.Name)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            if (wishList.Purchased)
            {
                flag = 1;
            }
            pur = wishList.Purchasable;
            cost = wishList.Cost;
            var acct = db.Accounts.Where(acc => acc.Id == wishList.AccountId);
            ViewBag.AccountId = new SelectList(acct, "Id", "Recipient", wishList.AccountId);
            return View(wishList);
            
        }

        // POST: WishLists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AccountId,DateAdded,Cost,Description,Link,Purchased")] WishList wishList)
        {
            if (ModelState.IsValid)
            {
                
                if(flag == 0 && wishList.Purchased)
                {
                    Account acct = db.Accounts.Single(a => a.Id == wishList.AccountId);
                    if(acct.TotalAmount < wishList.Cost)
                    {
                        ModelState.AddModelError("Cost", "You don't have enough funds to buy this product.");
                        
                        var accounts = db.Accounts
                          .Where(a => a.Id == wishList.AccountId);
                        ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient", wishList.AccountId);
                        
                        return View(wishList);
                        
                    }
                    if (wishList.Cost<=0)
                    {
                        ModelState.AddModelError("Cost", "Cost of the item can't be zero or less.");

                        var accounts = db.Accounts
                          .Where(a => a.Id == wishList.AccountId);
                        ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient", wishList.AccountId);

                        return View(wishList);

                    }
                    if (ModelState.IsValid)
                    {
                        wishList.Purchasable = 2;
                        db.Entry(wishList).State = EntityState.Modified;
                        db.SaveChanges();
                        UpdateBalanceWish(wishList.AccountId, wishList.Cost, wishList.Description);
                    }
                }
                if(flag==1)
                {if(!wishList.Purchased) 
                 {
                    ModelState.AddModelError("Purchased", "State of item once purchased can never be changed.");

                    var accounts = db.Accounts
                      .Where(a => a.Id == wishList.AccountId);
                    ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient", wishList.AccountId);

                    return View(wishList);
                }
                if (cost != wishList.Cost)
                {
                    ModelState.AddModelError("Cost", "Cost of item once purchased can never be changed.");

                    var accounts = db.Accounts
                      .Where(a => a.Id == wishList.AccountId);
                    ViewBag.AccountId = new SelectList(accounts, "Id", "Recipient", wishList.AccountId);

                    return View(wishList);
                }
                if (ModelState.IsValid)
                {
                    Account acct1 = db.Accounts.Single(a => a.Id == wishList.AccountId);
                    wishList.Owner = acct1.Owner;
                    wishList.Purchasable = pur;
                    db.Entry(wishList).State = EntityState.Modified;
                    db.SaveChanges();
                }
                }
                db.SaveChanges();
                flag = 0;
                    pur = 0;
                    cost = 0;

                return RedirectToAction("Details","Accounts", new { id = wishList.AccountId });
                
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Owner", wishList.AccountId);
            return View(wishList);
        }

        // GET: WishLists/Delete/5
        [Authorize(Roles ="Owner")]
        public ActionResult Delete(int? id)
        {
            wpath = id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WishList wishList = db.WishLists.Find(id);
            if (wishList == null)
            {
                return HttpNotFound();
            }
            Account aa = db.Accounts.Single(a => a.Id == wishList.AccountId);
            if (wishList.Owner != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(wishList);
        }

        // POST: WishLists/Delete/5
        [Authorize(Roles ="Owner")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            WishList wishList = db.WishLists.Find(id);
            Account acc = db.Accounts.Single(a => a.Id == wishList.AccountId);
            db.WishLists.Remove(wishList);
            db.SaveChanges();
            return RedirectToAction("Details","Accounts", new { id = acc.Id});
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
