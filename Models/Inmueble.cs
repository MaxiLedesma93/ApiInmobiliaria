using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiInmobiliaria.Models
{
    public class Inmueble 
   {
		[Key]
        [Display(Name = "Código Interno")]
		public int IdInmueble { get; set; }
		[Required]
		[Display(Name = "Dirección")]
		public string? Direccion { get; set; }
		[Required]
		public int Ambientes { get; set; }
		[Required]
		public int Superficie { get; set; }
		public decimal Latitud { get; set; }

        public string? Uso {get; set; }
		public decimal Longitud { get; set; }
		
		[Display(Name = "Dueño"), Required]
		public int PropietarioId { get; set; }
		public Propietario? Duenio { get; set; }

		[Display(Name = "Tipo Inmueble")]
		public string? Tipo {get; set;}
        
		[Display(Name = "Importe")]
		public int Importe {get; set;}

		[Display(Name = "Disponible")]
		public int Disponible {get; set;}

		public string? Disp {get; set;}
   }
   
}