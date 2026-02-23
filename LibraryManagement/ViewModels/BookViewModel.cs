using System.Collections.ObjectModel;
using System.Linq;
using LibraryManagement.Models;

namespace LibraryManagement.ViewModels
{
    public class BookViewModel : ViewModelBase
    {
        private Book _book;
        private readonly LibraryContext _context;

        public BookViewModel(Book book, LibraryContext context)
        {
            _book = book;
            _context = context;
            Authors = new ObservableCollection<Author>(_context.Authors.Local);
            Genres = new ObservableCollection<Genre>(_context.Genres.Local);
        }

        public Book Book
        {
            get => _book;
            set { _book = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Author> Authors { get; }
        public ObservableCollection<Genre> Genres { get; }

        public Author SelectedAuthor
        {
            get => Authors.FirstOrDefault(a => a.Id == _book.AuthorId);
            set
            {
                if (value != null)
                {
                    _book.AuthorId = value.Id;
                    _book.Author = value;
                    OnPropertyChanged();
                }
            }
        }

        public Genre SelectedGenre
        {
            get => Genres.FirstOrDefault(g => g.Id == _book.GenreId);
            set
            {
                if (value != null)
                {
                    _book.GenreId = value.Id;
                    _book.Genre = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}