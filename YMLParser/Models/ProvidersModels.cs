using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
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
        /// Ссылка на исходящий файл
        /// </summary>
        [DisplayName("Ссылка на исходящий файл")]
        public string Link { get; set; }

        /// <summary>
        /// Поставщики, включенные в ссылку
        /// </summary>
        public ICollection<Provider> SelectedProviders { get; set; }

        /// <summary>
        /// Категории, включенные в ссылку
        /// </summary>
        public ICollection<Category> SelectedCategories { get; set; }

        [ForeignKey("UserSelection")]
        public int? UserSelectionId { get; set; }

        public UserSelection UserSelection { get; set; }

        public OutputLink()
        {
            SelectedProviders = new List<Provider>();
            SelectedCategories = new List<Category>();
        }
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
                string[] tab = this.Aliases.Split(',');
                return tab.ToList();
            }
            set { this.Aliases = string.Join(",", value.ToArray()); }
        }

        /// <summary>
        /// Список поставщиков имеющих эту категорию
        /// </summary>
        public ICollection<Provider> Owners { get; set; }


        public ICollection<OutputLink> Links { get; set; }

        public Category()
        {
            AliasesList = new List<string>();
            Owners = new List<Provider>();
        }
    }
}