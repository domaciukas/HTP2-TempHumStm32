using System;
using System.Text;
using System.Threading;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace HTP2_TempHumStm32 {
    class TempHumProducer {
        public string MqttBrokerIp;
        public string MqttClientGuid;
        public TempHumProvider TempHumProvider;

        private MqttClient _mqttClient;
        private HostDevice _hostDevice;
        private HostFloatProperty _temperature;
        private HostFloatProperty _humidity;

        public TempHumProducer(string brokerIp) {
            MqttBrokerIp = brokerIp;
            _mqttClient = new MqttClient(MqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
        }
        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _hostDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
        }

        public void Initialize() {
            _mqttClient.Connect(MqttClientGuid);

            _hostDevice = DeviceFactory.CreateHostDevice("si7020-stm32", "Si7020 on STM32F769I DISCO");

            _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");

            _temperature = _hostDevice.CreateHostFloatProperty(PropertyType.State, "general", "temperature", "Measured temperature", 0.00F, "°C");
            _humidity = _hostDevice.CreateHostFloatProperty(PropertyType.State, "general", "humidity", "Measured humidity", 0.00F, "%");


            

            _hostDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), 1, true);

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });

            var gettingValues = new Thread(GetTemperatureAndHumidityValues);
            gettingValues.Start();
        }

        private void GetTemperatureAndHumidityValues() {
            while (true) {
                _temperature.Value = (float) TempHumProvider.GetTemperature();
                //Thread.Sleep(1000);
                _humidity.Value = (float) TempHumProvider.GetHumidity();
                Thread.Sleep(1500);
            }
        }

    }
}
