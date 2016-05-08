using System;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;

namespace PhoneApp6
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher watcher;
        
        public MainPage()
        {
            InitializeComponent();
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                watcher.MovementThreshold = 1; // ruch o ile metrów uznać jako zmianę
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged); 
                // przypisanie eventu dotyczącego zmiany statusu GPS (wyłączony itd.)
                watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
                //przypisanie eventu dla zmiany pozycji
            }
            else
            {
                watcher.Stop();
                watcher.Dispose();
                GC.Collect();
            }
            watcher.Start();
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    MessageBox.Show("Włącz GPS geniuszu");
                    break;

                case GeoPositionStatus.NoData:
                    MessageBox.Show(" GPS działa, ale coś sie zepsuło...");
                    break;
            }
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position.Location.IsUnknown)
            {
                MessageBox.Show("Poczekaj, trwa wyszukiwanie pozycji...");
                return;
            }
            txt_lat.Text = watcher.Position.Location.ToString();

            this.map.Center = new GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude);
            map.ZoomLevel = 15.7; // wartości double od 0.0 do 20.0 (minimalne do maksymalnego przybliżenia)

            MapLayer layer1 = new MapLayer(); // utworzenie nakładki dla mapy

            Pushpin pushpin1 = new Pushpin(); // utworzenie pinezki 

            pushpin1.GeoCoordinate = e.Position.Location;
            pushpin1.Content = "Tu jestem"; // nazwa
            MapOverlay overlay1 = new MapOverlay(); // utworzenie nakładki z elementami
            overlay1.Content = pushpin1; // dodanie pinezki do nakładki z elementami
            overlay1.GeoCoordinate = e.Position.Location;
            layer1.Add(overlay1); //dodanie nakładki d;a mapy

            map.Layers.Clear(); // usuniecie starych punktow
            map.Layers.Add(layer1); // wstawienie nakładki mapy do mapy
        }

        private void map_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "ApplicationID";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "AuthenticationToken";
            //w to miejce można wstawić dane aplikacji do osługi map (płatne) tak aby nie wyświetlał się napis o niewłaściwym tokenie
        }
    }
}