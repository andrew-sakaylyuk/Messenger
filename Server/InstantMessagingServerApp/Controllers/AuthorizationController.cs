using System;
using InstantMessagingServerApp.ViewModels;
using System.Web.Http;
using AutoMapper;
using InstantMessagingServerApp.Services;
using InstantMessagingServerApp.Models;
using InstantMessagingServerApp.Repositories;
using InstantMessagingServerApp.Filters;

namespace InstantMessagingServerApp.Controllers
{
    [RoutePrefix("api/account")]
    public class AuthorizationController : ApiController
    {
        protected readonly IUnitOfWork UnitOfWork;

        protected readonly JwtManager JwtManager;

        public AuthorizationController(IUnitOfWork unitOfWork, JwtManager jwtManager)
        {
            UnitOfWork = unitOfWork;
            JwtManager = jwtManager;
        }

        // POST /api/account/signup
        [HttpPost]
        [Route("signup")]
        [AllowAnonymous]
        public IHttpActionResult RegisterUser([FromBody] 
            CreateUserBindingModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if ((UnitOfWork.UserRepository.GetByName(userModel.UserName) != null) ||
                    (UnitOfWork.UserRepository.GetByEmail(userModel.Email) != null))
                    return Conflict();
                var user = Mapper.Map<CreateUserBindingModel, User>(userModel);
                user.Id = 0; // reset Id
                user.UserRole = UnitOfWork.RoleRepository.Get("User");
                UnitOfWork.UserRepository.Add(user);
                UnitOfWork.Save();
                Uri locationHeader = new Uri(Url.Link("GetUserById",
                    new { userId = user.Id }));
                return Created(locationHeader,
                    Mapper.Map<User, UserReturnModelWithToken>(user));
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        // POST /api/account/signin
        [HttpPost]
        [Route("signin")]
        [AllowAnonymous]
        public IHttpActionResult LoginUser([FromBody] LoginUserBindingModel loginUserModel)
        {
            try
            {
                var user = UnitOfWork.UserRepository.GetByName(loginUserModel.UserName);
                if (user != null && 
                    user.PasswordHash == PasswordEncoder.Encode(loginUserModel.Password))
                {
                    return Ok(Mapper.Map<User, UserReturnModelWithToken>(user));
                }
                return BadRequest();
            }
            catch (Exception)
            {

                return InternalServerError();
            }
        } 

        // POST /api/account/refreshToken
        [HttpPost]
        [Route("refreshToken")]
        [JwtAuthentication]
        public IHttpActionResult RefreshToken()
        {
            try
            {
                var userName = JwtManager.GetUserNameFromToken(
                    Request.Headers.Authorization.Parameter);
                var user = UnitOfWork.UserRepository.GetByName(userName);
                if (user == null) return NotFound();
                return Ok(new
                {
                    Token = JwtManager.GenerateToken(user.UserName, user.PasswordHash)
                });
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

    }
}
