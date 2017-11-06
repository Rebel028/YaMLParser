using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace YMLParser.Models
{
    public class FileOutput
    {
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Vendor { get; set; }
        public Dictionary<string, string> Categories { get; set; }
        public FileInfo Info { get; set; }
    }
}