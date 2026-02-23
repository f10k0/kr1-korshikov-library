using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
            _context.Authors.Load();
            Authors = new ObservableCollection<Author>(_context.Authors.Local);

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
            var newAuthor = new Author
            {
                FirstName = "Новый",
                LastName = "Автор",
                BirthDate = DateTime.Today,
                Country = ""
            };
            _context.Authors.Add(newAuthor);
            Authors.Add(newAuthor);
            SelectedAuthor = newAuthor;
        }

        private bool CanDelete(object param) => SelectedAuthor != null;

        private void Delete(object param)
        {
            if (SelectedAuthor == null) return;

            if (_context.Books.Any(b => b.AuthorId == SelectedAuthor.Id))
            {
                MessageBox.Show("Нельзя удалить автора, у которого есть книги. Сначала удалите или переназначьте его книги.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _context.Authors.Remove(SelectedAuthor);
            Authors.Remove(SelectedAuthor);
        }

        private void Save(object param)
        {
            foreach (var author in Authors)
            {
                if (string.IsNullOrWhiteSpace(author.FirstName) || string.IsNullOrWhiteSpace(author.LastName))
                {
                    MessageBox.Show("У всех авторов должны быть заполнены Имя и Фамилия.",
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