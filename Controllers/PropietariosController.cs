using ApiInmobiliaria.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
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
using ApiInmobiliaria.Servicios;
using MimeKit;

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
                else if(p.Clave == hashed)
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
                    else{
                        return BadRequest("Nombre de usuario o clave incorrecta");
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
                   
                    if(p.Avatar!=null){
                        p.AvatarUrl = await guardarImagen(p);
                    }else{
                         p.AvatarUrl = propietario.AvatarUrl;
                    }
                if (ModelState.IsValid){ 
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

        // GET api/<controller>/token
		[HttpGet("token")]
		public async Task<IActionResult> Token()
		{
			try
			{ //este método si tiene autenticación, al entrar, genera una clave aleatoria y la envia por correo
				var perfil = new
				{
					Email = User.Identity.Name,
					Nombre = User.Claims.First(x => x.Type == "FullName").Value,
					Rol = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value
				};
				Random rand = new Random(Environment.TickCount);
				string randomChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
				string nuevaClave = "";
				for (int i = 0; i < 6; i++)
				{
					nuevaClave += randomChars[rand.Next(0, randomChars.Length)];
				}
                //se hashea la nueva clave y se envia la clave sin hashear
                var claveHasheada = Hashear(nuevaClave);
                Propietario p = await contexto.Propietarios.SingleOrDefaultAsync(x => x.Email == perfil.Email);
                p.Clave = claveHasheada;
                contexto.Propietarios.Update(p);
                contexto.SaveChanges();
				var message = new MimeKit.MimeMessage();
				message.To.Add(new MailboxAddress(perfil.Nombre, perfil.Email));
				message.From.Add(new MailboxAddress("Inmobiliaria Ledesma", config["SMTPUser"]));
				message.Subject = "Envio de nueva contraseña";
				message.Body = new TextPart("html")
				{
					Text = @$"<h1>Hola</h1>
					<p> {perfil.Nombre} Tu nueva contraseña es: {nuevaClave} </p>",//falta enviar la clave generada (sin hashear)
				};
			//	message.Headers.Add("Encabezado", "Valor");//solo si hace falta
			//	message.Headers.Add("Otro", config["Valor"]);//otro ejemplo
				MailKit.Net.Smtp.SmtpClient client = new SmtpClient();
				client.ServerCertificateValidationCallback = (object sender,
					System.Security.Cryptography.X509Certificates.X509Certificate certificate,
					System.Security.Cryptography.X509Certificates.X509Chain chain,
					System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
				{ return true; };
				client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
				client.Authenticate(config["SMTPUser"], config["SMTPPass"]);//estas credenciales deben estar en el user secrets
				await client.SendAsync(message);
				var htmlEnviado = @"<dialog open>
                            <p>Clave Reseteada</p>
                            <button onclick=window.close()>Cerrar ventana</button>
                            </dialog>";
                return Content(htmlEnviado,"text/html");
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}



        // GET api/<controller>/email
		[HttpPost("email")]
		[AllowAnonymous]
		public async Task<IActionResult> GetByEmail([FromForm] string email)
		{
			try
			{ //método sin autenticar, busca el propietario x email
				var entidad = await contexto.Propietarios.FirstOrDefaultAsync(x => x.Email == email);
               var key = new SymmetricSecurityKey(
                            System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, entidad.Email),
                            new Claim("FullName", entidad.Nombre + " " + entidad.Apellido),
                            new Claim(ClaimTypes.Role, "Propietario"),
                        };

                        var token = new JwtSecurityToken(
                            issuer: config["TokenAuthentication:Issuer"],
                            audience: config["TokenAuthentication:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddDays(360),
                            signingCredentials: credenciales
                        );
                        var vToken = new JwtSecurityTokenHandler().WriteToken(token);
				//para hacer: si el propietario existe, mandarle un email con un enlace con el token
				//ese enlace servirá para resetear la contraseña
				//Dominio sirve para armar el enlace, en local será la ip y en producción será el dominio www...
				var url = this.GenerarUrlCompleta("Token", "Propietarios", environment);
				var dominio = url+"?access_token="+vToken;
				//añadir: .....?access_token=token
                var message = new MimeKit.MimeMessage();
				message.To.Add(new MailboxAddress(entidad.Nombre, entidad.Email));
				message.From.Add(new MailboxAddress("Inmobiliaria Ledesma", config["SMTPUser"]));
				message.Subject = "Link para resetear contraseña";
				message.Body = new TextPart("html")
				{
					Text = @$"<h1>Hola</h1>
					<p>¡Bienvenido, {entidad.Nombre}! Haz <a class=btn href={dominio}>click aqui</a> para resetear tu
                    contraseña</p>"
				};


				MailKit.Net.Smtp.SmtpClient client = new SmtpClient();
				client.ServerCertificateValidationCallback = (object sender,
					System.Security.Cryptography.X509Certificates.X509Certificate certificate,
					System.Security.Cryptography.X509Certificates.X509Chain chain,
					System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
				{ return true; };
				client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
				client.Authenticate(config["SMTPUser"], config["SMTPPass"]);
				await client.SendAsync(message);
               
                return Ok(entidad);
				//return entidad != null ? Ok("Email enviado.") : NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
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