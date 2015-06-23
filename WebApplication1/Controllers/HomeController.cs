using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }

            else
            {
                return View("StartPage");
            }
        }

        public ActionResult UserProfilePartial()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            string currentID = User.Identity.GetUserId();

            currentUser.Interests = (from i in db.Interest
                                     where i.UserID == currentID
                                     select i).ToList();

            currentUser.Friends = (from f in db.Friends
                                   where f.UserID == currentID
                                   select f).ToList();

            List<ApplicationUser> friends = new List<ApplicationUser>();
            foreach (Friend friend in currentUser.Friends)
            {
                friends.Add((from u in db.Users where u.Id == friend.FriendID select u).ToList().ElementAt(0));
            }

            ViewBag.Friends = friends;

            ViewBag.FirstName = currentUser.FirstName;
            ViewBag.LastName = currentUser.LastName;

            string[] interestsArray = (from s in currentUser.Interests select s.InterestString).ToList().ToArray();
            ViewBag.Interests = interestsArray;

            string imageBase64Data = Convert.ToBase64String(currentUser.UserImage);
            string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);

            ViewBag.Image = imageDataURL;

            return View();
        }

        [HttpPost]
        public ActionResult Search(string search)
        {
            if (search == string.Empty)
            {
                return RedirectToAction("Index", "Home");
            }

            ApplicationDbContext context = new ApplicationDbContext();
            SearchViewModel model = new SearchViewModel();

            string[] sp = search.Split(' ');

            string potentialFirstName;
            string potentialLastName;
            List<ApplicationUser> potentialUsers = new List<ApplicationUser>();
            List<ApplicationUser> distinctPotentialUsers = new List<ApplicationUser>();
            if (sp.Length == 1)
            {
                potentialFirstName = sp[0];
                potentialLastName = sp[0];

                var queryRes = (from u in context.Users where u.FirstName.Contains(potentialFirstName) select u).ToList();

                if (queryRes.Any())
                    potentialUsers.AddRange(queryRes);

                queryRes = (from u in context.Users where u.LastName.Contains(potentialLastName) select u).ToList();

                if (queryRes.Any())
                    potentialUsers.AddRange(queryRes);

                distinctPotentialUsers.AddRange(potentialUsers.Distinct());

            }

            else if (sp.Length > 1)
            {
                potentialFirstName = sp[0];
                potentialLastName = sp[1];

                var queryRes = (from u in context.Users where u.FirstName.Contains(potentialFirstName) && u.LastName.Contains(potentialLastName) select u).ToList();

                if (queryRes.Any())
                    potentialUsers.AddRange(queryRes);

                queryRes = (from u in context.Users where u.LastName.Contains(potentialFirstName) && u.FirstName.Contains(potentialLastName) select u).ToList();

                if (queryRes.Any())
                    potentialUsers.AddRange(queryRes);

                distinctPotentialUsers.AddRange(potentialUsers.Distinct());


            }

            model.searchResult = distinctPotentialUsers;

            model.imageDataURLs = new List<string>();

            string imageBase64Data;
            string imageDataURL;
            for (int i = 0; i < model.searchResult.Count; i++)
            {
                imageBase64Data = Convert.ToBase64String(model.searchResult.ElementAt(i).UserImage);
                imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);

                model.imageDataURLs.Add(imageDataURL);
            }

            return View(model);
        }

        public ActionResult UserPage(string userName)
        {

            if (userName == User.Identity.GetUserName())
            {
                return RedirectToAction("myPage", "Home");
            }

            ApplicationDbContext context = new ApplicationDbContext();

            var list = (from u in context.Users where u.UserName == userName select u).ToList();

            if (list.Any())
            {
                ApplicationUser potentialUser = list.ElementAt(0);

                var potentialUserID = potentialUser.Id;

                potentialUser.Interests = (from i in context.Interest
                                           where i.UserID == potentialUserID
                                           select i).ToList();

                potentialUser.Friends = (from f in context.Friends
                                         where f.UserID == potentialUserID
                                         select f).ToList();

                List<ApplicationUser> friends = new List<ApplicationUser>();
                foreach (Friend friend in potentialUser.Friends)
                {
                    friends.Add((from u in context.Users where u.Id == friend.FriendID select u).ToList().ElementAt(0));
                }

                ViewBag.Friends = friends;

                ViewBag.userFirstName = potentialUser.FirstName;
                ViewBag.userLastName = potentialUser.LastName;

                string[] interestsArray = (from s in potentialUser.Interests select s.InterestString).ToList().ToArray();
                ViewBag.userInterests = interestsArray;

                string imageBase64Data = Convert.ToBase64String(potentialUser.UserImage);
                string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);

                ViewBag.userImage = imageDataURL;

                var currentUserID = User.Identity.GetUserId();

                List<Friend> friendList = (from i in context.Friends where i.UserID == currentUserID select i).ToList();
                bool flag = true;

                if (!friendList.Any())
                    flag = false;

                if (flag)
                    flag = (from i in friendList where i.FriendID == potentialUser.Id select i).ToList().Any();

                ViewBag.isFriend = flag;
                ViewBag.UserName = potentialUser.UserName;

            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddFriend(string friendUserName)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var friendID = (from f in context.Users where f.UserName == friendUserName select f).ToList().ElementAt(0).Id;
            var userID = User.Identity.GetUserId();

            context.Friends.Add(new Friend() { UserID = userID, FriendID = friendID });
            context.Friends.Add(new Friend() { UserID = friendID, FriendID = userID });
            await context.SaveChangesAsync();

            return RedirectToAction("UserPage", "Home", new { userName = friendUserName });
        }

        [HttpPost]
        public async Task<ActionResult> RemoveFriend(string friendUserName)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var friendID = (from f in context.Users where f.UserName == friendUserName select f).ToList().ElementAt(0).Id;
            var currentUserID = User.Identity.GetUserId();

            var friend = (from f in context.Friends where f.UserID == friendID && f.FriendID == currentUserID select f).ToList().ElementAt(0);
            var user = (from f in context.Friends where f.UserID == currentUserID && f.FriendID == friendID select f).ToList().ElementAt(0);

            context.Friends.Remove(friend);
            context.Friends.Remove(user);
            await context.SaveChangesAsync();

            return RedirectToAction("UserPage", "Home", new { userName = friendUserName });
        }


        public ActionResult myPage()
        {
            return View();
        }

        public ActionResult frndify()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var currentUserID = User.Identity.GetUserId();

             List<Interest> interestList = (from i in context.Interest
                                     where i.UserID == currentUserID
                                     select i).ToList();

             string[] currentUserInterests = (from s in interestList select s.InterestString).ToList().ToArray();

            List<ComparableInterest> pq = new List<ComparableInterest>();
            InterestComparer comp = new InterestComparer();

            foreach(ApplicationUser aUser in context.Users)
            {
                currentUserID = aUser.Id;
                if (currentUserID == User.Identity.GetUserId())
                    continue;
                interestList = (from i in context.Interest
                                where i.UserID == currentUserID
                                select i).ToList();
                string[] UserInterests = (from s in interestList select s.InterestString).ToList().ToArray();

                int Intersect = currentUserInterests.Intersect(UserInterests).ToList().Count;

                if (Intersect == 0)
                    continue;

                pq.Add(new ComparableInterest() { user = aUser, intersect = Intersect });
                pq.Sort(comp);

            }

            pq.Reverse();
            int n = 3;
            if (pq.Count < 3)
                n = pq.Count;
            List<ApplicationUser> topList = (from t in pq select t.user).ToList().GetRange(0, n);
            List<string> ImageDataURLs = new List<string>();

            string imageBase64Data;
            string imageDataURL;
            for (int i = 0; i < topList.Count; i++)
            {
                imageBase64Data = Convert.ToBase64String(topList.ElementAt(i).UserImage);
                imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);

                ImageDataURLs.Add(imageDataURL);
            }

            ViewBag.Top = topList;
            ViewBag.imageDataURLs = ImageDataURLs;

            return View();
        }

        public class ComparableInterest
        {
            public ApplicationUser user;
            public int intersect;

        }

        public class Class2
        {
            public void test()
            {
                List<ComparableInterest> classList = new List<ComparableInterest>();
                //add some data to the list
                InterestComparer comp = new InterestComparer();
                classList.Sort(comp);
            }
        }

        public class InterestComparer : Comparer<ComparableInterest>
        {
            public override int Compare(ComparableInterest x, ComparableInterest y)
            {
                int val = x.intersect.CompareTo(y.intersect);
                return val;
            }
        }
    }
}