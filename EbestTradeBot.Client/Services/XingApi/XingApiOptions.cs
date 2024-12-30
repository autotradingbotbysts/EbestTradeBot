using Prism.Mvvm;

namespace EbestTradeBot.Client.Services.XingApi
{
    public class XingApiOptions : BindableBase
    {
        private string _xingApiId = string.Empty;
        public string XingApiId
        {
            get => _xingApiId;
            set => SetProperty(ref _xingApiId, value);
        }

        private string _xingApiPassword = string.Empty;
        public string XingApiPassword
        {
            get => _xingApiPassword;
            set => SetProperty(ref _xingApiPassword, value);
        }

        private string _certificationPassword = string.Empty;
        public string CertificationPassword
        {
            get => _certificationPassword;
            set => SetProperty(ref _certificationPassword, value);
        }

        private string _acfFilePath = string.Empty;
        public string AcfFilePath
        {
            get => _acfFilePath;
            set => SetProperty(ref _acfFilePath, value);
        }
    }
}
