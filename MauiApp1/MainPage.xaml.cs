using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using SQLite;
using System.Collections.ObjectModel;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private SQLiteAsyncConnection _database;
        public ObservableCollection<TripData> TripsCollection { get; set; } = new ObservableCollection<TripData>();

        public MainPage()
        {
            InitializeComponent();
            TripList.ItemsSource = TripsCollection;
            InitializeDatabase();
            LoadDeviceData();
        }

        private async void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tracking.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<TripData>();
            LoadHistory();
        }

        private async void LoadHistory()
        {
            var trips = await _database.Table<TripData>().OrderByDescending(t => t.Timestamp).ToListAsync();
            TripsCollection.Clear();
            foreach (var trip in trips) TripsCollection.Add(trip);
        }

        private async void LoadDeviceData()
        {
           
            NetLabel.Text = Connectivity.Current.NetworkAccess == NetworkAccess.Internet ? "Online" : "Offline";

            
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
                if (location != null)
                {
                    LatLabel.Text = location.Latitude.ToString("F4");
                    LonLabel.Text = location.Longitude.ToString("F4");
                }
            }
            catch { LatLabel.Text = "Error"; LonLabel.Text = "Error"; }
        }

        private async void OnSaveTripClicked(object sender, EventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(TripIdEntry.Text))
            {
                ErrorLabel.Text = "Error: Trip ID is required.";
                ErrorLabel.IsVisible = true;
                return;
            }

            var newTrip = new TripData
            {
                TripId = TripIdEntry.Text,
                Latitude = double.Parse(LatLabel.Text == "Loading..." ? "0" : LatLabel.Text),
                Longitude = double.Parse(LonLabel.Text == "Loading..." ? "0" : LonLabel.Text),
                Timestamp = DateTime.Now
            };

            await _database.InsertAsync(newTrip);
            ErrorLabel.IsVisible = false;
            TripIdEntry.Text = string.Empty;
            LoadHistory();
            await DisplayAlert("Saved", "Data stored in SQLite", "OK");
        }
    }
}