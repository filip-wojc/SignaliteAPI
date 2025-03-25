using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Exceptions;

namespace SignaliteWebAPI.Controllers;
/// <summary>
/// Controller used for the purpose of tests in the client, should be removed or dropped later on
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class ErrorController(SignaliteDbContext context) : ControllerBase
{

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetAuth(){
            return "secret";
        }

      
        [HttpGet("not-found")]
        public ActionResult GetNotFound(){
            return NotFound();
        }

      
        [HttpGet("server-error")]
        public ActionResult<User> GetServerError(){
         
            var temp = context.Users.Find(-1) ?? throw new Exception("User not found!"); // null dereference casues server error
         
            return temp;
        }

      
        [HttpGet("bad-request")]
        public ActionResult GetBadRequest(){
         
            return BadRequest("Bad request message :)");
        }
        
        [HttpGet("forbidden")]
        public ActionResult GetForbidden(){
         
            return Forbid();
        }
        
        // To test custom exceptions just create another endpoint like token-error and throw it.
        [HttpGet("token-exception")]
        public ActionResult GetTokenException(){
         
            throw new TokenException("Invalid token");
            
        }
        
        [HttpGet("cloudinary-exception")]
        public ActionResult GetCloudinaryException(){
         
            throw new CloudinaryException("Cloudinary Error");
        }
        
        [HttpGet("config-exception")]
        public ActionResult GetConfigException(){
         
            throw new ConfigException("Invalid configuration");
        }
        
        [HttpGet("auth-exception")]
        public ActionResult GetAuthException(){
         
            throw new AuthException("Unauthorized :(");
        }
    
        [HttpGet("forbid-exception")]
        public ActionResult GetForbidException(){
         
            throw new ForbidException("Forbidden!");
        }
        
        [HttpGet("validator-exception")]
        public ActionResult GetValidatorException(){
         
            throw new ValidatorException("Validation failure");
        }
        


}