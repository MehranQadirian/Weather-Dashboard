namespace Weather.Dashboard.Avalonia.Models
{
    public class City
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public City(string name, string country, double lat, double lon)
        {
            Id = $"{name}_{country}".ToLower().Replace(" ", "_");
            Name = name;
            Country = country;
            Lat = lat;
            Lon = lon;
        }
    }
}