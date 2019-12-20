using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System.Threading;
using Plugin.Clipboard;
using System.Collections.Generic;
using System.Collections;
using Plugin.Messaging;
using Plugin.Settings;
using Android.Telephony;
using Android.Content;

namespace GpsTryForms
{
    public partial class MainPage : ContentPage
    {
        public bool CoordsAreAvailable = false;
        
        string CustomRecipient;
        public double start_x, start_y, diff_x;
        public bool IsDialog = false;
        bool MainStackValidPan = false;
        int IsHorPan = 0;

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            var panGesture_main = new PanGestureRecognizer();
            panGesture_main.PanUpdated += OnPanUpdated_main;
            MainStack.GestureRecognizers.Add(panGesture_main);

            var panGesture_cover = new PanGestureRecognizer();
            panGesture_cover.PanUpdated += OnPanUpdated_cover;
            CoverMenu.GestureRecognizers.Add(panGesture_cover);
            
            var panGesture_menu = new PanGestureRecognizer();
            panGesture_menu.PanUpdated += OnPanUpdated_menu;
            MenuBox.GestureRecognizers.Add(panGesture_menu);

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) =>
            {
                CloseMenu();
            };
            CoverMenu.GestureRecognizers.Add(tapGestureRecognizer);
            var tapGestureRecognizer2 = new TapGestureRecognizer();
            tapGestureRecognizer2.Tapped += (s, e) =>
            {
                CoverDialog.FadeTo(0, 250, Easing.CubicOut);
                CoverDialog.IsVisible = false;
                DialogGPSSettings.IsVisible = false;
            };
            CoverDialog.GestureRecognizers.Add(tapGestureRecognizer2);
        }

        protected override bool OnBackButtonPressed()
        {
            if (!CoverDialog.IsVisible)
            {
                base.OnBackButtonPressed();
                return true;
            }
            else
            {
                CloseDialogGPSSettings();
                return true;
            }
        }

        protected override void OnAppearing()
        {
            StartListening();
        }

        async Task StartListening()
        {
            if (!CrossGeolocator.Current.IsGeolocationAvailable)
            {
                EmergencySms.IsEnabled = false;
                OptionableSms.IsEnabled = false;
                CopyCoordinates.IsEnabled = false;
                LatitudeLabel.Text = "";
                LongitudeLabel.Text = "Координаты недоступны, т.к. приложению не доступны сервисы GPS";
                LatitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
                LongitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
                CoordsAreAvailable = false;
            }
            else if (!CrossGeolocator.Current.IsGeolocationEnabled)
            {
                EmergencySms.IsEnabled = false;
                OptionableSms.IsEnabled = false;
                CopyCoordinates.IsEnabled = false;
                LatitudeLabel.Text = "";
                LongitudeLabel.Text = "Координаты недоступны, т.к. у Вас не включены сервисы GPS";
                LatitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
                LongitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
                CoordsAreAvailable = false;
                OpenDialogGPSSettings();
            }
            else
            {
                if (CrossGeolocator.Current.IsListening)
                    return;
                try
                {
                    EmergencySms.IsEnabled = true;
                    OptionableSms.IsEnabled = true;
                    CopyCoordinates.IsEnabled = true;
                    var position = await CrossGeolocator.Current.GetLastKnownLocationAsync();
                    LatitudeLabel.Text = position.Latitude.ToString();
                    LongitudeLabel.Text = position.Longitude.ToString();
                    LatitudeLabel.TextColor = Xamarin.Forms.Color.Orange;
                    LongitudeLabel.TextColor = Xamarin.Forms.Color.Orange;
                    CoordsAreAvailable = true;
                }
                catch(Exception e)
                {
                    LatitudeLabel.Text = "Подождите...";
                    LongitudeLabel.Text = "Подождите...";
                    LatitudeLabel.TextColor = Xamarin.Forms.Color.Orange;
                    LongitudeLabel.TextColor = Xamarin.Forms.Color.Orange;
                }
            }

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true);
            CrossGeolocator.Current.PositionChanged += PositionChanged;
            CrossGeolocator.Current.PositionError += PositionError;
        }

        private void StartActivity(Intent intent)
        {
            throw new NotImplementedException();
        }

        private void PositionChanged(object sender, PositionEventArgs e)
        {
            var position = e.Position;
            EmergencySms.IsEnabled = true;
            OptionableSms.IsEnabled = true;
            CopyCoordinates.IsEnabled = true;
            LatitudeLabel.Text = String.Format("{0:f7}", position.Latitude);
            LongitudeLabel.Text = String.Format("{0:f7}", position.Longitude);
            LatitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#512da8");
            LongitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#512da8");
            CoordsAreAvailable = true;
        }

        private void PositionError(object sender, PositionErrorEventArgs e)
        {
            if (!CrossGeolocator.Current.IsGeolocationAvailable)
            {
                EmergencySms.IsEnabled = false;
                OptionableSms.IsEnabled = false;
                CopyCoordinates.IsEnabled = false;
                LatitudeLabel.Text = "Координаты недоступны, т.к.";
                LongitudeLabel.Text = "приложению не доступны сервисы GPS";
                LatitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
                LongitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
            }
            else if (!CrossGeolocator.Current.IsGeolocationEnabled)
            {
                EmergencySms.IsEnabled = false;
                OptionableSms.IsEnabled = false;
                CopyCoordinates.IsEnabled = false;
                LatitudeLabel.Text = "Координаты недоступны, т.к.";
                LongitudeLabel.Text = "у Вас не включены сервисы GPS";
                LatitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
                LongitudeLabel.TextColor = Xamarin.Forms.Color.FromHex("#ec407a");
            }
        }

        async Task StopListening()
        {
            if (!CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StopListeningAsync();

            CrossGeolocator.Current.PositionChanged -= PositionChanged;
            CrossGeolocator.Current.PositionError -= PositionError;
        }

        public async void SendEmergencySms(object sender, EventArgs e)
        {
            if (Device.OS == TargetPlatform.Android)
            {
                if (CoordsAreAvailable)
                {
                    string _latitude_text = LatitudeLabel.Text;
                    string _longitude_text = LongitudeLabel.Text;
                    string sendText = "Нужна помощь, мои координаты: широта=" + _latitude_text + " долгота=" + _longitude_text;
                    string emergency_recipient = CrossSettings.Current.GetValueOrDefault("EmergencyRecipient", "112");
                    SmsManager.Default.SendTextMessage(emergency_recipient, null, sendText, null, null);
                    SendLabel.FadeTo(1, 250, Easing.CubicOut);
                    SendLabel.IsVisible = true;
                    await Task.Delay(500);
                    await SendLabel.FadeTo(0, 1500, Easing.CubicOut);
                    SendLabel.IsVisible = false;
                }
            }
        }

        public void SendOptionableSms(object sender, EventArgs e)
        {
            if (CoordsAreAvailable)
            {
                string _latitude_text = LatitudeLabel.Text;
                string _longitude_text = LongitudeLabel.Text;
                string sendText = "Нужна помощь, мои координаты: широта=" + _latitude_text + " долгота=" + _longitude_text;
                var smsMessenger = CrossMessaging.Current.SmsMessenger;
                CustomRecipient = CrossSettings.Current.GetValueOrDefault("CustomRecipient", "112");
                if (smsMessenger.CanSendSms)
                {
                    smsMessenger.SendSms(CustomRecipient, sendText);
                }
            }
        }

        public async void CopyCoords(object sender, EventArgs e)
        {
            if (CoordsAreAvailable)
            {
                string _latitude_text = LatitudeLabel.Text;
                string _longitude_text = LongitudeLabel.Text;
                string sendText = "Широта: " + _latitude_text + ", долгота: " + _longitude_text;
                CrossClipboard.Current.SetText(sendText);
                SavedLabel.FadeTo(1, 250, Easing.CubicOut);
                SavedLabel.IsVisible = true;
                await Task.Delay(500);
                await SavedLabel.FadeTo(0, 1500, Easing.CubicOut);
                SavedLabel.IsVisible = false;
            }
        }

        public void OpenMenu_button(object sender, EventArgs e)
        {
            OpenMenu();            
        }

        public void OpenMenu()
        {
            CoverMenu.IsVisible = true;
            CoverMenu.FadeTo(1, 250, Easing.CubicOut);
            MenuBox.TranslateTo(0, 0, 250, Easing.CubicOut);
        }

        public void CloseMenu_button(object sender, EventArgs e)
        {
            CloseMenu();
        }

        public void CloseMenu()
        {
            CoverMenu.FadeTo(0, 250, Easing.CubicOut);
            MenuBox.TranslateTo(-MenuBox.Width, 0, 250, Easing.CubicOut);
            CoverMenu.IsVisible = false;
        }

        

        public async void OnOptionsPageAsync(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OptionsPage());
        }

        public async void OnHelpPageAsync(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HelpPage());
        }

        public void OnSwiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Left:
                    CloseMenu();
                    break;
                case SwipeDirection.Right:
                    OpenMenu();
                    break;
                case SwipeDirection.Up:
                    // Handle the swipe
                    break;
                case SwipeDirection.Down:
                    // Handle the swipe
                    break;
            }
        }

        public void OnPanUpdated_main(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    MainStackValidPan = true;
                    MenuBox.TranslationX = -MenuBox.Width;
                    start_x = e.TotalX;
                    CoverMenu.IsVisible = true;
                    break;

                case GestureStatus.Running:
                    if (Math.Abs(e.TotalX) > Math.Abs(e.TotalY) && IsHorPan==0)
                    {
                        IsHorPan = 1;
                    }
                    else if (IsHorPan == 0)
                    {
                        IsHorPan = 2;
                    }
                    if (IsHorPan == 1)
                    {
                        diff_x = e.TotalX - start_x;
                        if ((MenuBox.TranslationX < 0 && diff_x > 0) || (MenuBox.TranslationX > -MenuBox.Width && diff_x < 0))
                        {
                            MenuBox.TranslationX = MenuBox.TranslationX + diff_x;
                        }
                        else if (MenuBox.TranslationX >= 0 && diff_x > 0)
                        {
                            MenuBox.TranslationX = 0;
                        }
                        else if (MenuBox.TranslationX < -MenuBox.Width && diff_x < 0)
                        {
                            MenuBox.TranslationX = -MenuBox.Width;
                        }
                        start_x = e.TotalX;
                        CoverMenu.Opacity = 1 - (MenuBox.TranslationX / -MenuBox.Width);
                    }
                    else if (IsHorPan == 2)
                    {
                        double ms_content_height = LatitudeText.Height + LatitudeText.Margin.VerticalThickness +
                            LongitudeText.Height + LongitudeText.Margin.VerticalThickness +
                            LatitudeLabel.Height + LatitudeLabel.Margin.VerticalThickness +
                            LongitudeLabel.Height + LongitudeLabel.Margin.VerticalThickness +
                            EmergencySms.Height + EmergencySms.Margin.VerticalThickness +
                            OptionableSms.Height + OptionableSms.Margin.VerticalThickness + 90;
                        double ms_height = MainStack.Height;
                        if (ms_content_height > ms_height && MainStack.TranslationY <= 0 && MainStack.TranslationY >= -(ms_content_height - ms_height))
                        {
                            MainStack.TranslationY = MainStack.TranslationY + e.TotalY;
                        }
                        if (MainStack.TranslationY > 0)
                        {
                            MainStack.TranslationY = 0;
                        }
                        else if (MainStack.TranslationY < -(ms_content_height - ms_height) && ms_content_height > ms_height)
                        {
                            MainStack.TranslationY = -(ms_content_height - ms_height);
                        }
                    }
                    break;

                case GestureStatus.Completed:
                    if (MenuBox.TranslationX > -MenuBox.Width * 0.7)
                    {
                        OpenMenu();
                    }
                    else
                    {
                        CloseMenu();
                    }
                    IsHorPan = 0;
                    break;
            }
        }

        public void OnPanUpdated_cover(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    MainStackValidPan = true;
                    if (MenuBox.TranslationX < -MenuBox.Width)
                    {
                        MenuBox.TranslationX = -MenuBox.Width;
                    }
                    start_x = e.TotalX;
                    CoverMenu.IsVisible = true;
                    break;

                case GestureStatus.Running:
                    diff_x = e.TotalX - start_x;
                    if ((MenuBox.TranslationX < 0 && diff_x > 0) || (MenuBox.TranslationX > -MenuBox.Width && diff_x < 0))
                    {
                        MenuBox.TranslationX = MenuBox.TranslationX + diff_x;
                    }
                    else if (MenuBox.TranslationX >= 0 && diff_x > 0)
                    {
                        MenuBox.TranslationX = 0;
                    }
                    else if (MenuBox.TranslationX < -MenuBox.Width && diff_x < 0)
                    {
                        MenuBox.TranslationX = -MenuBox.Width;
                    }
                    if (MenuBox.TranslationX > 0)
                    {
                        MenuBox.TranslationX = 0;
                    }
                    start_x = e.TotalX;
                    CoverMenu.Opacity = 1 - (MenuBox.TranslationX / -MenuBox.Width);
                    break;

                case GestureStatus.Completed:
                    if (MenuBox.TranslationX > -MenuBox.Width * 0.3)
                    {
                        OpenMenu();
                    }
                    else
                    {
                        CloseMenu();
                    }
                    break;
            }
        }

        public void OnPanUpdated_menu(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    start_x = MenuBox.TranslationX;
                    CoverMenu.IsVisible = true;
                    break;
                case GestureStatus.Running:
                    if (e.TotalX > -MenuBox.Width && e.TotalX < 0)
                    {
                        MenuBox.TranslationX = MenuBox.TranslationX + e.TotalX;
                    }
                    else if (e.TotalX > 0)
                    {
                        MenuBox.TranslationX = MenuBox.TranslationX + e.TotalX;
                        if (MenuBox.TranslationX > 0)
                        {
                            MenuBox.TranslationX = 0;
                        }
                    }
                    CoverMenu.Opacity = 1 - (MenuBox.TranslationX / -MenuBox.Width);
                    break;
                case GestureStatus.Completed:
                    if (MenuBox.TranslationX > -MenuBox.Width * 0.3)
                    {
                        OpenMenu();
                    }
                    else
                    {
                        CloseMenu();
                    }
                    break;
            }
        }

        public void OpenDialogGPSSettings()
        {
            CoverDialog.IsVisible = true;
            CoverDialog.FadeTo(1, 250, Easing.CubicOut);
            DialogGPSSettings.IsVisible = true;
            IsDialog = true;
        }

        public void CloseDialogGPSSettings_button(object sender, PanUpdatedEventArgs e)
        {
            CloseDialogGPSSettings();
        }

        public void CloseDialogGPSSettings()
        {
            CoverDialog.FadeTo(0, 250, Easing.CubicOut);
            CoverDialog.IsVisible = false;
            DialogGPSSettings.IsVisible = false;
            IsDialog = false;
        }

        public void OpenGPSSettings(object sender, PanUpdatedEventArgs e)
        {
            if (Device.OS == TargetPlatform.Android)
            {
                var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                intent.SetFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
                CoverDialog.FadeTo(0, 250, Easing.CubicOut);
                CoverDialog.IsVisible = false;
                DialogGPSSettings.IsVisible = false;
            }
        }

        private double width = 0;
        private double height = 0;
        private double _width = 0;
        private double _height = 0;
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (_width == 0)
            {
                _width = width;
            }
            if (_height == 0)
            {
                _height = height;
            }
            if (width != _width || height != _height)
            {
                MainStack.TranslationY = 0;
                _height = height;
                _width = width;
            }
            if (MenuBox.TranslationX < 0)
            {
                MenuBox.TranslationX = -MenuBox.Width;
            }
        }
    }
}
