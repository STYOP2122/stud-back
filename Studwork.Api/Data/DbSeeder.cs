using Microsoft.EntityFrameworkCore;
using Studwork.Api.Entities;

namespace Studwork.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var admin = new User
        {
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Name = "Администратор",
            Role = UserRole.Admin
        };

        var customer = new User
        {
            Email = "customer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Name = "Анна Заказчик",
            Role = UserRole.Customer,
            City = "Москва"
        };

        var executor1 = new User
        {
            Email = "executor@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Name = "Иван Исполнитель",
            Role = UserRole.Executor,
            Bio = "Пишу курсовые и дипломы по экономике и менеджменту",
            Skills = "Экономика, Менеджмент, Финансы",
            City = "Санкт-Петербург",
            IsPro = true,
            Rating = 4.8,
            CompletedOrders = 42
        };

        var executor2 = new User
        {
            Email = "writer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Name = "Мария Автор",
            Role = UserRole.Executor,
            Bio = "Специализация: гуманитарные науки, рефераты, эссе",
            Skills = "История, Философия, Литература",
            City = "Казань",
            Rating = 4.5,
            CompletedOrders = 28
        };

        db.Users.AddRange(admin, customer, executor1, executor2);
        await db.SaveChangesAsync();

        var orders = new[]
        {
            new Order
            {
                Title = "Курсовая по микроэкономике",
                Description = "Нужна курсовая работа на 35-40 страниц. Тема: рыночные структуры. Требуется список литературы из 15+ источников, оформление по ГОСТ.",
                WorkType = WorkType.Coursework,
                Subject = "Экономика",
                Budget = 5000,
                Deadline = DateTime.UtcNow.AddDays(14),
                CustomerId = customer.Id
            },
            new Order
            {
                Title = "Реферат по истории России",
                Description = "Реферат на 15 страниц по теме реформ Петра I. Нужны ссылки на источники.",
                WorkType = WorkType.Essay,
                Subject = "История",
                Budget = 1500,
                Deadline = DateTime.UtcNow.AddDays(7),
                CustomerId = customer.Id
            },
            new Order
            {
                Title = "Дипломная работа по программированию",
                Description = "Разработка веб-приложения на React + .NET. Нужна теоретическая часть и практическая реализация.",
                WorkType = WorkType.Diploma,
                Subject = "IT",
                Budget = 15000,
                Deadline = DateTime.UtcNow.AddDays(30),
                CustomerId = customer.Id
            },
            new Order
            {
                Title = "Приватный заказ: эссе по философии",
                Description = "Только для Марии. Эссе на 10 страниц по Канту.",
                WorkType = WorkType.Essay,
                Subject = "Философия",
                Budget = 3000,
                Deadline = DateTime.UtcNow.AddDays(10),
                CustomerId = customer.Id,
                IsPrivate = true,
                InvitedExecutorId = executor2.Id
            }
        };

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
    }
}
