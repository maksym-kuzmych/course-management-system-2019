using CourseManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CourseManagement.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public ActionResult Users()
        {
            using (var dc = new MyDatabaseEntities())
            {
                var list = dc.Users.Where(x => x.EmailId != null && x.IsAdmin == false);
                return View(list.ToList());
            }
        }

        [HttpPost]
        public ActionResult Users(List<User> users)
        {
            bool changes = false;
            using (var dc = new MyDatabaseEntities())
            {
                IEnumerable<User> toChange = dc.Users.Where(x => x != null && !x.IsAdmin).ToList();
                foreach (var item in toChange)
                {
                    var new_val = users.Where(x => x.UserId == item.UserId).FirstOrDefault();
                    if (new_val.isLecturer)
                    {
                        item.isLecturer = new_val.isLecturer;
                        item.isStudent = false;
                    }
                    else
                    {
                        item.isLecturer = new_val.isLecturer;
                        item.isStudent = true;
                    }
                    item.Password = new_val.Password;
                    item.ConfirmPassword = new_val.Password;
                }
                foreach (var item in toChange)
                {
                    dc.Entry(item).State = EntityState.Modified;
                }
                dc.SaveChanges();
                changes = true;
                ViewBag.Changes = changes;
                return View();
            }
        }
    }
}