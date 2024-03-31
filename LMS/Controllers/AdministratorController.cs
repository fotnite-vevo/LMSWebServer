﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            try 
            {
                Department d = new Department();
                d.Subject = subject;
                d.Name = name;

                db.Departments.Add(d);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }

        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subject">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from c in db.Courses
                        join d in db.Departments on c.DId equals d.DId
                        where d.Subject == subject
                        select new { number = c.Num, name = c.Name };

            return Json(query.ToArray()) ;
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from p in db.Professors
                        join d in db.Departments on p.DId equals d.DId
                        where d.Subject == subject
                        select new { lname = p.LName, fname = p.FName, uid = p.UId };

            return Json(query.ToArray());
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            try
            {
                Course c = new Course();
                c.CourseId = number;
                c.Name = name;
                c.DId = (from d in db.Departments
                    where d.Name == subject
                    select d.DId).First();

                db.Courses.Add(c);
                db.SaveChanges();
                
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            try
            {
                Class c = new Class();
                c.Season = season;
                c.Year = year;
                c.Start = TimeOnly.FromDateTime(start);
                c.End = TimeOnly.FromDateTime(end);
                c.Loc = location;
                c.CourseId = (from d in db.Departments
                    join co in db.Courses on d.DId equals co.DId
                    where co.Num == number && d.Subject == subject
                    select co.CourseId).First();
                
                int uID;
                Int32.TryParse(instructor, out uID);
                c.Instructor = uID;

                db.Classes.Add(c);
                db.SaveChanges();

                return Json(new { success = true});
            }
            catch (Exception e)
            {
                return Json(new { success = false});
            }
        }


        /*******End code to modify********/

    }
}

