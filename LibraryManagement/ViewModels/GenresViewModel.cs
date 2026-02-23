using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Commands;

namespace LibraryManagement.ViewModels
{
    public class GenresViewModel : ViewModelBase
    {
        private readonly LibraryContext _context;
        private ObservableCollection<Genre> _genres;
        private Genre _selectedGenre;

        public GenresViewModel(LibraryContext context)
        {
            _context = context;
            _context.Genres.Load();
            Genres = new ObservableCollection<Genre>(_context.Genres.Local);

            AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            SaveCommand = new RelayCommand(Save);
        }

        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set { _genres = value; OnPropertyChanged(); }
        }

        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set { _selectedGenre = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        private void Add(object param)
        {
            var newGenre = new Genre
            {
                Name = "Новый жанр",
                Description = ""
            };
            _context.Genres.Add(newGenre);
            Genres.Add(newGenre);
            SelectedGenre = newGenre;
        }

        private bool CanDelete(object param) => SelectedGenre != null;

        private void Delete(object param)
        {
            if (SelectedGenre == null) return;

            if (_context.Books.Any(b => b.GenreId == SelectedGenre.Id))
            {
                MessageBox.Show("Нельзя удалить жанр, который используется в книгах. Сначала удалите или измените книги.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _context.Genres.Remove(SelectedGenre);
            Genres.Remove(SelectedGenre);
        }

        private void Save(object param)
        {
            foreach (var genre in Genres)
            {
                if (string.IsNullOrWhiteSpace(genre.Name))
                {
                    MessageBox.Show("У всех жанров должно быть заполнено Название.",
                                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.InnerException?.Message}",
                                "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}