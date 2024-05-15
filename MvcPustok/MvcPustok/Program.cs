using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcPustok.Data;
using MvcPustok.Models;
using MvcPustok.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
		.AddNewtonsoftJson(options =>
		options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

builder.Services.AddDbContext<AppDbContext>(options => {
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(opt => {
	opt.Password.RequireNonAlphanumeric = false;
	opt.Password.RequireUppercase = false;
	opt.Password.RequiredLength = 8;
	opt.User.RequireUniqueEmail = true;
	opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
	opt.Lockout.MaxFailedAccessAttempts = 5;
}).AddDefaultTokenProviders().AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<LayoutService>();

builder.Services.AddSession(opt => {
	opt.IdleTimeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(opt => {
	opt.Events.OnRedirectToLogin = opt.Events.OnRedirectToAccessDenied = context => {
		if (context.Request.Path.Value.ToLower().StartsWith("/manage")) {
			var uri = new Uri(context.RedirectUri);
			context.Response.Redirect("/manage/auth/login" + uri.Query);
		}
		else {
			var uri = new Uri(context.RedirectUri);
			context.Response.Redirect("/auth/login" + uri.Query);
		}

		return Task.CompletedTask;
	};
});

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
					 name: "areas",
					 pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
				 );

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();