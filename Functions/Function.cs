using System.Drawing;

namespace WindowsFormsApp2
{
    class Function
    {
        private Bitmap bitmap;
        public Bitmap Bitmap
        {
            get { return bitmap; }
            private set { bitmap = value; }
        }


        public Function(PointF[] listOfPoints, int width, int height)
        {
            bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen p = new Pen(Color.Black, 1);

            for (int i = 0; i < listOfPoints.Length; i++)
            {
                listOfPoints[i].X = listOfPoints[i].X + 10;
                listOfPoints[i].Y = listOfPoints[i].Y + 10;
            }

            g.DrawLines(p, listOfPoints);
            g.DrawLine(p, 10, 0, 10, height);
            g.DrawLine(p, 0, height-10, width, height-10);
        }
    }
}
