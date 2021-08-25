using System;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace PlacePicker.Models
{
    public class PickerConfig
    {
        public Location InitialLocation { get; set; }

        public string ConfirmText { get; set; } = "Confirm Location";

        public string CancelText { get; set; } = "Cancel";

        public Color ConfirmBackgroundColor { get; set; } = Color.FromHex("#1AA260");

        public Color ConfirmTextColor { get; set; } = Color.White;

        public int ButtonCornerRadius { get; set; } = 10;
    }
}
