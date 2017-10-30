using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using YMLParser.Models;

namespace YMLParser
{
    public class Parser
    {
        private int imageCounter = 0;
        public Dictionary<string, string> catDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Создает документ из 1 файла
        /// </summary>
        /// <param name="file">Input file</param>
        /// <returns></returns>
        public XDocument CreateDocument(XDocument file)
        {
            XDocument xdoc = new XDocument();
            XElement docRoot = new XElement(file.Root.Name);
            XElement shop = new XElement("shop");

            XElement root = file.Root.Element("shop");

            foreach (XElement item in root.Elements())
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
                }
                else if (item.Name == "offers")
                {
                    XElement offers = new XElement("offers");

                    foreach (XElement off in item.Elements())
                    {
                        XElement offer = new XElement("offer");
                        offer.SetAttributeValue("id", off.Attribute("id").Value);
                        offer.SetAttributeValue("available", off.Attribute("available").Value);

                        imageCounter = 0;

                        foreach (XElement child in off.Elements())
                        {
                            ParseXelement(child, ref offer);
                        }
                        offers.Add(offer);
                    }
                    shop.Add(offers);
                }
            }
            docRoot.Add(shop);
            xdoc.Add(docRoot);
            return xdoc;
        }

        /// <summary>
        /// Создает документ из неск файлов
        /// </summary>
        /// <param name="files">Input files</param>
        /// <returns></returns>
        public XDocument CreateDocument(XDocument[] files)
        {
            XDocument xdoc = new XDocument();

            return xdoc;
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
                    output.Add(new XElement("model", element.Value));
                    output.Add(new XElement("metaTitle", element.Value));
                    output.Add(new XElement("metaDescription", element.Value));
                    break;
                case "description":
                    output.Add(new XElement("description", element.Value));
                    output.Add(new XElement("metaKeywords", element.Value));
                    output.Add(new XElement("seoKeyword", element.Value));
                    break;
                case "model":
                    output.Add(new XElement("name", element.Value));
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
                    output.Add(new XElement("category", catDictionary[element.Value]));
                    break;
                case "param":
                    var par = ParseParam(element);
                    if (par != null)
                    {
                        output.Add(par);
                    }
                    break;
                case "picture":
                    if (imageCounter < 1)
                    {
                        output.Add(new XElement("image", element.Value));
                        imageCounter++;
                    }
                    else
                    {
                        output.Add(new XElement("additionalImage" + imageCounter++, element.Value));
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Создать файл только с выбранными категориями
        /// </summary>
        /// <param name="file">файл</param>
        /// <param name="selectedCategories">категории</param>
        /// <returns></returns>
        public XDocument SelectCategories(XDocument file, IList<string> selectedCategories)
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
        /// Парсит категории
        /// </summary>
        /// <param name="categories"></param>
        private void ParseCategories(XElement categories)
        {
            foreach (XElement category in categories.Elements())
            {
                catDictionary.Add(category.Attribute("id").Value, category.Value);
            }
        }

        /// <summary>
        /// Парсит элемент &#60;param&#62;
        /// </summary>
        private XElement ParseParam(XElement param)
        {
            switch (param.Attribute("name").ToString().ToLower())
            {
                case "stock":
                    return new XElement("stock", param.Value);
                case "вес":
                    return new XElement("weight", param.Value);
                case "высота":
                    return new XElement("height", param.Value);
                case "ширина":
                    return new XElement("width", param.Value);
                case "глубина":
                    return new XElement("length", param.Value);
                case "material":
                    return new XElement("material", param.Value);
                case "материал":
                    return new XElement("material", param.Value);
                default:
                    return null;
            }
        }


    }
}