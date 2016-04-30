using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GT = GHIElectronics.UWP.GadgeteerCore;
using GTMB = GHIElectronics.UWP.Gadgeteer.Mainboards;
using GTMO = GHIElectronics.UWP.Gadgeteer.Modules;


using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HoverBot
{
    public class MobilRemote
    {
        public enum ArahJalan { Maju, Mundur, Kanan, Kiri, Stop }
        public ArahJalan Arah { set; get; }
        public int Kecepatan { set; get; }

        public MobilRemote()
        {
            Kecepatan = 0;
            Arah = ArahJalan.Stop;
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region BTInit
        private DeviceInformationCollection deviceCollection;
        private DeviceInformation selectedDevice;
        private RfcommDeviceService deviceService;

        public string deviceName = "RNBT-76B7"; // Specify the device name to be selected; You can find the device name from the webb under bluetooth 

        StreamSocket streamSocket = new StreamSocket();
        #endregion

        #region MQTT
       
        const string MQTT_BROKER_ADDRESS = "192.168.1.100";
        static bool isNavigating = false;
        static MqttClient client { set; get; }
        static MobilRemote Mobil { set; get; }
        #endregion

        private GTMB.FEZCream mainboard;
        private GTMO.BreakoutTB10 breakout;
        private GTMO.MotorDriverL298 motor;
        private DispatcherTimer timer;
        private GT.SocketInterfaces.DigitalIO GreenLed;
        private GT.SocketInterfaces.DigitalIO RedLed;
        private GT.SocketInterfaces.DigitalIO Buzz;

        public MainPage()
        {
            
            this.InitializeComponent();
            this.Setup();
            // InitializeRfcommServer();
        }
        private async void Setup()
        {
            this.mainboard = await GT.Module.CreateAsync<GTMB.FEZCream>();
            this.motor = await GT.Module.CreateAsync<GTMO.MotorDriverL298>(this.mainboard.GetProvidedSocket(8));
            this.breakout = await GT.Module.CreateAsync<GTMO.BreakoutTB10>(this.mainboard.GetProvidedSocket(4));
            this.RedLed = await breakout.CreateDigitalIOAsync(GHIElectronics.UWP.GadgeteerCore.SocketPinNumber.Eight,false);
            this.GreenLed = await breakout.CreateDigitalIOAsync(GHIElectronics.UWP.GadgeteerCore.SocketPinNumber.Nine,false);
            this.Buzz = await breakout.CreateDigitalIOAsync(GHIElectronics.UWP.GadgeteerCore.SocketPinNumber.Seven,false);

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(100);

            this.timer.Start();

            Mobil = new MobilRemote();
            this.timer.Tick += (a, b) =>
            {
                
                /*
                double x, y, z;

                this.hat.GetAcceleration(out x, out y, out z);

                this.LightTextBox.Text = this.hat.GetLightLevel().ToString("P2");
                this.TempTextBox.Text = this.hat.GetTemperature().ToString("N2");
                this.AccelTextBox.Text = $"({x:N2}, {y:N2}, {z:N2})";
                this.Button18TextBox.Text = this.hat.IsDIO18Pressed().ToString();
                this.Button22TextBox.Text = this.hat.IsDIO22Pressed().ToString();
                this.AnalogTextBox.Text = this.hat.ReadAnalog(GIS.FEZHAT.AnalogPin.Ain1).ToString("N2");
                */
                if (isNavigating) return;
                isNavigating = true;
                //this.hat.D2.Color = GIS.FEZHAT.Color.Black;
                //this.hat.D3.Color = GIS.FEZHAT.Color.Black;

                switch (Mobil.Arah)
                {
                    case MobilRemote.ArahJalan.Maju:
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor1, 1.0);
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor2, 1.0);

                        GreenLed.Write(true);
                        RedLed.Write(false);
                        Buzz.Write(false);
                        break;
                    case MobilRemote.ArahJalan.Mundur:
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor1, -1.0);
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor2, -1.0);

                        GreenLed.Write(false);
                        RedLed.Write(true);
                        Buzz.Write(false);

                        break;
                    case MobilRemote.ArahJalan.Kiri:
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor1, -0.7);
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor2, 0.7);

                        GreenLed.Write(false);
                        RedLed.Write(false);
                        Buzz.Write(false);
                        break;
                    case MobilRemote.ArahJalan.Kanan:
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor1, 0.7);
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor2, -0.7);

                        GreenLed.Write(false);
                        RedLed.Write(false);
                        Buzz.Write(false);
                        break;
                    case MobilRemote.ArahJalan.Stop:
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor1, 0.0);
                        this.motor.SetSpeed(GHIElectronics.UWP.Gadgeteer.Modules.MotorDriverL298.Motor.Motor2, 0.0);

                        GreenLed.Write(false);
                        RedLed.Write(false);
                        Buzz.Write(false);
                        break;

                }
                isNavigating = false;
            };
            timer.Start();

            client = new MqttClient(MQTT_BROKER_ADDRESS);
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            SubscribeMessage();

          
        }

     

      
        #region MQTT
        void SubscribeMessage()
        {
            // register to message received 
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived; ;
            client.Subscribe(new string[] { "/robot/control" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

        }

        private async void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

                string Message = new string(Encoding.UTF8.GetChars(e.Message));
                if (Message.IndexOf(":") < 1) return;
                // handle message received 
                TxtLog.Text = "Message Received = " + Message;
                string[] CmdStr = Message.Split(':');
                if (CmdStr[0] == "MOVE")
                {
                    if (Mobil == null) return;
                    var ArahStr = string.Empty;
                    switch (CmdStr[1])
                    {
                        case "F":
                            Mobil.Arah = MobilRemote.ArahJalan.Maju;
                            ArahStr = "Maju";
                            break;
                        case "B":
                            Mobil.Arah = MobilRemote.ArahJalan.Mundur;
                            ArahStr = "Mundur";
                            break;
                        case "L":
                            Mobil.Arah = MobilRemote.ArahJalan.Kiri;
                            ArahStr = "Kiri";
                            break;
                        case "R":
                            Mobil.Arah = MobilRemote.ArahJalan.Kanan;
                            ArahStr = "Kanan";
                            break;
                        case "S":
                            Mobil.Arah = MobilRemote.ArahJalan.Stop;
                            ArahStr = "Stop";
                            break;
                    }

                    TxtLog.Text = "Arah : " + ArahStr;
                    PublishMessage("/robot/status", "Robot Status:" + CmdStr[1]);

                }
                else if (CmdStr[0] == "REQUEST" && CmdStr[1] == "STATUS")
                {
                    PublishMessage("/robot/state", "ONLINE");
                }
            });
        }

        void PublishMessage(string Topic, string Pesan)
        {
            client.Publish(Topic, Encoding.UTF8.GetBytes(Pesan), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }
        #endregion
        #region BT
        private void ConnectDevice_Click(object sender, RoutedEventArgs e)
        {
            ConnectToDevice();
        }

        private async void InitializeRfcommServer()
        {
            try
            {
                string device1 = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
                deviceCollection = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(device1);

            }
            catch (Exception exception)
            {
                //errorStatus.Visibility = Visibility.Visible;
                //errorStatus.Text = exception.Message;
            }
        }

        private async void ConnectToDevice()
        {
            foreach (var item in deviceCollection)
            {
                if (item.Name == deviceName)
                {
                    selectedDevice = item;
                    break;
                }
            }

            if (selectedDevice == null)
            {
                //errorStatus.Visibility = Visibility.Visible;
                //errorStatus.Text = "Cannot find the device specified; Please check the device name";
                return;
            }
            else
            {
                deviceService = await RfcommDeviceService.FromIdAsync(selectedDevice.Id);

                if (deviceService != null)
                {
                    //connect the socket   
                    try
                    {
                        await streamSocket.ConnectAsync(deviceService.ConnectionHostName, deviceService.ConnectionServiceName);
                    }
                    catch (Exception ex)
                    {
                       // errorStatus.Visibility = Visibility.Visible;
                       // errorStatus.Text = "Cannot connect bluetooth device:" + ex.Message;
                    }

                }
                else
                {
                   // errorStatus.Visibility = Visibility.Visible;
                   // errorStatus.Text = "Didn't find the specified bluetooth device";
                }
            }

        }

        private async void SendData_Click(object sender, RoutedEventArgs e)
        {
            if (deviceService != null)
            {
                //send data
                string sendData = "test";// messagesent.Text;
                if (string.IsNullOrEmpty(sendData))
                {
                  //  errorStatus.Visibility = Visibility.Visible;
                   // errorStatus.Text = "Please specify the string you are going to send";
                }
                else
                {
                    DataWriter dwriter = new DataWriter(streamSocket.OutputStream);
                    UInt32 len = dwriter.MeasureString(sendData);
                    dwriter.WriteUInt32(len);
                    dwriter.WriteString(sendData);
                    await dwriter.StoreAsync();
                    await dwriter.FlushAsync();
                }

            }
            else
            {
              //  errorStatus.Visibility = Visibility.Visible;
               // errorStatus.Text = "Bluetooth is not connected correctly!";
            }

        }

        private async void ReceiveData_Click(object sender, RoutedEventArgs e)
        {
            // read the data

            DataReader dreader = new DataReader(streamSocket.InputStream);
            uint sizeFieldCount = await dreader.LoadAsync(sizeof(uint));
            if (sizeFieldCount != sizeof(uint))
            {
                return;
            }

            uint stringLength;
            uint actualStringLength;

            try
            {
                stringLength = dreader.ReadUInt32();
                actualStringLength = await dreader.LoadAsync(stringLength);

                if (stringLength != actualStringLength)
                {
                    return;
                }
                string text = dreader.ReadString(actualStringLength);

                //message.Text = text;

            }
            catch (Exception ex)
            {
               // errorStatus.Visibility = Visibility.Visible;
               // errorStatus.Text = "Reading data from Bluetooth encountered error!" + ex.Message;
            }


        }
        #endregion
    }

}
