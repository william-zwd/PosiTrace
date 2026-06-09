namespace PosiTrace
{
    public class geocodingOuter
    {
        public string version { get; set; }
        public string attribution { get; set; }
        public string licence { get; set; }
        public string query { get; set; }
    }

    public class Admin
    {
        public string level9 { get; set; }
        public string level8 { get; set; }
        public string level6 { get; set; }
        public string level4 { get; set; }
    }

    public class geocodingInner
    {
        public ulong place_id { get; set; }
        public string osm_type { get; set; }
        public ulong osm_id { get; set; }
        public string osm_key { get; set; }
        public string osm_value { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public string name { get; set; }
        public string housenumber { get; set; }
        public string postcode { get; set; }
        public string street { get; set; }
        public string district { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public Admin admin { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public double[] coordinates { get; set; }
    }

    public class Properties
    {
        public geocodingInner geocoding { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public Properties properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class GEOCode
    {
        public string type { get; set; }
        public geocodingOuter geocoding { get; set; }
        public Feature[] features { get; set; }
    }
}
