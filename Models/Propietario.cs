using System.ComponentModel.DataAnnotations;

namespace ApiInmobiliaria.Models{

public class Propietario
	{
		[Key]
		[Display(Name = "Código Interno")]
		public int IdPropietario { get; set; }
		[Required]
		public string? Nombre { get; set; }
		[Required]
		public string? Apellido { get; set; }
		[Required]
		public string? Dni { get; set; }
		[Display(Name = "Teléfono")]
		public string? Telefono { get; set; }
		[Required, EmailAddress]
		public string? Email { get; set; }
		[Required, DataType(DataType.Password)]
		public string? Clave { get; set; }

		public override string ToString()
		{
			return $"{Nombre} {Apellido}";
		}
	}
}