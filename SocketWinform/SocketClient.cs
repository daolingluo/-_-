using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketWinform
{
    public class SocketClient : BaseForm
    {
        public SocketClient()
        {
            CheckForIllegalCrossThreadCalls = false;
            this.btnStart.MouseClick += BtnStart_MouseClick;
            this.btnEnd.MouseClick += BtnEnd_MouseClick;
            this.btnSend.MouseClick += btnSend_Click;
            this.btnSend.Enabled = false;
            this.Name = "Client";
            this.windowName.Text = "客户端";
            

        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEnd_MouseClick(object? sender, MouseEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 创建客户端
        /// </summary>
        Socket client;
        private void BtnStart_MouseClick(object? sender, MouseEventArgs e)
        {
            //禁用启动按钮
            this.btnStart.Enabled = false;
            //开启服务端
            try
            {
                //把端口号传过去
                // SocketServer server = new SocketServer(Int32.Parse(txtPort.Text));
                SocketServer server = SocketServer.SocketServerMethod(Int32.Parse(txtPort.Text));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            ///创建客户端
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            ///IP地址
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            try
            {
                ///端口号
                IPEndPoint endPoint = new IPEndPoint(ip, int.Parse(txtPort.Text));
                client.Bind(endPoint);
                ///建立与服务器的远程连接
                client.Connect(endPoint);
                //连接成功时，启用发送按钮
                this.btnSend.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //连接失败时启用启动按钮
                this.btnStart.Enabled = true;
            }
            ///线程问题
            Thread thread = new Thread(ReciveMsg);
            thread.IsBackground = true;
            thread.Start(client);
        }
        /// <summary>
        /// 客户端接收到服务器发送的消息
        /// </summary>
        /// <param name="o">客户端</param>
        void ReciveMsg(object obj)
        {
            Socket client = obj as Socket;
            while (true)
            {
                try
                {
                    ///定义客户端接收到的信息大小
                    byte[] arrList = new byte[1024 * 1024];
                    ///接收到的信息大小(所占字节数)
                    int length = client.Receive(arrList);
                    string msg = "客户端："+ Encoding.UTF8.GetString(arrList, 0, length) + "\r\n";
                    txtMes.Text=msg;
                }
                catch (Exception ex)
                {
                    ///关闭客户端
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 客户端发送消息给服务端
        /// </summary>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMes.Text != "")
            {
                SendMsg(txtMes.Text);
            }
        }

        /// <summary>
        /// 客户端发送消息，服务端接收到
        /// </summary>
        void SendMsg(string str)
        {
            byte[] arrMsg = Encoding.UTF8.GetBytes(str);
            client.Send(arrMsg);
        }
    }
}
