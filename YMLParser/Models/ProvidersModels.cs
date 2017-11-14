using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace YMLParser.Models
{
    public class UserSelection
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Id пользователя (<see cref="ApplicationUser"/>)
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Добавленные поставщики
        /// </summary>
        public ICollection<Provider> AddedProviders { get; set; }

        /// <summary>
        /// Имеющиеся ссылки
        /// </summary>
        public ICollection<OutputLink> ExistingLinks { get; set; }

        public UserSelection()
        {
            AddedProviders = new List<Provider>();
            ExistingLinks = new List<OutputLink>();
        }
    }

    public class Provider
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        /// <summary>
        /// Имя поставщика
        /// </summary>
        [DisplayName("Имя поставщика")]
        public string Name { get; set; }

        /// <summary>
        /// Ссылка на входящий файл
        /// </summary>
        [DisplayName("Ссылка на входящий файл")]
        public string Link { get; set; }

        /// <summary>
        /// Категории товаров поставщика
        /// </summary>
        [DisplayName("Категории")]
        public ICollection<Category> Categories { get; set; }

        /// <summary>
        /// Где добавлено
        /// </summary>
        public ICollection<UserSelection> UserSelections { get; set; }

        public Provider()
        {
            Categories = new List<Category>();
            UserSelections = new List<UserSelection>();
        }
    }

    public class OutputLink
    {
        public int Id { get; set; }

        /// <summary>
        /// Название ссылки
        /// </summary>
        [DisplayName("Название ссылки")]
        public string Name { get; set; }

        /// <summary>
        /// Ссылка на исходящий файл
        /// </summary>
        [DisplayName("Ссылка на исходящий файл")]
        public string Link { get; set; }

        [ForeignKey("File")]
        public int? File_Id { get; set; }

        /// <summary>
        /// Файл
        /// </summary>
        public FileOutput File { get; set; }

        /// <summary>
        /// Выбранные категории
        /// </summary>
        public string Selected { get; set; }

        /// <summary>
        /// Выбранные категории
        /// </summary>
        public ILookup<string, string> SelectedLookup
        {
            get
            {
                string[] tab = this.Selected.Split(';');
                return tab.ToLookup(x => x.Split('_')[0], x => x.Split('_')[1]);
            }
        }

        [ForeignKey("UserSelection")]
        public int? UserSelectionId { get; set; }

        public UserSelection UserSelection { get; set; }
        
    }

    public class Category
    {
        public int Id { get; set; }

        /// <summary>
        /// Название категории
        /// </summary>
        [DisplayName("Название категории")]
        public string Name { get; set; }

        /// <summary>
        /// Варианты названия категории, разделенные запятой
        /// </summary>
        public string Aliases { get; set; }

        /// <summary>
        /// Варианты названия категории
        /// </summary>
        [NotMapped]
        public ICollection<string> AliasesList
        {
            get
            {
                string[] tab = this.Aliases.Split(';');
                return tab.ToList();
            }
            set { this.Aliases = string.Join(";", value.ToArray()); }
        }

        /// <summary>
        /// Список поставщиков имеющих эту категорию
        /// </summary>
        public ICollection<Provider> Owners { get; set; }


        public Category()
        {
            AliasesList = new List<string>();
            Owners = new List<Provider>();
        }
    }

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