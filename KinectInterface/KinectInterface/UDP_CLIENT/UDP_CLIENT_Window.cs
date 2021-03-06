﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDP
{
    public partial class UDP_CLIENT_Window : Form
    {
        UDP_CLIENT client;
        public event _DataReceived DataReceived;

        public UDP_CLIENT_Window()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.client == null)
            {
                int myPort = int.Parse(this.textBox1.Text);
                string remoteIP = this.textBox2.Text;
                int remotePort = int.Parse(this.textBox3.Text);
                this.client = new UDP_CLIENT(remoteIP, remotePort, myPort);
                this.client.DataReceived += this.DataReceived;
                this.button1.Text = "disconnect";
            }
            else
            {
                this.client.Close();
                this.button1.Text = "Connect";
            }
        }

        public void Send(byte[] data)
        {
            if (this.client != null)
                this.client.Send(data);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if(this.client != null)
                this.client.Close();
        }
    }
}
