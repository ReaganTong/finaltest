using SQLite;

public class TripData
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string TripId { get; set; } // User-defined ID
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}