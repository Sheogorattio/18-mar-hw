

using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IUserService, UserService>();


var app = builder.Build();
app.UseStaticFiles();

app.MapWhen(
    context => context.Request.Path == "/" && context.Request.Method == "GET",
    builder => builder.Run(async (context) =>
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("/getUser?id={id} - get user by id \n");
        stringBuilder.Append("/deleteUser?id={id} - delete user by id \n");
        stringBuilder.Append("/allUsers - get all users \n");
        stringBuilder.Append("/addUser?name={name}&email={email}&phone={phone} - add user \n");
        stringBuilder.Append("/editUser?id={id}&name={name}&email={email}&phone={phone} - edit user by id \n");
        await context.Response.WriteAsync(stringBuilder.ToString());
        var userService = app.Services.GetRequiredService<IUserService>();

    })
); ;

app.MapWhen(
    context => context.Request.Path == "/getUser" && context.Request.Method == "GET",
    builder => builder.Run(async (context) =>
    {
        var userService = app.Services.GetRequiredService<IUserService>();
        string id = context.Request.Query["id"];
        if(int.TryParse(id, out int userId))
        {
            if(userService.ifExists(userId))
            {
                await context.Response.WriteAsync(userService.GetUser(userId));
            }
            else
            {
                await context.Response.WriteAsync("Not Found");
            }
        }

    })
); ;

app.MapWhen(
    context => context.Request.Path == "/deleteUser" && context.Request.Method == "GET",
    builder => builder.Run(async (context) =>
    {
        var userService = app.Services.GetRequiredService<IUserService>();
        string id = context.Request.Query["id"];
        if (int.TryParse(id, out int userId))
        {
            if(userService.ifExists(userId))
            {
                userService.DeleteUser(userId);
                await context.Response.WriteAsync("Deleted successfully\n");
                await context.Response.WriteAsync(userService.GetAllUsers());
            }
            else
            {
                await context.Response.WriteAsync("Not Found");
            }
        }

    })
); ;

app.MapWhen(
    context => context.Request.Path == "/allUsers" && context.Request.Method == "GET",
    builder => builder.Run(async (context) =>
    {
        var userService = app.Services.GetRequiredService<IUserService>();
        await context.Response.WriteAsync(userService.GetAllUsers());
    })
); ;

app.MapWhen(
    context => context.Request.Path == "/addUser" && context.Request.Method == "GET",
    builder => builder.Run(async (context) =>
    {
        var userService = app.Services.GetRequiredService<IUserService>();
        string id = context.Request.Query["id"];
        string name = context.Request.Query["name"];
        string email= context.Request.Query["email"];
        string phone = context.Request.Query["phone"];
        if (int.TryParse(id, out int userId))
        {
            if (!userService.ifExists(userId))
            {
                userService.AddUser(new User(userId, name, email, phone));
                await context.Response.WriteAsync("Added successfully\n");
                await context.Response.WriteAsync(userService.GetAllUsers());
            }
            else
            {
                await context.Response.WriteAsync("Already exists");
            }
        }

    })
); ;
app.MapWhen(
    context => context.Request.Path == "/editUser" && context.Request.Method == "GET",
    builder => builder.Run(async (context) =>
    {
        var userService = app.Services.GetRequiredService<IUserService>();
        string id = context.Request.Query["id"];
        string name = context.Request.Query["name"];
        string email = context.Request.Query["email"];
        string phone = context.Request.Query["phone"];
        if (int.TryParse(id, out int userId))
        {
            if (userService.ifExists(userId))
            {
                userService.EditUser(new User(userId,name, email, phone));
                await context.Response.WriteAsync("Edited successfully\n");
                await context.Response.WriteAsync(userService.GetAllUsers());
            }
            else
            {
                await context.Response.WriteAsync("Not Found");
            }
        }

    })
); ;

app.Run();

interface IUserService
{
    string GetUser(int userId);
    void AddUser(User user);
    void DeleteUser(int userId);
    string GetAllUsers();
    void EditUser(User user);
    bool ifExists(int userId);
}
class UserService : IUserService
{
    List<User> users=new List<User>();
    public UserService()
    {
        users.Add(new User(0,"name", "email", "phone"));
        users.Add(new User(1,"name", "email", "phone"));
        users.Add(new User(2,"name", "email", "phone"));
        users.Add(new User(3,"name", "email", "phone"));
        users.Add(new User(4,"name", "email", "phone"));
    }
    public void AddUser(User user)
    {
        users.Add(user);
    }

    public void DeleteUser(int userId)
    {
        foreach (var u in users.ToList())
        {
            if (u.Id == userId)
            {
                users.Remove(u);
            }
        }
    }

    public void EditUser(User user)
    {
       foreach(var u in users)
        {
            if(u.Id == user.Id)
            {
                u.Name=user.Name;
                u.Email=user.Email;
                u.Phone=user.Phone;
            }
        }
    }

    public string GetAllUsers()
    {
        return String.Concat(users.Select(o => o.ToString()));
    }

    public string GetUser(int userId)
    {
        return users.FirstOrDefault(e => e.Id == userId).ToString();
    }

    public bool ifExists(int id)
    {
        bool found = false;
        foreach(var u in users)
        {
            if (u.Id == id)
            {
                found = true;
                return true;
            }
        }
        if (!found) return false;
        return found;
    }
}

public class User { 
    public int Id {  get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public User(int id,string name, string email, string phone)
    {
        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
    }
    public override string ToString()
    {
        return "Id:" +Id+" Name: "+Name+" Email: "+Email+" Phone: "+Phone +"\n";
    }
}
/*Создать класс «User». Определить интерфейс и репозиторий по управлению пользователями, 
    с доступными действиями: добавить, удалить, получить конкретного пользователя, редактировать, вывести всех пользователей. 

Создать веб-сайт по управлению этими пользователями на несколько страниц (без базы данных, использовать подходящий жизненный цикл сервиса для полноценной 
работы в одном сеансе).
Весь код можно писать в классе Program.cs или использовать отдельные представления. Обработать возможные ошибочные ситуации, к примеру передачу неверного Id 
(как в формате так и в плане существования). */

