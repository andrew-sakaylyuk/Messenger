using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantMessagingServerApp.Services
{
    public class Pagination
    {
        public int CountPages(int itemsCount, int itemsPerPage)
        {
            return (int)Math.Ceiling(itemsCount * 1.0 / itemsPerPage);
        }
    }
}