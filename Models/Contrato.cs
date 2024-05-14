using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ApiInmobiliaria.Models{

    public class Contrato
   {
		 [Key]
        [Display(Name = "Codigo")]
        public int IdContrato { get; set; }

        public Inmueble? Inmueble { get; set; }

        [Required, Display (Name ="Direccion")]
        public int InmuebleId { get; set; }

        public Inquilino? Inquilino { get; set; }

        [Required, Display(Name ="Inquilino")]
        public int InquilinoId { get; set; }

        [Required, Display(Name ="Fecha Inicio Contrato")]
        public DateTime FecInicio { get; set; }

        [Required, Display(Name ="Fecha Fin contrato")]
        public DateTime FecFin { get; set; }

        public decimal Monto { get; set; }

        public bool Estado { get; set; }
   }
}
