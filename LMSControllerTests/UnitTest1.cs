using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Cryptography;
using NuGet.ContentModel;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        // CommonController tests

        [Fact]
        public void TestGetDepartmentsAndUsers()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeSmallDB());

            var allDepts = ctrl.GetDepartments() as JsonResult;

            dynamic x = allDepts.Value;

            Assert.Equal( 1, x.Length );
            Assert.Equal( "CS", x[0].subject );

            x = (ctrl.GetUser("0000002") as JsonResult).Value;
            Assert.Equal("prof1", x.fname);

            x = (ctrl.GetUser("0000003") as JsonResult).Value;
            Assert.Equal("stud2", x.fname);
        }

        [Fact]
        public void TestCatalog()
        {
            CommonController ctrl = new CommonController(MakeSmallDB());

            dynamic x = (ctrl.GetCatalog() as JsonResult).Value;
            Assert.Equal("CS", x[0].subject);
            Assert.Equal("KSoC", x[0].dname);
            
            Assert.Equal(1, x[0].courses.Length);
            Assert.Equal(3500, x[0].courses[0].number);
            Assert.Equal("Software Practices", x[0].courses[0].cname);
        }

        [Fact]
        public void TestClassOfferings()
        {
            CommonController ctrl = new CommonController(MakeSmallDB());

            dynamic x = (ctrl.GetClassOfferings("CS", 3500) as JsonResult).Value;
            Assert.Equal("Fall", x[0].season);
            Assert.Equal(2024, x[0].year);
            Assert.Equal("WEB", x[0].location);
            Assert.Equal("prof1", x[0].fname);
        }

        [Fact]
        public void TestAssignmentContents()
        {
            CommonController ctrl = new CommonController(MakeSmallDB());

            string x =
                (ctrl.GetAssignmentContents("CS", 3500, "Fall", 2024, "Assignments", "assign1") as ContentResult).Content;
            
            Assert.Equal("testcontents", x);
        }

        [Fact]
        public void TestSubmissionText()
        {
            CommonController ctrl = new CommonController(MakeSmallDB());
            
            string x = (ctrl.GetSubmissionText("CS", 3500, "Fall", 2024, "Assignments", "assign1", "0000003") as ContentResult).Content;
            Assert.Equal("testsubmission", x);
        }
        
        
        // AdministrotorController tests

        [Fact]
        public void TestCreateDepartment()
        {
            var db = MakeSmallDB();
            AdministratorController ctrl = new AdministratorController(db);
            Assert.Equal(1, db.Departments.ToArray().Length);

            dynamic x = (ctrl.CreateDepartment("MATH", "College of Science") as JsonResult).Value;
            Assert.True(x.success);
            Assert.Equal(2, db.Departments.ToArray().Length);
            
            Assert.Equal("MATH", db.Departments.ToArray()[1].Subject);
            Assert.Equal("College of Science", db.Departments.ToArray()[1].Name);
            
            x = (ctrl.CreateDepartment("MATH", "College of Science") as JsonResult).Value;
            Assert.False(x.success);
            Assert.Equal(2, db.Departments.ToArray().Length);
        }

        [Fact]
        public void TestGetCourses()
        {
            AdministratorController ctrl = new AdministratorController(MakeSmallDB());

            dynamic x = (ctrl.GetCourses("CS") as JsonResult).Value;

            Assert.Equal(3500, x[0].number);
            Assert.Equal("Software Practices", x[0].name);
        }

        [Fact]
        public void TestGetProfessors()
        {
            AdministratorController ctrl = new AdministratorController(MakeSmallDB());

            dynamic x = (ctrl.GetProfessors("CS") as JsonResult).Value;
            Assert.Equal("prof1", x[0].fname);
        }

        [Fact]
        public void TestCreateCourse()
        {
            var db = MakeSmallDB();
            AdministratorController ctrl = new AdministratorController(db);
            Assert.Equal(1, db.Courses.ToArray().Length);

            dynamic x = (ctrl.CreateCourse("CS", 2420, "Algorithms and Data Structures") as JsonResult).Value;
            Assert.True(x.success);
            Assert.Equal(2, db.Courses.ToArray().Length);
            
            Assert.Equal(2420, db.Courses.ToArray()[1].Num);
            Assert.Equal("Algorithms and Data Structures", db.Courses.ToArray()[1].Name);
            
            x = (ctrl.CreateCourse("CS", 2420, "Algorithms and Data Structures") as JsonResult).Value;
            Assert.False(x.success);
            Assert.Equal(2, db.Courses.ToArray().Length);
        }
        
        [Fact]
        public void TestCreateClass()
        {
            var db = MakeSmallDB();
            AdministratorController ctrl = new AdministratorController(db);
            Assert.Equal(1, db.Classes.ToArray().Length);

            db.Courses.Add(new Course { Name = "Algorithms and Data Structures", Num = 2420, DId = 1 });

            dynamic x = (ctrl.CreateClass("CS", 2420, "Fall", 2024, new DateTime(2024, 8, 24, 12, 40, 00),
                new DateTime(2024, 8, 24, 1, 40, 00),
                "WEB", "0000002") as JsonResult).Value;
            Assert.False(x.success);
            Assert.Equal(1, db.Courses.ToArray().Length);
            
            x = (ctrl.CreateClass("CS", 3500, "Spring", 2024, new DateTime(2024, 8, 24, 12, 40, 00),
                new DateTime(2024, 8, 24, 1, 40, 00),
                "WEB", "0000002") as JsonResult).Value;
            Assert.True(x.success);
            Assert.Equal(2, db.Courses.ToArray().Length);
            
            Assert.Equal("Spring", db.Classes.ToArray()[1].Season);
            
            x = (ctrl.CreateClass("CS", 3500, "Spring", 2024, new DateTime(2024, 8, 24, 12, 40, 00),
                new DateTime(2024, 8, 24, 1, 40, 00),
                "WEB", "0000002") as JsonResult).Value;
            Assert.False(x.success);
            Assert.Equal(2, db.Courses.ToArray().Length);
        }
        
        
        // StudentController tests

        [Fact]
        public void TestStudentGetMyClasses()
        {
            var db = MakeSmallDB();
            StudentController ctrl = new StudentController(db);

            dynamic x = (ctrl.GetMyClasses("0000003") as JsonResult).Value;
            Assert.Equal(0, x.Length);

            db.Enrolleds.Add(new Enrolled { ClassId = 1, UId = 3, Grade = "B" });
            db.SaveChanges();
            
            x = (ctrl.GetMyClasses("0000003") as JsonResult).Value;
            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
            Assert.Equal(3500, x[0].number);
            Assert.Equal("Fall", x[0].season);
            Assert.Equal(2024, x[0].year);
            Assert.Equal("B", x[0].grade);
        }

        [Fact]
        public void TestGetAssignmentsInClass()
        {
            var db = MakeSmallDB();
            StudentController ctrl = new StudentController(db);

            dynamic x = (ctrl.GetAssignmentsInClass("CS", 3500, "Fall", 2024, "0000003") as JsonResult).Value;
            Assert.Equal(0, x.Length);

            db.Enrolleds.Add(new Enrolled { ClassId = 1, UId = 3, Grade = "B" });
            db.SaveChanges();

            x = (ctrl.GetAssignmentsInClass("CS", 3500, "Fall", 2024, "0000003") as JsonResult).Value;
            Assert.Equal(1, x.Length);
            Assert.Equal("assign1", x[0].aname);
            Assert.Equal("Assignments", x[0].cname);
            Assert.Equal((uint?) 40, x[0].score);

            db.Submissions.Remove((from sub in db.Submissions where sub.UId == 3 select sub).First());
            db.SaveChanges();
            x = (ctrl.GetAssignmentsInClass("CS", 3500, "Fall", 2024, "0000003") as JsonResult).Value;
            Assert.Null(x[0].score);
        }
        
        [Fact]
        public void TestSubmitAssignmentText()
        {
            var db = MakeSmallDB();
            StudentController ctrl = new StudentController(db);
            
            dynamic x = (ctrl.SubmitAssignmentText("CS", 3500, "Fall", 2024, "Assignments", 
                "assign2", "0000003", "testcontents2") as JsonResult).Value;
            Assert.False(x.success);

            DateTime previous = (from sub in db.Submissions where sub.UId == 3 select sub.Time).First();

            x = (ctrl.SubmitAssignmentText("CS", 3500, "Fall", 2024, "Assignments", 
                "assign1", "0000003", "testcontents2") as JsonResult).Value;
            Submission submit = (from sub in db.Submissions where sub.UId == 3 select sub).First();
            Assert.True(x.success);
            Assert.Equal(40, submit.Score);
            Assert.Equal("testcontents2", submit.Contents);
            Assert.NotEqual(previous, submit.Time);

            db.Submissions.Remove((from sub in db.Submissions where sub.UId == 3 select sub).First());
            db.SaveChanges();
            
            x = (ctrl.SubmitAssignmentText("CS", 3500, "Fall", 2024, "Assignments", 
                "assign1", "0000003", "testcontents2") as JsonResult).Value;
            submit = (from sub in db.Submissions where sub.UId == 3 select sub).First();
            Assert.True(x.success);
            Assert.Equal(0, submit.Score);
            Assert.Equal("testcontents2", submit.Contents);
        }

        [Fact]
        public void TestEnroll()
        {
            var db = MakeSmallDB();
            StudentController ctrl = new StudentController(db);

            dynamic x = (ctrl.Enroll("CS", 5000, "Fall", 2024, "0000003") as JsonResult).Value;
            Assert.False(x.success);
            
            x = (ctrl.Enroll("CS", 3500, "Fall", 2024, "0000003") as JsonResult).Value;
            Enrolled enrolled = (from en in db.Enrolleds where en.UId == 3 select en).First();
            Assert.True(x.success);
            Assert.Equal(1, enrolled.ClassId);
            Assert.Equal("--", enrolled.Grade);
            
            x = (ctrl.Enroll("CS", 3500, "Fall", 2024, "0000003") as JsonResult).Value;
            enrolled = (from en in db.Enrolleds where en.UId == 3 select en).First();
            Assert.False(x.success);
            Assert.Equal(1, enrolled.ClassId);
            Assert.Equal("--", enrolled.Grade);
        }

        [Fact]
        public void TestGetGPA()
        {
            var db = MakeSmallDB();
            StudentController ctrl = new StudentController(db);

            dynamic x = (ctrl.GetGPA("0000003") as JsonResult).Value;
            Assert.Equal(0, x.gpa);

            db.Enrolleds.Add(new Enrolled { ClassId = 1, UId = 3, Grade = "--" });
            db.Enrolleds.Add(new Enrolled { ClassId = 2, UId = 3, Grade = "--" });
            db.SaveChanges();
            x = (ctrl.GetGPA("0000003") as JsonResult).Value;
            Assert.Equal(0, x.gpa);

            var enrolleds = (from en in db.Enrolleds where en.ClassId == 1 select en);
            foreach (Enrolled en in enrolleds)
            {
                en.Grade = "B+";
            }
            db.SaveChanges();
            x = (ctrl.GetGPA("0000003") as JsonResult).Value;
            Assert.Equal(3.3, x.gpa);
        }
        
        
        // ProfessorController tests

        [Fact]
        public void TestGetStudentsInClass()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);

            dynamic x = (ctrl.GetStudentsInClass("CS", 3500, "Fall", 2024) as JsonResult).Value;
            Assert.Equal(0, x.Length);
            
            db.Enrolleds.Add(new Enrolled { ClassId = 1, UId = 3, Grade = "B+" });
            db.SaveChanges();
            x = (ctrl.GetStudentsInClass("CS", 3500, "Fall", 2024) as JsonResult).Value;
            Assert.Equal("stud2", x[0].fname);
            Assert.Equal("B+", x[0].grade);
        }

        [Fact]
        public void TestGetAssignmentsInCategory()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);

            dynamic x = (ctrl.GetAssignmentsInCategory("CS", 3500, "Fall", 2024, "Assignments") as JsonResult).Value;
            Assert.Equal("assign1", x[0].aname);
            Assert.Equal(1, x[0].submissions);

            db.Assignments.Add(new Assignment
                { AcId = 1, Name = "assign2", Due = DateTime.Now, Points = 73, Contents = "testcontents2" });
            db.SaveChanges();
            x = (ctrl.GetAssignmentsInCategory("CS", 3500, "Fall", 2024, "Assignments") as JsonResult).Value;
            Assert.Equal("assign2", x[1].aname);
            Assert.Equal(0, x[1].submissions);
        }
        
        [Fact]
        public void TestGetAssignmentCategories()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);

            dynamic x = (ctrl.GetAssignmentCategories("CS", 3500, "Fall", 2024) as JsonResult).Value;
            Assert.Equal("Assignments", x[0].name);
            Assert.Equal(75, x[0].weight);
        }

        [Fact]
        public void TestCreateAssignmentCategory()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);

            dynamic x = (ctrl.CreateAssignmentCategory("CS", 3500, "Fall", 2024, "Assignments", 55) as JsonResult).Value;
            AssignmentCategory category = (from ac in db.AssignmentCategories select ac).First();
            Assert.False(x.success);
            Assert.Equal(75, category.Weight);
            
            x = (ctrl.CreateAssignmentCategory("CS", 3500, "Fall", 2024, "Tests", 25) as JsonResult).Value;
            category = (from ac in db.AssignmentCategories where ac.Name == "Tests" select ac).First();
            Assert.True(x.success);
            Assert.Equal(25, category.Weight);
        }

        [Fact]
        public void TestCreateAssignment()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);
            db.Enrolleds.Add(new Enrolled { ClassId = 1, UId = 3, Grade = "A" });
            db.SaveChanges();

            dynamic x = (ctrl.CreateAssignment("CS", 3500, "Fall", 2024, "Assignments", "assign1", 50, DateTime.Now,
                "testcontents2") as JsonResult).Value;
            Enrolled enrolled = (from en in db.Enrolleds where en.UId == 3 select en).First();
            Assert.False(x.success);
            Assert.Equal("A", enrolled.Grade);
            
            x = (ctrl.CreateAssignment("CS", 3500, "Fall", 2024, "Assignments", "assign2", 50, DateTime.Now,
                "testcontents2") as JsonResult).Value;
            enrolled = (from en in db.Enrolleds where en.UId == 3 select en).First();
            Assignment assignment = (from a in db.Assignments where a.Name == "assign2" select a).First();
            Assert.True(x.success);
            Assert.Equal(50, assignment.Points);
            Assert.Equal("E", enrolled.Grade);
        }

        [Fact]
        public void TestGetSubmissionsToAssignment()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);
            db.Submissions.Add(new Submission
                { AId = 2, UId = 3, Time = DateTime.Now, Contents = "testcontents2", Score = 0 });
            db.SaveChanges();

            dynamic x = (ctrl.GetSubmissionsToAssignment("CS", 3500, "Fall", 2024, "Assignments", "assign1") as JsonResult).Value;
            Assert.Equal(1, x.Length);
            Assert.Equal("stud2", x[0].fname);
            Assert.Equal(3, x[0].uid);
            Assert.Equal(40, x[0].score);
        }

        [Fact]
        public void TestGradeSubmission()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);
            db.Enrolleds.Add(new Enrolled { ClassId = 1, UId = 3, Grade = "--" });
            db.SaveChanges();

            dynamic x = (ctrl.GradeSubmission("CS", 3500, "Fall", 2024, "Assignments", "assign1", "0000003", 38) as JsonResult).Value;
            Submission submission = (from sub in db.Submissions where sub.UId == 3 select sub).First();
            Enrolled enrolled = (from en in db.Enrolleds where en.UId == 3 select en).First();
            Assert.True(x.success);
            Assert.Equal(38, submission.Score);
            Assert.Equal("A-", enrolled.Grade);

            db.AssignmentCategories.Add(new AssignmentCategory { ClassId = 1, Name = "Tests", Weight = 25 });
            db.Assignments.Add(new Assignment
                { AcId = 2, Contents = "testcontent", Due = DateTime.Now, Name = "test1", Points = 100 });
            db.Submissions.Add(new Submission
            { Contents = "testcontent", Score = 0, Time = DateTime.Now, UId = 3, AId = 2 });
            db.SaveChanges();
            
            x = (ctrl.GradeSubmission("CS", 3500, "Fall", 2024, "Tests", "test1", "0000003", 0) as JsonResult).Value;
            submission = (from sub in db.Submissions where sub.UId == 3 && sub.AId == 2 select sub).First();
            enrolled = (from en in db.Enrolleds where en.UId == 3 select en).First();
            Assert.True(x.success);
            Assert.Equal(0, submission.Score);
            Assert.Equal("D+", enrolled.Grade);
        }
        
        [Fact]
        public void TestProfessorGetMyClasses()
        {
            var db = MakeSmallDB();
            ProfessorController ctrl = new ProfessorController(db);

            dynamic x = (ctrl.GetMyClasses("0000002") as JsonResult).Value;
            Assert.Equal("CS", x[0].subject);
            Assert.Equal(3500, x[0].number);
            Assert.Equal(2024, x[0].year);
        }
        
        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeSmallDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase( "LMSControllerTest" )
            .ConfigureWarnings( b => b.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
            .UseApplicationServiceProvider( NewServiceProvider() )
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add( new Department { Name = "KSoC", Subject = "CS" } );
            db.SaveChanges();

            db.Admins.Add(new Admin
            {
                Dob = new DateOnly(), FName = "admin1", LName = "admin1",
                UId = 1,
            });
            
            db.Professors.Add(new Professor
            {
                UId = 2, FName = "prof1", LName = "prof1", Dob = new DateOnly(),
                DId = 1
            });
            db.Students.Add(new Student
            {
                UId = 3, FName = "stud2", LName = "stud2", Dob = new DateOnly(),
                DId = 1
            });

            db.Courses.Add(new Course
            {
                Name = "Software Practices", Num = 3500,
                DId = 1,
            });
            db.SaveChanges();

            db.Classes.Add(new Class
            {
                Season = "Fall", Year = 2024, Loc = "WEB",
                Start = new TimeOnly(12,25,0), End = new TimeOnly(2,45,0),
                CourseId = 1,
                Instructor = 2,
            });
            db.SaveChanges();

            db.AssignmentCategories.Add(new AssignmentCategory
            {
                Name = "Assignments", Weight = 75,
                ClassId = 1,
            });
            db.SaveChanges();

            db.Assignments.Add(new Assignment
            {
                Name = "assign1",
                Points = 42,
                Contents = "testcontents",
                Due = new DateTime(2024, 8, 30, 23, 59, 59),
                AcId = 1,
            });
            db.SaveChanges();

            db.Submissions.Add(new Submission
            {
                UId = 3, AId = 1,
                Time = new DateTime(2024, 8, 30, 23, 59, 59),   
                Score = 40,
                Contents = "testsubmission",
            });
            db.SaveChanges();

            return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}