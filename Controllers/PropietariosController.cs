using ApiInmobiliaria.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataContext = ApiInmobiliaria.Models.DataContext;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiInmobiliaria.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PropietariosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;


        public PropietariosController(DataContext contexto, IConfiguration config,
            IWebHostEnvironment environment)
        {
            this.contexto = contexto;
            this.config = config;
            this.environment = environment;
        }


        // GET: api/<PropietariosController>
        //obtiene datos del propietario logueado
        [HttpGet]
        public async Task<ActionResult<Propietario>> Get()
        {
            try
            {
                /*
                var usuario = User.Identity.Name;
                Propietario p = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
                if(p!=null){
                     p.Clave="";
                }
               
                return p;
                */
                var usuario = User.Identity.Name;
                return await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        [AllowAnonymous]
        
        public async Task<IActionResult> Login([FromForm] Login login)
        {
            Propietario p = null;
            try
            {
               

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: login.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                 p = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email== login.Email);
                if (p == null || p.Clave != hashed)
                {
                    return BadRequest("Nombre de usuario o clave incorrecta");
                }
                else
                {
                    var key = new SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                    var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, p.Email),
                        new Claim("FullName", p.Nombre + " " + p.Apellido),
                        new Claim(ClaimTypes.Role, "Propietario"),
                    };

                    var token = new JwtSecurityToken(
                        issuer: config["TokenAuthentication:Issuer"],
                        audience: config["TokenAuthentication:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddDays(360),
                        signingCredentials: credenciales
                    );
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost ("Nuevo")]
        [AllowAnonymous]
        public async Task<IActionResult> Nuevo([FromForm] Propietario p){
            try{
                if(ModelState.IsValid){

                     
                     p.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
							password: p.Clave,
							salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
							prf: KeyDerivationPrf.HMACSHA1,
							iterationCount: 1000,
							numBytesRequested: 256 / 8));
                    contexto.Propietarios.Add(p);
                    await contexto.SaveChangesAsync();
                    return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
                  

                }
                return BadRequest();
               
                 

            }catch(Exception ex){
                 return BadRequest(ex.InnerException?.Message ?? ex.Message);
                
            }

        }

       

        //PUT propietarios/editar
        //edita los datos del propietario logueado
        [HttpPatch ("editar")]
        public async Task<IActionResult> Patch([FromBody] Propietario p) {
            try
            {   
                    //Obtengo el mail del Propietario logueado mediante la claim Name.
                    var usuario = User.Identity.Name;
                    /*Asigno el propietario que contiene ese mail en la base de datos a la
                    a la variable propietario.
                    */
                    var propietario = await contexto.Propietarios.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Email == usuario);
                    /*Recupero la clave, el Email y el Id del usuario logueado y se los asigno
                    al propietario
                    que viene en la peticion 
                    */
				    p.Clave = propietario.Clave;
                    p.Email = propietario.Email;
                    p.Id = propietario.Id;
                    p.AvatarUrl = propietario.AvatarUrl;
                if (ModelState.IsValid){ 
                   
                    // Actualizo el propietario.
                    if(p.Avatar!=null){
                        p.AvatarUrl = await guardarImagen(p);
                    }
					contexto.Propietarios.Update(p);
					await contexto.SaveChangesAsync();
					return Ok(p);
				}
                return BadRequest();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message.ToString());
            }
        }
        //editar contraseña
        [HttpPatch("cambiarPass")]
		public async Task<IActionResult> CambiarPass([FromForm] string clVieja, string clNueva ){
		
			var user = User.Identity.Name;
            var propietario = await contexto.Propietarios.FirstOrDefaultAsync(u=>u.Email==user);
			string hashed =  Hashear(clVieja);
			try{
                if(propietario.Clave==hashed){
                    clNueva = Hashear(clNueva);
                    propietario.Clave = clNueva;
                    contexto.Propietarios.Update(propietario);
                    await contexto.SaveChangesAsync();
                    
                }
            
                return Ok(propietario);
            }catch(Exception ex){
                return BadRequest(ex.Message.ToString());
            }
			
		}


        //funcion para hashear clave
        private String Hashear(String clave){
           clave =  Convert.ToBase64String(KeyDerivation.Pbkdf2(
							password: clave,
							salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
							prf: KeyDerivationPrf.HMACSHA1,
							iterationCount: 1000,
							numBytesRequested: 256 / 8));
            return (clave);

        }

         //funcion asincrona para guardar la imagen y modificarle tamaño.
       public async Task<string> guardarImagen(Propietario entidad)
        {
            try
            {
                string wwwPath = environment.WebRootPath;
                string path = Path.Combine(wwwPath, "uploads/avatares");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = "avatar_" + entidad.Id + Path.GetExtension(entidad.Avatar.FileName);
                string pathCompleto = Path.Combine(path, fileName);
                
                // Esta operación guarda la foto en memoria en la ruta que necesitamos
                using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                {
                    await entidad.Avatar.CopyToAsync(stream);
                    stream.Dispose();
                }
                using (var avatar = Image.Load(pathCompleto))
                {
                    avatar.Mutate(x => x.Resize(500, 500));
                    var resizedImagePath = Path.Combine(environment.WebRootPath, "uploads/avatares", Path.GetFileName(fileName));
                    avatar.Save(resizedImagePath);
                    return Path.Combine("uploads/avatares", Path.GetFileName(pathCompleto)).Replace("\\", "/");
                }
                   
            }
            catch (Exception ex)
            {
                return "Excepcion en cargar imagen";
            }
        }
    }
              
}