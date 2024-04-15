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
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

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