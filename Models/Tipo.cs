using System.ComponentModel.DataAnnotations;

namespace ApiInmobiliaria.Models
{
    public class Tipo
    {
        [Key]
		[Display(Name = "Código Interno")]
		public int IdTipo { get; set; }
		[Required]
		public string? Descripcion { get; set; }

        public override string ToString()
		{
			return $"{Descripcion}";
		}
    }
}