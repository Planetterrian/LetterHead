using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LetterHeadServer.Models;

namespace LetterHeadServer.Controllers
{

    public class TooltipsController : BaseLetterHeadController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private bool IsAuthenticated()
        {
            return HttpContext.Session["authenticated"] != null && (bool)HttpContext.Session["authenticated"] == true;
        }

        // GET: Tooltips
        public ActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            return View(db.Tooltips.ToList());
        }

        // GET: Tooltips/Details/5
        public ActionResult Details(int? id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tooltip tooltip = db.Tooltips.Find(id);
            if (tooltip == null)
            {
                return HttpNotFound();
            }
            return View(tooltip);
        }

        // GET: Tooltips/Create
        public ActionResult Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            return View();
        }

        // POST: Tooltips/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Content")] Tooltip tooltip)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            if (ModelState.IsValid)
            {
                db.Tooltips.Add(tooltip);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tooltip);
        }

        // GET: Tooltips/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tooltip tooltip = db.Tooltips.Find(id);
            if (tooltip == null)
            {
                return HttpNotFound();
            }
            return View(tooltip);
        }

        // POST: Tooltips/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Content")] Tooltip tooltip)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            if (ModelState.IsValid)
            {
                db.Entry(tooltip).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tooltip);
        }

        // GET: Tooltips/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tooltip tooltip = db.Tooltips.Find(id);
            if (tooltip == null)
            {
                return HttpNotFound();
            }
            return View(tooltip);
        }

        // POST: Tooltips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Backend");
            }

            Tooltip tooltip = db.Tooltips.Find(id);
            db.Tooltips.Remove(tooltip);
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
