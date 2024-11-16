namespace Library.Properties;

public class Exhibition
{
    public int id;
    public string title;
    public int YearBased  { get; set; }
    public Book[] Books;

    public Exhibition(int id, string title, int yearBased, Book[] books)
    {
        this.id = id;
        this.title = title;
        this.YearBased = yearBased;
        this.Books = books;
    }
}