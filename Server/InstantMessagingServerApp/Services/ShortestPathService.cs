using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using InstantMessagingServerApp.Repositories;

namespace InstantMessagingServerApp.Services
{
    public class ShortestPathService
    {
        private static bool flag = false;

        public IEnumerable<Models.User> FriendshipWay(int ThisId, int TargetId)
        {
            var unitOfWork = new UnitOfWork();

            var dic = new Dictionary<int, List<int>>();

            var users = unitOfWork.UserRepository.GetAllUsers().Select(x => x.Id).ToList();
            foreach (int user in users)
                dic.Add(user, unitOfWork.FriendshipRepository.GetFriends(user).Select(x => x.Id).ToList());

            List<Models.User> err = new List<Models.User>();
            if (!dic.ContainsKey(ThisId) || !dic.ContainsKey(TargetId) || TargetId < 0 || ThisId < 0)
            {
                Models.User noUser = new Models.User();
                noUser.Id = ThisId;
                noUser.FirstName = "No Such User";
                noUser.LastName = "No Such User";
                noUser.UserName = "No Such User";
                noUser.Email = "No@Such.User";
                err.Add(noUser);
                return err;
            }

            Dictionary<int, List<int>> done = new Dictionary<int, List<int>>();
            List<int> path = new List<int>();
            path.Add(ThisId);
            done.Add(ThisId, path);

            int depth = 1;
            while (_FriendWay(ThisId, TargetId, dic, done, depth++))
            {
                if (depth > 20)
                {
                    Models.User noUser = new Models.User();
                    noUser.Id = ThisId;
                    noUser.FirstName = "No Such User";
                    noUser.LastName = "No Such User";
                    noUser.UserName = "No Such User";
                    noUser.Email = "No@Such.User";
                    err.Add(noUser);
                    return err;
                }
            }
            flag = false;

            var way = done[TargetId];

            var users2 = unitOfWork.UserRepository.GetAllUsers().ToList();
            List<Models.User> result = new List<Models.User>();
            if (way.Count > 0)
            {
                if (users.Contains(way.ElementAt(0)))
                    foreach (var id in way)
                        result.Add(users2.Where(x => x.Id == id).ToArray().ElementAt(0));
                else
                {
                    Models.User noUser = new Models.User();
                    noUser.Id = ThisId;
                    noUser.FirstName = "No Such User";
                    noUser.LastName = "No Such User";
                    noUser.UserName = "No Such User";
                    noUser.Email = "No@Such.User";
                    result.Add(noUser);
                }
            }
            else
            {
                Models.User noUser = new Models.User();
                noUser.Id = ThisId;
                noUser.FirstName = "No Such User";
                noUser.LastName = "No Such User";
                noUser.UserName = "No Such User";
                noUser.Email = "No@Such.User";
                result.Add(noUser);
            }
            return result;
        }

        private bool _FriendWay(int ThisId, int TargetId, Dictionary<int, List<int>> db,
                                Dictionary<int, List<int>> done, int depth = 1)
        {
            if (depth < 1)
                return true;

            List<int> fr = db[ThisId];
            foreach (int f in fr)
            {
                if (f == TargetId)
                {
                    int[] vec = new int[done[ThisId].Count];
                    done[ThisId].CopyTo(vec);
                    List<int> v = vec.ToList();
                    v.Add(f);
                    done.Add(f, v);
                    flag = true;
                    return false;
                }
                else if (flag)
                    return false;
                else if (depth > 1)
                {
                    _FriendWay(f, TargetId, db, done, depth - 1);
                }
                else if (done.ContainsKey(f))
                    continue;
                else
                {
                    int[] vec = new int[done[ThisId].Count];
                    done[ThisId].CopyTo(vec);
                    List<int> v = vec.ToList();
                    v.Add(f);
                    done.Add(f, v);
                    _FriendWay(f, TargetId, db, done, depth - 1);
                }
            }
            return true;
        }
    }
}