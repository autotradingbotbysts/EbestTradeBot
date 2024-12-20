using EbestTradeBot.Client.Services.OpenApi;
using EbestTradeBot.Client.Services.XingApi;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace EbestTradeBot.Client.ViewModels
{
    public class ConfigurationViewModel : BindableBase
    {
        private readonly IOptionsMonitor<XingApiOptions> _xingApiOptionsMonitor;
        private readonly IOptionsMonitor<OpenApiOptions> _openApiOptionsMonitor;
        private readonly IOptionsMonitor<DefaultOptions> _defaultOptionsMonitor;

        public XingApiOptions XingApiOptions => _xingApiOptionsMonitor.CurrentValue;
        public OpenApiOptions OpenApiOptions => _openApiOptionsMonitor.CurrentValue;
        public DefaultOptions DefaultOptions => _defaultOptionsMonitor.CurrentValue;

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand LoginXingApiCommand { get; private set; }
        public DelegateCommand FileDialogCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public Action? CloseWindowAction { get; set; }

        #region Constructor
        public ConfigurationViewModel(
            IOptionsMonitor<XingApiOptions> xingApiOptionsMonitor,
            IOptionsMonitor<OpenApiOptions> openApiOptionsMonitor,
            IOptionsMonitor<DefaultOptions> defaultOptionsMonitor
            )
        {
            _xingApiOptionsMonitor = xingApiOptionsMonitor;
            _openApiOptionsMonitor = openApiOptionsMonitor;
            _defaultOptionsMonitor = defaultOptionsMonitor;

            SaveCommand = new DelegateCommand(ExecuteSaveCommand);
            LoginXingApiCommand = new DelegateCommand(ExecuteLoginXingApiCommand);
            FileDialogCommand = new DelegateCommand(ExecuteFileDialogCommand);
            CancelCommand = new DelegateCommand(ExecuteCancelCommand);
        }

        #endregion

        #region DelegateCommand Execute

        private void ExecuteFileDialogCommand()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "ACF files (*.acf)|*.acf|All files (*.*)|*.*";
            bool result = ofd.ShowDialog() ?? false;
            if (result)
            {
                XingApiOptions.AcfFilePath = ofd.FileName;
            }
        }

        private void ExecuteLoginXingApiCommand()
        {

        }

        private void ExecuteCancelCommand()
        {
            CloseWindowAction?.Invoke();
        }

        private void ExecuteSaveCommand()
        {
            try
            {
                var configData = new
                {
                    XingApiOptions,
                    OpenApiOptions,
                    DefaultOptions
                };

                string jsonData = JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText("appsettings.json", jsonData);
                CloseWindowAction?.Invoke();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        #endregion
    }
}
