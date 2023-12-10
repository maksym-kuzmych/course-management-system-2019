using CourseManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CourseManagement.Controllers
{
    public class CourseController : Controller
    {
        public ActionResult AllCourses()
        {
            bool status = false;
            string email = User.Identity.Name;
            using (var dc = new MyDatabaseEntities())
            {
                var user = dc.Users.Where(x => x.EmailId == email).FirstOrDefault();
                List<Course> courses = dc.Courses.ToList();
                if (user == null)
                {
                    return View(courses);
                }
                if (user.isLecturer) status = true;
                ViewBag.Status = status;
                return View(courses);
            }
        }

        public ActionResult MyCourses()
        {
            bool status = false;
            string email = User.Identity.Name;
            List<Course> courses = new List<Course>();
            using (var dc = new MyDatabaseEntities())
            {
                var user = dc.Users.Where(x => x.EmailId == email).FirstOrDefault();
                if (user.isLecturer)
                {
                    status = true;
                    courses = dc.Courses.Where(x => x.LecturerId == user.UserId).ToList();
                }
                else
                {
                    var subscriber_id = dc.SubscribedCourses.Where(x => x.UserId == user.UserId).ToList();
                    if (subscriber_id != null)
                    {
                        foreach (var item in subscriber_id)
                        {
                            courses.Add(dc.Courses.Where(x => x.CourseId == item.CoursesId).FirstOrDefault());
                        }
                    }
                }
                ViewBag.Status = status;
                return View(courses);
            }
        }

        [HttpGet]
        public ActionResult CreateCourse()
        {
            string email = User.Identity.Name;
            using (var dc = new MyDatabaseEntities())
            {
                var user = dc.Users.Where(x => x.EmailId == email).FirstOrDefault();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourse(Course course)
        {
            string imgPath = "/Content/noimage.png";
            if (course.ImageObj != null && course.ImageObj.ContentLength > 0)
            {
                string filePath = Path.Combine(Server.MapPath("/Content"), Path.GetFileName(course.ImageObj.FileName));
                imgPath = "/Content/" + Path.GetFileName(course.ImageObj.FileName);
                course.ImageObj.SaveAs(filePath);
            }
            course.Image = imgPath;
            string email = User.Identity.Name;
            using (var dc = new MyDatabaseEntities())
            {
                var user = dc.Users.Where(x => x.EmailId == email).FirstOrDefault();
                course.LecturerId = user.UserId;
                CreatedCourse created_course = new CreatedCourse()
                {
                    UserId = user.UserId,
                    CoursesId = course.CourseId,
                    CourseName = course.Name,
                    Date = DateTime.Now
                };
                dc.CreatedCourses.Add(created_course);
                dc.Courses.Add(course);
                try
                {
                    dc.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        string message = "Object: " + validationError.Entry.Entity.ToString();

                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            message = message + err.PropertyName + err.ErrorMessage + "";
                        }
                    }
                }
                return RedirectToAction("MyCourses");
            }
        }

        [HttpGet]
        public ActionResult ViewCourse(int id)
        {
            string status = "";
            bool subscribe = true;
            bool course_lecturer = false;
            string email = User.Identity.Name;
            string lect_name = "";
            using (var dc = new MyDatabaseEntities())
            {
                var course = dc.Courses.Where(m => m.CourseId == id).FirstOrDefault();
                var user = dc.Users.Where(m => m.EmailId == email).FirstOrDefault();
                var sub_course = dc.SubscribedCourses.Where(m => m.CoursesId == id && m.UserId == user.UserId).FirstOrDefault();
                int lecturer_id = dc.Courses.Where(m => m.CourseId == id).First().LecturerId;
                lect_name = dc.Users.Where(m => m.UserId == lecturer_id).FirstOrDefault().FullName;
                if (sub_course != null) subscribe = false;
                if (user.isLecturer)
                {
                    status = "lecturer";
                    if (user.UserId == lecturer_id) course_lecturer = true;
                }
                else
                {
                    status = "student";
                }
                ViewBag.Lecturer_Name = lect_name;
                ViewBag.Status = status;
                ViewBag.CourseLecturer = course_lecturer;
                ViewBag.Subscribe = subscribe;
                return View(course);
            }
        }

        [HttpPost]
        public ActionResult ViewCourse(Course course)
        {
            string email = User.Identity.Name;
            using (var dc = new MyDatabaseEntities())
            {
                var user = dc.Users.Where(m => m.EmailId == email).FirstOrDefault();
                var subscriber = dc.SubscribedCourses.Where(m => m.CoursesId == course.CourseId && m.UserId == user.UserId).FirstOrDefault();
                var lector = dc.Users.Where(m => m.UserId == course.LecturerId).FirstOrDefault();
                if (subscriber != null)
                {
                    dc.SubscribedCourses.Remove(subscriber);
                }
                else
                {
                    SubscribedCours sub_course = new SubscribedCours()
                    {
                        UserId = user.UserId,
                        CoursesId = course.CourseId,
                        CourseName = course.Name,
                        Date = DateTime.Now
                    };
                    dc.SubscribedCourses.Add(sub_course);
                }
                dc.SaveChanges();
                return RedirectToAction("ViewCourse", "Course", new { id = course.CourseId });
            }
        }

        [NonAction]
        public void SendEmailToLector(string emailId, string emailFor, string CourseName, string UserName)
        {
            var fromEmail = new MailAddress("sigmaautoresponse@gmail.com", "SigmaProject");
            var toEmail = new MailAddress(emailId);
            var fromEmailPassword = "Qwerty322q";

            string subject = "";
            string body = "";
            if (emailFor == "subscribe")
            {
                subject = "New user in your course!";
                body = "We are excited to tell you that student " + UserName + "has joined your course " + CourseName + ". Good luck with new student!";
            }
            else if (emailFor == "unsubscribe")
            {
                subject = "User left the course";
                body = "Hi, user" + UserName + "left your course " + CourseName + ".";
            }
            else if (emailFor == "course passed")
            {
                subject = "User passed the course";
                body = "Hi, user" + UserName + "passed your course " + CourseName + ".";
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

        [HttpGet]
        public ActionResult ViewLesson(int lesson_id, int course_id)
        {
            string email = User.Identity.Name;
            string status = "";
            using (var dc = new MyDatabaseEntities())
            {
                int user_id = dc.Users.Where(m => m.EmailId == email).FirstOrDefault().UserId;
                var lesson = dc.Topics.Where(m => m.TopicId == lesson_id).FirstOrDefault();
                int lecturer = dc.Courses.Where(m => m.CourseId == course_id).FirstOrDefault().LecturerId;
                var subscriber = dc.SubscribedCourses.Where(m => m.CoursesId == course_id && m.UserId == user_id).FirstOrDefault();
                if (subscriber != null)
                {
                    status = "subscriber";
                }
                if (user_id == lecturer)
                {
                    status = "course_lecturer";
                }
                ViewBag.Status = status;
                ViewBag.CourseId = course_id;
                return View(lesson);
            }
        }

        [HttpGet]
        public ActionResult DeleteCourse(int course_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var course = dc.Courses.Where(m => m.CourseId == course_id).FirstOrDefault();
                return View(course);
            }
        }

        [HttpPost]
        public ActionResult DeleteCourse(Course course)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var del_course = dc.Courses.Where(m => m.CourseId == course.CourseId).FirstOrDefault();
                dc.Courses.Remove(del_course);
                dc.SaveChanges();
                return RedirectToAction("MyCourses");
            }
        }
    }
}