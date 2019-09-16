using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            trackBar1.Value = Convert.ToInt32(Common.C / 0.01);
            trackBar6.Value = (Common.S - 1) / 2;

            label22.Text = (trackBar1.Value * 0.01).ToString();
            label24.Text = (trackBar6.Value * 2 + 1) + " x " + (trackBar6.Value * 2 + 1);
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            Common.C = trackBar1.Value * 0.01;
            Common.S = trackBar6.Value * 2 + 1;

            Form1 main = (Form1)this.Owner;
            label22.Text = (trackBar1.Value * 0.01).ToString();
            label24.Text = (trackBar6.Value * 2 + 1) + " x " + (trackBar6.Value * 2 + 1);
            if (main.RedImage != null && main.pictureBox2.Image != null)
                main.Adaptive();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1 main = (Form1)this.Owner;
            main.окноНастроекToolStripMenuItem.CheckState = CheckState.Unchecked;
        }

        private void label24_Click(object sender, EventArgs e)
        {

        }
    }
}
