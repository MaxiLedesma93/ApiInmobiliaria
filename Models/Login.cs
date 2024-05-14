using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiInmobiliaria.Models
{
    public class Login
    {
            [Required]
            [DataType(DataType.EmailAddress)]
            public String Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public String Clave { get; set; }

    }
}