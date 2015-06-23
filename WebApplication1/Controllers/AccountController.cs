using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using WebApplication1.Models;
using System.IO;
using System.Collections;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Index", "Home");

        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.list = GetInterestCategories();
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            bool flag = false;
            if (ModelState.IsValid)
            {
                /* Validate for x amount of Intersts selected */
                if ((from i in model.Interests where i != "---" select i).ToList().Count < 2)
                    flag = true;

                if (!flag)
                {
                    /* Validate for distinct Interest choices */
                    List<string> chckList = new List<string>();
                    for (int i = 0; i < model.Interests.Length; i++)
                    {
                        string str = model.Interests[i];
                        if (str == "---")
                            continue;

                        if ((from _i in chckList where _i == str select _i).Any())
                        {
                            flag = true;
                            break;
                        }
                        chckList.Add(str);
                    }
                }

                if (!flag)
                {
                    var user = new ApplicationUser()
                    {
                        UserName = model.UserName,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        BirthDate = model.BirthDate,
                        Gender = model.Gender,
                    };

                    /* Get Interests from model, if element equals "---" don't add it */
                    user.Interests = new List<Interest>();
                    for (int i = 0; i < model.Interests.Length; i++)
                    {
                        String str = model.Interests.ElementAt(i);
                        if (str.Equals("---"))
                            continue;
                        user.Interests.Add(new Interest() { InterestString = str, UserID = user.Id });
                    }

                    /* Get Image from model form, if it's empty load default image */
                    HttpPostedFileBase file = Request.Files["file"];
                    byte[] imgBytes = null;

                    BinaryReader reader = new BinaryReader(file.InputStream);
                    imgBytes = reader.ReadBytes(file.ContentLength);

                    if (imgBytes.Length == 0)
                    {
                        FileStream fs = System.IO.File.Open(Server.MapPath("~/Content/Images/JohnDoe.png"), FileMode.Open);
                        imgBytes = null;
                        reader = new BinaryReader(fs);
                        imgBytes = reader.ReadBytes((int)fs.Length);
                        fs.Close();
                    }

                    user.UserImage = imgBytes;

                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            if (flag)
                AddErrors(new IdentityResult("Please Enter at least two different Interests"));

            ViewBag.list = GetInterestCategories();

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.InterestsSuccess ? "Interests Changed Successfully."
                : message == ManageMessageId.InterestsFailure ? "Enter at least two different Interests."
                : message == ManageMessageId.ImageSuccess ? "Picture Changed Successfully."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            ViewBag.list = GetInterestCategories();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageInterests(ManageInterestsUserViewModel model)
        {
            bool flag = false;
            if (ModelState.IsValid)
            {
                /* Validate for x amount of Intersts selected */
                if ((from i in model.Interests where i != "---" select i).ToList().Count < 2)
                    flag = true;

                if (!flag)
                {
                    /* Validate for distinct Interest choices */
                    List<string> chckList = new List<string>();
                    for (int i = 0; i < model.Interests.Length; i++)
                    {
                        string str = model.Interests[i];
                        if (str == "---")
                            continue;

                        if ((from _i in chckList where _i == str select _i).Any())
                        {
                            flag = true;
                            break;
                        }
                        chckList.Add(str);
                    }
                }
                if (!flag)
                {
                    var currentUser = UserManager.FindById(User.Identity.GetUserId());
                    var currentUserId = User.Identity.GetUserId();
                    ApplicationDbContext db = new ApplicationDbContext();
                    List<Interest> list = (from i in db.Interest where i.UserID == currentUserId select i).ToList();
                    db.Interest.RemoveRange(list);
                    db.SaveChanges();
                    /* Get Interests from model, if element equals "---" don't add it */
                    currentUser.Interests = new List<Interest>();
                    for (int i = 0; i < model.Interests.Length; i++)
                    {
                        String str = model.Interests.ElementAt(i);
                        if (str.Equals("---"))
                            continue;
                        currentUser.Interests.Add(new Interest() { InterestString = str, UserID = currentUser.Id });
                    }

                    IdentityResult result = await UserManager.UpdateAsync(currentUser);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.InterestsSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            if(flag)
                return RedirectToAction("Manage", new { Message = ManageMessageId.InterestsFailure});

            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageImage(ManageImageUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = UserManager.FindById(User.Identity.GetUserId());

                /*Get Image from model form, if it's empty load default image*/
                HttpPostedFileBase file = Request.Files["file"];
                byte[] imgBytes = null;

                BinaryReader reader;
                if (file != null)
                {
                    reader = new BinaryReader(file.InputStream);
                    imgBytes = reader.ReadBytes(file.ContentLength);
                }

                if (imgBytes == null || imgBytes.Length == 0)
                {
                    FileStream fs = System.IO.File.Open(Server.MapPath("~/Content/Images/JohnDoe.png"), FileMode.Open);
                    imgBytes = null;
                    reader = new BinaryReader(fs);
                    imgBytes = reader.ReadBytes((int)fs.Length);
                    fs.Close();
                }

                currentUser.UserImage = imgBytes;

                IdentityResult result = await UserManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                {
                    return RedirectToAction("Manage", new { Message = ManageMessageId.ImageSuccess });
                }
                else
                {
                    AddErrors(result);
                }
            }

            return RedirectToAction("Manage");
        }


        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            ViewBag.list = GetInterestCategories();

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        private SelectList GetInterestCategories()
        {
            string[] sp = new string[] { "---", "Music", "Movies", "Sports", "Travel", "Love", "Programming",
                        "TV", "Parties", "Games" };
            List<string> list = new List<string>(sp);
            SelectList ls = new SelectList(list);

            return ls;
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            InterestsSuccess,
            InterestsFailure,
            ImageSuccess,
            Error,
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }
}