using SQLite;

namespace MauiApp1
{
    public class TripData
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string TripId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}