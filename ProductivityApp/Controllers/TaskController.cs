using System;
using System.Linq;
using System.Web.Mvc;
using ProductivityApp.Models;

namespace ProductivityApp.Controllers
{
    public class TaskController : Controller
    {
        private AppdbContext db = new AppdbContext();

        private int? GetCurrentUserId()
        {
            return Session["UserID"] as int?;
        }

        public ActionResult Index()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var tasks = db.Tasks.Where(t => t.UserId == userId).ToList();
            return View(tasks);
        }

        public ActionResult Create()
        {
            return View(new UserTask());
        }

        [HttpPost]
        public ActionResult Create(UserTask task)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                task.UserId = userId.Value;
                db.Tasks.Add(task);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(task);
        }

        public ActionResult Edit(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var task = db.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (task == null)
                return HttpNotFound();

            return View(task);
        }

        [HttpPost]
        public ActionResult Edit(UserTask task)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var originalTask = db.Tasks.FirstOrDefault(t => t.Id == task.Id && t.UserId == userId);
            if (originalTask == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                originalTask.Title = task.Title;
                originalTask.Description = task.Description;
                originalTask.DueDate = task.DueDate;
                originalTask.IsComplete = task.IsComplete;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(task);
        }

        public ActionResult Delete(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var task = db.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (task == null)
                return HttpNotFound();

            db.Tasks.Remove(task);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Complete(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var task = db.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (task == null)
                return HttpNotFound();

            task.IsComplete = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
