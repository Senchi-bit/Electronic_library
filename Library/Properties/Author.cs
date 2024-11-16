namespace Library.Properties;

public class Author
{
    public int id;
    public string fullName;
    public Book[] books;

    public Author(int id, string fullName, Book[] books)
    {
        this.id = id;
        this.fullName = fullName;
        this.books = books;
    }

}