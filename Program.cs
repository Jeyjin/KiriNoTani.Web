using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using KiriNoTani.Web.Data;
using KiriNoTani.Web.Models; // <--- ВОТ ЭТОЙ СТРОКИ НЕ ХВАТАЛО

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка Cookie Аутентификации
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    // Забиваем БД услугами, если она пуста
    if (!db.Services.Any())
    {
        db.Services.AddRange(
            new Service 
            { 
                Title = "Японская баня (Фуросэ)", 
                Description = "Традиционное прогревание в кедровых купелях под открытым небом с целевыми травами.",
                FullDescription = "Фуросэ — это древняя японская традиция омовения и расслабления. Горячая вода в кедровой купели насыщается сбором горно-алтайских трав и эфирных масел сакуры. Процедура снимает стресс, улучшает кровообращение и дарит глубокое умиротворение под открытым туманным небом.",
                Price = 3500,
                ImageUrl = "/images/furose.jpg"
            },
            new Service 
            { 
                Title = "Аренда домика «Сакура» у воды", 
                Description = "Уютные домики в минималистичном стиле с панорамными окнами и видом на туманную долину.",
                FullDescription = "Двухэтажный павильон в стиле традиционной архитектуры минка. Внутри: минималистичная мебель, панорамное остекление с видами на пруд с карпами кои, приватная терраса, современная система климат-контроля и комната для медитаций с татами.",
                Price = 7000,
                ImageUrl = "/images/house.jpg"
            },
            new Service 
            { 
                Title = "Чайная церемония (Тэмаэ)", 
                Description = "Медитативный процесс дегустации элитных сортов японского чая в уединенном павильоне.",
                FullDescription = "Аутентичный ритуал приготовления чая маття мастером высшей категории. Церемония проходит в отдельном бамбуковом павильоне под звуки журчания воды и легкую музыку на традиционном инструменте кото.",
                Price = 2000,
                ImageUrl = "/images/tea.jpg"
            }
        );
        db.SaveChanges();
    }

    // Забиваем БД начальными отзывами
    if (!db.Reviews.Any())
    {
    db.Reviews.AddRange(
        new Review { Author = "Елена В.", Content = "Невероятная атмосфера! Баня Фуросэ под открытым небом вечером — это лучшее, где я бывала.", Rating = 5, IsApproved = true, IsFeatured = true },
        new Review { Author = "Александр М.", Content = "Очень тихий и гармоничный отдых. Домики идеальные, стиль минка выдержан до мелочей.", Rating = 5, IsApproved = true, IsFeatured = true },
        new Review { Author = "Ирина С.", Content = "Чайная церемония превзошла все ожидания. Атмосфера полного спокойствия и релакса.", Rating = 5, IsApproved = true, IsFeatured = true }
    );
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ОБЯЗАТЕЛЬНО: Авторизация должна идти после UseRouting и перед UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();