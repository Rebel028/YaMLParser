using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
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

        private void GetUserSelection()
        {
            CurrentUserSelection = db.UserSelections.First(s => s.UserId == CurrentUser.Id);
            var providers = db.Providers.Include(p => p.UserSelections);
            var links = db.OutputLinks.Include(p => p.SelectedProviders);

            ViewBag.Providers = providers.ToList();
            ViewBag.Links = links.ToList();

            //if (CurrentUserSelection.AddedProviders == null)
            //{
            //    CurrentUserSelection.AddedProviders = new List<Provider>();
            //}
            //if (CurrentUserSelection.ExistingLinks == null)
            //{
            //    CurrentUserSelection.ExistingLinks = new List<OutputLink>();
            //}
        }

        private void CreateUserSelection()
        {
            try
            {
                if (CurrentUser != null)
                {
                    CurrentUserSelection = new UserSelection
                    {
                        UserId = CurrentUser.Id,
                        AddedProviders = new List<Provider>(),
                        ExistingLinks = new List<OutputLink>()
                    };
                    db.UserSelections.Add(CurrentUserSelection);
                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }
        }

        private bool UserSelectionExists()
        {
            if (CurrentUser != null && db.UserSelections.Any(s => s.UserId == CurrentUser.Id))
            {
                return true;
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
            GetCurrentUserInfo();

            if (!UserSelectionExists())
            {
                CreateUserSelection();
            }
            else
            {
                GetUserSelection();
            }

            return View(CurrentUserSelection);
        }

        // GET: UserSelections/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            GetCurrentUserInfo();
            GetUserSelection();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (CurrentUserSelection == null)
            {
                return HttpNotFound();
            }
            Provider provider = CurrentUserSelection.AddedProviders.First(p => p.Id == id);
            if (provider == null)
            {
                return HttpNotFound();
            }
            return View(provider);
        }

        // POST: UserSelections/add
        [HttpPost]
        public ActionResult Add()
        {
            return PartialView("Create");
        }

        // POST: UserSelections/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Provider provider)
        {
            GetCurrentUserInfo();
            GetUserSelection();

            if (ModelState.IsValid)
            {
                Provider newProvider;
                if (!db.Providers.Any(p=>p.Link==provider.Link))
                {
                    db.Providers.Add(provider);
                    newProvider = provider;
                    db.SaveChanges();
                }
                else
                {
                    newProvider = db.Providers.First(p => p.Link == provider.Link);
                }

                newProvider.UserSelections.Add(CurrentUserSelection);
                CurrentUserSelection.AddedProviders.Add(newProvider);

                db.Entry(CurrentUserSelection).State = EntityState.Modified;
                db.Entry(newProvider).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", CurrentUserSelection);
            }

            return PartialView(provider);
        }

        //// GET: UserSelections/Edit/5
        //public async Task<ActionResult> EditProvider(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Provider provider = CurrentUserSelection.AddedProviders.First(p => p.Id == id);
        //    UserSelection userSelection = await db.UserSelections.FindAsync(id);
        //    if (userSelection == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userSelection);
        //}


        //// POST: UserSelections/Edit/5
        //// Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        //// сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,UserId")] UserSelection userSelection)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(userSelection).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(userSelection);
        //}

        // GET: UserSelections/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            GetCurrentUserInfo();
            GetUserSelection();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (CurrentUserSelection == null)
            {
                return HttpNotFound();
            }
            Provider provider = CurrentUserSelection.AddedProviders.First(p => p.Id == id);
            if (provider == null)
            {
                return HttpNotFound();
            }
            return View(provider);
        }

        // POST: UserSelections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            GetCurrentUserInfo();
            GetUserSelection();

            Provider provider = await db.Providers.FindAsync(id);

            CurrentUserSelection.AddedProviders.Remove(provider);
            provider.UserSelections.Remove(CurrentUserSelection);
            //удаляем поставщика если больше никем не используется
            db.Entry(provider).State = provider.UserSelections.Count < 1 ? EntityState.Deleted : EntityState.Modified;

            db.Entry(CurrentUserSelection).State = EntityState.Modified;
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
