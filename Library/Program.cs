using System.Net.Mime;
using Library.Context;
using Library.Entities;
using Mapster;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
    // app.UseExceptionHandler(exceptionHandlerApp =>
    // {
    //     exceptionHandlerApp.Run(async context =>
    //     {
    //         context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    //
    //         // using static System.Net.Mime.MediaTypeNames;
    //         context.Response.ContentType = MediaTypeNames.Text.Plain;
    //
    //         await context.Response.WriteAsync("An exception was thrown. ");
    //
    //         var exceptionHandlerPathFeature =
    //             context.Features.Get<IExceptionHandlerPathFeature>();
    //
    //         if (exceptionHandlerPathFeature?.Error is KeyNotFoundException)
    //         {
    //             await context.Response.WriteAsync(" The object was not found.");
    //         }
    //
    //         if (exceptionHandlerPathFeature?.Error is Exception)
    //         {
    //             await context.Response.WriteAsync("Server error!");
    //         }
    //
    //         if (exceptionHandlerPathFeature?.Path == "/")
    //         {
    //             await context.Response.WriteAsync(" Page: Home.");
    //         }
    //     });
    // });
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
    var book = context.Books.ProjectToType<BookDto>().ToList().FirstOrDefault(b => b.Id == id); 
    if (book != null)
        return Results.Ok(book);
    else
    {
        throw new KeyNotFoundException("Book not found");
    }
});

//<summary>
//API for add new book
//</summary>
app.MapPost("/api/books", async (BookPostDto book, MyDbContext db) =>
{
    var books = await db.Books.ToListAsync();
    var authors =await db.Authors.Where(b => b.Id == book.AuthorId).ToListAsync();
    if (authors == null)
    {
        throw new KeyNotFoundException("Author not found");
    }
    var author = authors.FirstOrDefault();
    var lastBookId = books.Count() != 0 ? books.Max(x => x.Id) : 1;
    if (book.Title != "" && book.ReleaseYear != null)
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
        throw new Exception("Некорректные данные");
    }
});

//<summary>
//Фильтрация книг по имени
//</summary>
app.MapPut("/api/books/nameFilter",async (Book book, MyDbContext db) =>
{
    if (book.Title == "")
    {
        throw new Exception("Неверный запрос");
    }

    var books =await db.Books.Where(b => b.Title.ToLower().Contains(book.Title.ToLower())).ToListAsync();
    if (books != null)
    {
        return Results.Ok(books);
    }
    else
    {
        throw new Exception("Not found");
    }
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
    if (bookData != null)
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
            throw new Exception();
        }
    }
    else
    {
        throw new Exception();
    }
    
});

//<summary>
//API for delete book by id
//</summary>
app.MapDelete("/api/books/{id}", async (int id, MyDbContext db) =>
{
    var books = await db.Books.Where(b => b.Id == id).ToListAsync();
    var book = books.FirstOrDefault();
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

//<summary>
//API for get all authors
//</summary>
app.MapGet("/api/authors", async (MyDbContext context) => 
    await context.Authors.ProjectToType<AuthorDto>().ToListAsync());

//<summary>
//API for get author by id
//</summary>
app.MapGet("/api/authors/{id}", async (int id, MyDbContext context) => 
    await context.Authors.ProjectToType<AuthorDto>().FirstOrDefaultAsync(u => u.Id == id));

//<summary>
//API for add author
//</summary>
app.MapPost("/api/authros", async (Author author, MyDbContext db) =>
{
    var authors = await db.Authors.ToListAsync();
    var lastBookId = authors.Count() != 0 ? authors.Max(x => x.Id) : 1;
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
        throw new Exception("Некорректные данные");
    }
});

//<summary>
//API for update book
//</summary>
app.MapPut("/api/authors", async (Author author, MyDbContext db) =>
{
    var authors = await db.Authors.Where(u => u.Id == author.Id).ToListAsync();
    if (authors.Count == 0) throw new Exception();
    var updateAuthor = authors.FirstOrDefault();
    if (updateAuthor.FullName != "")
    {
        updateAuthor.FullName = author.FullName;   
        await db.SaveChangesAsync();
        return Results.Ok(updateAuthor);
    }
    else
    {
        throw new Exception("Name the must not be empty!");
    }
});

//<summary>
//API for delete book by id
//</summary>
app.MapDelete("/api/authros/{id}", async (int id, MyDbContext db) =>
{
    var authors = await db.Authors.Where(u => u.Id == id).ToListAsync();
    db.Authors.Remove(authors.FirstOrDefault());
    await db.SaveChangesAsync();
    return Results.Ok(authors.FirstOrDefault(u => u.Id == id));
});

//<summary>
//Фильтрация авторов по имени
//</summary>
app.MapPut("/api/authors/nameFilter", async (Author author, MyDbContext db) =>
{
    if (author.FullName == "")
    {
        throw new Exception("Неверный запрос");
    }

    var filtredAuthors =  await db.Authors.Where(b => b.FullName.ToLower().Contains(author.FullName.ToLower())).ToListAsync();
    if (filtredAuthors != null)
    {
        return Results.Ok(filtredAuthors);
    }
    else
    {
        throw new Exception("Not found");
    }
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
    await db.Exhibitions.ProjectToType<ExhibitionDto>().FirstOrDefaultAsync(u => u.Id == id));

///<summary>
///API for add exhibitions
///</summary>
app.MapPost("/api/exhibition", async (Exhibition exhibition, MyDbContext db) =>
{
    var exhibitions = await db.Exhibitions.ToListAsync();
    var lastExhibitionId = exhibitions.Count != 0 ? exhibitions.Max(x => x.Id) : 1;
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
        throw new Exception("Не хватает данных");
    }
});

//<summary>
//<param name="id">id = id for exhibition</param>
//</summary>
app.MapPut("api/exhibition/addBook/{id}",async (int id, Book book, MyDbContext db) =>
{
    var books = await db.Books.Where(b => b.Id == book.Id).ToListAsync();
    var exhibitions = await db.Exhibitions.Where(b => b.Id == id).ToListAsync();
    var exhibition = exhibitions.FirstOrDefault();
    var nowBook = books.FirstOrDefault();
    if (exhibition.YearBased == nowBook.ReleaseYear)
    {
        exhibition.Books.Add(nowBook);
        await db.SaveChangesAsync();
    }
    return Results.Ok(exhibition);
});
app.MapPut("/api/exhibition", async (ExhibitionDto exhibition, MyDbContext db) =>
{
    var exhibitions = await db.Exhibitions.Where(b => b.Id == exhibition.Id).ToListAsync();
    var updateExhibition = exhibitions.FirstOrDefault();
    if (updateExhibition.Title != "" && updateExhibition.YearBased == exhibition.YearBased)
    {
        updateExhibition.YearBased = exhibition.YearBased;
        updateExhibition.Title = exhibition.Title;
    }
    
    db.Exhibitions.Update(updateExhibition);
    await db.SaveChangesAsync();
    return Results.Ok(exhibition);
});

app.MapDelete("/api/exhibition/{id}", async (int id, MyDbContext db) =>
{
    var exhibition = await db.Exhibitions.Include(e => e.Books)
        .Where(b => b.Id == id).FirstAsync();
    //var exhibition = exhibitions.FirstOrDefault();
    foreach (var book in exhibition.Books)
    {
        book.ExhibitionId = null;
    }
    db.Exhibitions.Remove(exhibition);
    await db.SaveChangesAsync();
});
app.Run();



