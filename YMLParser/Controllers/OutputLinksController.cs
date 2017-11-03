using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using YMLParser.Models;

namespace YMLParser.Controllers
{
    public class OutputLinksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: OutputLinks
        public async Task<ActionResult> Index()
        {
            var outputLinks = db.OutputLinks.Include(o => o.UserSelection);
            return PartialView(await outputLinks.ToListAsync());
        }

        // GET: OutputLinks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OutputLink outputLink = await db.OutputLinks.FindAsync(id);
            if (outputLink == null)
            {
                return HttpNotFound();
            }
            return View(outputLink);
        }

        // GET: OutputLinks/Create
        public ActionResult Create()
        {
            ViewBag.UserSelectionId = new SelectList(db.UserSelections, "Id", "UserId");
            return View();
        }

        // POST: OutputLinks/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Link,UserSelectionId")] OutputLink outputLink)
        {
            if (ModelState.IsValid)
            {
                db.OutputLinks.Add(outputLink);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.UserSelectionId = new SelectList(db.UserSelections, "Id", "UserId", outputLink.UserSelectionId);
            return View(outputLink);
        }

        // GET: OutputLinks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OutputLink outputLink = await db.OutputLinks.FindAsync(id);
            if (outputLink == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserSelectionId = new SelectList(db.UserSelections, "Id", "UserId", outputLink.UserSelectionId);
            return View(outputLink);
        }

        // POST: OutputLinks/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Link,UserSelectionId")] OutputLink outputLink)
        {
            if (ModelState.IsValid)
            {
                db.Entry(outputLink).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.UserSelectionId = new SelectList(db.UserSelections, "Id", "UserId", outputLink.UserSelectionId);
            return View(outputLink);
        }

        // GET: OutputLinks/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OutputLink outputLink = await db.OutputLinks.FindAsync(id);
            if (outputLink == null)
            {
                return HttpNotFound();
            }
            return View(outputLink);
        }

        // POST: OutputLinks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            OutputLink outputLink = await db.OutputLinks.FindAsync(id);
            db.OutputLinks.Remove(outputLink);
            await db.SaveChangesAsync();
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
