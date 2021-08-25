using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PlacePicker.Helpers;
using PlacePicker.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace PlacePicker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Preserve(AllMembers = true)]
    public partial class LocationPicker : ContentPage, INotifyPropertyChanged
    {
        public static string PinImage { get; set; }

        private TaskCompletionSource<Place> _taskCompletionSource;

        public static Place Place;

        public bool IsCancelled = false;

        public PickerConfig Config { get; private set; }

        public new event PropertyChangedEventHandler PropertyChanged;

        public LocationPicker(PickerConfig config = null)
        {
            InitializeComponent();

            if (config == null) config = new PickerConfig();

            Place = new Place();
            LocationMap.UiSettings.ZoomGesturesEnabled = true;
            LocationMap.UiSettings.ScrollGesturesEnabled = true;
            pin.Source = PinImage;
            LocationMap.CameraIdled += LocationMap_CameraIdled;
            BindingContext = this;

            Config = config;
            OnPropertyChanged(nameof(Config));
        }

        protected override bool OnBackButtonPressed()
        {
            Handle_CancelTapped(this, new EventArgs());
            return true;
        }

        async private void LocationMap_CameraIdled(object sender, CameraIdledEventArgs e)
        {
            var pos = e.Position.Target;
            var loc = new Location(pos.Latitude, pos.Longitude);
            var x = await GetUserLocation.GetAddress(loc);
            if (x?.Data != null)
            {
                Place.Location = loc;
                Place.Placemark = x.Data;
                Place.LocationAddress = LocationAddress.GetLocationString(x.Data);
                CurrentLocationText.Text = LocationAddress.GetLocationString(x.Data);
            }
            else
            {
                Place.Location = loc;
                Place.Placemark = null;
                Place.LocationAddress = "Unnamed";
                CurrentLocationText.Text = "Unnamed";
            }
        }

        async private Task UpdateCamera()
        {
            var lastLocation = Config.InitialLocation;
            if (lastLocation != null)
            {
                var lastPosition = new Position(lastLocation.Latitude, lastLocation.Longitude);
                LocationMap.InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(lastPosition,15);
            }
            else
            { 
                var request = new GeolocationRequest(GeolocationAccuracy.Low);
                var currentLocation = await Geolocation.GetLocationAsync(request);
                var currentPosition = new Position(currentLocation.Latitude, currentLocation.Longitude);
                LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition, Distance.FromMiles(0.1)));
            }
        }

        public async void Handle_ConfirmClicked(object sender, EventArgs e)
        {
            if (_taskCompletionSource != null)
            {
                _taskCompletionSource.SetResult(Place);
                _taskCompletionSource = null;
                await Navigation.PopModalAsync();
            }
        }

        public async void Handle_CancelTapped(object sender, EventArgs e)
        {
            if (_taskCompletionSource != null)
            {
                IsCancelled = true;
                _taskCompletionSource.SetResult(null);
                _taskCompletionSource = null;
                await Navigation.PopModalAsync();
            }
        }
        
        private Task<Place> GetPlace()
        {
            _taskCompletionSource = new TaskCompletionSource<Place>();
            return _taskCompletionSource.Task;
        }

        async public static Task<BaseModel<Place>> SelectPlace(PickerConfig config = null)
        {
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            var currentNav = Application.Current.MainPage.Navigation;
            if (currentNav != null)
            {
                var status = await GetUserLocation.CheckPermissions();
                var result = new BaseModel<Place>();
                var placePicker = new LocationPicker(config);
                
                if (status == Status.Success)
                {
                    placePicker.LocationMap.MyLocationEnabled = true;
                    placePicker.LocationMap.UiSettings.MyLocationButtonEnabled = true;

                    if (currentNav.ModalStack.Where(x => x is LocationPicker).Count() == 0)
                    {
                        await currentNav.PushModalAsync(placePicker);
                        await placePicker.UpdateCamera();

                        var selectedPlace = await placePicker.GetPlace();
                        result.Data = selectedPlace;
                        result.Status = !placePicker.IsCancelled ? Status.Success : Status.Cancelled;
                    }
                    else
                        return null;
                }
                else
                {
                    result.Status = status;
                }
                return result;
            }
            else
            {
                return null;
            }
        }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
