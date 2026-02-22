using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Commands;
using LibraryManagement.Views; // для открытия окон

namespace LibraryManagement.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Контекст базы данных
        private readonly LibraryContext _context;

        // Коллекции для отображения в интерфейсе
        private ObservableCollection<Book> _books;
        private ObservableCollection<Author> _authors;
        private ObservableCollection<Genre> _genres;

        // Выбранные элементы для фильтрации
        private Author _selectedAuthorFilter;
        private Genre _selectedGenreFilter;
        private string _searchText;

        // Выбранная книга в DataGrid (для редактирования/удаления)
        private Book _selectedBook;

        // Команды для кнопок
        public ICommand AddBookCommand { get; }
        public ICommand EditBookCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand ManageAuthorsCommand { get; }
        public ICommand ManageGenresCommand { get; }

        // Конструктор
        public MainViewModel()
        {
            // Создаём контекст базы данных
            _context = new LibraryContext();

            // Гарантируем, что база данных создана (для SQLite)
            _context.Database.EnsureCreated();

            // Загружаем авторов и жанры в память
            _context.Authors.Load();
            _context.Genres.Load();

            // Преобразуем локальные данные в ObservableCollection
            Authors = new ObservableCollection<Author>(_context.Authors.Local);
            Genres = new ObservableCollection<Genre>(_context.Genres.Local);

            // Загружаем книги с авторами и жанрами
            LoadBooks();

            // Инициализируем команды
            AddBookCommand = new RelayCommand(AddBook);
            EditBookCommand = new RelayCommand(EditBook, CanEditOrDelete);
            DeleteBookCommand = new RelayCommand(DeleteBook, CanEditOrDelete);
            ManageAuthorsCommand = new RelayCommand(ManageAuthors);
            ManageGenresCommand = new RelayCommand(ManageGenres);
        }

        // ---- Свойства для привязки ----

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
            set
            {
                _selectedAuthorFilter = value;
                OnPropertyChanged();
                FilterBooks(); // при изменении фильтра обновляем список книг
            }
        }

        public Genre SelectedGenreFilter
        {
            get => _selectedGenreFilter;
            set
            {
                _selectedGenreFilter = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        public Book SelectedBook
        {
            get => _selectedBook;
            set { _selectedBook = value; OnPropertyChanged(); }
        }

        // ---- Методы для работы с данными ----

        private void LoadBooks()
        {
            // Загружаем книги вместе с авторами и жанрами
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .ToList();

            Books = new ObservableCollection<Book>(books);
        }

        private void FilterBooks()
        {
            // Начинаем с запроса к базе
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsQueryable();

            // Фильтр по поисковому тексту (название книги)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(b => b.Title.Contains(SearchText));
            }

            // Фильтр по автору
            if (SelectedAuthorFilter != null)
            {
                query = query.Where(b => b.AuthorId == SelectedAuthorFilter.Id);
            }

            // Фильтр по жанру
            if (SelectedGenreFilter != null)
            {
                query = query.Where(b => b.GenreId == SelectedGenreFilter.Id);
            }

            // Выполняем запрос и обновляем коллекцию
            Books = new ObservableCollection<Book>(query.ToList());
        }

        // Проверка: можно ли редактировать/удалить (нужно, чтобы книга была выбрана)
        private bool CanEditOrDelete(object param)
        {
            return SelectedBook != null;
        }

        // ---- Команды (методы, вызываемые из UI) ----

        private void AddBook(object param)
        {
            // Создаём новую книгу
            var newBook = new Book();

            // Создаём ViewModel для окна книги
            var bookViewModel = new BookViewModel(newBook, _context);

            // Создаём окно и передаём ему ViewModel
            var window = new BookWindow
            {
                DataContext = bookViewModel
            };

            // Показываем окно как диалог
            if (window.ShowDialog() == true)
            {
                // Если пользователь нажал OK, сохраняем изменения в базу
                _context.Books.Add(newBook);
                _context.SaveChanges();

                // Обновляем список книг
                FilterBooks();
            }
        }

        private void EditBook(object param)
        {
            if (SelectedBook == null) return;

            // Загружаем связанные данные (ещё раз, на всякий случай)
            _context.Entry(SelectedBook).Reference(b => b.Author).Load();
            _context.Entry(SelectedBook).Reference(b => b.Genre).Load();

            // Создаём ViewModel с выбранной книгой
            var bookViewModel = new BookViewModel(SelectedBook, _context);

            var window = new BookWindow
            {
                DataContext = bookViewModel
            };

            if (window.ShowDialog() == true)
            {
                // Сохраняем изменения
                _context.SaveChanges();

                // Обновляем список
                FilterBooks();
            }
        }

        private void DeleteBook(object param)
        {
            if (SelectedBook == null) return;

            // Спрашиваем подтверждение (простое окно с MessageBox)
            var result = System.Windows.MessageBox.Show(
                $"Удалить книгу '{SelectedBook.Title}'?",
                "Подтверждение",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _context.Books.Remove(SelectedBook);
                _context.SaveChanges();
                FilterBooks();
            }
        }

        private void ManageAuthors(object param)
        {
            // Создаём ViewModel для управления авторами
            var authorsViewModel = new AuthorsViewModel(_context);

            var window = new AuthorsWindow
            {
                DataContext = authorsViewModel
            };

            window.ShowDialog(); // не диалог, а просто окно

            // После закрытия окна обновляем список авторов в фильтре
            _context.Authors.Load();
            Authors = new ObservableCollection<Author>(_context.Authors.Local);

            // Сбрасываем фильтр, если выбранный автор был удалён
            if (SelectedAuthorFilter != null &&
                !Authors.Any(a => a.Id == SelectedAuthorFilter.Id))
            {
                SelectedAuthorFilter = null;
            }
        }

        private void ManageGenres(object param)
        {
            // Аналогично авторам
            var genresViewModel = new GenresViewModel(_context);

            var window = new GenresWindow
            {
                DataContext = genresViewModel
            };

            window.ShowDialog();

            _context.Genres.Load();
            Genres = new ObservableCollection<Genre>(_context.Genres.Local);

            if (SelectedGenreFilter != null &&
                !Genres.Any(g => g.Id == SelectedGenreFilter.Id))
            {
                SelectedGenreFilter = null;
            }
        }
    }
}