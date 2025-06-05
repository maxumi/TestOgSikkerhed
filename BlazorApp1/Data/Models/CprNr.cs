using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Data.Models
{
    public class CprNr
    {
        public int Id { get; set; }
        public string User {  get; set; }

        [Required]
        [MaxLength(500)]
        public string CprNum { get; set; }

        public List<Todo> TodoList { get; set; } = new List<Todo>();
    }
}
