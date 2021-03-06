﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using YMLParser.Models;

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
        public async Task<ActionResult> CreateXmlFromLink(string link)
        {
            //запускаем парсер
            Parser parser = new Parser();

            //поучаем результат
            FileOutput file = await parser.ParseInitialFile(link);
            if (file == null)
            {
                return Content("Ссылка на XML не верна!");
            }
            //return File(file.Info.OpenRead(), file.FileType, file.FileName);
            return PartialView("ConvertPartial", file);
        }

        [HttpPost]
        public ActionResult CreateXmlFromFile()
        {
            if (Request.Files.Count > 0)
            {
                if (Request.Files.Count == 1) // если один файл
                {
                    var baseFile = Request.Files[0];

                    var fileName = Path.GetFileName(baseFile.FileName);
                    if (fileName.ToLower().EndsWith(".xml"))
                    {
                        var path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                        baseFile.SaveAs(path);
                        XDocument xdoc = XDocument.Load(path);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                        Parser parser = new Parser();
                        var output = parser.CreateBaseDocument(xdoc);
                        //поучаем результат
                        FileOutput file = SaveFile(output);
                        file.Categories = parser.CatDictionary;

                        return PartialView("ConvertPartial", file);
                    }
                }
                else // если больше
                {
                    return View("Index");

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
                    Parser parser = new Parser();
                }

            }
            return View("Index");
        }

        [HttpPost]
        public ActionResult DownloadFile(FileOutput file, string[] selected)
        {
            //создаем список выбранных категорий
            List<string> categories = new List<string>();
            foreach (var item in selected)
            {
                if (item!="false")
                {
                    categories.Add(item);
                }
            }

            var xdoc = XDocument.Load(file.FilePath);
            Parser parser = new Parser();
            var output = Parser.SelectCategories(xdoc, categories);
            FileOutput newFile = SaveFile(output);
            string fileType = newFile.FileType;
            string fileName = newFile.FileName;
            string filePath = newFile.FilePath;

            byte[] fileConent = System.IO.File.ReadAllBytes(filePath);
            var cd = new System.Net.Mime.ContentDisposition
            {
                // for example foo.bak
                FileName = fileName,

                // always prompt the user for downloading, set to true if you want 
                // the browser to try to show the file inline
                Inline = false,
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(fileConent, fileType);
        }

        //[HttpPost]
        //public ActionResult DownloadSelected()
        //{
        //    return View();
        //}

        /// <summary>
        /// Сохраняет файл на сервер
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public FileOutput SaveFile(XDocument output)
        {
            FileOutput file = new FileOutput
            {
                FileType = "application/xml",
                FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml"
            };
            var path = Server.MapPath(".\\App_Data\\");
            file.FilePath = path + file.FileName;
            //проверяем, существует ли папка
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            //Пишем файл
            if (!file.Info.Exists)
            {
                var stream = file.Info.Create();
                output.Save(stream);
                stream.Close();
            }

            return file;
        }

    }
}