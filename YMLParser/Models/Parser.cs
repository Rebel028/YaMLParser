using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using YMLParser.Models;

namespace YMLParser
{
    public class Parser
    {
        private int _imageCounter = 0;
        public Dictionary<string, string> CatDictionary = new Dictionary<string, string>();
        public string ProviderName="Multiple";

        private ApplicationDbContext _db;
        private readonly bool _disposeDbOnExit;
        //Где хранятся файлы
        private static readonly string Root = AppContext.BaseDirectory;
        private static readonly string FilesFolder = Root + "App_Data\\";


        public Parser()
        {
            _db = new ApplicationDbContext();
            _disposeDbOnExit = true;
        }

        public Parser(ApplicationDbContext db)
        {
            this._db = db;
        }

        /// <summary>
        /// Загружает файл из ссылки
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private static Task<XDocument> LinkToXDocument(string link)
        {
            if (IsValidUrl(link))
            {
                return Task.Run(async () =>
                {
                //загружаем файл
                var client = new HttpClient();
                    try
                    {
                        var response = await client.GetStreamAsync(link);
                        return XDocument.Load(response);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return null;
                    }
                });
            }
            return null;
        }

        #region Categories

        /// <summary>
        /// Парсит категории в список из объектов <see cref="Category"/>
        /// </summary>
        /// <param name="categoriesList">Входящий список имен категорий</param>
        /// <param name="provider">Поставщик</param>
        public void ParseAllCategories(IList<string> categoriesList, Provider provider)
        {
            CheckNewCategories(categoriesList, provider);
            RemoveOldCategories(categoriesList, provider);
        }

        /// <summary>
        ///Провиеряет есть ли новые категории и добавляет их
        /// </summary>
        /// <param name="categoriesList">Входящий список имен категорий</param>
        /// <param name="provider">Поставщик</param>
        private void CheckNewCategories(IList<string> categoriesList, Provider provider)
        {
            //превращаем имена в объекты
            foreach (string categoryName in categoriesList)
            {
                //есть ли такая категория у поставщика?
                var providerCategory =
                    provider.Categories.FirstOrDefault(c => c.Name.ToLower() == categoryName.ToLower());

                if (providerCategory == null)
                {
                    //есть ли она вообще?
                    var existingCategory =
                        _db.Categories.FirstOrDefault(c => c.Name.ToLower() == categoryName.ToLower());

                    if (existingCategory == null) //если нет
                    {
                        var category = new Category
                        {
                            Name = categoryName,
                            Aliases = categoryName + ";"
                        };
                        category.Owners.Add(provider);
                        provider.Categories.Add(category);
                        _db.Categories.Add(category);
                    }
                    else
                    {
                        existingCategory.Owners.Add(provider);
                        provider.Categories.Add(existingCategory);
                        _db.Entry(existingCategory).State = EntityState.Modified;
                        _db.Entry(provider).State = EntityState.Modified;
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет неиспользуемые категории у поставщика
        /// </summary>
        /// <param name="categoriesList">Входящий список имен категорий</param>
        /// <param name="provider">Поставщик</param>
        private void RemoveOldCategories(IList<string> categoriesList, Provider provider)
        {
            var listToRemove = provider.Categories
                .Where(providerCategory => !categoriesList
                .Contains(providerCategory.Name))
                .ToList();
            foreach (var category in listToRemove)
            {
                provider.Categories.Remove(category);
                category.Owners.Remove(provider);
                //TODO: удалять категорию если у нее нет владельцев
                _db.Entry(category).State = EntityState.Modified;
                _db.Entry(provider).State = EntityState.Modified;
            }
        }

        #endregion

        #region Parsing Mechanism Only

        /// <summary>
        /// Парсит документ от одного поставщика в формат опенкарта из 1 файла
        /// </summary>
        /// <param name="file">Input file</param>
        public XDocument CreateBaseDocument(XDocument file)
        {
            XDocument xdoc = new XDocument();//наш новый документ

            XElement docRoot = new XElement(file.Root.Name); //корень документа 
            XElement shop = new XElement("shop"); //элемент "shop"

            XElement root = file.Root.Element("shop");//задаем корнем этот элемент

            foreach (XElement item in root.Elements()) //и проходимся по нему
            {
                if (item.Name == "categories")
                {
                    XElement categories = new XElement(item);
                    shop.Add(categories);
                    ParseCategories(categories);
                }
                else if (item.Name == "company")
                {
                    shop.Add(new XElement("vendor", item.Value));
                    ProviderName = item.Value;
                }
                else if (item.Name == "offers")
                {
                    XElement offers = new XElement("offers");

                    foreach (XElement off in item.Elements())
                    {
                        XElement offer = new XElement("offer");
                        offer.SetAttributeValue("id", off.Attribute("id").Value);
                        offer.SetAttributeValue("available", off.Attribute("available").Value);

                        _imageCounter = 0;

                        foreach (XElement child in off.Elements())
                        {
                            ParseXelement(child, ref offer);
                        }
                        offers.Add(offer);
                    }
                    shop.Add(offers);
                }
            }
            docRoot.Add(shop);//засовываем одно в другое
            xdoc.Add(docRoot);
            return xdoc;
        }

        /// <summary>
        /// Парсит категории
        /// </summary>
        /// <param name="categories"></param>
        private void ParseCategories(XElement categories)
        {
            foreach (XElement category in categories.Elements())
            {
                CatDictionary.Add(category.Attribute("id").Value, category.Value);
            }
        }

        /// <summary>
        /// Парсит элемент дерева
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="output">Родительский элемент</param>
        private void ParseXelement(XElement element, ref XElement output)
        {
            switch (element.Name.ToString())
            {
                case "name":
                    output.Add(new XElement("name", element.Value));
                    break;
                case "description":
                    output.Add(new XElement("description", element.Value));
                    output.Add(new XElement("metaKeywords", element.Value));
                    output.Add(new XElement("seoKeyword", element.Value));
                    break;
                case "model":
                    output.Add(new XElement("model", element.Value));
                    output.Add(new XElement("metaTitle", element.Value));
                    output.Add(new XElement("metaDescription", element.Value));
                    break;
                case "vendorCode":
                    output.Add(new XElement("sku", element.Value));
                    break;
                case "vendor":
                    output.Add(new XElement("manufacturer", element.Value));
                    break;
                case "url":
                    output.Add(new XElement("url", element.Value));
                    break;
                case "price":
                    output.Add(new XElement("price", element.Value));
                    break;
                case "oldprice":
                    output.Add(new XElement("oldprice", element.Value));
                    break;
                case "store":
                    output.Add(new XElement("store", element.Value));
                    break;
                case "stock":
                    output.Add(new XElement("stock", element.Value));
                    break;
                case "production_time":
                    output.Add(new XElement("production_time", element.Value));
                    break;
                case "categoryId":
                    try
                    {
                        output.Add(new XElement("category", CatDictionary[element.Value]));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        output.Add(new XElement("category", "Unknown"));
                    }
                    break;
                case "param":
                    var par = ParseParam(element);
                    if (par != null)
                    {
                        output.Add(par);
                    }
                    break;
                case "picture":
                    if (_imageCounter < 1)
                    {
                        output.Add(new XElement("image", element.Value));
                        _imageCounter++;
                    }
                    else
                    {
                        output.Add(new XElement("additionalImage" + _imageCounter++, element.Value));
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Парсит элемент &#60;param&#62;
        /// </summary>
        private static XElement ParseParam(XElement param)
        {
            var attrName = param.Attribute("name").Value;
            switch (attrName.ToString().ToLower())
            {
                case "stock":
                    return new XElement("stock", param.Value);
                case "вес":
                case "ves":
                case "weight":
                    return new XElement("weight", param.Value);
                case "height":
                case "высота":
                    return new XElement("height", param.Value);
                case "width":
                case "ширина":
                    return new XElement("width", param.Value);
                case "depth":
                case "length":
                case "глубина":
                    return new XElement("length", param.Value);
                case "material":
                case "материал":
                    return new XElement("material", param.Value);
                case "цвет":
                case "cvet":
                case "color":
                    return new XElement("color", param.Value);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Создать файл только с выбранными категориями
        /// </summary>
        /// <param name="file">файл</param>
        /// <param name="selectedCategories">категории</param>
        public static XDocument SelectCategories(XDocument file, IList<string> selectedCategories)
        {
            XDocument xdoc = new XDocument();
            XElement docRoot = new XElement(file.Root.Name);
            XElement shop = new XElement("shop");

            XElement root = file.Root.Element("shop");

            foreach (XElement item in root.Elements())
            {
                if (item.Name == "categories")
                {
                    XElement categories = new XElement("categories");
                    foreach (XElement category in item.Elements())
                    {
                        if (selectedCategories.Contains(category.Value))
                        {
                            categories.Add(category);
                        }
                    }                    
                    shop.Add(categories);
                }                
                else if (item.Name == "offers")
                {
                    XElement offers = new XElement("offers");

                    foreach (XElement offer in item.Elements())
                    {
                        if (selectedCategories.Contains(offer.Element("category").Value))
                        {
                            offers.Add(offer);
                        }
                    }
                    shop.Add(offers);
                }
                else
                {
                    shop.Add(item);
                }
            }
            docRoot.Add(shop);
            xdoc.Add(docRoot);
            return xdoc;
        }

        /// <summary>
        /// Создать файл только с выбранными категориями и поставщиками
        /// </summary>
        /// <param name="selectedCategories"><see cref="Lookup{TKey,TElement}"/> с парами типа "поставщик-категория"</param>
        /// <returns>Документ</returns>
        public XDocument SelectCategories(ILookup<string, string> selectedCategories)
        {
            XDocument result = new XDocument();
            result.Declaration = new XDeclaration("1.0", "UTF-8", "yes");
            result.Add(new XElement("yml_catalog"));
            foreach (var group in selectedCategories)
            {
                var path = FindProviderFile(group.Key).FilePath;
                var xdoc = XDocument.Load(path);
                var list = group.ToList();
                var categoriesFile = SelectCategories(xdoc, list);
                result.Root.Add(categoriesFile.Descendants("shop"));
            }

            return result;
        }
        
        #endregion
        
        /// <summary>
        /// Парсит и сохраняет файл
        /// </summary>
        /// <param name="link">ссылка на файл</param>
        public Task<FileOutput> ParseInitialFile(string link)
        {
            return Task.Run(async () =>
            {
                var input = await LinkToXDocument(link); //загружаем документ
                if (input != null)
                {
                    var xdoc = CreateBaseDocument(input);
                    return SaveInitial(xdoc, link);
                }
                return null;
            });
        }

        /// <summary>
        /// Сохраняет файл на сервер и в БД
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public FileOutput SaveFile(XDocument output)
        {
            //TODO проверять наличие категорий
            var file = new FileOutput
            {
                FileType = "application/xml",
                FileName = ProviderName + DateTime.Now.ToString("_yyyyMMddHHmmss") + ".xml",
                Vendor = ProviderName,
                Categories = CatDictionary
            };
            file.FilePath = FilesFolder + file.FileName;
            //проверяем, существует ли папка
            if (!Directory.Exists(FilesFolder)) Directory.CreateDirectory(FilesFolder);
            //Пишем файл
            if (file.Info.Exists && file.Info.CreationTime > DateTime.Now.AddDays(-3))//если существует
            {
                return file;
            }
            else
            {
                var stream = file.Info.Create();
                output.Save(stream);
                stream.Close();
                _db.OutputFiles.Add(file);
                _db.SaveChanges();
            }
            return file;
        }

        /// <summary>
        /// Сохраняет базовый файл поставщика
        /// </summary>
        /// <param name="output"></param>
        /// <param name="link"></param>
        public FileOutput SaveInitial(XDocument output, string link)
        {
            FileOutput file = new FileOutput
            {
                FileType = "application/xml",
                FileName = ProviderName + ".xml",
                Vendor = ProviderName,
                Categories = CatDictionary
            };
            file.FilePath = FilesFolder + file.FileName;
            //проверяем, существует ли папка
            if (!Directory.Exists(FilesFolder)) Directory.CreateDirectory(FilesFolder);

            var providers = _db.Providers.Include(p => p.MainOutputFile).ToList();
            var provider = providers.FirstOrDefault(p => p.Link == link);

            //Пишем файл
            if (file.Info.Exists)
            {
                try
                {
                    if (file.Info.Length == provider.MainOutputFile.Info.Length) //если файл такой же как и был
                    {
                        return null;
                    }
                    else
                    {
                        var stream = file.Info.Create();
                        output.Save(stream);
                        stream.Close();
                        _db.OutputFiles.Add(file);
                        _db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    _db.OutputFiles.Add(file);
                    _db.SaveChanges();
                }
            }
            else
            {
                var stream = file.Info.Create();
                output.Save(stream);
                stream.Close();
                _db.OutputFiles.Add(file);
                _db.SaveChanges();
            }
            return file;
        }

        /// <summary>
        /// Находит базовый файл поставщика
        /// </summary>
        /// <param name="providerName"></param>
        private FileOutput FindProviderFile(string providerName)
        {
            var test = _db.OutputFiles.Where(f=>f.Id>0).ToList();
            var file = test.FirstOrDefault(f => f.Vendor == providerName);
            return _db.OutputFiles.FirstOrDefault(f => f.Vendor == providerName);
        }


        private static bool IsValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
        }

        ~Parser()
        {
            if (_disposeDbOnExit)
            {
                _db.Dispose();
            }
        }
    }
}