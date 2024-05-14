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
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ApiInmobiliaria.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    
    public class InmueblesController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public InmueblesController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            contexto = context;
            this.config = config;
            this.environment = environment;
        }

        // GET: api/Inmuebles/5
        [HttpGet]
        //este metodo se usa en la vista Inmuebles.
        //Me devuelve una lista de todos los inmuebles del usuario actual
        public async Task<ActionResult<Inmueble>> GetListaInmuebles()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(contexto.Inmuebles.Include(e => e.Duenio).Where(e => e.Duenio.Email == usuario));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/Inmuebles/5
        //este metodo se usa en la vista detalle inmueble
        //y me devuelve los datos de un inmueble
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInmueblePorId(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(contexto.Inmuebles.Include(e => e.Duenio).Where(e => e.Duenio.Email == usuario).Single(e => e.IdInmueble == id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /*/
        //Put api/Inmuebles/EditarEstado/5
        //Para cambiar el estado entre disponible y no disponible
     
        [HttpPut("{id}")] 
        public async Task<IActionResult> EditarEstado(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                var i = contexto.Inmuebles.Include(x => x.Duenio)
                    .FirstOrDefault(x => x.IdInmueble == id && x.Duenio.Email == usuario);
                if (i != null) { 
                    i.Disponible = !i.Disponible;
                    contexto.Inmuebles.Update(i);
                    await contexto.SaveChangesAsync();
                    return Ok(i);
                }
                return BadRequest();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        } /*/

       //este metodo se usa en la vista Inquilinos
       //Me devuelve una lista con los inmuebles alquilados del usuario actual
        [HttpGet("InmueblesConContrato")]
        public async Task<IActionResult> GetInmueblesAlquilados()
        {
            try
            {
                var usuario = User.Identity.Name;
                var fecha_actual = DateTime.Now;

                var query = from inmu in contexto.Inmuebles
                              join cont in contexto.Contratos
                                on inmu.IdInmueble equals cont.InmuebleId
                            where cont.FecInicio <= fecha_actual && cont.FecFin >= fecha_actual && usuario == inmu.Duenio.Email
                            select cont;
             return Ok(query);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        //este metodo envia la foto del inmueble
       /* [HttpPost]
       public async Task<IActionResult> Post([FromBody] Inmueble entidad)
        {
            try
            {
                var direccion = entidad.Direccion;
                var duenioId = User.Identity.Name;

                if (entidad.Imagen.Length > 0)
                {
                    if (ModelState.IsValid)
                    {
                        entidad.Id = contexto.Propietarios.Single(e => e.Mail == User.Identity.Name).Id;
 
                        var stream1 = new MemoryStream(Convert.FromBase64String(entidad.Imagen));
                        IFormFile ImagenInmo = new FormFile(stream1, 0, stream1.Length, "inmueble", ".jpg");
                        string wwwPath = environment.WebRootPath;
                        string path = Path.Combine(wwwPath, "Uploads");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        Random r = new Random();
                        //Path.GetFileName(u.AvatarFile.FileName);//este nombre se puede repetir
                        string fileName = "foto_"+entidad.Id+Path.GetExtension(entidad.ImagenFile.FileName);
                        string pathCompleto = Path.Combine(path, fileName);

                        entidad.Imagen = Path.Combine("/Uploads", fileName);
                        using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                        {
                            entidad.ImagenFile.CopyTo(stream);
                        }
                        
                        contexto.Inmuebles.Add(entidad);
                        contexto.SaveChanges();
                        return CreatedAtAction(nameof(GetInmueblePorId), new { id = entidad.Id }, entidad);

                    }
                    return BadRequest(ModelState);
                }
                return BadRequest(Ok("Imagen no encontrada"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }*/
    }
}