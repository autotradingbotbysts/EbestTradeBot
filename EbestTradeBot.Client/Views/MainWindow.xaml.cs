using EbestTradeBot.Client.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace EbestTradeBot.Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MainWindowViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = (MainWindowViewModel)DataContext;
        }

        private void Cofiguration_Clicked(object sender, RoutedEventArgs e)
        {
            if (_vm.IsRun)
            {
                MessageBox.Show("매매 진행중엔 설정 창을 열 수 없습니다", "오류");
                return;
            }

            var configurationWindow = new ConfigurationWindow();
            configurationWindow.ShowDialog();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
