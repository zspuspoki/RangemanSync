using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman.Views.Coordinates
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CoordinatesPage : ContentPage
    {
        private readonly ILogger<CoordinatesPage> logger;

        public CoordinatesPage(ILogger<CoordinatesPage> logger)
        {
            InitializeComponent();
            this.logger = logger;
        }

        private async void OpenButton_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    if (result.FileName.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = await result.OpenReadAsync();
                        using StreamReader streamReader = new StreamReader(stream);
                        var jsonText = streamReader.ReadToEnd();
                        var coordinateInfos = JsonConvert.DeserializeObject<IList<CoordinateInfo>>(jsonText);
                        var coordinateCollection = ((CoordinatesViewModel)BindingContext).CoordinateInfoCollection;

                        coordinateCollection.Clear();

                        foreach (var coordinateInfo in coordinateInfos)
                        {
                            coordinateCollection.Add(coordinateInfo);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Please choose a valid Coordinates .xml file", "Cancel");
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Error using OpenButton_Clicked in CoordinatesPage");
                await DisplayAlert("Error", "An error occured during loading the file. Is that a valid a coordinates file?", "Cancel");
            }
        }
    }
}