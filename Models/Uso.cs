using System.ComponentModel.DataAnnotations;

namespace ApiInmobiliaria.Models
{
	public class Uso
	{
		[Display(Name = "ID del uso")]
		public int Id { get; set; }

		public string Descripcion { get; set; } = "";
	}
}