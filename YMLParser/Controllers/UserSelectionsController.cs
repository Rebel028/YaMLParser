using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YMLParser.Models;

namespace YMLParser.Controllers
{
    [Authorize]
    public class UserSelectionsController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

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
                    _db.UserSelections.Add(CurrentUserSelection);
                    _db.SaveChanges();
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
            CurrentUserSelection = _db.UserSelections.First(s => s.UserId == CurrentUser.Id);
            var providers = _db.Providers.Include(p => p.UserSelections);
            var links = _db.OutputLinks.Include(p => p.UserSelection);

            ViewBag.Providers = providers.ToList();
            ViewBag.Links = CurrentUserSelection.ExistingLinks.ToList();
            ViewBag.Link = links.ToList();
        }

        private bool UserSelectionExists()
        {
            if (CurrentUser != null && _db.UserSelections.Any(s => s.UserId == CurrentUser.Id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private ApplicationUser CurrentUser { get; set; }
        private UserSelection CurrentUserSelection { get; set; }

        private ApplicationUserManager UserManager { get; set; }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
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
                provider.Link = provider.Link.Trim();

                Provider newProvider;
                //пробиваем поставщика по базе
                if (_db.Providers.Any(p => p.Link == provider.Link))//если встречается в БД
                {
                    newProvider = await _db.Providers.FirstOrDefaultAsync(p => p.Link == provider.Link);
                    if (!newProvider.UserSelections.Contains(CurrentUserSelection))
                    {
                        newProvider.UserSelections.Add(CurrentUserSelection);
                        CurrentUserSelection.AddedProviders.Add(newProvider);
                        _db.Entry(newProvider).State = EntityState.Modified;
                        _db.Entry(CurrentUserSelection).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                }
                else //если нет
                {
                    newProvider = provider;
                    _db.Providers.Add(newProvider);
                    _db.SaveChanges();

                    //запускаем парсер
                    Parser parser = new Parser(_db);
                    //получаем физический файл
                    var output = await parser.ParseInitialFile(provider.Link);
                    if (output == null)
                    {
                        _db.Providers.Remove(newProvider);
                        _db.Entry(provider).State = EntityState.Deleted;
                        _db.SaveChanges();
                        return Content("Ссылка на XML не верна!");
                    }
                    newProvider.Name = output.Vendor;
                    newProvider.MainOutputFile = output;
                    newProvider.UserSelections.Add(CurrentUserSelection);

                    //парсим полученные из файла категории и добавляем их куда надо
                    parser.ParseAllCategories(output.Categories.Values.ToList(), newProvider);

                    CurrentUserSelection.AddedProviders.Add(newProvider);
                    _db.Entry(CurrentUserSelection).State = EntityState.Modified;
                    _db.Entry(newProvider).State = EntityState.Modified;
                    _db.SaveChanges();
                }
            }

            return RedirectToAction("Index", CurrentUserSelection);
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

            Provider provider = await _db.Providers.FindAsync(id);

            CurrentUserSelection.AddedProviders.Remove(provider);
            provider.UserSelections.Remove(CurrentUserSelection);
            //удаляем поставщика если больше никем не используется
            if (provider.UserSelections.Count < 1)
            {
                _db.Entry(provider).State = EntityState.Deleted;
                var remove = _db.OutputFiles.Where(f => f.Vendor == provider.Name);
                _db.OutputFiles.RemoveRange(remove);
            }
            else
            {
                _db.Entry(provider).State = EntityState.Modified;
            }

            _db.Entry(CurrentUserSelection).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
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
            Provider provider = _db.Providers.Include(p => p.Categories).FirstOrDefault(p => p.Id == id);
            if (provider == null || !provider.UserSelections.Contains(CurrentUserSelection))
            {
                return HttpNotFound();
            }
            ViewBag.Categories = provider.Categories.ToList();
            return View(provider);
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

        public async Task<ActionResult> GetFile(int? id)
        {
            if (id != null)
            {
                var file = await _db.OutputFiles.FindAsync(id);

                if (file != null)
                {
                    var fileContent = System.IO.File.ReadAllBytes(file.FilePath);

                    return File(fileContent, file.FileType);
                }
            }
            return HttpNotFound();
        }
    }
}
