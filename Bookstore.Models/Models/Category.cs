using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Models.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Category Name")]
        [MinLength(2, ErrorMessage ="Invalid, text is too short")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1,100)]
        public int DisplayOrder { get; set; }
    }
}
