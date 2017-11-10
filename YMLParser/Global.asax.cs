using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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
            RemoveTemporaryFilesSchedule();
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
                            File.Delete(file);
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        public void RemoveTemporaryFilesSchedule()
        {
            HttpRuntime.Cache.Insert("RemoveTemporaryFiles", string.Empty, null, DateTime.Now.AddHours(12), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, delegate (string id, object o, CacheItemRemovedReason cirr) {
                if (id.Equals("RemoveTemporaryFiles", StringComparison.OrdinalIgnoreCase))
                {
                    RemoveTemporaryFiles();
                    RemoveTemporaryFilesSchedule();
                }
            });
        }
    }
}
