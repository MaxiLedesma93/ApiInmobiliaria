using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiInmobiliaria.Models;

namespace ApiInmobiliaria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly DataContext contexto;

        public PagosController(DataContext context)
        {
            contexto = context;
        }

        // GET
        //este metodo se usa en la vista de Contratos en el boton pagos
        //me devuelve la lista de pagos de un contrato vigente
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Pago>>> GetPagos(int id)
        {

            try
            {
                var pagos = await contexto.Pagos.Include(x => x.contrato).Where(x =>
                     x.ContratoId == id
                    ).ToListAsync();

                return Ok(pagos);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

        }
}