using FlexTasker.Database;
using FlexTasker.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System;

//using (ApplicationContext db = new ApplicationContext())
//{
//	// создаем два объекта User
//	User user1 = new User { Name = "Tom", Password = "123" };
//	User user2 = new User { Name = "Alice", Password = "123" };
//	// добавляем их в бд
//	db.users.AddRange(user1, user2);
//    TodoList list1 = new TodoList { Name = "Goida", UserId = 1 };
//	TodoList list2 = new TodoList { Name = "Goida", UserId = 2 };
//	TodoList list3 = new TodoList { Name = "Kekeke", UserId = 1 };
//    db.todoLists.AddRange(list1, list2, list3);
//    TodoItem item1 = new TodoItem { Name = "Stick finger", Description = "Stick finger", IsStarred = //false, listId = 3 };
//    db.todoItems.AddRange(item1);
//	db.SaveChanges();
//}
// получение данных
using (ApplicationContext db = new ApplicationContext())
{
	// получаем объекты из бд и выводим на консоль
	var users = db.users.ToList();
	Console.WriteLine("Users list:");
	foreach (User u in users)
	{
		Console.WriteLine($"{u.Name}");
	}
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationContext>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDeveloperExceptionPage();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.MapControllers();

app.Run();
