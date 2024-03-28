using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCategory> AssignmentCategories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enrolled> Enrolleds { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=LMS:LMSConnectionString", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.3-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.Property(e => e.UId)
                    .HasColumnType("int(11)")
                    .HasColumnName("uID");

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FName)
                    .HasMaxLength(16)
                    .HasColumnName("fName");

                entity.Property(e => e.LName)
                    .HasMaxLength(16)
                    .HasColumnName("lName");
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.AId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.Name, e.AcId }, "Name")
                    .IsUnique();

                entity.HasIndex(e => e.AcId, "acID");

                entity.Property(e => e.AId)
                    .HasColumnType("int(11)")
                    .HasColumnName("aID");

                entity.Property(e => e.AcId)
                    .HasColumnType("int(11)")
                    .HasColumnName("acID");

                entity.Property(e => e.Contents).HasColumnType("text");

                entity.Property(e => e.Due).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Points).HasColumnType("int(11)");

                entity.HasOne(d => d.Ac)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.AcId)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasKey(e => e.AcId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.Name, e.ClassId }, "Name")
                    .IsUnique();

                entity.HasIndex(e => e.ClassId, "classID");

                entity.Property(e => e.AcId)
                    .HasColumnType("int(11)")
                    .HasColumnName("acID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("classID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Weight).HasColumnType("tinyint(4)");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.ClassId)
                    .HasConstraintName("AssignmentCategories_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => new { e.Season, e.Year, e.CourseId }, "Season")
                    .IsUnique();

                entity.HasIndex(e => e.CourseId, "courseID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("classID");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("courseID");

                entity.Property(e => e.End).HasColumnType("time");

                entity.Property(e => e.Loc).HasMaxLength(100);

                entity.Property(e => e.Season).HasMaxLength(6);

                entity.Property(e => e.Start).HasColumnType("time");

                entity.Property(e => e.Year).HasColumnType("int(11)");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("Classes_ibfk_1");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => new { e.Num, e.DId }, "Num")
                    .IsUnique();

                entity.HasIndex(e => e.DId, "dID");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("courseID");

                entity.Property(e => e.DId)
                    .HasColumnType("int(11)")
                    .HasColumnName("dID");

                entity.Property(e => e.Name).HasMaxLength(16);

                entity.Property(e => e.Num).HasColumnType("int(11)");

                entity.HasOne(d => d.DIdNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.DId)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Subject, "Subject")
                    .IsUnique();

                entity.Property(e => e.DId)
                    .HasColumnType("int(11)")
                    .HasColumnName("dID");

                entity.Property(e => e.Name).HasMaxLength(16);
            });

            modelBuilder.Entity<Enrolled>(entity =>
            {
                entity.HasKey(e => new { e.UId, e.ClassId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("Enrolled");

                entity.HasIndex(e => e.ClassId, "classID");

                entity.Property(e => e.UId)
                    .HasColumnType("int(11)")
                    .HasColumnName("uID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("classID");

                entity.Property(e => e.Grade).HasMaxLength(2);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrolleds)
                    .HasForeignKey(d => d.ClassId)
                    .HasConstraintName("Enrolled_ibfk_2");

                entity.HasOne(d => d.UIdNavigation)
                    .WithMany(p => p.Enrolleds)
                    .HasForeignKey(d => d.UId)
                    .HasConstraintName("Enrolled_ibfk_1");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.DId, "dID");

                entity.Property(e => e.UId)
                    .HasColumnType("int(11)")
                    .HasColumnName("uID");

                entity.Property(e => e.DId)
                    .HasColumnType("int(11)")
                    .HasColumnName("dID");

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FName)
                    .HasMaxLength(16)
                    .HasColumnName("fName");

                entity.Property(e => e.LName)
                    .HasMaxLength(16)
                    .HasColumnName("lName");

                entity.HasOne(d => d.DIdNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.DId)
                    .HasConstraintName("Professors_ibfk_1");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.DId, "dID");

                entity.Property(e => e.UId)
                    .HasColumnType("int(11)")
                    .HasColumnName("uID");

                entity.Property(e => e.DId)
                    .HasColumnType("int(11)")
                    .HasColumnName("dID");

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FName)
                    .HasMaxLength(16)
                    .HasColumnName("fName");

                entity.Property(e => e.LName)
                    .HasMaxLength(16)
                    .HasColumnName("lName");

                entity.HasOne(d => d.DIdNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.DId)
                    .HasConstraintName("Students_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => new { e.UId, e.AId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("Submission");

                entity.HasIndex(e => e.AId, "aID");

                entity.Property(e => e.UId)
                    .HasColumnType("int(11)")
                    .HasColumnName("uID");

                entity.Property(e => e.AId)
                    .HasColumnType("int(11)")
                    .HasColumnName("aID");

                entity.Property(e => e.Contents).HasColumnType("text");

                entity.Property(e => e.Score).HasColumnType("int(11)");

                entity.Property(e => e.Time).HasColumnType("datetime");

                entity.HasOne(d => d.AIdNavigation)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AId)
                    .HasConstraintName("Submission_ibfk_2");

                entity.HasOne(d => d.UIdNavigation)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.UId)
                    .HasConstraintName("Submission_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
