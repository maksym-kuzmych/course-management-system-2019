using CourseManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CourseManagement.Controllers
{
    public class StudentController : Controller
    {
        public ActionResult AttemptQuiz(int quiz_id, int student_id)
        {
            return View();
        }
    }
}