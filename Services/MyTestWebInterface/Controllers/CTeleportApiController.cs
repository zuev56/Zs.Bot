using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyTestWebInterface.Models;

namespace MyTestWebInterface.Controllers
{
    public class CTeleportApiController : Controller
    {
        private CTeleportApiViewModel _viewModel;
        private static readonly HttpClient _httpClient = new HttpClient();

        public CTeleportApiController()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _viewModel = new CTeleportApiViewModel();
        }


        public async Task<IActionResult> Index(string airport1Code, string airport2Code)
        {
            _viewModel.Airport1Code = airport1Code?.ToUpperInvariant();
            _viewModel.Airport2Code = airport2Code?.ToUpperInvariant();

            if (_viewModel.Airport1Code is {} && _viewModel.Airport2Code is {})
            {
                _viewModel.DistanceInKiloMeters = await GetDistanceInMeters() / 1000;
                _viewModel.DistanceInMiles = (_viewModel.DistanceInKiloMeters * 0.621371f);
            }

            return View(_viewModel);
        }

        private async Task<float> GetDistanceInMeters()
        {
            if (_viewModel.Airport1Code is null || _viewModel.Airport2Code is null)
                return 0;

            var airport1 = await JsonSerializer.DeserializeAsync<Airport>(
                await _httpClient.GetStreamAsync($"https://places-dev.cteleport.com/airports/{_viewModel.Airport1Code}")
                );
            var airport2 = await JsonSerializer.DeserializeAsync<Airport>(
                await _httpClient.GetStreamAsync($"https://places-dev.cteleport.com/airports/{_viewModel.Airport2Code}")
                );

            return GetDistance(airport1.Location, airport2.Location);
        }

        private float GetDistance(Location location1, Location location2)
        {
            //var p = 0.017453292519943295;    // Math.PI / 180
            //var a = 0.5 - Math.Cos((location2.Latitude - location1.Latitude) * p)/2 +
            //        Math.Cos(location1.Latitude * p) * Math.Cos(location2.Latitude * p) *
            //        (1 - Math.Cos((location2.Longitude - location1.Longitude) * p))/2;
            //
            //return (float)(12742 * Math.Asin(Math.Sqrt(a)) * 1000); // 2 * R; R = 6371 km
            double earthRadius = 6371000; //meters == 3958.75 miles
            double dLat = ToRadians(location2.Latitude - location1.Latitude);
            double dLon = ToRadians(location2.Longitude - location1.Longitude);
            double a = Math.Sin(dLat / 2) 
                       * Math.Sin(dLat / 2)
                     + Math.Cos(ToRadians(location1.Latitude))
                       * Math.Cos(ToRadians(location2.Latitude))
                       * Math.Sin(dLon / 2) 
                       * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return (float)(earthRadius * c);
        }

        private double ToRadians(double degrees)
            => (Math.PI / 180) * degrees;

    }
}
