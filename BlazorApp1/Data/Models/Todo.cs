using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Data.Models
{
    public class Todo
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public CprNr CprNr { get; set; }

        [Required]
        [MaxLength(500)]
        public string Item {  get; set; }

    }
}