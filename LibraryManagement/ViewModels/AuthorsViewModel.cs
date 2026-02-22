using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Commands;

namespace LibraryManagement.ViewModels
{
    public class AuthorsViewModel : ViewModelBase
    {
        private readonly LibraryContext _context;
        private ObservableCollection<Author> _authors;
        private Author _selectedAuthor;

        public AuthorsViewModel(LibraryContext context)
        {
            _context = context;

            // Загружаем авторов из базы
            _context.Authors.Load();
            Authors = new ObservableCollection<Author>(_context.Authors.Local);

            // Команды
            AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            SaveCommand = new RelayCommand(Save);
        }

        public ObservableCollection<Author> Authors
        {
            get => _authors;
            set { _authors = value; OnPropertyChanged(); }
        }

        public Author SelectedAuthor
        {
            get => _selectedAuthor;
            set { _selectedAuthor = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        private void Add(object param)
        {
            var newAuthor = new Author();
            _context.Authors.Add(newAuthor);
            Authors.Add(newAuthor);
            SelectedAuthor = newAuthor;
        }

        private bool CanDelete(object param)
        {
            return SelectedAuthor != null;
        }

        private void Delete(object param)
        {
            if (SelectedAuthor == null) return;

            _context.Authors.Remove(SelectedAuthor);
            Authors.Remove(SelectedAuthor);
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