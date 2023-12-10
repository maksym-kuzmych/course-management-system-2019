using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CourseManagement.Models;

namespace CourseManagement.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified, ActivationCode")]User user)
        {
            bool Status = false;
            string message = "";
            if (ModelState.IsValid)
            {
                var isExist = IsEmailExist(user.EmailId);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                user.ActivationCode = Guid.NewGuid();
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                user.IsEmailVerified = false;
                user.IsAdmin = false;
                user.isLecturer = false;
                user.isStudent = true;
                string imgPath = "/Content/img.png";
                if (user.PhotoObj != null && user.PhotoObj.ContentLength > 0)
                {
                    string filePath = Path.Combine(Server.MapPath("/Content"), Path.GetFileName(user.PhotoObj.FileName));
                    imgPath = "/Content/" + Path.GetFileName(user.PhotoObj.FileName);
                    user.PhotoObj.SaveAs(filePath);
                }
                user.Photo = imgPath;
                using (MyDatabaseEntities dc = new MyDatabaseEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();
                    SendVerificationLinkEmail(user.EmailId, user.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link " +
                              " has been sent to your email id: " + user.EmailId;
                    Status = true;
                }
            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }


        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false;
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string returnUrl)
        {
            string message = "";
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailId == login.EmailId).FirstOrDefault();
                if (v != null)
                {
                    FormsAuthentication.SetAuthCookie(login.ToString(), true);
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;
                        var ticket = new FormsAuthenticationTicket(login.EmailId, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);
                        if (v.IsEmailVerified)
                            return RedirectToActionPermanent("Index", "Home", v);
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailId == emailID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailId, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/User/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("sigmaautoresponse@gmail.com", "SigmaProject");
            var toEmail = new MailAddress(emailId);
            var fromEmailPassword = "Qwerty322q";

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account was successfully created";
                body = "<br></br>We are excited to tell you that your account is successfully created. Please click on the link below to verify your account" +
                    " <br></br><a href='" + link + "'>" + link + "</a>";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br></br>We got request for reset your account password. Please click on the link below to reset your password" +
                    " <br></br><a href='" + link + "'>Reset password link</a>";
            }

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            string message = "";
            bool status;
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var account = dc.Users.Where(a => a.EmailId == EmailID).FirstOrDefault();
                if (account != null)
                {
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(account.EmailId, resetCode, "ResetPassword");
                    account.ResetPasswordCode = resetCode;
                    status = true;
                    dc.Configuration.ValidateOnSaveEnabled = false;
                    dc.SaveChanges();
                    message = "Reset password link has been sent to your email.";
                }
                else
                {
                    status = false;
                    message = "Account not found.";
                }
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
        }

        public ActionResult ResetPassword(string id)
        {
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var user = dc.Users.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            string message = "";
            bool status = false;
            if (ModelState.IsValid)
            {
                using (MyDatabaseEntities dc = new MyDatabaseEntities())
                {
                    var user = dc.Users.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(model.NewPassword);
                        user.ResetPasswordCode = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        status = true;
                        message = "New password updated successfully.";
                    }
                }
            }
            else
            {
                status = false;
                message = "Something invalid.";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(model);
        }

        [HttpGet]
        public ActionResult Account()
        {
            using (var dc = new MyDatabaseEntities())
            {
                string username = User.Identity.Name;
                var user = dc.Users.Where(a => a.EmailId == username).FirstOrDefault();
                return View(user);
            }
        }

        [HttpPost]
        public ActionResult Account(User user)
        {
            using (var dc = new MyDatabaseEntities())
            {
                string imgPath = user.Photo;
                if (user.PhotoObj != null && user.PhotoObj.ContentLength > 0)
                {
                    string filePath = Path.Combine(Server.MapPath("/Content"), Path.GetFileName(user.PhotoObj.FileName));
                    imgPath = "/Content/" + Path.GetFileName(user.PhotoObj.FileName);
                    user.PhotoObj.SaveAs(filePath);
                }
                user.Photo = imgPath;
                user.ConfirmPassword = user.Password;
                dc.Entry(user).State = EntityState.Modified;
                dc.SaveChanges();
                return View(user);
            }
        }
    }
}