using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;
using ProductivityApp.Models;

public class AccountController : Controller
{
    private string connectionString = ConfigurationManager.ConnectionStrings["ProductivityDB"].ConnectionString;

    // GET: Account/Register
    public ActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    public ActionResult Register(User model)
    {
        if (ModelState.IsValid)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ProductivityDB"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Users (FullName, Email, PasswordHash, IsFirstLogin) 
                             VALUES (@FullName, @Email, HASHBYTES('SHA2_256', @PasswordHash), 1)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FullName", model.FullName);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", model.PasswordHash); // Plain password (hashed in query)

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // ✅ Success: Redirect to Login
                        TempData["SuccessMessage"] = "Registration successful. Please log in.";
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Registration failed. Please try again.";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Error: " + ex.Message;
                }
            }
        }

        return View(model);
    }



    // GET: Account/Login
    public ActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    public ActionResult Login(User model)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ProductivityDB"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = "SELECT Id, FullName, IsFirstLogin FROM Users WHERE Email = @Email AND PasswordHash = HASHBYTES('SHA2_256', @Password)";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Password", model.PasswordHash); // Plain password (hashed in SQL)

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int userIdFromDb = Convert.ToInt32(reader["Id"]);
                string fullNameFromDb = reader["FullName"].ToString();
                bool isFirstLogin = Convert.ToBoolean(reader["IsFirstLogin"]);

                Session["UserID"] = userIdFromDb;
                Session["UserName"] = fullNameFromDb;

                reader.Close();
                con.Close();

                if (isFirstLogin)
                {
                    return RedirectToAction("Profile", "Account");
                }
                else
                {
                    return RedirectToAction("MainDashboard", "Home");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid credentials";
                return View();
            }
        }
    }




    // GET: Account/Profile
    public ActionResult Profile()
    {
        if (Session["UserID"] == null) return RedirectToAction("Login");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string query = "SELECT FullName, Email FROM Users WHERE Id = @Id";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Id", Session["UserID"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        User user = new User
                        {
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString()
                        };
                        return View(user);
                    }
                }
            }
        }
        return RedirectToAction("Login");
    }

    // POST: Account/Profile
    [HttpPost]
    public ActionResult Profile(User user)
    {
        if (Session["UserID"] == null) return RedirectToAction("Login");

        bool passwordUpdated = false;

        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = "UPDATE Users SET FullName = @FullName, Email = @Email";

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    query += ", PasswordHash = HASHBYTES('SHA2_256', @PasswordHash), IsFirstLogin = 0"; // 👈 Update IsFirstLogin here
                    passwordUpdated = true;
                }

                query += " WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Id", Session["UserID"]);

                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    }

                    cmd.ExecuteNonQuery();
                }
            }

            if (passwordUpdated)
            {
                TempData["SuccessMessage"] = "Profile updated successfully. Please login with your new password.";
                Session.Clear();
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.SuccessMessage = "Profile updated successfully!";
                return View(user);
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error updating profile: " + ex.Message;
            return View(user);
        }
    }


    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Login");
    }
}