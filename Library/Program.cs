using Library.Context;
using Library.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/error");
    app.UseStatusCodePages(async statusCodeContext =>
    {
        var response = statusCodeContext.HttpContext.Response;
        var path = statusCodeContext.HttpContext.Request.Path;
 
        response.ContentType = "text/plain; charset=UTF-8";
        if (response.StatusCode == 403)
        {
            await response.WriteAsync($"Path: {path}. Access Denied ");
        }
        else if (response.StatusCode == 404)
        {
            await response.WriteAsync($"Resource {path} Not Found");
        }
    });
}
app.UseDefaultFiles();
app.UseStaticFiles();


app.UseHttpsRedirection();

// app.MapSwagger().RequireAuthorization();
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
app.Map("/error", app => app.Run(async context =>
{
    context.Response.StatusCode = 500;
    await context.Response.WriteAsync("Error 500!");
}));
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
});
app.MapPut("/api/books/nameFilter", (Book book) =>
{
    try
    {
        if (book.Title == "")
        {
            throw new Exception("Неверный запрос");
        }
        MyDbContext db = new MyDbContext();
        var books = db.Books.Where(b => b.Title.ToLower().Contains(book.Title.ToLower())).ToList();
        if (books != null)
        {
            foreach (var OneBook in books)
            {
                if (OneBook.Title != "")
                {
                    return Results.Ok(OneBook);
                }
                throw new Exception("Not found");
            }
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

    return Results.StatusCode(404);
});
app.MapPut("/api/books",async (Book bookData) =>
{

    MyDbContext db = new MyDbContext();
    var books =await db.Books.ToListAsync();
    if (bookData != null)
    {
        var book = books.FirstOrDefault(u => u.Id == bookData.Id);
        if (book != null && bookData.Title != "")
        {
            book.ReleaseYear = bookData.ReleaseYear;
            book.Title = bookData.Title;
            db.Books.Update(book);
            await db.SaveChangesAsync();
            return Results.Ok(book);
            
        }
        else
        {
            return Results.StatusCode(404);
        }
    }
    else
    {
        return Results.StatusCode(404);
    }
    
});
app.MapDelete("/api/books/{id}", async (int id) =>
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
