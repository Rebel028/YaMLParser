using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace YMLParser.Models
{
    public class UserSelection
    {
        [Required]
        public string Id { get; set; }
        /// <summary>
        /// Id пользователя (<see cref="ApplicationUser"/>)
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Добавленные поставщики
        /// </summary>
        public IEnumerable<Provider> AddedProviders { get; set; }

        /// <summary>
        /// Имеющиеся ссылки
        /// </summary>
        public IEnumerable<OutputLink> ExistingLinks { get; set; }
    }

    /// <summary>
    /// Класс, представляющий поставщика
    /// </summary>
    public class Provider
    {
        [Required]
        public string Id { get; set; }

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
        public IEnumerable<Category> Categories { get; set; }
    }

    public class OutputLink
    {
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Ссылка на исходящий файл
        /// </summary>
        [DisplayName("Ссылка на исходящий файл")]
        public string Link { get; set; }

        /// <summary>
        /// Поставщики, включенные в ссылку
        /// </summary>
        public IEnumerable<Provider> Providers { get; set; }

        /// <summary>
        /// Категории, включенные в ссылку
        /// </summary>
        public IEnumerable<Category> SelectedCategories { get; set; }
    }

    public class Category
    {
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Название категории
        /// </summary>
        [DisplayName("Название категории")]
        public string Name { get; set; }

        /// <summary>
        /// Варианты названия категории
        /// </summary>
        [DisplayName("Варианты названия")]
        public IEnumerable<string> Aliases { get; set; }
    }
}