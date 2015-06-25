using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebApplication1.Models;
using System.IO;

namespace WebApplication1.Controllers
{
    public class PostsController : Controller
    {
        private PostsDbContext db = new PostsDbContext();

        // GET: /Posts/
        public async Task<ActionResult> Index()
        {
            return View(await db.Posts.ToListAsync());
        }

        // GET: /Posts/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = await db.Posts.FindAsync(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: /Posts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="ID,UserID,DateTime,PostContent,PostFile")] Post post)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase file = Request.Files["file"];
                if (file.ContentLength > 0) 
                { 
                    byte[] imgBytes = null;

                    BinaryReader reader = new BinaryReader(file.InputStream);
                    imgBytes = reader.ReadBytes(file.ContentLength);

                    post.PostFile = imgBytes;
                }
                else
                   post.PostFile = null;

                post.UserID = User.Identity.GetUserId();
                post.DateTime = DateTime.Now;

                db.Posts.Add(post);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Posts/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = await db.Posts.FindAsync(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return PartialView("EditPartial", post);
        }

        // POST: /Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="ID,UserID,DateTime,PostContent,PostFile")] Post newPost)
        {
            if (ModelState.IsValid)
            {

                Post post = await db.Posts.FindAsync(newPost.ID);
                newPost.UserID = post.UserID;
                newPost.DateTime = post.DateTime;

                if (newPost.PostContent == null)
                    newPost.PostContent = post.PostContent;

                HttpPostedFileBase file = Request.Files["file"];
                bool isDelete = bool.Parse(Request.Form["isDeleted"]);

                if (!isDelete)
                {
                    if (file.ContentLength > 0)
                    {
                        byte[] imgBytes = null;

                        BinaryReader reader = new BinaryReader(file.InputStream);
                        imgBytes = reader.ReadBytes(file.ContentLength);

                        newPost.PostFile = imgBytes;
                    }
                    else
                        newPost.PostFile = post.PostFile;
                }
                else
                    newPost.PostFile = null;

                db.Entry(post).CurrentValues.SetValues(newPost);

                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Posts/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = await db.Posts.FindAsync(id);
            if (post == null)
            {
                return HttpNotFound();
            }
           return PartialView("DeletePartial", post);
            
        }

        // POST: /Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Post post = await db.Posts.FindAsync(id);
            db.Posts.Remove(post);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
