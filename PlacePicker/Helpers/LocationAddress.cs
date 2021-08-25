using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;

namespace PlacePicker.Helpers
{
    [Preserve(AllMembers = true)]
    public class LocationAddress
    {
        public static string GetLocationString(Placemark placemark)
        {
            string address;
            if (placemark != null)
            {
                var array = new[]
                {
                    $"{(placemark.FeatureName == placemark.Thoroughfare ? placemark.FeatureName : $"{placemark.FeatureName} {placemark.Thoroughfare}")}".Trim(),
                    placemark.SubLocality,
                    placemark.Locality,
                    placemark.SubAdminArea,
                    placemark.AdminArea,
                    placemark.PostalCode,
                    placemark.CountryName
                };

                array = array.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                address = string.Join(", ", array.Where(s => !string.IsNullOrWhiteSpace(s)));

                return address;
            }
            return string.Empty;
        }
    }
}
