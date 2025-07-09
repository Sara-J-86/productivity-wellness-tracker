using ProductivityApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity;

namespace ProductivityApp.Controllers
{
    public class MoodController : Controller
    {
        private AppdbContext db = new AppdbContext();

        private int? GetCurrentUserId()
        {
            return Session["UserID"] as int?;
        }

        public ActionResult Index(string searchDate = "")
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var moods = db.Moods.Where(m => m.UserId == userId);

            if (!string.IsNullOrEmpty(searchDate) && DateTime.TryParse(searchDate, out DateTime date))
            {
                moods = moods.Where(m => DbFunctions.TruncateTime(m.MoodDate) == date.Date);
            }

            return View(moods.OrderByDescending(m => m.MoodDate).ToList());
        }

        public ActionResult LogMood()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogMood(int moodValue)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var existing = db.Moods.FirstOrDefault(m => m.UserId == userId && DbFunctions.TruncateTime(m.MoodDate) == today);

            if (existing == null)
            {
                db.Moods.Add(new Mood
                {
                    UserId = userId.Value,
                    MoodValue = moodValue,
                    MoodDate = today
                });
            }
            else
            {
                existing.MoodValue = moodValue;
                db.Entry(existing).State = EntityState.Modified;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            int? userId = GetCurrentUserId();
            var mood = db.Moods.Find(id);

            if (mood == null || mood.UserId != userId)
                return HttpNotFound();

            return View(mood);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Mood mood)
        {
            if (ModelState.IsValid)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ProductivityDB"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Moods SET MoodDate = @MoodDate, MoodValue = @MoodValue WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@MoodDate", mood.MoodDate);
                    cmd.Parameters.AddWithValue("@MoodValue", mood.MoodValue);
                    cmd.Parameters.AddWithValue("@Id", mood.Id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Mood updated successfully.";
                return RedirectToAction("Index");
            }

            return View(mood);
        }

    }

}
