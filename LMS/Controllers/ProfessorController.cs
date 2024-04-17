using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Exception = System.Exception;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            try
            {
                var query = from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    join c in db.Classes on co.CourseId equals c.CourseId
                    where d.Subject == subject && co.Num == num && c.Season == season && c.Year == year
                    join e in db.Enrolleds on c.ClassId equals e.ClassId
                    join s in db.Students on e.UId equals s.UId
                    select new
                    {
                        fname = s.FName,
                        lname = s.LName,
                        uid = s.UId,
                        dob = s.Dob,
                        grade = e.Grade,
                    };

                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            try
            {
                var query = from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    join c in db.Classes on co.CourseId equals c.CourseId
                    join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                    join a in db.Assignments on ac.AcId equals a.AcId
                    where d.Subject == subject && co.Num == num && c.Season == season && c.Year == year && 
                          (category == null || ac.Name == category)
                    select new
                    {
                        aname = a.Name,
                        cname = ac.Name,
                        due = a.Due,
                        submissions = a.Submissions.Count
                    };

                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            try
            {
                var query = from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    join c in db.Classes on co.CourseId equals c.CourseId
                    join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                    where d.Subject == subject && co.Num == num && c.Season == season && c.Year == year
                    select new
                    {
                        name = ac.Name,
                        weight = ac.Weight,
                    };

                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            try
            {
                var cl = (from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    join c in db.Classes on co.CourseId equals c.CourseId
                    where d.Subject == subject && co.Num == num && c.Season == season && c.Year == year
                    select c).First();

                var query = from ac in db.AssignmentCategories
                    where ac.ClassId == cl.ClassId && ac.Name == category
                    select ac;
                if (query.Any())
                { 
                    return Json(new { success = false });
                }

                AssignmentCategory cat = new AssignmentCategory();
                cat.ClassId = cl.ClassId;
                cat.Name = category;
                cat.Weight = (sbyte) catweight;
                db.AssignmentCategories.Add(cat);
                db.SaveChanges();
                
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            try
            {
                var cat = (from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    join c in db.Classes on co.CourseId equals c.CourseId
                    join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                    where d.Subject == subject && co.Num == num && c.Season == season && c.Year == year && ac.Name == category
                    select ac).First();

                var query = from a in db.Assignments
                    where a.AcId == cat.AcId && a.Name == asgname
                    select a;
                if (query.Any())
                { 
                    return Json(new { success = false });
                }

                Assignment assign = new Assignment();
                assign.AcId = cat.AcId;
                assign.Name = asgname;
                assign.Points = asgpoints;
                assign.Due = asgdue;
                assign.Contents = asgcontents;
                db.Assignments.Add(assign);
                db.SaveChanges();
                
                // FIXME: excepts out somewhere in UpdateGrades
                UpdateGrades(subject, num, season, year, null);
                
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {

            try
            {
                var query = from d in db.Departments
                            join co in db.Courses on d.DId equals co.DId
                            join c in db.Classes on co.CourseId equals c.CourseId
                            join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                            join assign in db.Assignments on ac.AcId equals assign.AcId
                            join sub in db.Submissions on assign.AId equals sub.AId
                            join st in db.Students on sub.UId equals st.UId
                            where d.Subject == subject && co.Num == num && c.Season == season && 
                                c.Year == year && ac.Name == category && assign.Name == asgname
                            select new
                            {
                                fname = st.FName,
                                lname = st.LName,
                                uid = st.UId,
                                time = sub.Time,
                                score = sub.Score
                            };

                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            try
            {
                int userId;
                Int32.TryParse(uid, out userId);

                var query = from d in db.Departments
                            join co in db.Courses on d.DId equals co.DId
                            join c in db.Classes on co.CourseId equals c.CourseId
                            join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                            join assign in db.Assignments on ac.AcId equals assign.AcId
                            join sub in db.Submissions on assign.AId equals sub.AId
                            where d.Subject == subject && co.Num == num && c.Season == season &&
                                c.Year == year && ac.Name == category && assign.Name == asgname &&
                                sub.UId == userId
                            select sub;

                if (!query.Any()) return Json(new { success = false });

                foreach (var q in query)
                {
                    q.Score = score;
                }

                db.SaveChanges();
                
                UpdateGrades(subject, num, season, year, userId);

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        private void UpdateGrades(string subject, int num, string season, int year, int? uid)
        {
            var students = from d in db.Departments
                join co in db.Courses on d.DId equals co.DId
                join c in db.Classes on co.CourseId equals c.CourseId
                join en in db.Enrolleds on c.ClassId equals en.ClassId
                where uid == null || en.UId == uid
                select en;

            foreach (Enrolled student in students)
            {
                var categories = from c in db.Classes
                    where c.ClassId == student.ClassId
                    join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                    select ac;

                float grade = 0;
                int weights = 0;

                foreach (AssignmentCategory category in categories)
                {
                    int scores = (from a in db.Assignments
                        where category.AcId == a.AcId
                        join sub in db.Submissions on a.AId equals sub.AId
                        where sub.UId == student.UId
                        select sub.Score).Sum();

                    int points = (from a in db.Assignments
                        where category.AcId == a.AcId
                        select a.Points).Sum();

                    float total = (float) scores / (float) points;
                    grade += total * category.Weight;
                    weights += category.Weight;
                }

                grade = 100 * grade / weights;

                if (grade >= 93)
                {
                    student.Grade = "A";
                }
                else if (grade >= 90)
                {
                    student.Grade = "A-";
                }
                else if (grade >= 87)
                {
                    student.Grade = "B+";
                }
                else if (grade >= 83)
                {
                    student.Grade = "B";
                }
                else if (grade >= 80)
                {
                    student.Grade = "B-";
                }
                else if (grade >= 77)
                {
                    student.Grade = "C+";
                }
                else if (grade >= 73)
                {
                    student.Grade = "C";
                }
                else if (grade >= 70)
                {
                    student.Grade = "C-";
                }
                else if (grade >= 67)
                {
                    student.Grade = "D+";
                }
                else if (grade >= 63)
                {
                    student.Grade = "D";
                }
                else if (grade >= 60)
                {
                    student.Grade = "D-";
                }
                else
                {
                    student.Grade = "E";
                }

                db.SaveChanges();
            }
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            try
            {
                string uidNum = uid.Substring(1);
                int userId;
                Int32.TryParse(uidNum, out userId);

                var query = from p in db.Professors
                            join cl in db.Classes on p.UId equals cl.Instructor
                            join co in db.Courses on cl.CourseId equals co.CourseId
                            join d in db.Departments on co.DId equals d.DId
                            where p.UId == userId
                            select new
                            {
                                subject = d.Subject,
                                number = co.Num,
                                name = co.Name,
                                season = cl.Season,
                                year = cl.Year
                            };

                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }


        
        /*******End code to modify********/
    }
}

