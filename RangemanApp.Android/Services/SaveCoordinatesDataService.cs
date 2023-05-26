using Android.Content;
using Rangeman;
using Xamarin.Forms.Platform.Android;

namespace RangemanSync.Android.Services
{
    public class SaveCoordinatesDataService : ISaveCoordinatesDataService
    {
        private readonly FormsAppCompatActivity activity;

        public SaveCoordinatesDataService(FormsAppCompatActivity activity)
        {
            this.activity = activity;
        }

        public void SaveCoordinatesData(string fileName)
        {
            Intent intentCreate = new Intent(Intent.ActionCreateDocument);
            intentCreate.AddCategory(Intent.CategoryOpenable);
            intentCreate.SetType("application/xml");
            intentCreate.PutExtra(Intent.ExtraTitle, fileName);
            activity.StartActivityForResult(intentCreate, ActivityRequestCode.SaveCoordinatesData);
        }
    }
}