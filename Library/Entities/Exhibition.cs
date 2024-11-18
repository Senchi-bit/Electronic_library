using System;
using System.Collections.Generic;

namespace Library.Entities;

public partial class Exhibition
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int YearBased { get; set; }
}
