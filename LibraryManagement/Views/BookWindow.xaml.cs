using System.Windows;

namespace LibraryManagement.Views
{
    public partial class BookWindow : Window
    {
        public BookWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Просто закрываем окно с результатом true
            DialogResult = true;
            Close();
        }
    }
}