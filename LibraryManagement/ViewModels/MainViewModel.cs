using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Commands;
using LibraryManagement.Views;

namespace LibraryManagement.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly LibraryContext _context;
        private ObservableCollection<Book> _books;
        private ObservableCollection<Author> _authors;
        private ObservableCollection<Genre> _genres;
        private Author _selectedAuthorFilter;
        private Genre _selectedGenreFilter;
        private string _searchText;
        private Book _selectedBook;

        public ICommand AddBookCommand { get; }
        public ICommand EditBookCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand ManageAuthorsCommand { get; }
        public ICommand ManageGenresCommand { get; }

        public MainViewModel()
        {
            _context = new LibraryContext();
            _context.Database.EnsureCreated();
            _context.Authors.Load();
            _context.Genres.Load();
            Authors = new ObservableCollection<Author>(_context.Authors.Local);
            Genres = new ObservableCollection<Genre>(_context.Genres.Local);
            LoadBooks();

            AddBookCommand = new RelayCommand(AddBook);
            EditBookCommand = new RelayCommand(EditBook, CanEditOrDelete);
            DeleteBookCommand = new RelayCommand(DeleteBook, CanEditOrDelete);
            ManageAuthorsCommand = new RelayCommand(ManageAuthors);
            ManageGenresCommand = new RelayCommand(ManageGenres);
        }

        public ObservableCollection<Book> Books
        {
            get => _books;
            set { _books = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Author> Authors
        {
            get => _authors;
            set { _authors = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set { _genres = value; OnPropertyChanged(); }
        }

        public Author SelectedAuthorFilter
        {
            get => _selectedAuthorFilter;
            set { _selectedAuthorFilter = value; OnPropertyChanged(); FilterBooks(); }
        }

        public Genre SelectedGenreFilter
        {
            get => _selectedGenreFilter;
            set { _selectedGenreFilter = value; OnPropertyChanged(); FilterBooks(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterBooks(); }
        }

        public Book SelectedBook
        {
            get => _selectedBook;
            set { _selectedBook = value; OnPropertyChanged(); }
        }

        private void LoadBooks()
        {
            var books = _context.Books.Include(b => b.Author).Include(b => b.Genre).ToList();
            Books = new ObservableCollection<Book>(books);
        }

        private void FilterBooks()
        {
            var query = _context.Books.Include(b => b.Author).Include(b => b.Genre).AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(b => b.Title.Contains(SearchText));
            if (SelectedAuthorFilter != null)
                query = query.Where(b => b.AuthorId == SelectedAuthorFilter.Id);
            if (SelectedGenreFilter != null)
                query = query.Where(b => b.GenreId == SelectedGenreFilter.Id);
            Books = new ObservableCollection<Book>(query.ToList());
        }

        private bool CanEditOrDelete(object param) => SelectedBook != null;

        private string GenerateUniqueISBN()
        {
            Random rand = new Random();
            string isbn;
            bool exists;
            do
            {
                // Формат: 978-5-XXXXX-XXX-X (13 цифр)
                int part1 = rand.Next(10000, 100000);   // 5 цифр
                int part2 = rand.Next(100, 1000);       // 3 цифры
                int check = rand.Next(0, 10);            // контрольная цифра
                isbn = $"978-5-{part1}-{part2}-{check}";
                exists = _context.Books.Any(b => b.ISBN == isbn);
            } while (exists);
            return isbn;
        }

        private void AddBook(object param)
        {
            var newBook = new Book
            {
                ISBN = GenerateUniqueISBN()
            };
            var bookViewModel = new BookViewModel(newBook, _context);
            var window = new BookWindow { DataContext = bookViewModel };
            if (window.ShowDialog() == true)
            {
                _context.Books.Add(newBook);
                _context.SaveChanges();
                FilterBooks();
            }
        }

        private void EditBook(object param)
        {
            if (SelectedBook == null) return;
            _context.Entry(SelectedBook).Reference(b => b.Author).Load();
            _context.Entry(SelectedBook).Reference(b => b.Genre).Load();
            var bookViewModel = new BookViewModel(SelectedBook, _context);
            var window = new BookWindow { DataContext = bookViewModel };
            if (window.ShowDialog() == true)
            {
                _context.SaveChanges();
                FilterBooks();
            }
        }

        private void DeleteBook(object param)
        {
            if (SelectedBook == null) return;
            var result = System.Windows.MessageBox.Show($"Удалить книгу '{SelectedBook.Title}'?", "Подтверждение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _context.Books.Remove(SelectedBook);
                _context.SaveChanges();
                FilterBooks();
            }
        }

        private void ManageAuthors(object param)
        {
            var authorsViewModel = new AuthorsViewModel(_context);
            var window = new AuthorsWindow { DataContext = authorsViewModel };
            window.ShowDialog();
            _context.Authors.Load();
            Authors = new ObservableCollection<Author>(_context.Authors.Local);
            if (SelectedAuthorFilter != null && !Authors.Any(a => a.Id == SelectedAuthorFilter.Id))
                SelectedAuthorFilter = null;
        }

        private void ManageGenres(object param)
        {
            var genresViewModel = new GenresViewModel(_context);
            var window = new GenresWindow { DataContext = genresViewModel };
            window.ShowDialog();
            _context.Genres.Load();
            Genres = new ObservableCollection<Genre>(_context.Genres.Local);
            if (SelectedGenreFilter != null && !Genres.Any(g => g.Id == SelectedGenreFilter.Id))
                SelectedGenreFilter = null;
        }
    }
}