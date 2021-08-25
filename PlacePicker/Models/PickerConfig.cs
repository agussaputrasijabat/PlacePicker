using System;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace PlacePicker.Models
{
    public class PickerConfig
    {
        public Location InitialLocation { get; set; }

        public string ConfirmText { get; set; }

        public string CancelText { get; set; }

        public Color ConfirmBackgroundColor { get; set; }

        public Color ConfirmTextColor { get; set; }

        public int ButtonCornerRadius { get; set; }
    }
}
