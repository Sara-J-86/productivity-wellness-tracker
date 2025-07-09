using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Configuration;

namespace ProductivityApp.Controllers
{
    public class HabitController : Controller
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProductivityDB"].ConnectionString;

        public ActionResult Index()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public ActionResult AddHabit(string habitName, DateTime startDate)
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Habits (UserID, HabitName, StartDate) VALUES (@UserID, @HabitName, @StartDate)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@HabitName", habitName);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Track");
        }
        [HttpPost]
        public ActionResult Log(int habitId, DateTime logDate)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connStr = ConfigurationManager.ConnectionStrings["ProductivityDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string checkQuery = "SELECT COUNT(*) FROM HabitLogs WHERE HabitId = @HabitId AND LogDate = @LogDate";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@HabitId", habitId);
                checkCmd.Parameters.AddWithValue("@LogDate", logDate);

                int exists = (int)checkCmd.ExecuteScalar();
                if (exists == 0)
                {
                    string insertQuery = "INSERT INTO HabitLogs (HabitId, LogDate) VALUES (@HabitId, @LogDate)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@HabitId", habitId);
                    insertCmd.Parameters.AddWithValue("@LogDate", logDate);
                    insertCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Track", new { id = habitId });
        }

        [HttpPost]
        public ActionResult Delete(int habitId)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connStr = ConfigurationManager.ConnectionStrings["ProductivityDB"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Delete logs first due to foreign key constraints
                var deleteLogsCmd = new SqlCommand("DELETE FROM HabitLogs WHERE HabitID = @HabitID", conn);
                deleteLogsCmd.Parameters.AddWithValue("@HabitID", habitId);
                deleteLogsCmd.ExecuteNonQuery();

                // Then delete the habit
                var deleteHabitCmd = new SqlCommand("DELETE FROM Habits WHERE HabitID = @HabitID AND UserID = @UserID", conn);
                deleteHabitCmd.Parameters.AddWithValue("@HabitID", habitId);
                deleteHabitCmd.Parameters.AddWithValue("@UserID", userId);
                deleteHabitCmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }



        public ActionResult Track()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LogHabit(int habitId, DateTime logDate)
        {
            string msg = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO HabitLogs (HabitID, LogDate) VALUES (@HabitID, @LogDate)", conn);
                cmd.Parameters.AddWithValue("@HabitID", habitId);
                cmd.Parameters.AddWithValue("@LogDate", logDate);
                cmd.ExecuteNonQuery();
                msg = "Logged successfully";
            }
            return Json(new { message = msg });
        }
    }
}
