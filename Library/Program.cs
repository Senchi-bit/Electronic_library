using Library.Context;
using Library.Entities;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

MyDbContext db = new MyDbContext();
var books = db.Books.ToList();
var authors = db.Authors.ToList();
var exhibitions = db.Exhibitions.ToList();





app.Run((async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;
    string regexString = @"^/api/books/\d*$";
    
    if (path == "/api/books" && request.Method=="GET")
    {
        await GetAllBooks(response); 
    }

    else if (path == "/authors/api/authors" && request.Method == "GET")
    {
        await GetAllAuthors(response);
    }

    else if (path == "/api/exhibitions" && request.Method == "GET")
    {
        await GetAllExhibitions(response);
    }

    else if (path == "/api/books" && request.Method == "POST")
    {
        await AddBook(response, request);
    }
    else if (Regex.IsMatch(path, regexString) && request.Method == "GET")
    {
        int id = Convert.ToInt32(path.Value?.Split("/")[3]);
        await GetBook(id, response);
    }
    else if(path == "/api/books" && request.Method == "PUT")
    {
        await UpdateBook(response, request);
    }
    else if (Regex.IsMatch(path, regexString) && request.Method == "DELETE")
    {
        int id = Convert.ToInt32(path.Value?.Split("/")[3]);
        await DeleteBook(id, response);
    }
    else if(path == "/authors" && request.Method == "GET")
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/authors.html");
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
}));
app.Run();


async Task GetAllBooks(HttpResponse response)
{
    await response.WriteAsJsonAsync(books);
}
async Task GetAllAuthors(HttpResponse response)
{
    await response.WriteAsJsonAsync(authors);
}

async Task GetAllExhibitions(HttpResponse response)
{
    await response.WriteAsJsonAsync(exhibitions);
}

async Task AddBook(HttpResponse response, HttpRequest request)
{
    try
    {
        var lastBook = books.LastOrDefault();
        var book = await request.ReadFromJsonAsync<Book>();
        if (book != null)
        {
            Book newBook = new Book()
            {
                Id = lastBook.Id + 1,
                Title = book.Title,
                ReleaseYear = book.ReleaseYear
            };
            await db.Books.AddAsync(newBook);
            await db.SaveChangesAsync();
            await response.WriteAsJsonAsync(newBook);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception e)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Пользователь не найден" });
    }
}

async Task GetBook(int id,HttpResponse response){
    var book = books.FirstOrDefault(b => b.Id == id); 
    if (book != null)
        await response.WriteAsJsonAsync(book);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Книга не найдена" });
    }
}

async Task UpdateBook(HttpResponse response, HttpRequest request)
{
    try
    {
        var bookData = await request.ReadFromJsonAsync<Book>();
        if (bookData != null)
        {
            var book = books.FirstOrDefault(u => u.Id == bookData.Id);
            if (book != null)
            {
                book.ReleaseYear = bookData.ReleaseYear;
                book.Title = bookData.Title;
                db.Books.Update(book);
                await db.SaveChangesAsync();
                await response.WriteAsJsonAsync(book);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Пользователь не найден" });
            }
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

async Task DeleteBook(int id, HttpResponse response)
{
    try
    {
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book != null)
        {
            db.Books.Remove(book);
            await db.SaveChangesAsync();
            await response.WriteAsJsonAsync(book);
        }
        else
        {
            throw new Exception("Not found");
        }
    }
    catch (Exception e)
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = e.Message });
    }
}