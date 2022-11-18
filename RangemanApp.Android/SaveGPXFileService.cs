using Android.Content;
using Rangeman;
using Xamarin.Forms.Platform.Android;

namespace employeeID.Droid
{
    public class SaveGPXFileService : ISaveGPXFileService
    {
        private readonly FormsAppCompatActivity activity;

        public SaveGPXFileService(FormsAppCompatActivity activity)
        {
            this.activity = activity;
        }

        public void SaveGPXFile(string fileName)
        {
            Intent intentCreate = new Intent(Intent.ActionCreateDocument);
            intentCreate.AddCategory(Intent.CategoryOpenable);
            intentCreate.SetType("application/gpx+xml");
            intentCreate.PutExtra(Intent.ExtraTitle, fileName);
            activity.StartActivityForResult(intentCreate, ActivityRequestCode.SaveGPXFile);
        }
    }
}