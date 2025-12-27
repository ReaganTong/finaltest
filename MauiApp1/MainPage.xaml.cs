using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using SQLite;
using System.Collections.ObjectModel;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private SQLiteAsyncConnection _database;

        // This collection allows the UI to update automatically when we add items
        public ObservableCollection<TripData> TripsCollection { get; set; } = new ObservableCollection<TripData>();

        public MainPage()
        {
            InitializeComponent();

            // Connect the UI List to our data collection
            TripList.ItemsSource = TripsCollection;

            InitializeDatabase();
            LoadDeviceData();
        }

        private async void InitializeDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tracking_system.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<TripData>();

            // Load existing history immediately
            LoadHistory();
        }

        private async void LoadHistory()
        {
            // Get all items from database, newest first
            var trips = await _database.Table<TripData>().OrderByDescending(t => t.Timestamp).ToListAsync();

            TripsCollection.Clear();
            foreach (var trip in trips)
            {
                TripsCollection.Add(trip);
            }
        }

        private async void LoadDeviceData()
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            NetLabel.Text = accessType == NetworkAccess.Internet ? "Connected" : "Offline";
            NetLabel.TextColor = accessType == NetworkAccess.Internet ? Colors.Green : Colors.Red;

            try
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
                if (location != null)
                {
                    LatLabel.Text = location.Latitude.ToString("F5");
                    LonLabel.Text = location.Longitude.ToString("F5");
                }
            }
            catch (Exception)
            {
                LatLabel.Text = "GPS Error";
                LonLabel.Text = "GPS Error";
            }
        }

        private async void OnSaveTripClicked(object sender, EventArgs e)
        {
            string tripId = TripIdEntry.Text;

            // Validation
            if (string.IsNullOrWhiteSpace(tripId))
            {
                ErrorLabel.Text = "Trip ID cannot be empty.";
                ErrorLabel.IsVisible = true;
                return;
            }

            // Create Data Object
            var newTrip = new TripData
            {
                TripId = tripId,
                Latitude = double.Parse(LatLabel.Text == "GPS Error" || LatLabel.Text == "Loading..." ? "0" : LatLabel.Text),
                Longitude = double.Parse(LonLabel.Text == "GPS Error" || LonLabel.Text == "Loading..." ? "0" : LonLabel.Text),
                Timestamp = DateTime.Now
            };

            // Save to Database
            await _database.InsertAsync(newTrip);

            // Update UI
            ErrorLabel.IsVisible = false;
            TripIdEntry.Text = string.Empty;

            // Refresh the list to show the new item
            LoadHistory();

            await DisplayAlert("Success", "Trip saved and history updated!", "OK");
        }
    }
}