using ITIExaminationSystem.Interfaces;
using ITIExaminationSystem.Services;

namespace ITIExaminationSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── MVC ───────────────────────────────────────────────────────────
            builder.Services.AddControllersWithViews();

            // ─── Session ──────────────────────────────────────────────────────
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); // 1 hour session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ─── Application Services (Clean Architecture DI) ─────────────────
            // Register interfaces → implementations
            // Scoped: one instance per HTTP request (correct for DB connections)
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IExamService, ExamService>();

            // ─── Build & Configure Pipeline ───────────────────────────────────
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Session must come before UseAuthorization
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            // Default route: redirect to Login
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
