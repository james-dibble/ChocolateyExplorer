using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChocolateyExplorer.WPF.ViewModel;
using Microsoft.Win32;

namespace ChocolateyExplorer.WPF.Views
{
    /// <summary>
    /// Interaction logic for InstallList.xaml
    /// </summary>
    public partial class InstallList : Page
    {
        public InstallList()
        {
            InitializeComponent();
        }
        
        private void SaveSetupScriptClick(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as InstallListViewModel;

            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "ps1",
                Filter = "PowerShell Script|*.ps1",
                Title = "Save Setup Script"
            };

            if (saveFileDialog.ShowDialog().Value)
            {
                viewModel.SetupScriptSaveLocation = saveFileDialog.FileName;
            }
            else
            {
                viewModel.SetupScriptSaveLocation = null;
            }
        }
    }
}
