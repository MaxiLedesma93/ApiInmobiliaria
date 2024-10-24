using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ApiInmobiliaria.Models
{
    public class Inmueble 
   {
		[Key]
        [Display(Name = "Código Interno")]
		public int Id { get; set; }
		[Required]
		[Display(Name = "Dirección")]
		public string? Direccion { get; set; }
		[Required]
		public int Ambientes { get; set; }
		
		public string? imgUrl {get; set;}
		
		[NotMapped]
		public IFormFile imagen { get; set;}
		
		public int? PropietarioId { get; set; }
		
		public int? TipoId{get; set;}
	
		public int? Importe {get; set;}
		public bool Disponible {get; set;}

        public string? uso {get; set; }

		
		[ForeignKey(nameof(TipoId))]
		
		public Tipo? Tipo {get; set;}
		
		[ForeignKey(nameof(PropietarioId))]
		
		public Propietario? Duenio { get; set; }

   }
   
}