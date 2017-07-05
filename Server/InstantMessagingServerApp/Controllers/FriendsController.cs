using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using InstantMessagingServerApp.Filters;
using InstantMessagingServerApp.ViewModels;
using InstantMessagingServerApp.Services;
using InstantMessagingServerApp.Repositories;
using AutoMapper;
using InstantMessagingServerApp.Models;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity.Utility;

namespace InstantMessagingServerApp.Controllers
{
    [RoutePrefix("api/friends")]
    [JwtAuthentication]
    public class FriendsController : ApiController
    {
        protected readonly IUnitOfWork UnitOfWork;

        private const int UsersPerPage = 25;

        protected OnlineUsersWorker OnlineWorker;

        protected readonly JwtManager JwtManager;

        public FriendsController(
            IUnitOfWork unitOfWork, OnlineUsersWorker onlineUsersWorker, JwtManager jwtManager)
        {
            UnitOfWork = unitOfWork;
            OnlineWorker = onlineUsersWorker;
            JwtManager = jwtManager;
        }

        // POST /api/friends/add/{receiverId}
        [HttpPost]
        [Route("add/{receiverId:int}")]
        public IHttpActionResult AddFriendshipRequest(int receiverId)
        {
            try
            {
                var userName = JwtManager.GetUserNameFromToken(
                    Request.Headers.Authorization.Parameter);
                var user = UnitOfWork.UserRepository.GetByName(userName);
                if (user == null || UnitOfWork.UserRepository.GetById(receiverId) == null)
                {
                    return BadRequest();
                }
                UnitOfWork.FriendshipRepository.AddFriendshipRequest(user.Id, receiverId);
                UnitOfWork.Save();
                return StatusCode(HttpStatusCode.Created);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        // PUT /api/friends/confirm/{senderId}
        [HttpPut]
        [Route("confirm/{senderId:int}")]
        public IHttpActionResult ConfirmFriendshipRequest(int senderId)
        {
            try
            {
                var userName = JwtManager.GetUserNameFromToken(
                    Request.Headers.Authorization.Parameter);
                var user = UnitOfWork.UserRepository.GetByName(userName);
                if (user == null || UnitOfWork.UserRepository.GetById(senderId) == null)
                {
                    return BadRequest();
                }
                UnitOfWork.FriendshipRepository.ConfirmFriendshipRequest(senderId, user.Id);
                UnitOfWork.Save();
                return StatusCode(HttpStatusCode.Created);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        // GET /api/friends/onlineCount
        [HttpGet]
        [Route("onlineCount")]
        public IHttpActionResult GetOnlineFriendsCount()
        {
            var userName = JwtManager.GetUserNameFromToken(
                Request.Headers.Authorization.Parameter);
            var user = UnitOfWork.UserRepository.GetByName(userName);
            int onlineCounter = 0;
            var friends = UnitOfWork.FriendshipRepository.GetFriends(user.Id);
            var onlineFriends = OnlineWorker.MarkIfOnline(friends);
            onlineFriends.ForEach(f =>
            {
                if (f.Online) onlineCounter++;
            });
            return Ok(onlineCounter);
        }

        // GET /api/friends/count
        [HttpGet]
        [Route("count")]
        public IHttpActionResult GetFriendsCount()
        {
            var userName = JwtManager.GetUserNameFromToken(
                Request.Headers.Authorization.Parameter);
            var user = UnitOfWork.UserRepository.GetByName(userName);
            var friendsCount = UnitOfWork.FriendshipRepository.GetFriendsCount(user.Id);
            return Ok(friendsCount);
        }

        // GET /api/friends?p={page}
        [HttpGet]
        public IHttpActionResult GetFriends([FromUri(Name = "p")] int page)
        {
            var userName = JwtManager.GetUserNameFromToken(
                Request.Headers.Authorization.Parameter);
            var user = UnitOfWork.UserRepository.GetByName(userName);
            var friends = UnitOfWork.FriendshipRepository.GetFriends(user.Id, UsersPerPage, page);
            var users = OnlineWorker.MarkIfOnline(friends);
            var pageCount = UnitOfWork.FriendshipRepository.GetFriendsPageCount(user.Id, UsersPerPage);
            Pair<UserReturnModel[], int> data = Pair.Make(users, pageCount);
            return Ok(data);
        }

        // GET /api/friends/requests?p={page}
        [HttpGet]
        [Route("requests")]
        public IHttpActionResult GetFriendshipRequests([FromUri(Name = "p")] int page)
        {
            var userName = JwtManager.GetUserNameFromToken(
                Request.Headers.Authorization.Parameter);
            var user = UnitOfWork.UserRepository.GetByName(userName);
            var friendshipRequests = UnitOfWork.FriendshipRepository
                .GetAllFriendshipRequests(user.Id, UsersPerPage, page);
            var users = OnlineWorker.MarkIfOnline(friendshipRequests);
            var pageCount = UnitOfWork.FriendshipRepository
                .GetAllFriendshipRequestsPageCount(user.Id, UsersPerPage);
            Pair<UserReturnModel[], int> data = Pair.Make(users, pageCount);
            return Ok(data);
        }

        // GET /api/friends/mutual?userId={userId}&p={page}
        [HttpGet]
        [Route("mutual")]
        public IHttpActionResult GetMutualFriends([FromUri] int userId, [FromUri(Name = "p")] int page)
        {
            var userName = JwtManager.GetUserNameFromToken(
                Request.Headers.Authorization.Parameter);
            var thisUser = UnitOfWork.UserRepository.GetByName(userName);
            var mutualFriends = UnitOfWork.FriendshipRepository
                .GetMutualFriends(thisUser.Id, userId, UsersPerPage, page);
            var users = OnlineWorker.MarkIfOnline(mutualFriends);
            var pageCount = UnitOfWork.FriendshipRepository
                .GetMutualFriendsPageCount(thisUser.Id, userId, UsersPerPage);
            Pair<UserReturnModel[], int> data = Pair.Make(users, pageCount);
            return Ok(data);
        }

        // GET /api/friends/areFriends?userId={userId}&friendId={friendId}
        [HttpGet]
        [Route("areFriends")]
        public bool AreFriends(
            [FromUri] int userId, [FromUri] int friendId)
        {
            return UnitOfWork.FriendshipRepository.AreFriends(userId, friendId);
        }

        // GET /api/friends/friendshipRequestAlreadyExists?senderId={senderId}&receiverId={receiverId}
        [HttpGet]
        [Route("friendshipRequestAlreadyExists")]
        public bool FriendshipRequestAlreadyExists(
            [FromUri] int senderId, [FromUri] int receiverId)
        {
            return UnitOfWork.FriendshipRepository
                .FriendshipRequestAlreadyExists(senderId, receiverId);
        }

        // GET /api/friends/shortestPath?userId={userId}
        [HttpGet]
        [Route("shortestPath")]
        public IHttpActionResult GetShortestPath([FromUri] int userId)
        {
            var userName = JwtManager.GetUserNameFromToken(
                Request.Headers.Authorization.Parameter);
            var thisUser = UnitOfWork.UserRepository.GetByName(userName);
            var friendshipWay = new ShortestPathService().
                FriendshipWay(thisUser.Id, userId);
            var users = OnlineWorker.MarkIfOnline(friendshipWay);
            return Ok(users);
        }

        // DELETE /api/friends/{friendId}
        [HttpDelete]
        [Route("{friendId:int}")]
        public IHttpActionResult RemoveFriend(int friendId)
        {
            var userName = JwtManager.GetUserNameFromToken(
                Request.Headers.Authorization.Parameter);
            var user = UnitOfWork.UserRepository.GetByName(userName);
            if (user == null || !UnitOfWork.FriendshipRepository
                    .AreFriends(user.Id, friendId))
            {
                return NotFound();
            }
            try
            {
                UnitOfWork.FriendshipRepository.RemoveFriendship(user.Id, friendId);
                UnitOfWork.Save();
                return Ok();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}
