using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace YMLParser.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult CreateXMLFromLink(string link)
        {
            XDocument xdoc = XDocument.Load(link);
            var Output = CreateDocument(xdoc);

            return PartialView("ConvertPartial");
        }

        [HttpPost]
        public ActionResult CreateXMLFromFile()
        {
            if (Request.Files.Count > 0)
            {
                if (Request.Files.Count==1)
                {
                    var file = Request.Files[0];

                    var fileName = Path.GetFileName(file.FileName);
                    if (fileName.ToLower().EndsWith(".xml"))
                    {
                        var path = Path.Combine(Server.MapPath("~/Docs/"), fileName);
                        file.SaveAs(path);
                        XDocument xdoc = XDocument.Load(path);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }

                        var Output = CreateDocument(xdoc);                        
                    }
                }
                else
                {
                    List<XDocument> files = new List<XDocument>();
                    foreach (HttpPostedFileBase file in Request.Files)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        if (fileName.ToLower().EndsWith(".xml"))
                        {
                            var path = Path.Combine(Server.MapPath("~/Docs/"), fileName);
                            file.SaveAs(path);
                            XDocument xdoc = XDocument.Load(path);
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                            files.Add(xdoc);
                        }
                    }
                    var Output = CreateDocument(files.ToArray());
                }
                
            }
            return PartialView("ConvertPartial");
        }

        /// <summary>
        /// Creates document from a single file
        /// </summary>
        /// <param name="file">Input file</param>
        /// <returns></returns>
        private XDocument CreateDocument(XDocument file)
        {
            XDocument xdoc = new XDocument();

            XElement root = file.Element("shop");

            foreach (XElement item in root.Elements())
            {
                if (item.Name == "categories")
                {
                    XElement categories = new XElement(item);
                    xdoc.Add(categories);
                }
                else if (item.Name == "offers")
                {
                    XElement offers = new XElement("offers");

                    foreach(XElement offer in item.Elements())
                    {
                        XElement offerOut = new XElement("offer");
                        offerOut.SetAttributeValue("id", offer.Attribute("id"));
                        offerOut.SetAttributeValue("availible", offer.Attribute("availible"));

                        foreach (XElement child in offer.Elements())
                        {
                            switch (child.Name.ToString())
                            {
                                case "name":
                                    XElement name = new XElement("name", child);
                                    offerOut.Add(name);
                                    break;
                                case "description":
                                    XElement description = new XElement("description", child);
                                    offerOut.Add(description);
                                    break;
                                case "price":
                                    XElement price = new XElement("price", child);
                                    offerOut.Add(price);
                                    break;
                                case "":
                                    break;
                                default:
                                    break;
                            }
                        }
                        offers.Add(offerOut);
                    }
                    xdoc.Add(offers);
                }
            }
            return xdoc;
        }

        /// <summary>
        /// Creates document from multiple files
        /// </summary>
        /// <param name="files">Input files</param>
        /// <returns></returns>
        private XDocument CreateDocument(XDocument[] files)
        {
            XDocument xdoc = new XDocument();

            return xdoc;
        }
    }
}