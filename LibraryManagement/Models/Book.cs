namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public int GenreId { get; set; }
        public int PublishYear { get; set; }
        public string ISBN { get; set; }
        public int QuantityInStock { get; set; }

        // Навигационные свойства
        public Author Author { get; set; }
        public Genre Genre { get; set; }
    }
}