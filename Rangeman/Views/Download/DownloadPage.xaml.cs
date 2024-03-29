﻿using Microsoft.Extensions.Logging;
using Rangeman.Views.Download;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadPage : ContentPage, IDownloadPageView
    {
        private readonly ILogger<DownloadPage> logger;

        public DownloadPage(ILogger<DownloadPage> logger)
        {
            InitializeComponent();

            this.logger = logger;
            logger.LogInformation("MainPage instatiated");

            InitProgressLabel();
        }

        private void InitProgressLabel()
        {
            lblProgress.GestureRecognizers.Add(new TapGestureRecognizer
            {
                //TODO: Move this command to the viewmodel
                Command = new Command(() =>
                {
                    ViewModel.ProgressMessage = "";
                })
            });
        }

        private void LogHeadersList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is LogHeaderViewModel selectedLogHeader)
            {
                ViewModel.SelectedLogHeader = selectedLogHeader;
            }
        }

        public DownloadPageViewModel ViewModel
        {
            get
            {
                var vm = BindingContext as DownloadPageViewModel;
                return vm;
            }
        }

    }
}