using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiInmobiliaria.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ApiInmobiliaria.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TiposController : ControllerBase
    {
        private readonly DataContext contexto;

        public TiposController(DataContext context)
        {
            contexto = context;
        }

        // GET: api/Tipos
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTipo(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                var contrato = await contexto.Tipos.SingleOrDefaultAsync(x => x.Id == id);
                return contrato != null ? Ok(contrato) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("listatipos")]
        public async Task<IActionResult> GetListaTipos()
        {
            try
            {
                var usuario = User.Identity.Name;
                var lista = await contexto.Tipos.ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}