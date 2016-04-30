using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Configuration;
using uPLibrary.Networking.M2Mqtt;
using System.Net;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceStack.Redis;
using System.Linq;
using MoreLinq;

namespace IOT.Web
{
    //class untuk bikin chart
    public class DataSeries
    {
        public string name { set; get; }
        public List<double> data { set; get; }
    }

    //class untuk penampung data sensor
    [Serializable]

    public class Room
    {
        public double Temp { set; get; }
        public double Humid { set; get; }
        public double Light { set; get; }
        public double Gas { set; get; }
        public DateTime Created { set; get; }

    }
    //signal R start up class
    [HubName("IOTHub")]
    public class IOTHub : Hub
    {
        public static MqttClient client { set; get; }
        //ambil mqtt broker address dari konfigurasi
        public static string MQTT_BROKER_ADDRESS
        {
            get { return ConfigurationManager.AppSettings["MQTT_BROKER_ADDRESS"]; }
        }
        static void SubscribeMessage()
        {
            //handler untuk menangani mqtt message yang masuk
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            //subscribe ke topik mqtt
            client.Subscribe(new string[] { "/iot/room", "/iot/control","/iot/photo" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

        }


        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //konversi pesan mqtt dari byte ke string
            string Pesan = Encoding.UTF8.GetString(e.Message);
            switch (e.Topic)
            {
                case "/iot/control":
                    //dump data ke log, data yang terkirim ke mqtt broker
                    WriteMessage(Pesan);
                    break;
                case "/iot/room":
                    //data dari sensor, di deserialize dan di insert ke redis
                    var room = JsonConvert.DeserializeObject<Room>(Pesan);
                    var datas = InsertData(room);
                    //update tampilan chart realtime
                    UpdateChart(datas);
                    //WriteMessage(Pesan);
                    break;
                case "/iot/photo":
                    //kalau kiriman foto selesai di unggah oleh device, tampilkan
                    ShowPhoto(Pesan);
                    break;
            }

        }
        static List<Room> InsertData(Room node)
        {
            //isi tanggal saat ini ke data sensor
            node.Created = DateTime.Now;
            var data = new List<Room>();
            //init redis client
            using (var redisManager = new PooledRedisClientManager())
            using (var redis = redisManager.GetClient())
            {
                var redisRooms = redis.As<Room>();
                //masukan data ke redis                
                redisRooms.Store(node);
                //ambil data sensor 10 terakhir
                var temp = redisRooms.GetAll().ToList();
                data = (from c in temp
                        orderby c.Created ascending
                       select c).TakeLast(10).ToList();

            }
            //kirim ke web untuk di render
            return data;
        }
        public IOTHub()
        {
            if (client == null)
            {
                // bikin instance mqtt 
                client = new MqttClient(MQTT_BROKER_ADDRESS);
                string clientId = Guid.NewGuid().ToString();
                client.Connect(clientId, "guest", "guest");
                SubscribeMessage();
            }
        }

        [HubMethodName("TakePicture")]
        public void TakePicture()
        {
            //jika button foto di klik, kirim pesan ke device via mqtt untuk ambil foto
            string Pesan = "photo";
            client.Publish("/iot/control", Encoding.UTF8.GetBytes(Pesan), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

        }
        internal static void UpdateChart(List<Room> rooms)
        {
            //populate data chart di halaman web
            var datas = new List<DataSeries>();
            var context = GlobalHost.ConnectionManager.GetHubContext<IOTHub>();
            var timeseries = from c in rooms
                             select c.Created.ToString("HH:mm:ss");
            var tempseries = from c in rooms
                             select c.Temp;
            datas.Add(new DataSeries() { name = "temperatur", data = tempseries.ToList() });
            dynamic allClients = context.Clients.All.UpdateChart("Temperatur","div_temp",timeseries, datas);
            var humidseries = from c in rooms
                             select c.Humid;
            datas.Clear();
            datas.Add(new DataSeries() { name = "kelembapan", data = humidseries.ToList() });
            allClients = context.Clients.All.UpdateChart("Kelembapan", "div_humid", timeseries, datas);

            var lightseries = from c in rooms
                              select c.Light;
            datas.Clear();
            datas.Add(new DataSeries() { name = "cahaya", data = lightseries.ToList() });
            allClients = context.Clients.All.UpdateChart("Cahaya","div_light",timeseries, datas);
        }
        //fungsi log data ke halaman web 
        internal static void WriteMessage(string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<IOTHub>();
            dynamic allClients = context.Clients.All.WriteData(message);
        }
        //fungsi menampilkan foto
        internal static void ShowPhoto(string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<IOTHub>();
            dynamic allClients = context.Clients.All.ShowPhoto(message);
        }
    }
}