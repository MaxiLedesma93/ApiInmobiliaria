using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ApiInmobiliaria.Models
{
    public class Pago
    {
        [Key]
        [DisplayName("Código de Pago")]
        public int IdPago { get; set; }

        [DisplayName("Número de pago")]
        public int NumPago { get; set; }

        [DisplayName("Fecha de pago"), DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        public decimal Importe { get; set; }

        [DisplayName("Código de Contrato")]
        public int ContratoId { get; set; }

        [DisplayName("Detalle")]
        public string? Detalle { get; set; }

        [DisplayName("Datos del Contrato")]
        public Contrato? contrato { get; set; }

        public int Est {get; set;}

        [DisplayName("Estado")]
        public string? Activo {get; set;}
    }
}