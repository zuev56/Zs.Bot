using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyTestWebInterface.Models
{
    public class CTeleportApiViewModel
    {
        [Display(Name = "A1")]
        public string Airport1Code { get; set; }
        public string Airport2Code { get; set; }

        [Display(Name = "Miles")]
        public float DistanceInMiles { get; set; }

        [Display(Name = "Kilometers")]
        public float DistanceInKiloMeters { get; set; }

    }

    public struct Location
    {
        [JsonPropertyName("lat")]
        public float Longitude { get; set; }

        [JsonPropertyName("lon")]
        public float Latitude { get; set; }
    }

    public class Airport
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("iata")]
        public string IATA { get; set; }

        [JsonPropertyName("city_iata")]
        public string CityIATA { get; set; }

        [JsonPropertyName("country_iata")]
        public string CountryIATA { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("timezone_region_name")]
        public string TimezoneRegionName { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("hubs")]
        public int Hubs { get; set; }
    }
}
