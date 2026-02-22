using System.Collections.ObjectModel;
using System.Linq;
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
            var newGenre = new Genre();
            _context.Genres.Add(newGenre);
            Genres.Add(newGenre);
            SelectedGenre = newGenre;
        }

        private bool CanDelete(object param)
        {
            return SelectedGenre != null;
        }

        private void Delete(object param)
        {
            if (SelectedGenre == null) return;
            _context.Genres.Remove(SelectedGenre);
            Genres.Remove(SelectedGenre);
        }

        private void Save(object param)
        {
            _context.SaveChanges();
            System.Windows.MessageBox.Show("Сохранено!", "Успех",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}