using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TCP_Listener
{
    public partial class Form1 : Form
    {
        private const string securityToken = "xuhuixjsoh4s564c456dcsdc48d96c5cs";

        public Form1()
        {
            InitializeComponent();
        }

        private delegate void WriteOut(String str);
        private delegate void Success();
        private delegate void SendData(string host, int port, string str);

        private void buttonStartListening_Click(object sender, EventArgs e)
        {
            buttonStartListening.Enabled = false; 
            pictureBox2.Visible = true;
            numericUpDownPort.Enabled = false;
            toolStripStatusLabel.Text = "Listens on " + numericUpDownPort.Value.ToString();

            Thread th = new Thread(new ThreadStart(receiveData));
            th.Start();
        }

        private void WriteInTxtArea(String str)
        {
            textBox1.Text = str;
        }

        private void receiveData()
        {
            TcpListener tcpListener = null;

            string senderHostname = "";
            string outputText = "";

            int port = Int32.Parse(numericUpDownPort.Value.ToString());
            IPAddress ip = IPAddress.Parse("127.0.0.1");

            try
            {
                tcpListener = new TcpListener(ip, port);
                tcpListener.Start();

                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                StreamReader sr = new StreamReader(tcpClient.GetStream());
    
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null || line == "end" || line == "exit")
                    {
                        break;
                    }
                    else if (line.Contains(securityToken))
                    {
                        senderHostname = line.Substring(line.LastIndexOf("#") + 1);
                    }
                    else
                    {
                        outputText += line;
                    }
                }

                sr.Close();
                tcpClient.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                tcpListener.Stop();
            }

            Invoke(new WriteOut(WriteInTxtArea), new object[] { outputText });
            
            if (senderHostname != "")
            {
                Invoke(new SendData(sendData), new object[] { senderHostname, port, outputText }); 
            }

            Invoke(new Success(signalizeSuccess));

        }

        private void signalizeSuccess()
        {
            pictureBox2.Visible = false;
            pictureBox1.Visible = true;
            toolStripStatusLabel.Text = "Sucessfull - msg received, answer sent!";
        }

        private void sendData(string host, int port, string str)
        {
            TcpClient tcpCl = null;
            try
            {
                tcpCl = new TcpClient(host, port);
                NetworkStream ns = tcpCl.GetStream();

                byte[] bytes = Encoding.UTF8.GetBytes(str + Environment.NewLine + securityToken);
                ns.Write(bytes, 0, bytes.Length);

                ns.Flush();
                ns.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                tcpCl.Close();
            }
        }
    }
}
