using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from d in db.Departments
                        select new
                        {
                            name = d.Name,
                            subject = d.Subject
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            try
            {
                var query = from d in db.Departments
                    select new
                    {
                        subject = d.Subject,
                        dname = d.Name,
                        courses = (from co in db.Courses
                                where d.DId == co.DId
                                    select new
                                    {
                                        number = co.Num,
                                        cname = co.Name,
                                    }).ToArray(),
                    };

                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Falldbcontext"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            try
            {
                var query = from c in db.Classes
                    join p in db.Professors on c.Instructor equals p.UId
                    join co in db.Courses on c.CourseId equals co.CourseId
                    join d in db.Departments on co.DId equals d.DId
                    where d.Subject == subject && co.Num == number
                    select new
                    {
                        season = c.Season,
                        year = c.Year,
                        location = c.Loc,
                        start = c.Start.ToString("hh:mm:ss"),
                        end = c.End.ToString("hh:mm:ss"),
                        fname = p.FName,
                        lname = p.LName,
                    };

                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            try
            {
                var query = from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    join c in db.Classes on co.CourseId equals c.CourseId
                    join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                    join a in db.Assignments on ac.AcId equals a.AcId
                    where d.Subject == subject && co.Num == num && c.Season == season && c.Year == year &&
                          ac.Name == category && a.Name == asgname
                    select a.Contents;

                return Content(query.First());
            }
            catch (Exception e)
            {
                return Content("");
            }
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {            
            try
            {
                int uID;
                Int32.TryParse(uid, out uID);
                
                var query = from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    join c in db.Classes on co.CourseId equals c.CourseId
                    join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                    join a in db.Assignments on ac.AcId equals a.AcId
                    join s in db.Submissions on a.AId equals s.AId
                    where d.Subject == subject && co.Num == num && c.Season == season && c.Year == year &&
                          ac.Name == category && a.Name == asgname && s.UId == uID
                    select s.Contents;

                return Content(query.First());
            }
            catch (Exception e)
            {
                return Content("");
            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {           
            try
            {
                int uID;
                Int32.TryParse(uid, out uID);

                var query1 = from a in db.Admins
                    where a.UId == uID
                    select new
                    {
                        fname = a.FName,
                        lname = a.LName,
                        uid = uID,
                    };
                if (query1.Any())
                {
                    return Json(query1.ToArray());
                }
                
                var query2 = from p in db.Professors
                    join d in db.Departments on p.DId equals d.DId
                    where p.UId == uID
                    select new
                    {
                        fname = p.FName,
                        lname = p.LName,
                        uid = uID,
                        department = d.Name,
                    };
                if (query2.Any())
                {
                    return Json(query2.ToArray());
                }
                
                query2 = from s in db.Students
                    join d in db.Departments on s.DId equals d.DId
                    where s.UId == uID
                    select new
                    {
                        fname = s.FName,
                        lname = s.LName,
                        uid = uID,
                        department = d.Name,
                    };
                if (query2.Any())
                {
                    return Json(query2.ToArray());
                }
                
                return Json(null);
            }
            catch (Exception e)
            {
                return Json(null);
            }
        }


        /*******End code to modify********/
    }
}

