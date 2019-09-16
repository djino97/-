using System;


namespace WindowsFormsApp2
{
    struct RGB
    {
        public float R;
        public float G;
        public float B;
    }

    class AdaptivSharpness
    {
        int i, j, k, m, gap;
        int tmpH, tmpW;       // размеры временной матрицы 
        int widthtMatrix, heightMatrix; // размер входной матрицы изображения
        int matrixSize;

        double experimentalConstant;
        double[,] Hg;

        UInt32[,] tmpPixel;
        UInt32[,] pixel;
        UInt32[,] newPixel;


        RGB ColorOfPixel;
        RGB ColorOfCell;


        public UInt32[,] MatrixFiltration(int W, int H, UInt32[,] pixel, double k0, int N)
        {
            this.pixel = pixel;
            widthtMatrix = W;
            heightMatrix = H;
            experimentalConstant = k0;
            matrixSize = N;

            gap = (int)(N / 2);
            tmpH = H + 2 * gap; 
            tmpW = W + 2 * gap;

            tmpPixel = new UInt32[tmpH, tmpW];
            newPixel = new UInt32[H, W];

            Hg = new double[N, N];


            if (N == 3)
                Hg = new double[3, 3] { { -1, -1, -1 },
                                        { -1,  9, -1 },
                                        { -1, -1, -1 }
                };

            if (N == 5)
                Hg = new double[5, 5] { {-2,  -2, -2, -2, -2},
                                        { -2,  3,  3,  3, -2},
                                        { -2,  3,  10, 3, -2},
                                        { -2,  3,  3,  3, -2},
                                        { -2, -2, -2, -2, -2}
                };

            if (N == 7)
                Hg = new double[7, 7] { {-2, -2, -2, -2, -2, -2, -2},
                                        {-2,  3,  3,  3,  3,  3,  3},
                                        {-2,  3,  9,  9,  9,  3, -2},
                                        {-2,  3,  9, 15,  9,  3, -2},
                                        {-2,  3,  9,  9,  9,  3, -2},
                                        {-2,  3,  3,  3,  3,  3, -2},
                                        {-2, -2, -2, -2, -2, -2, -2}
                };


            IterationOfAnglesImage();

            IterationExtremeLeftRight();

            IterationExtremeTopBottom();

            IterationExtremeCenter();

            TheCalculatioOfSharpening();

            return newPixel;
        }


        //Заполнение временного расширенного изображения 
        //------------------------------------------------
        //углы
        private void IterationOfAnglesImage()
        {
            for (i = 0; i < gap; i++)
                for (j = 0; j < gap; j++)
                {
                    FillingTemporarilyAdvancedImage();
                }
        }

        private void FillingTemporarilyAdvancedImage()
        {
            tmpPixel[i, j] = pixel[0, 0];
            tmpPixel[i, tmpW - j - 1] = pixel[0, widthtMatrix - 1];
            tmpPixel[tmpH - i - 1, j] = pixel[heightMatrix - 1, 0];
            tmpPixel[tmpH - i - 1, tmpW - j - 1] = pixel[heightMatrix - 1, widthtMatrix - 1];
        }


        //крайние левая и правая стороны
        private void IterationExtremeLeftRight()
        {
            for (i = gap; i < tmpH - gap; i++)
                for (j = 0; j < gap; j++)
                {
                    tmpPixel[i, j] = pixel[i - gap, j];
                    tmpPixel[i, tmpW - j - 1] = pixel[i - gap, widthtMatrix - j - 1];
                }
        }

        //крайние верхняя и нижняя стороны
        private void IterationExtremeTopBottom()
        {
            for (i = 0; i < gap; i++)
                for (j = gap; j < tmpW - gap; j++)
                {
                    tmpPixel[i, j] = pixel[i, j - gap];
                    tmpPixel[tmpH - i - 1, j] = pixel[heightMatrix - i - 1, j - gap];
                }
        }

        //центр
        private void IterationExtremeCenter()
        {
            for (i = 0; i < heightMatrix; i++)
                for (j = 0; j < widthtMatrix; j++)
                    tmpPixel[i + gap, j + gap] = pixel[i, j];
        }

        //---------------------------------------------------


        public void TheCalculatioOfSharpening()
        {
            ColorOfPixel = new RGB();
            ColorOfCell = new RGB();

            double Y = 0, I = 0, Q = 0, Yn = 0;

            for (i = gap; i < tmpH - gap; i++)
                for (j = gap; j < tmpW - gap; j++)
                {
                    double Zg = 0, Dz = 0;

                    SelectionBrightnessAndColorCharacteristic(out Y, out I, out Q);

                    TheCalculatioOfLuminanceComponentAndVarianceOfBrightness(out Zg, out Dz);

                    Yn = TheCalculationNewLuminance(Zg, Dz, Y);

                    CorrectionColorValues(Yn, I, Q);

               }
        }


        private void SelectionBrightnessAndColorCharacteristic(out double Y, out double I, out double Q)
        {
            RGB ColorOfCurPixel = CalculationOfCurColor(tmpPixel[i - gap, j - gap]);

            // яркостная составляющая пикселя
            Y = 0.299 * ColorOfCurPixel.R + 0.587 * ColorOfCurPixel.G + 0.114 * ColorOfCurPixel.B;

            // цветовые составляющие пикселя
            I = 0.596 * ColorOfCurPixel.R - 0.275 * ColorOfCurPixel.G - 0.321 * ColorOfCurPixel.B; 
            Q = 0.212 * ColorOfCurPixel.R - 0.523 * ColorOfCurPixel.G + 0.311 * ColorOfCurPixel.B;    
        }


        private void TheCalculatioOfLuminanceComponentAndVarianceOfBrightness(out double Zg, out double Dz)
        {
            RGB ColorOfCellDz = new RGB();
            Zg = Dz = 0;

            // Проход по области N x N
            for (k = 0; k < matrixSize; k++)    
                for (m = 0; m < matrixSize; m++)
                {
                    // яркостная составляющая в результате свертки с Гауссианом
                    ColorOfCell = CalculationOfCurColorHg(tmpPixel[i - gap + k, j - gap + m], Hg[k, m]);
                    Zg += 0.299 * ColorOfCell.R + 0.587 * ColorOfCell.G + 0.114 * ColorOfCell.B;

                    // дисперсия яркости окрестности обрабатываемого пикселя
                    ColorOfCellDz = CalculationOfCurColor(tmpPixel[i - gap + k, j - gap + m]);
                    Dz += 0.299 * ColorOfCellDz.R + 0.587 * ColorOfCellDz.G + 0.114 * ColorOfCellDz.B;
                }
        }


        public static RGB CalculationOfCurColorHg(UInt32 pixel, double Hg)
        {
            RGB Color = new RGB()
            {
                R = (float)(Hg * ((pixel & 0x00FF0000) >> 16)),
                G = (float)(Hg * ((pixel & 0x0000FF00) >> 8)),
                B = (float)(Hg * (pixel & 0x000000FF))
            };
            return Color;
        }


        //вычисление текущего цвета
        public static RGB CalculationOfCurColor(UInt32 pixel)
        {
            RGB Color = new RGB()
            {
                R = (pixel & 0x00FF0000) >> 16,
                G = (pixel & 0x0000FF00) >> 8,
                B = (pixel & 0x000000FF)
            };
            return Color;
        }


        private double TheCalculationNewLuminance(double Zg, double Dz, double Z)
        {
            double Kg;
            //int u = 100; // средняя яркость всего изображения

            Kg = (experimentalConstant) / Math.Sqrt(Dz); // Вычисление коэфициента усиления резкости

            return Zg + Kg * (Z - Zg); //  яркостная составляющая после свертки с гауссианом
        }


        private void CorrectionColorValues(double Yn, double I, double Q)
        {
            ColorOfCell = CalculationOfNewColor(tmpPixel[i - gap, j - gap], Yn, I, Q);
            ColorOfPixel.R = ColorOfCell.R;
            ColorOfPixel.G = ColorOfCell.G;
            ColorOfPixel.B = ColorOfCell.B;

            // Коррекция значения
            if (ColorOfPixel.R < 0) ColorOfPixel.R = 0;
            else if (ColorOfPixel.R > 255) ColorOfPixel.R = 255;

            if (ColorOfPixel.G < 0) ColorOfPixel.G = 0;
            else if (ColorOfPixel.G > 255) ColorOfPixel.G = 255;

            if (ColorOfPixel.B < 0) ColorOfPixel.B = 0;
            else if (ColorOfPixel.B > 255) ColorOfPixel.B = 255;

            // Сборка каналов
            newPixel[i - gap, j - gap] = Build(ColorOfPixel);
        }


        //вычисление нового цвета
        public static RGB CalculationOfNewColor(UInt32 pixel, double Yn, double I, double Q)
        {
            RGB Color = new RGB()
            {
                R = (float)(Yn + 0.956 * I + 0.621 * Q),
                G = (float)(Yn - 0.272 * I - 0.647 * Q),
                B = (float)(Yn - 1.107 * I + 1.704 * Q)
            };
            return Color;
        }


        //сборка каналов
        public static UInt32 Build(RGB ColorOfPixel)
        {
            UInt32 Color = 0xFF000000 | ((UInt32)ColorOfPixel.R << 16) | ((UInt32)ColorOfPixel.G << 8) | ((UInt32)ColorOfPixel.B);
            return Color;
        }
    }


}
