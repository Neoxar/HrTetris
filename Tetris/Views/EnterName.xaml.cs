using System.Windows;
using HrTetris.ViewModel;

namespace HrTetris.Views
{
    /// <summary>
    /// Interaction logic for EnterName.xaml
    /// </summary>
    public partial class EnterName : Window
    {
        public string EnteredName;

        public EnterName()
        {
            InitializeComponent();
            DataContext = new EnterNameViewModel();
            Closing += EnterName_Closing;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void EnterName_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EnterNameViewModel enterNameViewModel = (EnterNameViewModel)DataContext;
            EnteredName = enterNameViewModel.Name;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
