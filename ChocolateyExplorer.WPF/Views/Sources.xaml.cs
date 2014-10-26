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

namespace ChocolateyExplorer.WPF.Views
{
    /// <summary>
    /// Interaction logic for Sources.xaml
    /// </summary>
    public partial class Sources : UserControl
    {
        private bool _sourcesLoaded;

        public Sources()
        {
            this.InitializeComponent();

            this._sourcesLoaded = false;

            this.Loaded += Sources_Loaded;
        }

        void Sources_Loaded(object sender, RoutedEventArgs e)
        {
            if(this._sourcesLoaded)
            {
                return;
            }

            var context = this.DataContext as ChocolateySourcesViewModel;

            context.PopulateSources();

            this._sourcesLoaded = true;
        }
    }
}
