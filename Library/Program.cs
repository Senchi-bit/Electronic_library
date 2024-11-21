using Library.Context;
using Library.Entities;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


// app.Run((async (context) =>
// {
//     var response = context.Response;
//     var request = context.Request;
//
//     else if (path == "/authors/api/author" && request.Method == "GET")
//     {
//         await GetAllAuthors(response);
//     }
//
//     else if (path == "/api/exhibitions" && request.Method == "GET")
//     {
//         await GetAllExhibitions(response);
//     }
//     else if(path == "/authors" && request.Method == "GET")
//     {
//         response.ContentType = "text/html; charset=utf-8";
//         await response.SendFileAsync("html/authors.html");
//     }
// }));

app.MapGet("/api/books",async () =>
{
    MyDbContext db = new MyDbContext();
    return await db.Books.ToListAsync();
});
app.MapGet("/api/books/{id}",(int id) =>
{
    MyDbContext db = new MyDbContext();
    var books = db.Books.ToList();
    var book = books.FirstOrDefault(b => b.Id == id); 
    if (book != null)
        return Results.Ok(book);
    else
    {
        return Results.NotFound();
    }
});
app.MapPost("/api/books", async (Book book) =>
{
    try
    {
        
        MyDbContext db = new MyDbContext();
        var books = await db.Books.ToListAsync();
        var lastBookId = books.Max(x => x.Id);
        if (book != null)
        {
            Book newBook = new Book()
            {
                Id = lastBookId + 1,
                Title = book.Title,
                ReleaseYear = book.ReleaseYear
            };
            await db.Books.AddAsync(newBook);
            await db.SaveChangesAsync();
            return Results.Ok(newBook);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception e)
    {
        return Results.StatusCode(400);
    }
});
app.MapPut("/api/books/nameFilter", (Book book) =>
{
    try
    {
        MyDbContext db = new MyDbContext();
        var books = db.Books.Where(b => b.Title.ToLower().Contains(book.Title.ToLower())).ToList();
        if (books != null)
        {
            return Results.Ok(books);
        }
        else
        {
            throw new Exception("Not found");
        }
    }
    catch (Exception e)
    {
        return Results.StatusCode(404);
    }
});
app.MapPut("/api/books",async (Book bookData) =>
{
    try
    {
        MyDbContext db = new MyDbContext();
        var books =await db.Books.ToListAsync();
        if (bookData != null)
        {
            var book = books.FirstOrDefault(u => u.Id == bookData.Id);
            if (book != null)
            {
                book.ReleaseYear = bookData.ReleaseYear;
                book.Title = bookData.Title;
                db.Books.Update(book);
                await db.SaveChangesAsync();
                return Results.Ok(book);
                
            }
            else
            {
                throw new Exception("Книга не найден");
            }
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        return Results.StatusCode(400);
    }
});
app.MapDelete("/api/books/{id}", async (int id) =>
{
    try
    {
        MyDbContext db = new MyDbContext();
        var books = await db.Books.ToListAsync();
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book != null)
        {
            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return Results.Ok(book);
        }
        else
        {
            throw new Exception("Not found");
        }
    }
    catch (Exception e)
    {
        return Results.StatusCode(404);
    }
});
app.Run();


/*async Task GetAllAuthors(HttpResponse response)
{
    await response.WriteAsJsonAsync(authors);
}

async Task GetAllExhibitions(HttpResponse response)
{
    await response.WriteAsJsonAsync(exhibitions);
}*/
