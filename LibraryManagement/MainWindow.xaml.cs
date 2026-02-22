using System.Windows;
using LibraryManagement.ViewModels;

namespace LibraryManagement
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Устанавливаем DataContext (главная модель)
            DataContext = new MainViewModel();
        }

        // Обработчик кнопки "Сбросить"
        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SearchText = "";
                viewModel.SelectedAuthorFilter = null;
                viewModel.SelectedGenreFilter = null;
            }
        }
    }
}