using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketWinform
{
    public class SocketServer : BaseForm
    {
        //单例，保证服务端只有一个实例
        //声明一个SocketServer类的静态变量来引用唯一的对象
        private static SocketServer socketServer;
        //声明一个私有的构造方法，让外部无法调用这个类的构造方法
        private SocketServer(int port)
        {
            //多线程编程中，如果子线程需要使用主线程中创建的对象和控件，
            //最好在主线程中体现进行检查取消
            CheckForIllegalCrossThreadCalls = false;
            this.btnSend.Enabled = false;
            this.btnStart.Enabled = false;
            this.Name = "Server";
            this.windowName.Text = "服务端";
            this.btnSend.MouseClick += btnSend_Click;
            this.btnEnd.MouseClick += BtnEnd_MouseClick;
            this.txtPort.Text = port.ToString();
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            Connection();
            this.Show();
        }


        //创建一个静态的方法，提供外部调用实例
        public static SocketServer SocketServerMethod(int port)
        {
            if (socketServer == null)
            {
                socketServer = new SocketServer(port);
            }
            return socketServer;
        }

        // <summary>
        /// 创建一个字典，用来存储记录服务器与客户端之间的连接(线程问题)
        /// </summary>
        Dictionary<string, Socket> clientList = new Dictionary<string, Socket>();
        /// <summary>
        /// 创建连接
        /// </summary>
       void Connection()
        {
            Thread myServer = new Thread(MySocket);
            //设置这个线程是后台线程
            myServer.IsBackground = true;
            myServer.Start();
        }
        /// <summary>
        /// 创建连接的方法
        /// </summary>
        void MySocket()
        {
            //1.创建服务器端电话
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                //2.创建手机卡
                IPAddress iP = IPAddress.Parse("127.0.0.3");
                IPEndPoint endPoint = new IPEndPoint(iP, int.Parse(txtPort.Text));
                //3.将电话卡插进电话中
                server.Bind(endPoint);
                //4.开始监听电话卡
                //同一时刻内允许同时加入链接的最大数量
                server.Listen(20);
                txtMes.AppendText("服务器已经成功开启!");
                this.btnSend.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            //5.等待来电接电话            
            while (true)
            {

                //接受接入的一个客户端
                Socket connectClient = server.Accept();
                if (connectClient != null)
                {
                    string infor = connectClient.RemoteEndPoint.ToString();
                    clientList.Add(infor, connectClient);
                  //  listBox1.Items.Add(infor + "加入服务器!");
                    ///服务器将消息发送至客服端
                    string msg = infor + "已成功进入到聊天室!";
                    SendMsg(msg);

                    //每有一个客户端接入时，需要有一个线程进行服务
                    Thread threadClient = new Thread(ReciveMsg);
                    threadClient.IsBackground = true;
                    //设置这个线程中的通信对象是对应的Socket和客户端Socket进行通信
                    threadClient.Start(connectClient);
                }
            }
        }

        /// <summary>
        /// 服务器接收到客户端发送的消息
        /// </summary>
        /// <param name="o">客户端</param>
        void ReciveMsg(object o)
        {
            Socket client = o as Socket;
            while (true)
            {
                try
                {
                    ///定义服务器接收的字节大小
                    byte[] arrMsg = new byte[1024 * 1024];
                    ///接收到的信息大小(所占字节数)
                    int length = client.Receive(arrMsg);

                    if (length > 0)
                    {
                        string recMsg = Encoding.UTF8.GetString(arrMsg, 0, length);
                        //获取客户端的端口号
                        IPEndPoint endPoint = client.RemoteEndPoint as IPEndPoint;
                        //服务器显示客户端的端口号和消息
                        txtMes.AppendText( "[" + endPoint.Port.ToString() + "]：" + recMsg);
                        //服务器发送接收到的客户端信息给客户端
                        SendMsg("[" + endPoint.Port.ToString() + "]：" + recMsg);
                    }
                }
                catch (Exception)
                {
                    ///关闭客户端
                    client.Close();
                    ///移除添加在字典中的服务器和客户端之间的线程
                    clientList.Remove(client.RemoteEndPoint.ToString());
                }
            }
        }

        /// <summary>
        /// 服务器发送消息，客户端接收到
        /// </summary>
        void SendMsg(string str)
        {
            ///遍历出字典中的所有线程
            foreach (var item in clientList)
            {
                byte[] arrMsg = Encoding.UTF8.GetBytes(str);
                ///获取键值，发送消息
                item.Value.Send(arrMsg);
            }
        }

        /// <summary>
        /// 服务器发送消息
        /// </summary>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMes.Text != "")
            {
                SendMsg(txtMes.Text);
                txtMes.Text = "";
            }
        }

        //关闭窗口
        private void BtnEnd_MouseClick(object? sender, MouseEventArgs e)
        {
            this.Close();
        }
    }
}
