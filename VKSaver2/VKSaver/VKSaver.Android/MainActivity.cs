using Android.App;
using Android.Widget;
using Android.OS;
using VKSaver.Core.FileSystem;

namespace VKSaver.Android
{
    [Activity(Label = "VKSaver.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var folder = new AndroidFolder(appDataPath);
            var file = await folder.CreateFileAsync("test.txt");
        }
    }
}

