using InstantMessagingServerApp.Filters;
using InstantMessagingServerApp.Models;
using InstantMessagingServerApp.Services;
using InstantMessagingServerApp.ViewModels;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using InstantMessagingServerApp.Repositories;
using AutoMapper;
using Microsoft.Practices.Unity.Utility;

namespace InstantMessagingServerApp.Controllers
{
    [RoutePrefix("api/users")]
    [JwtAuthentication]
    public class UsersController : ApiController
    {
        private const int UsersPerPage = 25;

        protected readonly IUnitOfWork UnitOfWork;

        protected OnlineUsersWorker OnlineWorker;

        protected readonly JwtManager JwtManager;

        public UsersController(
            IUnitOfWork unitOfWork, OnlineUsersWorker onlineUsersWorker, JwtManager jwtManager)
        {
            UnitOfWork = unitOfWork;
            OnlineWorker = onlineUsersWorker;
            JwtManager = jwtManager;
        }

        // GET /api/users/{id}
        [HttpGet]
        [Route("{userId:int}", Name = "GetUserById")]
        public IHttpActionResult GetUser(int userId)
        {
            var user = UnitOfWork.UserRepository.GetById(userId);
            if (user == null) return NotFound();
            UserReturnModel uModel = Mapper.Map<User, UserReturnModel>(user);
            if (OnlineWorker.IsOnline(user))
                uModel.Online=true;
            return Ok(uModel);
        }

        // GET /api/users/{username}
        [HttpGet]
        [Route("{username}")]
        public IHttpActionResult GetUserByName(string userName)
        {
            var user = UnitOfWork.UserRepository.GetByName(userName);
            if (user == null) return NotFound();
            UserReturnModel uModel = Mapper.Map<User, UserReturnModel>(user);
            if (OnlineWorker.IsOnline(user))
                uModel.Online = true;
            return Ok(uModel);
        }

        // GET /api/users?[search parameters]&p={page}
        [HttpGet]
        public IHttpActionResult FindUsers(
            [FromUri] UserInfoBindingModel searchUser,
            [FromUri(Name = "p")] int page)
        {
            var allUsers = UnitOfWork.UserRepository.FindUsers(searchUser, UsersPerPage, page);
            //use this method for efficient search and marking online users
            var users = OnlineWorker.MarkIfOnline(allUsers);
            var pageCount = UnitOfWork.UserRepository.FindUsersPageCount(searchUser, UsersPerPage);
            Pair<UserReturnModel[], int> data = Pair.Make(users, pageCount);
            return Ok(data);
        }

        // DELETE /api/users
        [HttpDelete]
        public IHttpActionResult RemoveUser()
        {
            try
            {
                var userName = JwtManager.GetUserNameFromToken(
                    Request.Headers.Authorization.Parameter);
                var user = UnitOfWork.UserRepository.GetByName(userName);
                UnitOfWork.UserRepository.Remove(user.Id);
                UnitOfWork.Save();
                return Ok();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        // PUT /api/users
        [HttpPut]
        public IHttpActionResult ChangeUserInfo(
            [FromBody] UpdateUserInfoBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userName = JwtManager.GetUserNameFromToken(
                    Request.Headers.Authorization.Parameter);
                var user = UnitOfWork.UserRepository.GetByName(userName);
                if (user == null) return NotFound();
                if (model.Email != null)
                {
                    if ((model.Email != user.Email) &&
                        (UnitOfWork.UserRepository.GetByEmail(model.Email) == null))
                    {
                        user.Email = model.Email;
                    }
                    else if (model.Email != user.Email)
                    {
                        return Conflict();
                    }
                }
                if (model.FirstName != null)
                {
                    user.FirstName = model.FirstName;
                }
                if (model.LastName != null)
                {
                    user.LastName = model.LastName;
                }
                if (model.BirthDate != null)
                {
                    user.BirthDate = DateTime.ParseExact(model.BirthDate, "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture);
                }
                switch (model.Sex)
                {
                    case "Male":
                        user.Sex = SexEnum.Male;
                        break;
                    case "Female":
                        user.Sex = SexEnum.Female;
                        break;
                    case null:
                        break;
                    default:
                        user.Sex = SexEnum.Unknown;
                        break;
                }
                /** todo: change hotfix with roles to another decision of fixing exception */
                user.UserRole = UnitOfWork.RoleRepository.Get("User");
                UnitOfWork.UserRepository.Update(user);
                UnitOfWork.Save();
                var sex = "Unknown";
                switch (user.Sex)
                {
                    case SexEnum.Male:
                        sex = "Male";
                        break;
                    case SexEnum.Female:
                        sex = "Female";
                        break;
                }
                return Ok(new {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    BirthDate = user.BirthDate.Date.ToString("yyyy-MM-dd"),
                    Sex = sex
                });
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        // PUT /api/users/ChangeUsername
        [HttpPut]
        [Route("ChangeUsername")]
        public IHttpActionResult ChangeUsername( 
            [FromBody] ChangeUserNameBindingModel model)
        {
            if (model.UserName == null)
            {
                return BadRequest();
            }
            try
            {
                var userName = JwtManager.GetUserNameFromToken(
                    Request.Headers.Authorization.Parameter);
                var user = UnitOfWork.UserRepository.GetByName(userName);
                if (user == null) return NotFound();
                if (UnitOfWork.UserRepository.GetByName(model.UserName) != null)
                    return Conflict();
                user.UserName = model.UserName;
                /** todo: change hotfix with roles to another decision of fixing exception */
                user.UserRole = UnitOfWork.RoleRepository.Get("User");
                UnitOfWork.UserRepository.Update(user);
                UnitOfWork.Save();
                return Ok(new
                    {
                        Token = JwtManager.GenerateToken(user.UserName, user.PasswordHash),
                        user.UserName
                    }
                );
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        // PUT /api/users/ChangePassword
        [HttpPut]
        [Route("ChangePassword")]
        public IHttpActionResult ChangePassword(
            [FromBody] ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var userName = JwtManager.GetUserNameFromToken(
                    Request.Headers.Authorization.Parameter);
                var user = UnitOfWork.UserRepository.GetByName(userName);
                if (user == null) return NotFound();
                if (user.PasswordHash != PasswordEncoder.Encode(model.OldPassword))
                    return BadRequest();
                user.PasswordHash = PasswordEncoder.Encode(model.NewPassword);
                /** todo: change hotfix with roles to another decision of fixing exception */
                user.UserRole = UnitOfWork.RoleRepository.Get("User");
                UnitOfWork.UserRepository.Update(user);
                UnitOfWork.Save();
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

        // POST /api/users/UploadAvatar
        [HttpPost]
        [Route("UploadAvatar")]
        public async Task<IHttpActionResult> UploadAvatar()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                var file = provider.Contents[0];
                if (file == null)
                {
                    return BadRequest();
                }
                var buffer = await file.ReadAsByteArrayAsync();
                using (var ms = new MemoryStream(buffer))
                {
                    var avatar = Image.FromStream(ms);
                    var ratioX = (double)300 / avatar.Width;
                    var ratioY = (double)300 / avatar.Height;
                    var ratio = Math.Min(ratioX, ratioY);
                    var newWidth = (int)(avatar.Width * ratio);
                    var newHeight = (int)(avatar.Height * ratio);
                    var newAvatar = new Bitmap(newWidth, newHeight);
                    using (var graphics = Graphics.FromImage(newAvatar))
                        graphics.DrawImage(avatar, 0, 0, newWidth, newHeight);
                    var userName = JwtManager.GetUserNameFromToken(
                        Request.Headers.Authorization.Parameter);
                    var filePath = HttpContext.Current.Server
                        .MapPath("~/wwwroot/Avatars/" + userName + ".png");
                    newAvatar.Save(filePath, ImageFormat.Png);
                    var user = UnitOfWork.UserRepository.GetByName(userName);
                    user.AvatarUrl = HttpContext.Current.Request.Url
                        .GetLeftPart(UriPartial.Authority) +
                        @"/Avatars/" + userName + ".png";
                    /** todo: change hotfix with roles to another decision of fixing exception */
                    user.UserRole = UnitOfWork.RoleRepository.Get("User");
                    UnitOfWork.UserRepository.Update(user);
                    UnitOfWork.Save();
                    return Ok(new
                    {
                        user.AvatarUrl
                    });
                }  
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

    }
}
