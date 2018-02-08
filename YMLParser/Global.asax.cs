using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using YMLParser.Models;

namespace YMLParser
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RemoveTemporaryFiles();
            RefreshProviders();
            RefreshLinks();
            RefreshSchedule();
        }

        public void RemoveTemporaryFiles()
        {
            string pathTemp = ".\\App_Data\\";
            if ((pathTemp.Length > 0) && (Directory.Exists(pathTemp)))
            {
                foreach (string file in Directory.GetFiles(pathTemp))
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(-3))
                        {
                            using (var db = new ApplicationDbContext())
                            {
                                var fileOutput = db.OutputFiles.First(f=>f.FilePath == fi.FullName);
                                db.OutputFiles.Remove(fileOutput);
                                db.SaveChanges();
                            }
                            File.Delete(file);
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        public void RefreshProviders()
        {
            using (var db = new ApplicationDbContext())
            {
                var providers = db.Providers;
                if (providers != null)
                {
                    foreach (Provider provider in providers.ToList())
                    {
                        //запускаем парсер
                        Parser parser = new Parser(db);
                        var output = parser.ParseInitialFile(provider.Link).Result;
                        if (output == null) continue;
                        //парсим категории если что-то поменялось
                        parser.ParseAllCategories(output.Categories.Values.ToList(), provider);
                        provider.MainOutputFile = output;
                        db.SaveChanges();
                    }
                }
            }
        }

        public void RefreshLinks()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                foreach (var link in db.OutputLinks?.ToList()) //берем каждую ссылку
                {                    
                    Parser parser = new Parser(db);
                    var output = parser.SelectCategories(link.SelectedLookup); //создаем новый 
                    link.File = parser.SaveFile(output); //добавляем
                    db.SaveChanges();
                }
            }
        }

        public void RefreshSchedule()
        {
            HttpRuntime.Cache.Insert("RemoveTemporaryFiles", string.Empty, null, DateTime.Now.AddHours(24), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, delegate (string id, object o, CacheItemRemovedReason cirr) {
                if (id.Equals("RemoveTemporaryFiles", StringComparison.OrdinalIgnoreCase))
                {
                    RemoveTemporaryFiles();
                    RefreshProviders();
                    RefreshLinks();
                    RefreshSchedule();
                }
            });
        }
    }
}
