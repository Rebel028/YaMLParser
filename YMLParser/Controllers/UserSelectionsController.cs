using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using YMLParser.Models;

namespace YMLParser.Controllers
{
    [Authorize]
    public class UserSelectionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager { get; set; }
        private ApplicationUser CurrentUser { get; set; }
        private  UserSelection CurrentUserSelection { get; set; }

        private void GetCurrentUserInfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                CurrentUser = UserManager.FindById(User.Identity.GetUserId());
            }
        }

        private void CreateUserSelection()
        {
            if (CurrentUser != null)
            {
                CurrentUserSelection = new UserSelection
                {
                    UserId = CurrentUser.Id
                };
                db.UserSelections.Add(CurrentUserSelection);
                db.SaveChanges();
            }
        }

        private bool UserSelectionExists()
        {
            if (CurrentUser!=null)
            {
                if (db.UserSelections.Where(s => s.UserId == CurrentUser.Id).Count()>0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // GET: UserSelections
        [Authorize]
        public async Task<ActionResult> Index()
        {

            return View(await db.UserSelections.ToListAsync());
        }

        // GET: UserSelections/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserSelection userSelection = await db.UserSelections.FindAsync(id);
            if (userSelection == null)
            {
                return HttpNotFound();
            }
            return View(userSelection);
        }

        // GET: UserSelections/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserSelections/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,UserId")] UserSelection userSelection)
        {
            if (ModelState.IsValid)
            {
                db.UserSelections.Add(userSelection);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(userSelection);
        }

        // GET: UserSelections/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserSelection userSelection = await db.UserSelections.FindAsync(id);
            if (userSelection == null)
            {
                return HttpNotFound();
            }
            return View(userSelection);
        }

        // POST: UserSelections/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,UserId")] UserSelection userSelection)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userSelection).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(userSelection);
        }

        // GET: UserSelections/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserSelection userSelection = await db.UserSelections.FindAsync(id);
            if (userSelection == null)
            {
                return HttpNotFound();
            }
            return View(userSelection);
        }

        // POST: UserSelections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            UserSelection userSelection = await db.UserSelections.FindAsync(id);
            db.UserSelections.Remove(userSelection);
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
