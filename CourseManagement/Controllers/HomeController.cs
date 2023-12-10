using CourseManagement.Models;
using System.Linq;
using System.Web.Mvc;

namespace CourseManagement.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index(User user)
        {
            string name = "";
            string status = "";
            if (user.IsAdmin)
            {
                name = "Admin.";
            }
            else
            {
                if (user.isLecturer)
                {
                    status = "lecturer";
                }
                if (user.isStudent)
                {
                    status = "student";
                }
                name = user.FullName;
            }
            ViewBag.Status = status;
            ViewBag.Name = name;
            return View();
        }

        public ActionResult NavBar()
        {
            string email = User.Identity.Name;
            using (var dc = new MyDatabaseEntities())
            {
                var user = dc.Users.Where(m => m.EmailId == email).FirstOrDefault();
                return PartialView(user);
            }
        }

        [HttpGet]
        public ActionResult FindCourse(string course_name)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var course = dc.Courses.Where(m => m.Name.ToLower() == course_name.ToLower()).FirstOrDefault();
                return Json(new { redirectToUrl = Url.Action("ShowResult", "Home", new { course.CourseId }) });
            }
        }

        [HttpGet]
        public ActionResult ShowResult(int course_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var course = dc.Courses.Where(m => m.CourseId == course_id).FirstOrDefault();
                return View(course);
            }
        }

        [HttpGet]
        public ActionResult AboutUs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ContactUs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SiteMap()
        {
            return View();
        }
    }
}