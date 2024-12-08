using System;
using System.Collections.Generic;

namespace Library.Entities;

public partial class Author
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
public partial class AuthorDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;
    public virtual ICollection<BookDto> Books { get; set; }
}
