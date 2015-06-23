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

            ViewBag.FirstName = currentUser.FirstName;
            ViewBag.LastName = currentUser.LastName;

            string[] interestsArray = (from s in currentUser.Interests select s.InterestString).ToList().ToArray();
            string[] friendsArray = (from s in currentUser.Friends select s.FriendID).ToList().ToArray();

            ViewBag.Interests = interestsArray;
            ViewBag.Friends = friendsArray;

            string imageBase64Data = Convert.ToBase64String(currentUser.UserImage);
            string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);

            ViewBag.Image = imageDataURL;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}