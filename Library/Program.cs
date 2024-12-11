using System.Net.Mime;
using Library.Context;
using Library.Entities;
using Mapster;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Library.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder()
        .AddJsonFile("appsettingsdb.json")
        .SetBasePath(Directory.GetCurrentDirectory())
        .Build()
    ?? throw new InvalidOperationException("Connection string"
                                           + "'DefaultConnection' not found.");

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRouting();
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {

            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;
    
            switch (exception)
            {
                case NotFoundException notFoundException:
                    context.Response.StatusCode = notFoundException.StatusCode;
                    await context.Response.WriteAsJsonAsync( new { error = notFoundException.Message });
                    break;
                case IncorrectDataException incorrectDataException:
                    context.Response.StatusCode = incorrectDataException.StatusCode;
                    await context.Response.WriteAsJsonAsync( new { error = incorrectDataException.Message });
                    break;
                default:
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync( new { error = exception.Message });
                    break;
            }
            
        });
    });
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

app.MapSwagger().RequireAuthorization();

//<summary>
//API for get all book
//</summary>
app.MapGet("/api/books",async (MyDbContext context) => 
    await context.Books.ProjectToType<BookDto>().ToListAsync());

//<summary>
//API for get book by id
//</summary>
app.MapGet("/api/books/{id}",(int id, MyDbContext context) =>
{
    var book = context.Books.Where(b => b.Id == id).ProjectToType<BookDto>().ToList(); 
    if (book.Count != 0)
        return Results.Ok(book);
    else
    {
        throw new NotFoundException(404, "Book Not Found");
    }
});

//<summary>
//API for add new book
//</summary>
app.MapPost("/api/books", async (BookPostDto book, MyDbContext db) =>
{
    var authors =await db.Authors.Where(b => b.Id == book.AuthorId).ToListAsync();
    if (authors == null)
    {
        throw new NotFoundException(404, "Author Not Found");
    }
    var author = authors.FirstOrDefault();
    var lastBookId = db.Books.Count() != 0 ? db.Books.Max(x => x.Id) : 1;
    if (book.Title != "")
    {
        Book newBook = new Book()
        {
            Id = lastBookId + 1,
            Title = book.Title,
            ReleaseYear = book.ReleaseYear
        };
        
        await db.Books.AddAsync(newBook);
        newBook.Authors.Add(author);
        author.Books.Add(newBook);
        await db.SaveChangesAsync();
    }
    else
    {
        throw new IncorrectDataException(400, "Некорректные данные");
    }
});

//<summary>
//Фильтрация книг по имени
//</summary>
app.MapPut("/api/books/nameFilter",async (Book book, MyDbContext db) =>
{
    if (book.Title == "")
    {
        throw new IncorrectDataException(400, "Некорректные данные");
    }

    var books =await db.Books.Where(b => b.Title.ToLower().Contains(book.Title.ToLower())).ToListAsync();
    return Results.Ok(books);
});
//WIP (Work in Progress)
// app.MapGet("/api/books/authorIdFilter/{id}", (int id, MyDbContext db) =>
// {
//     var books = db.Books..ToList();
//     if (books != null)
//     {
//         return Results.Ok(books);
//     }
//     else
//     {
//         throw new Exception("Not found");
//     }
// });

//<summary>
//API for update book
//</summary>
app.MapPut("/api/books",async (BookDto bookData, MyDbContext db) =>
{
    var books =await db.Books.Where(u => u.Id == bookData.Id).ToListAsync();
    if (books.Count == 0)
    {
        throw new NotFoundException(404, "Book Not Found");
    }
    if (bookData.Title != "" && bookData.Title != null)
    {
        var book = books.FirstOrDefault();
        if (book != null && bookData.Title != "")
        {
            book.ReleaseYear = bookData.ReleaseYear;
            book.Title = bookData.Title;
            await db.SaveChangesAsync();
            return Results.Ok(book);
            
        }
        else
        {
            throw new IncorrectDataException(400, "Некорректные данные");
        }
    }
    else
    {
        throw new IncorrectDataException(400, "Некорректные данные");
    }
    
});

//<summary>
//API for delete book by id
//</summary>
app.MapDelete("/api/books/{id}", async (int id, MyDbContext db) =>
{
    var books = await db.Books.Where(b => b.Id == id).ToListAsync();
    if (books.Count == 0)
    {
        throw new NotFoundException(404, "Book Not Found");
    }
    var book = books.FirstOrDefault();

    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.Ok(book);
});

//<summary>
//API for get all authors
//</summary>
app.MapGet("/api/authors", async (MyDbContext context) => 
    await context.Authors.ProjectToType<AuthorDto>().ToListAsync());

//<summary>
//API for get author by id
//</summary>
app.MapGet("/api/authors/{id}", async (int id, MyDbContext context) =>
{
    var authors = await context.Authors.Where(u => u.Id == id).ProjectToType<AuthorDto>().ToListAsync();
    var author = authors.FirstOrDefault();
    if (author == null) throw new NotFoundException(404, "Author Not Found");
    return Results.Ok(author);
});
//<summary>
//API for add author
//</summary>
app.MapPost("/api/authros", async (Author author, MyDbContext db) =>
{
    var lastBookId = db.Authors.Count() != 0 ? db.Authors.Max(x => x.Id) : 1;
    if (author.FullName != "")
    {
        Author newAuthor = new Author()
        {
            Id = lastBookId + 1,
            FullName = author.FullName
        };
        await db.Authors.AddAsync(newAuthor);
        await db.SaveChangesAsync();
        return Results.Ok(newAuthor);
    }
    else
    {
        throw new IncorrectDataException(400, "Некорректные данные");
    }
});

//<summary>
//API for update book
//</summary>
app.MapPut("/api/authors", async (Author author, MyDbContext db) =>
{
    var authors = await db.Authors.Where(u => u.Id == author.Id).ToListAsync();
    
    if (authors.Count == 0) throw new NotFoundException(404, "Authors Not Found");
    
    var updateAuthor = authors.FirstOrDefault();
    if (updateAuthor.FullName != "")
    {
        updateAuthor.FullName = author.FullName;   
        await db.SaveChangesAsync();
        return Results.Ok(updateAuthor);
    }
    else
    {
        throw new IncorrectDataException(400, "Некорректные данные");
    }
});

//<summary>
//API for delete book by id
//</summary>
app.MapDelete("/api/authros/{id}", async (int id, MyDbContext db) =>
{
    var authors = await db.Authors.Where(u => u.Id == id).ToListAsync();
    if (authors.Count == 0) throw new NotFoundException(404, "Authors Not Found");
    var author = authors.FirstOrDefault();
    db.Authors.Remove(author);
    await db.SaveChangesAsync();
    return Results.Ok(author);
});

//<summary>
//Фильтрация авторов по имени
//</summary>
app.MapPut("/api/authors/nameFilter", async (Author author, MyDbContext db) =>
{
    if (author.FullName == "") throw new IncorrectDataException(400, "Некорректные данные");
    
    var filtredAuthors =  await db.Authors.Where(b => b.FullName.ToLower().Contains(author.FullName.ToLower())).ToListAsync();
    return Results.Ok(filtredAuthors);
});


//<summary>
//API for get all exhibitions
//</summary>
app.MapGet("/api/exhibitions", async (MyDbContext db) => 
    await db.Exhibitions.ProjectToType<ExhibitionDto>().ToListAsync());

//<summary>
//API for get exhibitions by id
//</summary>
app.MapGet("/api/exhibition/{id}", async (int id, MyDbContext db) =>
{
    var exhibitions = await db.Exhibitions.Where(u => u.Id == id).ProjectToType<ExhibitionDto>().ToListAsync();
    var exhibition = exhibitions.FirstOrDefault(); 
    if (exhibition == null) throw new NotFoundException(404, "Exhibitions Not Found");
    return Results.Ok(exhibition);

});

//<summary>
//API for add exhibitions
//</summary>
app.MapPost("/api/exhibition", async (Exhibition exhibition, MyDbContext db) =>
{
    var lastExhibitionId = db.Exhibitions.Any() ? db.Exhibitions.Max(x => x.Id) : 1;
    if (exhibition.Title != "")
    {
        Exhibition newExhibition = new Exhibition();
        newExhibition.YearBased = exhibition.YearBased;
        newExhibition.Title = exhibition.Title;
        newExhibition.Id = lastExhibitionId + 1 ;
        await db.Exhibitions.AddAsync(newExhibition);
        await db.SaveChangesAsync();
        return Results.Ok(newExhibition);
    }
    else
    {
        throw new IncorrectDataException(400, "Некорректные данные");
        
    }
});

//<summary>
//<param name="id">id = id for exhibition</param>
//</summary>
app.MapPut("api/exhibition/addBook/{id}",async (int id, Book book, MyDbContext db) =>
{
    var books = await db.Books.Where(b => b.Id == book.Id).ToListAsync();
    var exhibitions = await db.Exhibitions.Where(b => b.Id == id).ToListAsync();
    if (books.Count == 0 | exhibitions.Count == 0) throw new NotFoundException(404, "Books or exhibitions Not Found!");
    var exhibition = exhibitions.FirstOrDefault();
    var nowBook = books.FirstOrDefault();
    if (exhibition.YearBased == nowBook.ReleaseYear)
    {
        exhibition.Books.Add(nowBook);
        await db.SaveChangesAsync();
        
    }
    else
    {
        throw new IncorrectDataException(400, "Год выпуска книги не совпадает с годом базирования выставки!");
    }
    return Results.Ok(exhibition);
});
app.MapPut("/api/exhibition", async (ExhibitionDto updateExhibition, MyDbContext db) =>
{
    var exhibitions = await db.Exhibitions.Where(b => b.Id == updateExhibition.Id).ToListAsync();
    var exhibition = exhibitions.FirstOrDefault();
    if (updateExhibition.Title != "" && exhibition != null && updateExhibition.YearBased == exhibition.YearBased)
    {
        exhibition.YearBased = updateExhibition.YearBased;
        exhibition.Title = updateExhibition.Title;
    }
    else
    {
        throw new IncorrectDataException(400, "Некорректные данные");
    }
    db.Exhibitions.Update(exhibition);
    await db.SaveChangesAsync();
    return Results.Ok(exhibition);
});

app.MapDelete("/api/exhibition/{id}", async (int id, MyDbContext db) =>
{
    var exhibition = await db.Exhibitions.Include(e => e.Books)
        .Where(b => b.Id == id).FirstAsync();
    
    if (exhibition == null) throw new NotFoundException(404, "Exhibition Not Found");

    foreach (var book in exhibition.Books)
    {
        book.ExhibitionId = null;
    }
    db.Exhibitions.Remove(exhibition);
    await db.SaveChangesAsync();
});
app.Run();