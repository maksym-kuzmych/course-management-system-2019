using CourseManagement.Models;
using CourseManagement.Models.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CourseManagement.Controllers
{
    public class QuizController : Controller
    {
        [HttpGet]
        public ActionResult CorrectOptions(int question_id)
        {
            bool status = false;
            using (var dc = new MyDatabaseEntities())
            {
                var answers = dc.Answers.Where(m => m.Question_Id == question_id && m.IsCorrect).ToList();
                if (answers.Count == 0)
                {
                    status = true;
                }
                ViewBag.NullQuestion = status;
                ModelState.Clear();
                return PartialView(answers);
            }
        }

        [HttpGet]
        public ActionResult Options(int question_id)
        {
            bool status = false;
            using (var dc = new MyDatabaseEntities())
            {
                var choices = dc.Answers.Where(m => m.Question_Id == question_id && !m.IsCorrect).ToList();
                if (choices.Count == 0)
                {
                    status = true;
                }
                ViewBag.NullQuestion = status;
                ModelState.Clear();
                return PartialView(choices);
            }
        }

        [HttpGet]
        public ActionResult QuestionsList(int quiz_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var questions_list = dc.Questions.Where(m => m.QuizId == quiz_id).ToList();
                ViewBag.QuizId = quiz_id;
                return View(questions_list);
            }
        }

        [HttpGet]
        public ActionResult DeleteQuiz(int quiz_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var quiz = dc.Quizs.Where(m => m.QuizId == quiz_id).FirstOrDefault();
                return View(quiz);
            }
        }

        [HttpPost]
        public ActionResult DeleteQuiz(Quiz quiz)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var del_quiz = dc.Quizs.Where(m => m.QuizId == quiz.QuizId).FirstOrDefault();
                int lesson_id = dc.Topics.Where(m => m.TopicId == quiz.TopicId).FirstOrDefault().TopicId;
                int course_id = dc.Topics.Where(m => m.TopicId == quiz.TopicId).FirstOrDefault().CourseId;
                dc.Quizs.Remove(del_quiz);
                dc.SaveChanges();
                return RedirectToAction("ViewLesson", "Course", new { lesson_id, course_id });
            }
        }

        [HttpGet]
        public ActionResult AttemptQuiz(int quiz_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var quiz = dc.Quizs.Where(m => m.QuizId == quiz_id).FirstOrDefault();
                ViewBag.QuestionsCount = quiz.Questions.Count();
                return View(quiz);
            }
        }

        [HttpPost]
        public ActionResult AttemptQuiz(Quiz quiz)
        {
            return RedirectToAction("TestProcess", new { quiz_id = quiz.QuizId });
        }

        [HttpGet]
        public ActionResult ShowAnswers(int question_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var answers = dc.Answers.Where(m => m.Question_Id == question_id).ToList();
                int answers_count = answers.Where(m => m.IsCorrect && m.Question_Id == question_id).Count();
                RandomizeData(answers);
                ViewBag.AnswersCount = answers_count;
                return PartialView(answers);
            }
        }

        [HttpGet]
        public ActionResult TestProcess(int quiz_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var quiz = dc.Quizs.Where(m => m.QuizId == quiz_id).FirstOrDefault();
                var questions = dc.Questions.Where(m => m.QuizId == quiz_id).ToList();
                string[] duration = quiz.DurationInMinutes.ToString().Split(':');
                int hours = Convert.ToInt32(duration[0]);
                int minutes = Convert.ToInt32(duration[1]);
                int seconds = Convert.ToInt32(duration[2]);
                int time = hours * 3600 + minutes * 60 + seconds;
                ViewBag.Duration = time - 1;
                ViewBag.QuizName = quiz.Name;
                return View(questions);
            }
        }

        [HttpPost]
        public ActionResult SaveResults(int qz_id, string[] radio, string[] check)
        {
            string email = User.Identity.Name;
            List<ReturnedAnswer> radios = new List<ReturnedAnswer>();
            for (int i = 0; i < radio.Length; i++)
            {
                string[] temp = Split_Input(radio[i]);
                string label = "" + temp[0];
                int quest_id = Convert.ToInt32(temp[temp.Length - 1]);
                radios.Add(new ReturnedAnswer { QuestionId = quest_id, Label = label });
            }
            List<ReturnedAnswer> checks = new List<ReturnedAnswer>();
            for (int i = 0; i < check.Length; i++)
            {
                string[] temp = Split_Input(check[i]);
                int quest_id = Convert.ToInt32(temp[temp.Length - 1]);
                for (int j = 0; j < temp.Length - 1; j++)
                {
                    string label = "" + temp[j];
                    checks.Add(new ReturnedAnswer { QuestionId = quest_id, Label = label });
                }
            }
            double count = 0;
            double temp_count = 0;
            using (var dc = new MyDatabaseEntities())
            {
                int user_id = dc.Users.Where(m => m.EmailId == email).FirstOrDefault().UserId;
                for (int i = 0; i < radios.Count; i++)
                {
                    int quest_id = radios[i].QuestionId;
                    var anss = dc.Answers.Where(m => m.Question_Id == quest_id && m.IsCorrect).FirstOrDefault();
                    if (anss.Label.Contains(radios[i].Label))
                    {
                        count++;
                    }
                }
                bool answer_checked = false;
                int question_id = 0;
                int remember_id = 0;
                for (int i = 0; i < checks.Count; i++)
                {
                    question_id = checks[i].QuestionId;
                    if (question_id != remember_id) answer_checked = false;
                    var anss_wrong = dc.Answers.Where(m => m.Question_Id == question_id && !m.IsCorrect).ToList();
                    var anss_correct = dc.Answers.Where(m => m.Question_Id == question_id && m.IsCorrect).ToList();
                    if (!answer_checked)
                    {
                        foreach (var item in anss_correct)
                        {
                            if (checks.Exists(m => m.Label == item.Label)) count += (1.0 / anss_correct.Count);
                        }
                        foreach (var item in anss_wrong)
                        {
                            if (checks.Exists(m => m.Label == item.Label)) temp_count += (1.0 / anss_correct.Count);
                        }
                        remember_id = question_id;
                        answer_checked = true;
                    }
                }
                count -= temp_count;
                count = Math.Round(count, 2);
                int questions_count = dc.Quizs.Where(m => m.QuizId == qz_id).FirstOrDefault().Questions.Count;
                double percent = Math.Round((count * 100) / questions_count, 2);
                //StudentProgress studentProgress = new StudentProgress()
                //{
                //    QuizId = qz_id,
                //    QuizMark = (decimal)count,
                //    StudentId = user_id
                //};
                return Json(new { redirectToUrl = Url.Action("ShowResult", "Quiz", new { qz_id, questions_count, count, percent }) });
            }
        }

        [HttpGet]
        public ActionResult ShowResult(int qz_id, int questions_count, double count, double percent)
        {
            using (var dc = new MyDatabaseEntities())
            {
                int topic_id = dc.Quizs.Where(m => m.QuizId == qz_id).FirstOrDefault().TopicId;
                int course_id = dc.Topics.Where(m => m.TopicId == topic_id).FirstOrDefault().CourseId;
                ViewBag.QuestionsCount = questions_count;
                ViewBag.Mark = count;
                ViewBag.Percent = percent;
                ViewBag.CourseID = course_id;
                return View();
            }
        }

        [NonAction]
        public string[] Split_Input(string str)
        {
            return str.Split('~');
        }

        [NonAction]
        public void RandomizeData(List<Answer> answers)
        {
            Random random = new Random();
            int cnt = answers.Count - 1;
            for (int i = cnt; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                var temp = answers[j];
                answers[j] = answers[i];
                answers[i] = temp;
            }
        }
    }
}