using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using System.Management;
using System.Diagnostics;

namespace ArduinoBluetoothController
{
    public partial class Form1 : Form
    {
        const char left = 'A';
        const char right = 'D';
        const char forward = 'S';   // felt lazy to rewire the mess, just wire correctly and set it to more obvious 'W'
        const char backward = 'W';
        const char vServo = 'V';
        const char hServo = 'H';
        const char buzzer = 'B';
        const char light = 'L';
        const char commandDelimiter = '|';
        const char valueDelimiter = ':';

        bool lightState = false;
        bool forwardState = false;
        bool backwardState = false;
        bool leftState = false;
        bool rightState = false;

        string buffer = null;

        public delegate void processDelegate(string text);
        processDelegate D1 = null;

        public SerialPort MySerialPort = new SerialPort();

        public class ComboboxItems
        {
            public string DispMem { get; set; }
            public string ValMem { get; set; }

            public ComboboxItems(string dispM, string valM)
            {
                DispMem = dispM;
                ValMem = valM;
            }
        }

        private void MySerialPort_DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            Invoke(D1, MySerialPort.ReadExisting());
        }

        public Form1()
        {
            D1 = new processDelegate(process);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;

            MySerialPort.DataReceived += MySerialPort_DataReceived;

            cmbParity.DataSource = Enum.GetValues(typeof(Parity));
            cmbStopBits.DataSource = Enum.GetValues(typeof(StopBits));
            cmbHandshake.DataSource = Enum.GetValues(typeof(Handshake));

            cmbStopBits.SelectedIndex = 1;

            //var cli = new BluetoothClient();
            //BluetoothDeviceInfo[] peers = cli.DiscoverDevices();

            //ManagementObjectCollection ManObjReturn;
            //ManagementObjectSearcher ManObjSearch;
            //ManObjSearch = new ManagementObjectSearcher("Select * from Win32_SerialPort");
            //ManObjReturn = ManObjSearch.Get();

            //foreach(ManagementObject ManObj in ManObjReturn)
            //{                
            //    ComboboxItems cmbItem = new ComboboxItems(ManObj["Name"].ToString(), ManObj["DeviceID"].ToString());
            //    comboBox1.Items.Add(cmbItem);
            //}

            foreach (string sp in SerialPort.GetPortNames())
            {
                if (!cmbPort.Items.Contains(sp))
                    cmbPort.Items.Add(sp);
            }

        }

        public void send_serial_data(string msg)
        {
            try
            {
                msg += commandDelimiter;

                if (MySerialPort.IsOpen)
                {
                    MySerialPort.Write(msg);

                    int startLength = txtReceived.Text.Length;
                    txtReceived.AppendText("Sent : " + msg + Environment.NewLine);
                    txtReceived.Select(startLength, 7);
                    txtReceived.SelectionFont = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
                    txtReceived.SelectionColor = Color.Red;
                    txtReceived.ScrollToCaret();

                    txtSend.Clear();
                }
                else
                {
                    MessageBox.Show("Serial connection to port not open.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to send data");
            }
        }

        public void process(string msg)
        {
            buffer = buffer + msg;

            // *** Debug section

            int startLength = txtReceived.Text.Length;
            //txtReceived.AppendText("Debug-msg : " + msg + Environment.NewLine);
            //txtReceived.Select(startLength, 11);
            //txtReceived.SelectionFont = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
            //txtReceived.SelectionColor = Color.LightGreen;
            //txtReceived.ScrollToCaret();

            //startLength = txtReceived.Text.Length;
            //txtReceived.AppendText("Debug-buffer : " + buffer + Environment.NewLine);
            //txtReceived.Select(startLength, 14);
            //txtReceived.SelectionFont = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
            //txtReceived.SelectionColor = Color.Salmon;
            //txtReceived.ScrollToCaret();

            // *** Debug section

            string tempStr = buffer;
            if (tempStr.Contains("~"))
            {
                string[] words = tempStr.Split(commandDelimiter);
                buffer = words[words.Length - 1];
                for (int i = 0; i < words.Length - 1; i++)
                {
                    if (words[i].Length > 0)
                    {
                        startLength = txtReceived.Text.Length;
                        txtReceived.AppendText("Received : " + words[i] + Environment.NewLine);
                        txtReceived.Select(startLength, 10);
                        txtReceived.SelectionFont = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
                        txtReceived.SelectionColor = Color.Blue;
                        txtReceived.ScrollToCaret();
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MySerialPort.IsOpen)
                MySerialPort.Close();
        }

        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (cmbPort.SelectedIndex != -1)
                    {
                        send_serial_data(txtSend.Text);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (toolStripTextBox1.Text != "")
            {
                try
                {
                    webBrowser1.Url = new System.Uri(toolStripTextBox1.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    toolStripTextBox1.Focus();
                }
            }
        }

        private void btnConDiscon_Click(object sender, EventArgs e)
        {
            if (cmbPort.SelectedIndex != -1)
            {
                try
                {
                    if (MySerialPort.IsOpen)
                    {
                        MySerialPort.Close();
                        btnConDiscon.Text = "Connect";
                    }
                    else
                    {
                        // MySerialPort.PortName = (comboBox1.SelectedItem as ComboboxItems).ValMem.ToString();
                        MySerialPort.PortName = cmbPort.Text;
                        MySerialPort.BaudRate = 9600;
                        MySerialPort.DataBits = 8;

                        Parity p;
                        Enum.TryParse<Parity>(cmbParity.SelectedValue.ToString(), out p);
                        MySerialPort.Parity = p;

                        StopBits sb;
                        Enum.TryParse<StopBits>(cmbStopBits.SelectedValue.ToString(), out sb);
                        MySerialPort.StopBits = sb;

                        Handshake hs;
                        Enum.TryParse<Handshake>(cmbHandshake.SelectedValue.ToString(), out hs);
                        MySerialPort.Handshake = hs;

                        MySerialPort.Encoding = System.Text.Encoding.Default;

                        MySerialPort.Open();

                        btnConDiscon.Text = "Disconnect";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            else
            {
                MessageBox.Show("Please select a serial port first.");
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up && !forwardState)
            {
                send_serial_data(forward + forwardSpeed.Value.ToString());
                forwardState = true;
                backwardState = false;
                leftState = false;
                rightState = false;
            }
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down && !backwardState)
            {
                send_serial_data(backward + reverseSpeed.Value.ToString());
                forwardState = false;
                backwardState = true;
                leftState = false;
                rightState = false;
            }
            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left && !leftState)
            {
                send_serial_data(left + turningSpeed.Value.ToString());
                forwardState = false;
                backwardState = false;
                leftState = true;
                rightState = false;
            }
            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right && !rightState)
            {
                send_serial_data(right + turningSpeed.Value.ToString());
                forwardState = false;
                backwardState = false;
                leftState = false;
                rightState = true;
            }
            else if (e.KeyCode == Keys.H)
            {
                btnHorn_Click(this, new EventArgs());
            }
            else if (e.KeyCode == Keys.L)
            {
                btnLight_Click(this, new EventArgs());
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void btnHorn_Click(object sender, EventArgs e)
        {
                send_serial_data(buzzer + hornFrequency.Value.ToString() + valueDelimiter + (hornDuration.Value * 100).ToString());
        }

        private void btnLight_Click(object sender, EventArgs e)
        {
            if (!lightState)
            {
                send_serial_data(light + lightBrightness.Value.ToString());
                lightState = true;
            }
            else
            {
                send_serial_data(light + "0");
                lightState = false;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            webBrowser1.Stop();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (toolStripTextBox1.Text != "")
                {
                    try
                    {
                        webBrowser1.Url = new System.Uri(toolStripTextBox1.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        toolStripTextBox1.Focus();
                    }
                }
            }
        }

        private void servo1Position_ValueChanged(object sender, EventArgs e)
        {
            send_serial_data(vServo + servo1Position.Value.ToString());
        }

        private void servo2Position_ValueChanged(object sender, EventArgs e)
        {
            send_serial_data(hServo + servo2Position.Value.ToString());
        }

        private void buttonUp_eventHandler(object sender, MouseEventArgs e)
        {
            send_serial_data("Z0");
        }

        private void btnUp_MouseDown(object sender, MouseEventArgs e)
        {
            send_serial_data(forward + forwardSpeed.Value.ToString());
        }

        private void btnDown_MouseDown(object sender, MouseEventArgs e)
        {
            send_serial_data(backward + reverseSpeed.Value.ToString());
        }

        private void btnLeft_MouseDown(object sender, MouseEventArgs e)
        {
            send_serial_data(left + turningSpeed.Value.ToString());
        }

        private void btnRight_MouseDown(object sender, MouseEventArgs e)
        {
            send_serial_data(right + turningSpeed.Value.ToString());
        }

        private void btnDown_MouseDown_1(object sender, MouseEventArgs e)
        {
            send_serial_data(backward + reverseSpeed.Value.ToString());
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            send_serial_data("Z0");
            forwardState = false;
            backwardState = false;
            leftState = false;
            rightState = false;
        }

    }
}
