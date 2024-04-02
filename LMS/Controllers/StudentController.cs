using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            try
            {
                int userId;
                Int32.TryParse(uid, out userId);
                var query = from st in db.Students
                            join en in db.Enrolleds on st.UId equals en.UId
                            join d in db.Departments on st.DId equals d.DId
                            join co in db.Courses on d.DId equals co.DId
                            join cl in db.Classes on co.CourseId equals cl.CourseId
                            where st.UId == userId
                            select new
                            {
                                subject = d.Subject,
                                number = co.Num,
                                name = co.Name,
                                season = cl.Season,
                                year = cl.Year,
                                grade = en.Grade
                            };

                return Json(query.ToArray());
            }
            catch (Exception e) {
                return Json(null);
            }

        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {   
            try
            {
                int userId;
                Int32.TryParse(uid, out userId);
                var query = from a in db.Assignments
                            join ac in db.AssignmentCategories on a.AcId equals ac.AcId
                            join sub in db.Submissions on a.AcId equals sub.AId
                            join cl in db.Classes on ac.ClassId equals cl.ClassId
                            join co in db.Courses on cl.CourseId equals co.CourseId
                            join d in db.Departments on co.DId equals d.DId
                            join en in db.Enrolleds on cl.ClassId equals en.ClassId
                            where d.Subject == subject && co.Num == num && cl.Season == season && cl.Year == year && en.UId == userId
                            select new
                            {
                                aname = a.Name,
                                cname = ac.Name,
                                due = a.Due,
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
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            try
            {
                int userId;
                Int32.TryParse(uid, out userId);

                Submission s = new Submission();

                int aId = (from a in db.Assignments
                         join ac in db.AssignmentCategories on a.AcId equals ac.AcId
                         join cl in db.Classes on ac.ClassId equals cl.ClassId
                         join co in db.Courses on cl.CourseId equals co.CourseId
                         join d in db.Departments on co.DId equals d.DId
                         where d.Subject == subject && co.Num == num && cl.Season == season && cl.Year == year && 
                            ac.Name == category && a.Name == asgname
                         select a.AId).First();

                var query = from sub in db.Submissions
                             where sub.AId == aId && sub.UId == userId
                             select sub ;

                if (query.Any())
                {
                    foreach (var q in query)
                    {
                        q.Contents = contents;
                        q.Time = DateTime.Now;
                        q.Score = 0;
                    }
                }
                else
                {
                    s.Contents = contents;
                    s.Time = DateTime.Now;
                    s.Score = 0;
                    s.UId = userId;
                    s.AId = aId;

                    db.Submissions.Add(s);
                }

                db.SaveChanges();

                return Json(new { success = true });

            }
            catch (Exception e)
            {
                return Json(new { success = false });

            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {   
            try
            {
                int userId;
                Int32.TryParse(uid.Substring(1), out userId);

                Enrolled e = new Enrolled();

                e.UId = userId;
                e.ClassId = (from cl in db.Classes
                             join co in db.Courses on cl.CourseId equals co.CourseId
                             join d in db.Departments on co.DId equals d.DId
                             where cl.Season == season && cl.Year == year && d.Subject == subject && co.Num == num
                             select cl.ClassId).First();

                db.Enrolleds.Add(e);
                db.SaveChanges();

                return Json(new { success = true });

            }
            catch (Exception e)
            {
                return Json(new { success = false });

            }
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            try
            {
                int userId;
                Int32.TryParse(uid, out userId);

                // get all the grades from Enrolled table for student
                var query = from e in db.Enrolleds
                            where e.UId == userId
                            select e.Grade;

                int count = query.Count();

                if (count == 0)
                {
                    return Json(new { gpa = 0.0 } );
                }

                // Map each grade to grade points 
                double gpa = 0.0;
                foreach (var grade in query)
                {
                    switch (grade)
                    {
                        case "A":
                            gpa += 4.0;
                            break;
                        case "A-":
                            gpa += 3.7;
                            break;
                        case "B+":
                            gpa += 3.3;
                            break;
                        case "B":
                            gpa += 3.0;
                            break;
                        case "B-":
                            gpa += 2.7;
                            break;
                        case "C+":
                            gpa += 2.3;
                            break;
                        case "C":
                            gpa += 2.0;
                            break;
                        case "C-":
                            gpa += 1.7;
                            break;
                        case "D+":
                            gpa += 1.3;
                            break;
                        case "D":
                            gpa += 1.0;
                            break;
                        case "D-":
                            gpa += 0.7;
                            break;
                        case "E":
                            gpa += 0.0;
                            break;
                    }
                }
                // take average of all grade points
                gpa /= count;

                return Json(new { gpa = gpa } );

            }
            catch (Exception e)
            {
                return Json(null);
            }
        }
                
        /*******End code to modify********/

    }
}

