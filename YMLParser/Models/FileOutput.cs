using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;

namespace YMLParser.Models
{
    public class FileOutput
    {
        public int Id { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// Имя поставщика
        /// </summary>
        public string Vendor { get; set; }
        /// <summary>
        /// MIME-тип файла
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// Словарь категорий
        /// </summary>
        public Dictionary<string, string> Categories { get; set; }
        [NotMapped]
        public FileInfo Info { get; set; }

    }
}