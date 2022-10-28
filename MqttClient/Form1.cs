using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;

namespace _MqttClient
{
    public partial class Form1 : Form
    {
        private delegate void WriteInfoDelegate(string msg);

        private bool authFlag = false;

        private Helper.SendMsg sender = new Helper.SendMsg();
        public Form1()
        {
            InitializeComponent();
            //var k_hook = new Helper.KeyboardHook();
            //k_hook.KeyDownEvent += K_hook_KeyDownEvent;
            //k_hook.Start();
        }
        private void auth()
        {
            richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), "客户端完成认证前，不可使用！");
        }
        //private void K_hook_KeyDownEvent(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.F10)
        //    {
        //        Helper.SendMsg msg = new Helper.SendMsg();
        //        msg.SendText("3526110as");
        //    }
        //}

        private void Form1_Load(object sender, EventArgs e)
        {           
            MqttConnectAsync();
        }
        private IMqttClient mqttClient;

        private void WriteInfoProxy(string msg)
        {
            //object obj = new object[] { msg };
            Invoke(new WriteInfoDelegate(SetTxt), msg);
        }
        private void SetTxt(string msg)
        {
            richTextBox1.AppendText(msg + "\r\n");
        }
        private void MqttConnectAsync()
        {
            try
            {   var mqttFactory = new MqttFactory();
                //使用Build构建
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("mqtt.hfgkgroup.com", 1883)
                    .WithClientId(GetMacByWmi())
                    .WithCleanSession(false)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                    .WithCredentials("xuye", "3526110as")
                    .Build();
                mqttClient = mqttFactory.CreateMqttClient();
                //与3.1对比，事件订阅名称和接口已经变化
                mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
                mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
                mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
                Task task = mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                task.Wait();
            }
            catch (Exception ex)
            {
                richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), $"Mqtt客户端尝试连接出错：" + ex.Message);
            }
        }
        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            string msg = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
            //richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), msg);
            ReadJsonStr(msg);
            return Task.CompletedTask;
        }

        private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), "Mqtt客户端连接成功.");
            MqttClientSubscribeOptions opt = new MqttClientSubscribeOptions();
            mqttClient.SubscribeAsync("app");
            return Task.CompletedTask;
        }

        private Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), $"Mqtt客户端连接断开");
            return Task.CompletedTask;
        }
        ///<summary>
        /// 通过WMI读取系统信息里的网卡MAC(方法二)
        ///</summary>
        ///<returns></returns>
        private string GetMacByWmi()
        {
            try
            {
                //创建ManagementClass对象
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                string macAddress = string.Empty;
                foreach (ManagementObject mo in moc)//遍历获取的集合
                {
                    if ((bool)mo["IPEnabled"])//判断IPEnabled的属性是否为true
                    {
                        macAddress = mo["MacAddress"].ToString();//获取网卡的序列号
                    }
                }
                return macAddress;
            }
            catch (Exception e)
            {
                //这里写异常的处理（最好写入日志文件）
                return e.Message;
            }
        }
        private void ReadJsonStr(string jsonStr)
        {
            if (utils.CheckJson.IsJson(jsonStr))
            {
                var obj = JObject.Parse(jsonStr);
                var code = obj["code"].Value<int>();
                var client_id = obj["client_id"].Value<string>();
                var state = obj["state"].Value<bool>();
                var msg = obj["msg"].Value<string>();
                switch (code){
                    case 0:
                        if (state)
                        {
                            richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), "客户端认证成功，客户端id:"+client_id);
                            authFlag = true;
                        }
                        break;
                    case 1:
                        if (!authFlag)
                        {
                            auth();
                        }
                        else
                        {
                            switch (msg.Substring(0, 2))
                            {
                                case "删除":
                                    string _tmp = System.Text.RegularExpressions.Regex.Replace
                                        (utils.MsgUtils.parseMsgToNumber(msg), @"[^0-9]+", "");
                                    int count = Convert.ToInt32(_tmp);
                                    for (int i = 0; i < count; i++)
                                    {
                                        utils.KeyBoard.keyPress(utils.KeyBoard.vKeyBack);
                                    }
                                    break;
                                default:
                                    sender.SendText(msg).ToString();
                                    richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), "RecStr："+msg);
                                    break;
                            }
                        }                        
                        break;
                }
            }
            else
            {
                richTextBox1.BeginInvoke(new WriteInfoDelegate(WriteInfoProxy), "Invalid JSONSTRING!!");
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.SelectionLength;
            richTextBox1.ScrollToCaret();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            richTextBox1.Text = "";
        }
    }

}
