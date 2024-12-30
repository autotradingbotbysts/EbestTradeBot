using EbestTradeBot.Client.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace EbestTradeBot.Client.Views
{
    /// <summary>
    /// ConfigurationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigurationWindow : MetroWindow
    {
        private ConfigurationViewModel _viewModel;

        public ConfigurationWindow()
        {
            InitializeComponent();

            _viewModel = (ConfigurationViewModel)DataContext;
            _viewModel.CloseWindowAction = Close;
            
            Password.Password = _viewModel.XingApiOptions.XingApiPassword;
            CertificationPassword.Password = _viewModel.XingApiOptions.CertificationPassword;
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;

            switch (passwordBox.Name)
            {
                case "Password":
                    _viewModel.XingApiOptions.XingApiPassword = passwordBox.Password;
                    break;
                case "CertificationPassword":
                    _viewModel.XingApiOptions.CertificationPassword = passwordBox.Password;
                    break;
                    
            }
        }
    }
}
