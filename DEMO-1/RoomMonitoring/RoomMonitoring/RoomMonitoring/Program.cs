using Gadgeteer.Modules.GHIElectronics;
using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Microsoft.SPOT.Net.NetworkInformation;

using GHI.Networking;
using System.IO.Ports;
using System.Text;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace System.Diagnostics
{
    public enum DebuggerBrowsableState
    {
        Never,
        Collapsed,
        RootHidden
    }
}
namespace RoomMonitoring
{
    public partial class Program
    {
        
        public static MqttClient client { set; get; }
        const string MQTT_BROKER_ADDRESS = "192.168.1.100";
        void SubscribeMessage()
        {
            //event handler saat message dari mqtt masuk
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            //subcribe ke topik tertentu di mqtt
            client.Subscribe(new string[] { "/iot/room", "/iot/control" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

        }

        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //conversi byte message ke string
            string pesan = new string(Encoding.UTF8.GetChars(e.Message));
            if(e.Topic=="/iot/control")
            {
                switch (pesan)
                {
                    case "photo":
                        var thPhoto = new Thread(new ThreadStart(Jepret));
                        thPhoto.Start();
                        break;
                    default:
                        break;
                }
            }
            //cetak ke console tuk kebutuhan debug
            Debug.Print("Message : " + pesan);
        }

        void Jepret()
        {
            //ambil foto dari camera
            serialCameraL1.StartStreaming();
            while (!serialCameraL1.NewImageReady)
            {
                Thread.Sleep(50);
            }

            byte[] dataImage = serialCameraL1.GetImageData();
            serialCameraL1.StopStreaming();
            // bikin konten yang mau di post ke server                    
            var content = Gadgeteer.Networking.POSTContent.CreateBinaryBasedContent(dataImage);

            // bikin request
            var request = Gadgeteer.Networking.HttpHelper.CreateHttpPostRequest(
                @"http://" + MQTT_BROKER_ADDRESS + ":991/api/Upload.ashx" // url service/handler
                , content // data gambar
                , "image/jpeg" // tipe mime di header
            );
            request.ResponseReceived += (HttpRequest s, HttpResponse response) =>
            {
                if (response.StatusCode == "200")
                {
                    Debug.Print("sukses:" + response.Text);
                    client.Publish("/iot/photo", Encoding.UTF8.GetBytes(response.Text), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                }

            };
            // kirim request via http post
            request.SendRequest();
        }
        void ProgramStarted()
        {

            Debug.Print("Program Started");
            //setup static ip biar cepet
            ethernetENC28.NetworkInterface.Open();
            ethernetENC28.NetworkSettings.EnableStaticIP("192.168.1.123", "255.255.255.0", "192.168.1.1");
            ethernetENC28.NetworkSettings.EnableStaticDns(new string[] { "192.168.1.1" });
            //bermanfaat kalau pake dhcp
            while (ethernetENC28.NetworkInterface.IPAddress == "0.0.0.0")
            {
                Debug.Print("Waiting for connect");
                Thread.Sleep(250);
            }
            Thread.Sleep(5000);
            //konek ke mqtt broker
            client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, "guest", "guest");
            //subcribe topik
            SubscribeMessage();
            //setup timer untuk ambil data sensor ruangan
            GT.Timer timer = new GT.Timer(2000);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(GT.Timer timer)
        {
            //ambil data dari sensor
            var Temp = tempHumidSI70.TakeMeasurement();
            //cetak di character display
            string DisplayData = "Temp:" + System.Math.Round(Temp.Temperature) + " C";
            characterDisplay.Clear();
            characterDisplay.Print(DisplayData);
            DisplayData= "Light :" + System.Math.Round(lightSense.GetIlluminance()) + ", Gas:" + System.Math.Round(gasSense.ReadProportion());
            characterDisplay.SetCursorPosition(1, 0);
            characterDisplay.Print(DisplayData);
            //masukin data dari sensor ke class room
            var CurrentCondition = new Room() { Temp = Temp.Temperature, Humid = Temp.RelativeHumidity, Gas=gasSense.ReadProportion(), Light=lightSense.GetIlluminance() };
            //serialize ke json
            string Data = Json.NETMF.JsonSerializer.SerializeObject(CurrentCondition);
            //kirim ke mqtt broker
            client.Publish("/iot/room", Encoding.UTF8.GetBytes(Data), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }

    }

    //class penampung data sensor
    public class Room
    {
        public double Temp { set; get; }
        public double Humid { set; get; }
        public double Light { set; get; }
        public double Gas { set; get; } 
        public DateTime Created { set; get; }

    }
}

