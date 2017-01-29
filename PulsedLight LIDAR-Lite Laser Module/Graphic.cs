using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using LiveCharts;
using LiveCharts.Configurations;
using Windows.UI.Xaml;

namespace PulsedLight_LIDAR_Lite_Laser_Module
{
    public class MeasureModel
    {
        public DateTime DateTime { get; set; }
        public short Value { get; set; }
    }

    public partial class Graphic : INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;

        public void ConstantChangesChart()
        {
            //InitializeComponent();

            //To handle live data easily, in this case we built a specialized type the MeasureModel class, it only contains 2 properties DateTime and Value. We need to configure LiveCharts to handle MeasureModel class. The next code configures MEasureModel  globally, this means that livecharts learns to plot MeasureModel and will use this config every time a ChartValues instance uses this type. This code ideally should only run once, when application starts is reccomended. You can configure series in many ways, learn more at http://lvcharts.net/App/examples/v1/wpf/Types%20and%20Configuration

            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the values property will store our values array
            ChartValues = new ChartValues<MeasureModel>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            SetAxisLimits(DateTime.Now);

            //The next code simulates data changes every 300 ms
            //Timer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromMilliseconds(300)
            //};
            //Timer.Tick += TimerOnTick;
            //IsDataInjectionRunning = false;
            //R = new Random();

            //DataContext = this;
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        public ChartValues<MeasureModel> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }

        public double AxisStep { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        //public static DispatcherTimer Timer { get; set; }
        //public static bool IsDataInjectionRunning { get; set; }
        //public Random R { get; set; }

        //private void RunDataOnClick(object sender, RoutedEventArgs e)
        //{
        //    if (IsDataInjectionRunning)
        //    {
        //        Timer.Stop();
        //        IsDataInjectionRunning = false;
        //    }
        //    else
        //    {
        //        Timer.Start();
        //        IsDataInjectionRunning = true;
        //    }
        //}

        public void addToGraph()
        {
            var now = DateTime.Now;

            ChartValues.Add(new MeasureModel
            {
                DateTime = now,
                Value = MainPage.Distance
            });

            SetAxisLimits(now);

            //lets only use the last 30 values
            if (ChartValues.Count > 100) ChartValues.RemoveAt(0);
        }
        
        //public void TimerOnTick(object sender, EventArgs eventArgs)
        //{
        //    var now = DateTime.Now;

        //    ChartValues.Add(new MeasureModel
        //    {
        //        DateTime = now,
        //        Value =  MainPage.Distance
        //    });

        //    SetAxisLimits(now);

        //    //lets only use the last 30 values
        //    if (ChartValues.Count > 100) ChartValues.RemoveAt(0);
        //}

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(2).Ticks; // lets force the axis to be 100ms ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(10).Ticks; //we only care about the last 8 seconds
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}