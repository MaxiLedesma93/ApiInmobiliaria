using System.ComponentModel.DataAnnotations;

namespace ApiInmobiliaria.Models
{
    public class Tipo
    {
        [Key]
		[Display(Name = "Código Interno")]
		public int Id { get; set; }
		[Required]
		public string? Descripcion { get; set; }

    }
}