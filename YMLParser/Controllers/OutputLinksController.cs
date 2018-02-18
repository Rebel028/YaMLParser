using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YMLParser.Models;

namespace YMLParser.Controllers
{
    public class OutputLinksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager { get; set; }
        private ApplicationUser CurrentUser { get; set; }
        private UserSelection CurrentUserSelection { get; set; }

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
            //var links = db.OutputLinks.Include(p => p.SelectedProviders);

            ViewBag.Providers = providers.ToList();
            //ViewBag.Links = links.ToList();
        }

        // GET: OutputLinks
        public async Task<ActionResult> Index()
        {
            GetCurrentUserInfo();
            GetUserSelection();

            var outputLinks = db.OutputLinks.Where(o => o.UserSelection == CurrentUserSelection);
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
            return PartialView(outputLink);
        }

        [HttpPost]
        public async Task<ActionResult> Details(OutputLink outputLink)
        {
            if (outputLink == null)
            {
                return HttpNotFound();
            }
            return PartialView(outputLink);
        }

        // GET: OutputLinks/Create
        public ActionResult Create()
        {
            GetCurrentUserInfo();
            GetUserSelection();

            var vendorsList = CurrentUserSelection.AddedProviders;
            ViewBag.Categories = db.Providers.Include(p => p.Categories).ToList();
            ViewBag.Providers = vendorsList.ToList();

            ViewBag.UserSelectionId = new SelectList(db.UserSelections, "Id", "UserId");

            return View(CurrentUserSelection);
        }

        // POST: OutputLinks/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,UserSelectionId")] OutputLink outputLink,
            List<string> selected)
        {
            if (ModelState.IsValid && selected.Count > 0)
            {
                GetCurrentUserInfo();
                GetUserSelection();

                try
                {
                    outputLink.Selected = string.Join(";", selected.ToArray());

                    var parser = new Parser(db);
                    var output = parser.SelectCategories(outputLink.SelectedLookup);

                    var file = parser.SaveFile(output);

                    outputLink.UserSelection = CurrentUserSelection;
                    outputLink.File = file;
                    outputLink.Name = outputLink.Name.Contains("_") ? outputLink.Name.Replace("_", string.Empty) : outputLink.Name;
                    db.OutputLinks.Add(outputLink);

                    await db.SaveChangesAsync();//получаем для ссылки id
                    outputLink.Link = this.Url.Action("Link", "OutputLinks", new { id = outputLink.Id + "_" + outputLink.Name }, Request.Url.Scheme);
                    db.Entry(outputLink).State = EntityState.Modified;
                    CurrentUserSelection.ExistingLinks.Add(outputLink);
                    await db.SaveChangesAsync();

                    return PartialView("Details", outputLink);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            ViewBag.UserSelectionId = new SelectList(db.UserSelections, "Id", "UserId", outputLink.UserSelectionId);
            return HttpNotFound();
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
            return RedirectToAction("Index", "UserSelections");
        }


        public async Task<ActionResult> Link(string id)
        {
            if (string.IsNullOrEmpty(id)) return HttpNotFound();
            var pars = id.Split('_');
            var linkId = Int32.Parse(pars[0]);
            var link = db.OutputLinks
                .Include(l => l.File)
                .FirstOrDefault(l => l.Id == linkId);
            var file = link.File;

            if (file == null || link.Name != pars[1])
            {
                return HttpNotFound();
            }
            var fileContent = System.IO.File.ReadAllBytes(file.FilePath);

            return File(fileContent, file.FileType);
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
