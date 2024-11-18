using System;
using System.Collections.Generic;

namespace Library.Entities;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int ReleaseYear { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
}
