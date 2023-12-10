using CourseManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CourseManagement.Controllers
{
    public class LecturerController : Controller
    {
        [HttpGet]
        public ActionResult AddLesson(int id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var lesson = new Topic();
                lesson.CourseId = id;
                List<Topic> lessons = dc.Topics.Where(m => m.CourseId == id).ToList();
                if (lessons.Count == 0)
                {
                    lesson.TopicNumber = 1;
                }
                else
                {
                    lesson.TopicNumber = lessons.Count + 1;
                }
                return View(lesson);
            }
        }

        [HttpPost]
        public ActionResult AddLesson(Topic lesson)
        {
            int course_id;
            using (var dc = new MyDatabaseEntities())
            {
                course_id = lesson.CourseId;
                dc.Topics.Add(lesson);
                dc.SaveChanges();
                return RedirectToAction("ViewCourse", "Course", new { id = course_id });
            }
        }

        public ActionResult ShowLessons(int id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var lessons = dc.Topics.Where(m => m.CourseId == id).ToList();
                return PartialView(lessons);
            }
        }

        [HttpGet]
        public ActionResult AddMaterial(int id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var material = dc.Materials.Where(m => m.TopicId == id).FirstOrDefault();
                if (material != null)
                {
                    return View(material);
                }
                else
                {
                    Material file = new Material()
                    {
                        TopicId = id
                    };
                    return View(file);
                }
            }
        }

        [HttpPost]
        public ActionResult AddMaterial(Material material)
        {
            int LessonId = 0;
            int CourseId = 0;
            using (var dc = new MyDatabaseEntities())
            {
                foreach (var item in material.MaterialsObj)
                {
                    if (item.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(item.FileName);
                        var path = Path.Combine(Server.MapPath("/Content/Materials"), fileName);
                        item.SaveAs(path);
                        LessonId = dc.Topics.Where(m => m.TopicId == material.TopicId).First().TopicId;
                        CourseId = dc.Topics.Where(m => m.TopicId == material.TopicId).First().CourseId;
                        Material file = new Material()
                        {
                            Path = path,
                            TopicId = LessonId,
                            Name = fileName
                        };
                        dc.Materials.Add(file);
                    }
                }
                dc.SaveChanges();
                return RedirectToAction("ViewLesson", "Course", new { lesson_id = LessonId, course_id = CourseId });
            }
        }

        [HttpGet]
        public ActionResult DeleteMaterial(int id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var material = dc.Materials.Where(m => m.TopicId == id).ToList();
                return View(material);
            }
        }

        [HttpPost]
        public ActionResult DeleteMaterial(List<Material> files)
        {
            using (var dc = new MyDatabaseEntities())
            {
                int topic_id = files.FirstOrDefault().TopicId;
                int courses_id = dc.Topics.Where(m => m.TopicId == topic_id).FirstOrDefault().CourseId;
                foreach (var item in files)
                {
                    var material = dc.Materials.Where(m => m.MaterialId == item.MaterialId).FirstOrDefault();
                    if (System.IO.File.Exists(material.Path))
                    {
                        System.IO.File.Delete(material.Path);
                    }
                    dc.Materials.Remove(material);
                }
                dc.SaveChanges();
                return RedirectToAction("ViewLesson", "Course", new { lesson_id = topic_id, course_id = courses_id });
            }
        }

        public FileResult Download(string path, string filename)
        {
            var doc = new byte[0];
            doc = System.IO.File.ReadAllBytes(path);
            return File(doc, "multipart/form-data", filename);
        }

        public ActionResult ShowMaterials(int topic_id, int course_id)
        {
            bool status = false;
            string email = User.Identity.Name;
            using (var dc = new MyDatabaseEntities())
            {
                var materials = dc.Materials.Where(m => m.TopicId == topic_id).ToList();
                int courses_id = dc.Topics.Where(m => m.TopicId == topic_id).First().CourseId;
                var course = dc.Courses.Where(m => m.CourseId == courses_id).FirstOrDefault();
                var user = dc.Users.Where(m => m.EmailId == email).FirstOrDefault();
                if (course.CourseId == course_id && course.LecturerId == user.UserId)
                {
                    status = true;
                }
                ViewBag.Status = status;
                ViewBag.TopicId = topic_id;
                return PartialView(materials);
            }
        }

        [HttpGet]
        public ActionResult AddQuiz(int id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var quiz = dc.Quizs.Where(m => m.TopicId == id).FirstOrDefault();
                if (quiz != null)
                {
                    return View(quiz);
                }
                else
                {
                    Quiz test = new Quiz()
                    {
                        TopicId = id
                    };
                    return View(test);
                }
            }
        }

        [HttpPost]
        public ActionResult AddQuiz(Quiz quiz)
        {
            using (var dc = new MyDatabaseEntities())
            {
                dc.Quizs.Add(quiz);
                dc.SaveChanges();
                return RedirectToAction("AddQuestions", new { quiz_id = quiz.QuizId });
            }
        }

        [HttpGet]
        public ActionResult AddQuestions(int quiz_id, int? question_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var quest = dc.Questions.Where(m => m.QuizId == quiz_id && m.QuestionId == question_id).FirstOrDefault();
                if (quest == null)
                {
                    Question question = new Question()
                    {
                        QuizId = quiz_id,
                        Question1 = " "
                    };
                    Answer answer = new Answer()
                    {
                        Question_Id = question.QuestionId,
                        Label = " ",
                        IsCorrect = true
                    };
                    Answer choice = new Answer()
                    {
                        Question_Id = question.QuestionId,
                        Label = " ",
                        IsCorrect = false
                    };
                    dc.Answers.Add(choice);
                    dc.Answers.Add(answer);
                    dc.Questions.Add(question);
                    dc.SaveChanges();
                    ModelState.Clear();
                    return View(question);
                }
                else
                {
                    ModelState.Clear();
                    return View(quest);
                }
            }
        }

        [HttpPost]
        public ActionResult AddQuestions(string quest_id, string qz_id, string action, string[] allNames)
        {
            using (var dc = new MyDatabaseEntities())
            {
                int question_idd = Convert.ToInt32(quest_id);
                int quiz_idd = Convert.ToInt32(qz_id);
                var quest = dc.Questions.Where(m => m.QuestionId == question_idd).FirstOrDefault();
                quest.Question1 = allNames[0];
                if (action == "AddCorrectOption")
                {
                    var correct_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && m.IsCorrect).ToList();
                    var wrong_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && !m.IsCorrect).ToList();
                    if (correct_answer_list.Count != 0)
                    {
                        for (int i = 0; i < correct_answer_list.Count; i++)
                        {
                            correct_answer_list[i].Label = allNames[i + 1];
                        }
                        if (wrong_answer_list != null)
                        {
                            for (int i = correct_answer_list.Count + 1, j = 0; i < allNames.Length; i++, j++)
                            {
                                wrong_answer_list[j].Label = allNames[i];
                            }
                        }
                        Answer answer = new Answer()
                        {
                            Question_Id = question_idd,
                            Label = " ",
                            IsCorrect = true
                        };
                        dc.Answers.Add(answer);
                    }
                    else
                    {
                        if (wrong_answer_list != null)
                        {
                            for (int i = correct_answer_list.Count + 1, j = 0; i < allNames.Length; i++, j++)
                            {
                                wrong_answer_list[j].Label = allNames[i];
                            }
                        }
                        Answer answer = new Answer()
                        {
                            Question_Id = question_idd,
                            Label = " ",
                            IsCorrect = true
                        };
                        dc.Answers.Add(answer);
                    }
                    dc.SaveChanges();
                }
                else if (action == "AddOption")
                {
                    var wrong_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && !m.IsCorrect).ToList();
                    var correct_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && m.IsCorrect).ToList();
                    if (wrong_answer_list.Count != 0)
                    {
                        if (correct_answer_list != null)
                        {
                            for (int i = 0; i < correct_answer_list.Count; i++)
                            {
                                correct_answer_list[i].Label = allNames[i + 1];
                            }
                        }
                        for (int i = correct_answer_list.Count + 1, j = 0; i < allNames.Length; i++, j++)
                        {
                            wrong_answer_list[j].Label = allNames[i];
                        }
                        Answer choice = new Answer()
                        {
                            Question_Id = question_idd,
                            Label = " ",
                            IsCorrect = false
                        };
                        dc.Answers.Add(choice);
                    }
                    else
                    {
                        if (correct_answer_list != null)
                        {
                            for (int i = 0; i < correct_answer_list.Count; i++)
                            {
                                correct_answer_list[i].Label = allNames[i + 1];
                            }
                        }
                        Answer choice = new Answer()
                        {
                            Question_Id = question_idd,
                            Label = " ",
                            IsCorrect = false
                        };
                        dc.Answers.Add(choice);
                    }
                    dc.SaveChanges();
                }
                else if (action == "SaveAndAdd")
                {
                    var wrong_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && !m.IsCorrect).ToList();
                    var correct_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && m.IsCorrect).ToList();
                    for (int i = 0; i < correct_answer_list.Count; i++)
                    {
                        correct_answer_list[i].Label = allNames[i + 1];
                    }
                    for (int i = correct_answer_list.Count + 1, j = 0; i < allNames.Length; i++, j++)
                    {
                        wrong_answer_list[j].Label = allNames[i];
                    }
                    dc.SaveChanges();
                    return Json(new { redirectToUrl = Url.Action("AddQuestions", "Lecturer", new { quiz_id = quiz_idd }) });
                }
                else if (action == "SaveAll")
                {
                    var wrong_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && !m.IsCorrect).ToList();
                    var correct_answer_list = dc.Answers.Where(m => m.Question_Id == question_idd && m.IsCorrect).ToList();
                    for (int i = 0; i < correct_answer_list.Count; i++)
                    {
                        correct_answer_list[i].Label = allNames[i + 1];
                    }
                    for (int i = correct_answer_list.Count + 1, j = 0; i < allNames.Length; i++, j++)
                    {
                        wrong_answer_list[j].Label = allNames[i];
                    }
                    dc.SaveChanges();
                    int lesson_id = dc.Quizs.Where(m => m.QuizId == quiz_idd).FirstOrDefault().TopicId;
                    int course_id = dc.Topics.Where(m => m.TopicId == lesson_id).FirstOrDefault().CourseId;
                    return Json(new { redirectToUrl = Url.Action("ViewLesson", "Course", new { lesson_id, course_id }) });
                }
                return Json(new { redirectToUrl = Url.Action("AddQuestions", "Lecturer", new { quiz_id = quiz_idd, question_id = question_idd }) });
            }
        }

        [HttpGet]
        public ActionResult DeleteQuestion(int question_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var question = dc.Questions.Where(m => m.QuestionId == question_id).FirstOrDefault();
                return View(question);
            }
        }

        [HttpPost]
        public ActionResult DeleteQuestion(Question question)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var del_question = dc.Questions.Where(m => m.QuestionId == question.QuestionId).FirstOrDefault();
                dc.Questions.Remove(del_question);
                dc.SaveChanges();
                return RedirectToAction("QuestionsList", "Quiz", new { quiz_id = question.QuizId });
            }
        }

        public ActionResult ShowQuiz(int topic_id, int course_id)
        {
            bool status = false;
            string email = User.Identity.Name;
            int count = 0;
            using (var dc = new MyDatabaseEntities())
            {
                var quiz = dc.Quizs.Where(m => m.TopicId == topic_id).FirstOrDefault();
                if(quiz != null) count = quiz.Questions.Count;
                int courses_id = dc.Topics.Where(m => m.TopicId == topic_id).First().CourseId;
                var course = dc.Courses.Where(m => m.CourseId == courses_id).FirstOrDefault();
                var user = dc.Users.Where(m => m.EmailId == email).FirstOrDefault();
                if (course.CourseId == course_id && course.LecturerId == user.UserId)
                {
                    status = true;
                }
                ViewBag.QuestionsCount = count;
                ViewBag.Status = status;
                ViewBag.TopicId = topic_id;
                return PartialView(quiz);
            }
        }

        public ActionResult CheckProgress()
        {
            return View();
        }
        public ActionResult ViewSubscribers(int course_id)
        {
            using (var dc = new MyDatabaseEntities())
            {
                var course = dc.SubscribedCourses.Where(m => m.CoursesId == course_id).ToList();
                string course_name = dc.Courses.Where(m => m.CourseId == course_id).FirstOrDefault().Name;
                List<User> subscribers = new List<User>();
                foreach (var user in course)
                {
                    var item = dc.Users.Where(m => m.UserId == user.UserId).FirstOrDefault();
                    subscribers.Add(item);
                }
                ViewBag.CourseName = course_name;
                return View(subscribers);
            }
        }
    }
}