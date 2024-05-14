﻿using System;
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
    public class ContratosController : ControllerBase
    {
        private readonly DataContext contexto;

        public ContratosController(DataContext context)
        {
            contexto = context;
        }

        // GET: api/Contratos
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContrato(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                var contrato = await contexto.Contratos
                                    .Include(x => x.Inquilino)
                                    .Include(x => x.Inmueble)
                                    .Where(x => x.Inmueble.Duenio.Email == usuario)
                                    .SingleOrDefaultAsync(x => x.IdContrato == id);
                return contrato != null ? Ok(contrato) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetListaContratos()
        {
            try
            {
                var usuario = User.Identity.Name;
                var lista = await contexto.Contratos
                                .Include(x => x.Inquilino)
                                .Include(x => x.Inmueble)
                               // .Include(x=> x.Garante)
                                .Where(x => x.Inmueble.Duenio.Email == usuario).ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}