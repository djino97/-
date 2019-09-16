using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Bitmap MyImage, RedImage, tempImage;
        public static UInt32[,] pixel, tempPixel;
        public int pb2X, pb2Y; bool change = false;

        private void OpenNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (MyImage == null)
                Open();
        }

        public void Open()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Изображения(*.jpg)|*.jpg|Изображения(*.bmp)|*.bmp|Изображения(*.png)|*.png|All files(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (MyImage != null)
                    MyImage.Dispose();
                if (RedImage != null)
                {
                    RedImage.Dispose();
                    pictureBox2.Image = null;
                }
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                MyImage = new Bitmap(ofd.FileName);
                pictureBox1.Image = (Image)MyImage;                
                panel5.Visible = true;
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RedImage == null || pictureBox2.Image == null)
                MessageBox.Show("Нет отредактированного изображения!");
            else
            {
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Title = "Сохранить изображение как...";
                savedialog.FileName = "Обработанное изображение";
                savedialog.Filter = "Изображение(*.bmp)|*.bmp|All files (*.*)|*.*";
                if (savedialog.ShowDialog() == DialogResult.OK)
                    try
                    {
                        RedImage.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        MessageBox.Show("Изображение сохранено");
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Ошибка при сохранении", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
            }
        }

        private void HistogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem.CheckState == CheckState.Checked)
            {
                menuItem.CheckState = CheckState.Unchecked;
                groupBox3.Visible = false;
            }
            else if (menuItem.CheckState == CheckState.Unchecked)
            {
                menuItem.CheckState = CheckState.Checked;
                if (MyImage != null)
                    GetHistogram(new Bitmap(pictureBox1.Image), pictureBox3);
                if (RedImage != null)
                    GetHistogram(new Bitmap(pictureBox2.Image), pictureBox4);
                groupBox3.Visible = true;
            }
        }

        private void SliceFunctionBrightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem.CheckState == CheckState.Checked)
            {
                menuItem.CheckState = CheckState.Unchecked;
                groupBox6.Visible = false;
                groupBox6.Text = "Яркостный разрез:";
            }
            else if (menuItem.CheckState == CheckState.Unchecked)
            {
                menuItem.CheckState = CheckState.Checked;
                groupBox6.Visible = true;
            }
        }        

        private void AdaptiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MyImage == null)
                MessageBox.Show("Изображение не выбрано!");
            else
                Adaptive();
        }

        public void Adaptive()
        {
            if (RedImage != null)
                RedImage.Dispose();
            pixel = new UInt32[MyImage.Height, MyImage.Width];
            for (int y = 0; y < MyImage.Height; y++)
                for (int x = 0; x < MyImage.Width; x++)
                    pixel[y, x] = (UInt32)(MyImage.GetPixel(x, y).ToArgb());
            RedImage = new Bitmap(MyImage);
            RedImage = Sharpness(RedImage);
            if (tempImage != null)
                tempImage.Dispose();
            tempImage = new Bitmap(RedImage);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.Image = (Image)RedImage;
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ComparisonSttingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem.CheckState == CheckState.Checked)
                menuItem.CheckState = CheckState.Unchecked;
            else if (menuItem.CheckState == CheckState.Unchecked)
                menuItem.CheckState = CheckState.Checked;
        }        

        private void WindowsSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new Form2();
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem.CheckState == CheckState.Checked)
            {
                //menuItem.CheckState = CheckState.Unchecked;
                //groupBox4.Visible = false;
                
            }
            else if (menuItem.CheckState == CheckState.Unchecked)
            {
                menuItem.CheckState = CheckState.Checked;
                f.Owner = this;
                f.Show();
                //groupBox4.Visible = true;
            }
        }

        private void Reset(object sender, EventArgs e)
        {
            label3.Text = ""; label5.Text = "";
            label4.Text = ""; label6.Text = "";
            label27.Text = ""; label28.Text = "";
        }                
        
        private void pictureBox_Points(object sender, MouseEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            try
            {
                if ((MyImage != null && pb.Name == pictureBox1.Name) || (pb.Image != null && RedImage != null && pb.Name == pictureBox2.Name) ||
                    (MyImage != null && RedImage != null && pb.Image != null))
                {
                    int x, y;
                    double zoom = (pictureBox1.Image.Width>=pictureBox1.Image.Height)?
                        1.0 * pictureBox1.Width / pictureBox1.Image.Width : 1.0 * pictureBox1.Height / pictureBox1.Image.Height;
                    Bitmap bmp = new Bitmap(pb.Image, pb.Image.Size.Width, pb.Image.Size.Height);
                    bmp.SetResolution(pb.Image.HorizontalResolution, pb.Image.VerticalResolution);
                    if ((e.Y >= (pb.Height - pb.Image.Height * zoom) / 2 && e.Y < pb.Height - (pb.Height - pb.Image.Height * zoom) / 2)
                       && (e.X >= (pb.Width - pb.Image.Width * zoom) / 2 && e.X < pb.Width - (pb.Width - pb.Image.Width * zoom) / 2))
                    {
                        x = (int)((e.X - (pb.Width - pb.Image.Width * zoom) / 2) / zoom);
                        pb2Y = y = (int)((e.Y - (pb.Height - pb.Image.Height * zoom) / 2) / zoom);
                        if (pictureBox2.Image != null)
                        {
                            label3.Text = (x + 1) + " x " + (y + 1);
                            label4.Text = MyImage.GetPixel(x, y).ToString();
                            label27.Text = "Яркость - " + (int)(0.299 * MyImage.GetPixel(x, y).R + 0.587 * MyImage.GetPixel(x, y).G + 0.114 * MyImage.GetPixel(x, y).B);
                            label6.Text = (x + 1) + " x " + (y + 1);
                            label5.Text = RedImage.GetPixel(x, y).ToString();
                            label28.Text = "Яркость - " + (int)(0.299 * RedImage.GetPixel(x, y).R + 0.587 * RedImage.GetPixel(x, y).G + 0.114 * RedImage.GetPixel(x, y).B);
                        }
                        if (pb.Name == pictureBox1.Name)
                        {
                            label3.Text = (x + 1) + " x " + (y + 1);
                            label4.Text = bmp.GetPixel(x, y).ToString();
                            label27.Text = "Яркость - " + (int)(0.299 * bmp.GetPixel(x, y).R + 0.587 * bmp.GetPixel(x, y).G + 0.114 * bmp.GetPixel(x, y).B);
                        }
                        else if (pb.Name == pictureBox2.Name)
                        {
                            label6.Text = (x + 1) + " x " + (y + 1);
                            label5.Text = bmp.GetPixel(x, y).ToString();
                            label28.Text = "Яркость - " + (int)(0.299 * bmp.GetPixel(x, y).R + 0.587 * bmp.GetPixel(x, y).G + 0.114 * bmp.GetPixel(x, y).B);
                            if (change == true)
                            {
                                pb2X = x;
                                pb2Y = y;
                                change = false;
                            }
                        }
                    }
                    else Reset(sender, e);
                    bmp.Dispose();
                }
            }
            catch{}
        }
        
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && groupBox6.Visible == true)
            {
                pictureBox_Points(sender, e);
                if (MyImage != null && pictureBox1.Image != null)
                    Grafik_shiness(MyImage, pb2Y, pictureBox11);
                if (RedImage != null && pictureBox2.Image != null)
                    Grafik_shiness(RedImage, pb2Y, pictureBox12);
                groupBox6.Text = "Яркостный срез по " + pb2Y + " строке:";
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (RedImage != null && pictureBox2.Image != null && groupBox6.Visible == true)
                if (e.Button == MouseButtons.Left)
                {
                    pictureBox_Points(sender, e);
                    Grafik_shiness(MyImage, pb2Y, pictureBox11);
                    Grafik_shiness(RedImage, pb2Y, pictureBox12);
                    groupBox6.Text = "Яркостный срез по "+pb2Y+" строке:";
                }                
        }

        private void Grafik_shiness(Bitmap Image, int str, PictureBox pb)
        {
            if (pb.Image != null)
                pb.Image = null;
            List<double> Points = new List<double>(), X = new List<double>();
            for (int x = 0; x < Image.Width; x++)
            {
                double yx = 0.299 * Image.GetPixel(x, str).R + 0.587 * Image.GetPixel(x, str).G + 0.114 * Image.GetPixel(x, str).B;
                double y = (yx < 255) ? yx : 255;
                Points.Add(y);
                X.Add(x);
            }
            PointF[] listOfPoints = new PointF[Points.Count];

            for (int i = 0; i < listOfPoints.Length; i++)
                listOfPoints[i] = new PointF((float)X[i],(float)Points[i]);

            Function gIGenerator = new Function(listOfPoints, Image.Width, Image.Height);
            pb.Image = gIGenerator.Bitmap;
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            Points.Clear(); X.Clear();
        }      
                     
        public void GetHistogram(Bitmap image, PictureBox pb)
        {
            int[] hist;
            hist = GetHistogramm(image);

            List<int> lstParameters = new List<int>();
            for (int i = 0; i < hist.Length; i++)
                lstParameters.Add(hist[i]);
            pb.Controls.Clear();
            var pb_ = new PictureBox();
            pb_.Dock = DockStyle.Fill;
            pb.Controls.Add(pb_);
            new ChartPanel { Parent = pb_, Dock = DockStyle.Fill, Data = lstParameters };
        }

        private static int[] GetHistogramm(Bitmap image)
        {
            int[] result = new int[256];
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    int i = (int)(255 * image.GetPixel(x, y).GetBrightness());
                    result[i]++;
                }

            return result;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void panel23_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void построитьГистограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem.CheckState == CheckState.Checked)
            {
                menuItem.CheckState = CheckState.Unchecked;
                groupBox6.Visible = false;
                groupBox6.Text = "Яркостный разрез:";
            }
            else if (menuItem.CheckState == CheckState.Unchecked)
            {
                menuItem.CheckState = CheckState.Checked;
                groupBox6.Visible = true;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        public class ChartPanel : Control
        {
            public List<int> Data { get; set; }

            public ChartPanel()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

                Padding = new Padding(20);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (Data == null) return;

                var w = ClientSize.Width - Padding.Left - Padding.Right;
                var h = ClientSize.Height - Padding.Top - Padding.Bottom;
                var g = e.Graphics;

                g.TranslateTransform(Padding.Left, Padding.Top);

                //Рисуем горизонтальную и вертикальную линии (левая и нижняя стенки)
                g.DrawLine(Pens.Black, 0, h, w, h);
                g.DrawLine(Pens.Black, 0, 0, 0, h);

                //Определяем наибольшее число в списке
                var max = int.MinValue;
                for (int j = 0; j < Data.Count; j++)
                    if (Data[j] > max)
                        max = Data[j];

                var stepX = w / Data.Count;

                for (int i = 0; i < Data.Count; i++)
                {
                    var barHeight = 1f * h * Data[i] / max;
                    var rect = new RectangleF(stepX * i, h - barHeight, stepX * 0.8f, barHeight);
                    g.FillRectangle(Brushes.Black, rect);
                }
            }
        }

        public void FromPixelToBitmap()
        {
            for (int y = 0; y < tempImage.Height; y++)
                for (int x = 0; x < tempImage.Width; x++)
                    tempImage.SetPixel(x, y, Color.FromArgb((int)tempPixel[y, x]));
        }
        
        public void FromOnePixelToBitmap(int x, int y, UInt32 pixel)
        {
            tempImage.SetPixel(y, x, Color.FromArgb((int)pixel));
        } 

        Bitmap Sharpness(Bitmap image)
        {
            double C = Common.C;
            int S = Common.S;
            AdaptivSharpness InstanceAdaptivSharpness = new AdaptivSharpness();
            pixel = InstanceAdaptivSharpness.MatrixFiltration(image.Width, image.Height, pixel, C, S);

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    image.SetPixel(x, y, Color.FromArgb((int)pixel[y, x]));

            return image;
        }
    }
}
