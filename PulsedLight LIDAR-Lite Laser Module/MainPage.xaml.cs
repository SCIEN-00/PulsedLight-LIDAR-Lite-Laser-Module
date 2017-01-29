using System;
using System.Threading;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.UI.Xaml;

//The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
namespace PulsedLight_LIDAR_Lite_Laser_Module
{
    public sealed partial class MainPage : Page
    {
        long i = 0, j = 0, k = 0, l = 0;
        public static short Distance; //Raw readings from the LIDAR-lit sensor
        Graphic gr = new Graphic();

        //private const string I2C_CONTROLLER_NAME = "I2C5";    //For Minnowboard Max, use I2C5
        private const string I2C_CONTROLLER_NAME = "I2C1";      //For Raspberry Pi 2, use I2C1
        private const byte LIDARLite_I2C_ADDR = 0x62;           //Default I2C Address of LIDAR-Lite
        private const byte LIDARLite_Reg = 0x00;                //Register to write to initiate ranging
        private const byte LIDARLite_Val = 0x04;                //Value to initiate ranging
        private const byte LIDARLite_RegHighLow = 0x8F;         //Register to get both High and Low bytes in 1 call

        private I2cDevice I2CLIDAR;
        //private Timer PeriodicTimer;
        public DispatcherTimer periodicTimer;

        byte[] LIDAR_Addr_Buf = new byte[] { LIDARLite_I2C_ADDR };
        byte[] Write_Init_Buf = new byte[] { LIDARLite_Reg, LIDARLite_Val };
        byte[] I2CWriteBuffer = new byte[] { LIDARLite_RegHighLow };
        byte[] NullBuff = new byte[] { 0, 0 };
        byte[] I2CReadBuffer = new byte[2];

        public MainPage()
        {
            i++;
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded; //Register for the unloaded event so we can clean up upon exit
            InitI2CLIDAR(); //Initialize the I2C bus, accelerometer, and timer
        }

        private async void InitI2CLIDAR()
        {
            try
            {
                var settings = new I2cConnectionSettings(0x62);
                settings.BusSpeed = I2cBusSpeed.FastMode;
                j++;

                //Initialize the I2C bus
                string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME); //Find the selector string for the I2C bus controller
                var dis = await DeviceInformation.FindAllAsync(aqs); //Find the I2C bus controller device with our selector string
                I2CLIDAR = await I2cDevice.FromIdAsync(dis[0].Id, settings); //Create an I2cDevice with our selected bus controller and I2C settings
                if (I2CLIDAR == null)
                {
                    Text_Status.Text = string.Format("Slave address {0} on I2C Controller {1} is currently in use by another application. Please ensure that no other applications are using I2C." + settings.SlaveAddress + dis[0].Id);
                    return;
                }
            }
            catch (Exception e) //If initialization fails, display the exception and stop running
            {
                Text_Status.Text = "I2C Initialization failed. Exception: " + e.Message;
                return;
            }

            periodicTimer = new DispatcherTimer();
            periodicTimer.Interval = TimeSpan.FromMilliseconds(10);
            periodicTimer.Tick += Timer_Tick;
            periodicTimer.Start();

            try
            {
                I2CLIDAR.Write(LIDAR_Addr_Buf);
                I2CLIDAR.Write(Write_Init_Buf);
                I2CLIDAR.Write(NullBuff);
                periodicTimer.Interval = TimeSpan.FromMilliseconds(20);
                init_fun.Text = "InitI2CLIDAR:\t" + j;
            }
            catch (Exception ex) //If the write fails display the error and stop running
            {
                Text_Status.Text = "I2C Initialization failed. Exception: " + ex.Message;
                return;
            }

            try
            {
                ReadI2CLIDAR();
            }
            catch (Exception e)
            {
                Text_Status.Text = "Failed to communicate with device: " + e.Message;
                return;
            }
        }

        void ReadI2CLIDAR()
        {
            try
            {
                I2CLIDAR.WriteRead(I2CWriteBuffer, I2CReadBuffer);
                I2CLIDAR.Write(NullBuff);

                periodicTimer.Interval = TimeSpan.FromMilliseconds(20);
                reading_moment.Text = "Reading moment:\t" + l++;
            }

            catch (Exception e) //If WriteRead() fails, display error messages
            {
                read_fun.Text = "Failed to communicate with device: \t" + k++;
                Text_Status.Text = "Failed to communicate with device: " + e.Message;
                LIDAR_val.Text = "Distance:\t\tMeasurement failed";
                Text_Status.Text = "Exception: " + e.Message;
                return;
            }

            LIDAR_val_Test.Text = "Raw reg val:\t" + (short)(I2CReadBuffer[0]) + "\t" + (short)(I2CReadBuffer[1]);
            Distance = (short)((I2CReadBuffer[0] << 8) + I2CReadBuffer[1]);

            //Ploting testing


            //Display the values
            LIDAR_val.Text = "Distance:\t\t" + Distance;
            DistanceBar.Value = Distance;
            Text_Status.Text = "Status:\tMeasureing";
            
            periodicTimer.Interval = TimeSpan.FromMilliseconds(500);
        }

        private void Timer_Tick(object sender, object e)
        {
            ReadI2CLIDAR(); //Read data from the I2C LIDAR module and display it
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            I2CLIDAR.Dispose(); //Cleanup
        }
    }
}