using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizBuilder.Data.Entities;

namespace QuizBuilder.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<SubjectStudent> SubjectStudents { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<StudentTest> StudentTests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubjectStudent>()
                .HasKey(ss => new { ss.SubjectId, ss.StudentId });

            modelBuilder.Entity<SubjectStudent>()
                .HasOne(ss => ss.Subject)
                .WithMany(s => s.SubjectStudents)
                .HasForeignKey(ss => ss.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubjectStudent>()
                .HasOne(ss => ss.Student)
                .WithMany()
                .HasForeignKey(ss => ss.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Test>()
                .HasOne(t => t.Subject)
                .WithMany(s => s.Tests)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Option>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.StudentTest)
                .WithMany()
                .HasForeignKey(sa => sa.StudentTestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Option)
                .WithMany()
                .HasForeignKey(sa => sa.OptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentTest>()
                .HasOne(st => st.Student)
                .WithMany()
                .HasForeignKey(st => st.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentTest>()
                .HasOne(st => st.Test)
                .WithMany()
                .HasForeignKey(st => st.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentTest>()
                .HasOne(st => st.Question)
                .WithMany()
                .HasForeignKey(st => st.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Test)
                .WithMany()
                .HasForeignKey(tr => tr.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.Student)
                .WithMany()
                .HasForeignKey(tr => tr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}