using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BookstoreWeb_Temp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Category Name")]
        [MinLength(2, ErrorMessage = "Invalid, text is too short")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100)]
        public int DisplayOrder { get; set; }
    }
}
