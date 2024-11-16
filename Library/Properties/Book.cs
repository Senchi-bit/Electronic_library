namespace Library.Properties;

public class Book
{
    public int id;
    public string title;
    public int releasedYear;
    public Author author;

    public Book(int id, string title, int releasedYear, Author author)
    {
        this.id = id;
        this.title = title;
        this.releasedYear = releasedYear;
        this.author = author;
    }
}