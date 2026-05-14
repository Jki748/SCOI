using System.Drawing.Imaging;
public struct Complex
{
    public double Re;
    public double Im;

    public Complex(double re, double im)
    {
        Re = re;
        Im = im;
    }
    public double Magnitude => Math.Sqrt(Re * Re + Im * Im);

    public static Complex operator +(Complex a, Complex b) => new Complex(a.Re + b.Re, a.Im + b.Im);
    public static Complex operator -(Complex a, Complex b) => new Complex(a.Re - b.Re, a.Im - b.Im);
    public static Complex operator *(Complex a, Complex b) => new Complex(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
    public static Complex operator *(double a, Complex b) => new Complex(a * b.Re, a * b.Im);

    public static Complex Exp(double angle) => new Complex(Math.Cos(angle), Math.Sin(angle));
}

namespace laboratornaja1
{
    public partial class Form1 : Form
    {
        // Хранение спектра между операциями
        private Complex[,] currentSpectrum = null;
        private int currentWidth = 0;
        private int currentHeight = 0;
        private string imagePath1;
        private string imagePath2;
        private OpenFileDialog openFileDialog;

        public Form1()
        {
            InitializeComponent();

            pictureBox6.BorderStyle = BorderStyle.FixedSingle;

            label14.Text = "Фурье-спектр";
            label14.ForeColor = Color.Black;
            label14.Font = new Font(label14.Font, FontStyle.Bold);

            comboBox4.Items.Clear();
            comboBox4.Items.AddRange(new string[] {
                "1. Гаврилова",
                "2. Отсу",
                "3. Ниблека",
                "4. Сауволы",
                "5. Вульфа",
                "6. Брэдли-Рота"
            });
            comboBox4.SelectedIndex = 0;

            numericUpDown1.Minimum = 3;
            numericUpDown1.Maximum = 101;
            numericUpDown1.Value = 15;
            numericUpDown1.Increment = 2;

            numericUpDown2.Minimum = -100;
            numericUpDown2.Maximum = 0;
            numericUpDown2.DecimalPlaces = 2;
            numericUpDown2.Increment = 0.05M;
            numericUpDown2.Value = -0.2M;

            numericUpDown3.Minimum = 0;
            numericUpDown3.Maximum = 100;
            numericUpDown3.DecimalPlaces = 2;
            numericUpDown3.Increment = 0.05M;
            numericUpDown3.Value = 0.2M;

            numericUpDown4.Minimum = 0;
            numericUpDown4.Maximum = 50;
            numericUpDown4.DecimalPlaces = 2;
            numericUpDown4.Increment = 0.01M;
            numericUpDown4.Value = 0.15M;

            numericUpDown5.Minimum = 0;
            numericUpDown5.Maximum = 100;
            numericUpDown5.DecimalPlaces = 2;
            numericUpDown5.Increment = 0.05M;
            numericUpDown5.Value = 0.5M;

            numericUpDown6.Minimum = 1;
            numericUpDown6.Maximum = 15;
            numericUpDown6.Value = 3;
            numericUpDown6.Increment = 2;

            numericUpDown7.Minimum = 1;
            numericUpDown7.Maximum = 25;
            numericUpDown7.Value = 3;
            numericUpDown7.Increment = 2;

            button6.Click += button6_Click;
            button7.Click += button7_Click;
            button8.Click += button8_Click;
            button9.Click += button9_Click;
            button10.Click += button10_Click;
            button11.Click += button11_Click;
            button12.Click += button12_Click;
            button13.Click += button13_Click;

            DrawEmptyHistograms();

            this.Text = "Обработка изображений";

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog.Title = "Выберите изображение";

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;

            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new string[] {
                "Попиксельная сумма",
                "Произведение",
                "Среднее арифметическое",
                "Минимум",
                "Максимум",
                "Наложение маски"
            });
            comboBox1.SelectedIndex = 0;

            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new string[] {
                "Круг",
                "Квадрат",
                "Прямоугольник"
            });
            comboBox2.SelectedIndex = 0;

            InitializeGradationComboBox();

            InitializeKernelTable();

            button15.Click += button15_Click;
            button16.Click += button16_Click;
        }
        private double[,] ImageToDoubleArray(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            double[,] result = new double[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    result[y, x] = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
                }
            }
            return result;
        }
        private void CenterImage(double[,] data)
        {
            int h = data.GetLength(0);
            int w = data.GetLength(1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if ((x + y) % 2 == 1)
                        data[y, x] = -data[y, x];
                }
            }
        }
        private Complex[] DFT1D(double[] signal, bool inverse)
        {
            int N = signal.Length;
            Complex[] result = new Complex[N];
            double sign = inverse ? 1.0 : -1.0;
            double norm = inverse ? 1.0 : 1.0 / N;

            for (int u = 0; u < N; u++)
            {
                Complex sum = new Complex(0, 0);
                for (int k = 0; k < N; k++)
                {
                    double angle = sign * 2.0 * Math.PI * u * k / N;
                    Complex exp = Complex.Exp(angle);
                    sum += (signal[k] * exp);
                }
                result[u] = norm * sum;
            }
            return result;
        }
        private Complex[,] DFT2D(double[,] image)
        {
            int h = image.GetLength(0);
            int w = image.GetLength(1);

            double[,] data = new double[h, w];
            Array.Copy(image, data, image.Length);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if ((x + y) % 2 == 1)
                        data[y, x] = -data[y, x];
                }
            }

            Complex[,] temp = new Complex[h, w];
            for (int y = 0; y < h; y++)
            {
                Complex[] row = new Complex[w];
                for (int x = 0; x < w; x++)
                    row[x] = new Complex(data[y, x], 0);

                Complex[] dftRow = DFT1DComplex(row);
                for (int x = 0; x < w; x++)
                    temp[y, x] = dftRow[x];
            }
            Complex[,] result = new Complex[h, w];
            for (int x = 0; x < w; x++)
            {
                Complex[] col = new Complex[h];
                for (int y = 0; y < h; y++)
                    col[y] = temp[y, x];

                Complex[] dftCol = DFT1DComplex(col);
                for (int y = 0; y < h; y++)
                    result[y, x] = dftCol[y];
            }

            return result;
        }
        private Complex[] DFT1DComplex(Complex[] signal)
        {
            int N = signal.Length;
            Complex[] result = new Complex[N];

            for (int u = 0; u < N; u++)
            {
                Complex sum = new Complex(0, 0);
                for (int k = 0; k < N; k++)
                {
                    double angle = -2.0 * Math.PI * u * k / N;
                    Complex exp = Complex.Exp(angle);
                    sum += signal[k] * exp;
                }
                result[u] = new Complex(sum.Re / N, sum.Im / N);
            }
            return result;
        }
        private double[,] InverseDFT2D(Complex[,] spectrum)
        {
            int h = spectrum.GetLength(0);
            int w = spectrum.GetLength(1);

            Complex[,] temp = new Complex[h, w];

            for (int y = 0; y < h; y++)
            {
                Complex[] row = new Complex[w];
                for (int x = 0; x < w; x++)
                    row[x] = spectrum[y, x];

                Complex[] idftRow = InverseDFT1DComplex(row);
                for (int x = 0; x < w; x++)
                    temp[y, x] = idftRow[x];
            }

            Complex[,] resultComplex = new Complex[h, w];
            for (int x = 0; x < w; x++)
            {
                Complex[] col = new Complex[h];
                for (int y = 0; y < h; y++)
                    col[y] = temp[y, x];

                Complex[] idftCol = InverseDFT1DComplex(col);
                for (int y = 0; y < w; y++)
                    resultComplex[y, x] = idftCol[y];
            }

            double[,] result = new double[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    result[y, x] = resultComplex[y, x].Re;
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if ((x + y) % 2 == 1)
                        result[y, x] = -result[y, x];
                }
            }

            return result;
        }
        private Complex[] InverseDFT1DComplex(Complex[] signal)
        {
            int N = signal.Length;
            Complex[] result = new Complex[N];

            for (int u = 0; u < N; u++)
            {
                Complex sum = new Complex(0, 0);
                for (int k = 0; k < N; k++)
                {
                    double angle = 2.0 * Math.PI * u * k / N;
                    Complex exp = Complex.Exp(angle);
                    sum += signal[k] * exp;
                }
                result[u] = new Complex(sum.Re / N, sum.Im / N);
            }
            return result;
        }
        private Bitmap VisualizeSpectrum(Complex[,] spectrum)
        {
            int h = spectrum.GetLength(0);
            int w = spectrum.GetLength(1);

            double maxLog = 0;
            double[,] logMagnitude = new double[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double mag = spectrum[y, x].Magnitude;
                    double logVal = Math.Log(mag + 1);
                    logMagnitude[y, x] = logVal;
                    if (logVal > maxLog) maxLog = logVal;
                }
            }

            Bitmap result = new Bitmap(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double normalized = (logMagnitude[y, x] / maxLog) * 255;
                    if (normalized > 255) normalized = 255;
                    if (normalized < 0) normalized = 0;
                    byte val = (byte)normalized;
                    result.SetPixel(x, y, Color.FromArgb(val, val, val));
                }
            }

            return result;
        }
        private Bitmap DoubleArrayToBitmap(double[,] data)
        {
            int h = data.GetLength(0);
            int w = data.GetLength(1);

            double min = double.MaxValue;
            double max = double.MinValue;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (data[y, x] < min) min = data[y, x];
                    if (data[y, x] > max) max = data[y, x];
                }
            }

            double range = max - min;
            if (range < 0.0001) range = 1;

            Bitmap result = new Bitmap(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double normalized = (data[y, x] - min) / range * 255;
                    if (normalized > 255) normalized = 255;
                    if (normalized < 0) normalized = 0;
                    byte val = (byte)normalized;
                    result.SetPixel(x, y, Color.FromArgb(val, val, val));
                }
            }

            return result;
        }
        private Bitmap ResizeForDFT(Bitmap src, int maxSize = 256)
        {
            int newW = src.Width;
            int newH = src.Height;

            if (newW > maxSize) newW = maxSize;
            if (newH > maxSize) newH = maxSize;

            if (newW == src.Width && newH == src.Height)
                return new Bitmap(src);

            return new Bitmap(src, new Size(newW, newH));
        }
        private void button14_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Сначала выберите изображение 1!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                button14.Enabled = false;
                button15.Enabled = false;
                button16.Enabled = false;
                Cursor = Cursors.WaitCursor;

                Bitmap original = new Bitmap(pictureBox1.Image);
                Bitmap resized = ResizeForDFT(original, 256);

                double[,] grayImage = ImageToDoubleArray(resized);

                currentSpectrum = DFT2D(grayImage);
                currentWidth = resized.Width;
                currentHeight = resized.Height;

                MessageBox.Show($"Фурье-образ вычислен!\n", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);

                button15.Enabled = true;
                button16.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button15.Enabled = false;
                button16.Enabled = false;
            }
            finally
            {
                button14.Enabled = true;
                Cursor = Cursors.Default;
            }
        }
        private void button15_Click(object sender, EventArgs e)
        {
            if (currentSpectrum == null)
            {
                MessageBox.Show("Сначала вычислите Фурье-образ (кнопка 1)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                Bitmap spectrumImage = VisualizeSpectrum(currentSpectrum);
                pictureBox6.Image = spectrumImage;
                MessageBox.Show("Спектр визуализирован!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void button16_Click(object sender, EventArgs e)
        {
            if (currentSpectrum == null)
            {
                MessageBox.Show("Сначала вычислите Фурье-образ (кнопка 1)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                double[,] restoredImage = InverseDFT2D(currentSpectrum);
                Bitmap restoredBitmap = DoubleArrayToBitmap(restoredImage);

                pictureBox3.Image = restoredBitmap;
                MessageBox.Show("Изображение восстановлено из спектра!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


        private void InitializeKernelTable()
        {
            int size = (int)numericUpDown6.Value;
            CreateKernelTable(size);
            SetKernelToOnes();
        }

        private void CreateKernelTable(int size)
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnCount = size;
            tableLayoutPanel1.RowCount = size;

            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            for (int i = 0; i < size; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / size));
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / size));
            }

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    NumericUpDown num = new NumericUpDown
                    {
                        DecimalPlaces = 5,
                        Increment = 0.1M,
                        Minimum = -1000M,
                        Maximum = 1000M,
                        Value = 1M,
                        TextAlign = HorizontalAlignment.Center,
                        Dock = DockStyle.Fill,
                        Font = new Font("Consolas", 9)
                    };
                    tableLayoutPanel1.Controls.Add(num, col, row);
                }
            }
        }

        private double[,] GetKernelFromTable()
        {
            int size = tableLayoutPanel1.ColumnCount;
            double[,] kernel = new double[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    NumericUpDown num = (NumericUpDown)tableLayoutPanel1.GetControlFromPosition(col, row);
                    kernel[row, col] = (double)num.Value;
                }
            }
            return kernel;
        }

        private void SetKernelToTable(double[,] kernel)
        {
            int size = kernel.GetLength(0);
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    NumericUpDown num = (NumericUpDown)tableLayoutPanel1.GetControlFromPosition(col, row);
                    if (num != null)
                        num.Value = (decimal)kernel[row, col];
                }
            }
        }

        private void SetKernelToOnes()
        {
            int size = tableLayoutPanel1.ColumnCount;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    NumericUpDown num = (NumericUpDown)tableLayoutPanel1.GetControlFromPosition(col, row);
                    if (num != null)
                        num.Value = 1;
                }
            }
        }

        private void SetKernelToIdentity()
        {
            int size = tableLayoutPanel1.ColumnCount;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    NumericUpDown num = (NumericUpDown)tableLayoutPanel1.GetControlFromPosition(col, row);
                    if (num != null)
                        num.Value = (row == col) ? 1 : 0;
                }
            }
        }

        private void NormalizeKernelInTable()
        {
            double[,] kernel = GetKernelFromTable();
            double sum = 0;
            int size = kernel.GetLength(0);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    sum += kernel[i, j];

            if (Math.Abs(sum) < 0.0001) return;

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= sum;

            SetKernelToTable(kernel);
        }

        private void SetGaussianKernel13()
        {
            double[,] gauss = new double[13, 13]
            {
                { 0.00032, 0.00060, 0.00098, 0.00145, 0.00192, 0.00226, 0.00239, 0.00226, 0.00192, 0.00145, 0.00098, 0.00060, 0.00032 },
                { 0.00060, 0.00110, 0.00181, 0.00267, 0.00353, 0.00417, 0.00441, 0.00417, 0.00353, 0.00267, 0.00181, 0.00110, 0.00060 },
                { 0.00098, 0.00181, 0.00299, 0.00441, 0.00582, 0.00688, 0.00727, 0.00688, 0.00582, 0.00441, 0.00299, 0.00181, 0.00098 },
                { 0.00145, 0.00267, 0.00441, 0.00651, 0.00859, 0.01015, 0.01073, 0.01015, 0.00859, 0.00651, 0.00441, 0.00267, 0.00145 },
                { 0.00192, 0.00353, 0.00582, 0.00859, 0.01134, 0.01339, 0.01416, 0.01339, 0.01134, 0.00859, 0.00582, 0.00353, 0.00192 },
                { 0.00226, 0.00417, 0.00688, 0.01015, 0.01339, 0.01582, 0.01673, 0.01582, 0.01339, 0.01015, 0.00688, 0.00417, 0.00226 },
                { 0.00239, 0.00441, 0.00727, 0.01073, 0.01416, 0.01673, 0.01768, 0.01673, 0.01416, 0.01073, 0.00727, 0.00441, 0.00239 },
                { 0.00226, 0.00417, 0.00688, 0.01015, 0.01339, 0.01582, 0.01673, 0.01582, 0.01339, 0.01015, 0.00688, 0.00417, 0.00226 },
                { 0.00192, 0.00353, 0.00582, 0.00859, 0.01134, 0.01339, 0.01416, 0.01339, 0.01134, 0.00859, 0.00582, 0.00353, 0.00192 },
                { 0.00145, 0.00267, 0.00441, 0.00651, 0.00859, 0.01015, 0.01073, 0.01015, 0.00859, 0.00651, 0.00441, 0.00267, 0.00145 },
                { 0.00098, 0.00181, 0.00299, 0.00441, 0.00582, 0.00688, 0.00727, 0.00688, 0.00582, 0.00441, 0.00299, 0.00181, 0.00098 },
                { 0.00060, 0.00110, 0.00181, 0.00267, 0.00353, 0.00417, 0.00441, 0.00417, 0.00353, 0.00267, 0.00181, 0.00110, 0.00060 },
                { 0.00032, 0.00060, 0.00098, 0.00145, 0.00192, 0.00226, 0.00239, 0.00226, 0.00192, 0.00145, 0.00098, 0.00060, 0.00032 }
            };
            numericUpDown6.Value = 13;
            CreateKernelTable(13);
            SetKernelToTable(gauss);
        }

        private Color GetPixelMirror(Bitmap bmp, int x, int y)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            while (x < 0) x = -x - 1;
            while (x >= w) x = w - (x - w) - 1;
            while (y < 0) y = -y - 1;
            while (y >= h) y = h - (y - h) - 1;
            return bmp.GetPixel(x, y);
        }

        private Bitmap ApplyLinearFilter(Bitmap src, double[,] kernel)
        {
            int w = src.Width, h = src.Height;
            int ksize = kernel.GetLength(0);
            int off = ksize / 2;

            Bitmap dst = new Bitmap(w, h);

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int stride = srcData.Stride;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        double sumR = 0, sumG = 0, sumB = 0;

                        for (int ky = -off; ky <= off; ky++)
                        {
                            for (int kx = -off; kx <= off; kx++)
                            {
                                int ix = x + kx;
                                int iy = y + ky;

                                while (ix < 0) ix = -ix - 1;
                                while (ix >= w) ix = w - (ix - w) - 1;
                                while (iy < 0) iy = -iy - 1;
                                while (iy >= h) iy = h - (iy - h) - 1;

                                byte* pixel = srcPtr + iy * stride + ix * 4;
                                double v = kernel[ky + off, kx + off];
                                sumB += pixel[0] * v;
                                sumG += pixel[1] * v;
                                sumR += pixel[2] * v;
                            }
                        }

                        int rr = (int)Math.Round(sumR);
                        int gg = (int)Math.Round(sumG);
                        int bb = (int)Math.Round(sumB);

                        if (rr < 0) rr = 0; if (rr > 255) rr = 255;
                        if (gg < 0) gg = 0; if (gg > 255) gg = 255;
                        if (bb < 0) bb = 0; if (bb > 255) bb = 255;

                        byte* dstPixel = dstPtr + y * stride + x * 4;
                        dstPixel[0] = (byte)bb;
                        dstPixel[1] = (byte)gg;
                        dstPixel[2] = (byte)rr;
                        dstPixel[3] = 255;
                    }
                }
            }

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }

        private Bitmap ApplyMedianFilter(Bitmap src, int windowSize)
        {
            int w = src.Width, h = src.Height;
            int off = windowSize / 2;
            int total = windowSize * windowSize;
            Bitmap dst = new Bitmap(w, h);

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int stride = srcData.Stride;

                byte[] reds = new byte[total];
                byte[] greens = new byte[total];
                byte[] blues = new byte[total];

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int idx = 0;
                        for (int wy = -off; wy <= off; wy++)
                        {
                            for (int wx = -off; wx <= off; wx++)
                            {
                                int ix = x + wx;
                                int iy = y + wy;

                                while (ix < 0) ix = -ix - 1;
                                while (ix >= w) ix = w - (ix - w) - 1;
                                while (iy < 0) iy = -iy - 1;
                                while (iy >= h) iy = h - (iy - h) - 1;

                                byte* pixel = srcPtr + iy * stride + ix * 4;
                                blues[idx] = pixel[0];
                                greens[idx] = pixel[1];
                                reds[idx] = pixel[2];
                                idx++;
                            }
                        }

                        Array.Sort(reds);
                        Array.Sort(greens);
                        Array.Sort(blues);

                        int medIdx = total / 2;
                        byte* dstPixel = dstPtr + y * stride + x * 4;
                        dstPixel[0] = blues[medIdx];
                        dstPixel[1] = greens[medIdx];
                        dstPixel[2] = reds[medIdx];
                        dstPixel[3] = 255;
                    }
                }
            }

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            int newSize = (int)numericUpDown6.Value;
            if (newSize % 2 == 0) newSize++;
            numericUpDown6.Value = newSize;
            CreateKernelTable(newSize);
            SetKernelToOnes();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SetKernelToOnes();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            SetKernelToIdentity();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            SetGaussianKernel13();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            NormalizeKernelInTable();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Сначала выберите изображение 1!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                pictureBox3.Image = ApplyLinearFilter(new Bitmap(pictureBox1.Image), GetKernelFromTable());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Сначала выберите изображение 1!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                int size = (int)numericUpDown7.Value;
                if (size % 2 == 0) size++;
                pictureBox3.Image = ApplyMedianFilter(new Bitmap(pictureBox1.Image), size);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawEmptyHistograms()
        {
            int w4 = pictureBox4.Width, h4 = pictureBox4.Height;
            Bitmap emptyHist4 = new Bitmap(w4, h4);
            using (Graphics g = Graphics.FromImage(emptyHist4))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.Black, 1)) g.DrawRectangle(pen, 0, 0, w4 - 1, h4 - 1);
            }
            pictureBox4.Image = emptyHist4;
            pictureBox4.BackColor = SystemColors.Control;

            int w5 = pictureBox5.Width, h5 = pictureBox5.Height;
            Bitmap emptyHist5 = new Bitmap(w5, h5);
            using (Graphics g = Graphics.FromImage(emptyHist5))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.Black, 1)) g.DrawRectangle(pen, 0, 0, w5 - 1, h5 - 1);
            }
            pictureBox5.Image = emptyHist5;
            pictureBox5.BackColor = SystemColors.Control;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    imagePath1 = openFileDialog.FileName;
                    pictureBox1.Image = new Bitmap(imagePath1);
                    CheckImageSizes();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка при загрузке изображения: " + ex.Message); }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    imagePath2 = openFileDialog.FileName;
                    pictureBox2.Image = new Bitmap(imagePath2);
                    CheckImageSizes();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка при загрузке изображения: " + ex.Message); }
            }
        }

        private void CheckImageSizes()
        {
            if (pictureBox1.Image != null && pictureBox2.Image != null)
            {
                Size s1 = pictureBox1.Image.Size, s2 = pictureBox2.Image.Size;
                if (s1 != s2)
                    MessageBox.Show($"ВНИМАНИЕ: изображения разного размера!\n{s1.Width}x{s1.Height} vs {s2.Width}x{s2.Height}", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string op = comboBox1.SelectedItem.ToString();
            if (op == "Наложение маски")
            {
                if (pictureBox1.Image == null) { MessageBox.Show("Выберите изображение для маски!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            }
            else
            {
                if (pictureBox1.Image == null || pictureBox2.Image == null) { MessageBox.Show("Выберите оба изображения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            }
            try
            {
                if (op != "Наложение маски")
                {
                    if (pictureBox1.Image.Size != pictureBox2.Image.Size) { MessageBox.Show("Изображения разного размера!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                }
                Size rs = pictureBox1.Image.Size;
                Bitmap res = new Bitmap(rs.Width, rs.Height);
                Bitmap img1 = new Bitmap(pictureBox1.Image);
                Bitmap img2 = (pictureBox2.Image != null) ? new Bitmap(pictureBox2.Image) : null;
                Bitmap mask = null;
                if (op == "Наложение маски") mask = GenerateMask(rs.Width, rs.Height, comboBox2.SelectedItem.ToString());

                for (int x = 0; x < rs.Width; x++)
                {
                    for (int y = 0; y < rs.Height; y++)
                    {
                        Color c1 = img1.GetPixel(x, y);
                        Color c2 = (img2 != null) ? img2.GetPixel(x, y) : Color.Black;
                        Color nc = Color.Black;
                        switch (op)
                        {
                            case "Попиксельная сумма":
                                nc = Color.FromArgb(Math.Min(c1.R + c2.R, 255), Math.Min(c1.G + c2.G, 255), Math.Min(c1.B + c2.B, 255)); break;
                            case "Произведение":
                                nc = Color.FromArgb((int)(c1.R * (c2.R / 255.0)), (int)(c1.G * (c2.G / 255.0)), (int)(c1.B * (c2.B / 255.0))); break;
                            case "Среднее арифметическое":
                                nc = Color.FromArgb((c1.R + c2.R) / 2, (c1.G + c2.G) / 2, (c1.B + c2.B) / 2); break;
                            case "Минимум":
                                nc = Color.FromArgb(Math.Min(c1.R, c2.R), Math.Min(c1.G, c2.G), Math.Min(c1.B, c2.B)); break;
                            case "Максимум":
                                nc = Color.FromArgb(Math.Max(c1.R, c2.R), Math.Max(c1.G, c2.G), Math.Max(c1.B, c2.B)); break;
                            case "Наложение маски":
                                nc = (mask.GetPixel(x, y).R > 128) ? c1 : c2; break;
                        }
                        res.SetPixel(x, y, nc);
                    }
                }
                pictureBox3.Image = res;
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private Bitmap GenerateMask(int width, int height, string shape)
        {
            Bitmap mask = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(mask))
            {
                g.Clear(Color.Black);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (SolidBrush white = new SolidBrush(Color.White))
                {
                    if (shape == "Круг") g.FillEllipse(white, 0, 0, width, height);
                    else if (shape == "Квадрат")
                    {
                        int side = Math.Min(width, height);
                        int x0 = (width - side) / 2, y0 = (height - side) / 2;
                        g.FillRectangle(white, x0, y0, side, side);
                    }
                    else if (shape == "Прямоугольник")
                    {
                        int rw = width / 2, rh = height / 2;
                        int x0 = (width - rw) / 2, y0 = (height - rh) / 2;
                        g.FillRectangle(white, x0, y0, rw, rh);
                    }
                }
            }
            return mask;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image == null) { MessageBox.Show("Нет результата!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp";
            sfd.FileName = "result.png";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try { pictureBox3.Image.Save(sfd.FileName); }
                catch (Exception ex) { MessageBox.Show("Ошибка сохранения: " + ex.Message); }
            }
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }

        private void InitializeGradationComboBox()
        {
            comboBox3.Items.Clear();
            comboBox3.Items.AddRange(new string[] { "1. Затемнение (квадрат)", "2. Осветление (корень)", "3. Инверсия (негатив)", "4. Контрастность" });
            comboBox3.SelectedIndex = 0;
            button5.Click += button5_Click;
        }

        private byte TransformPixel(byte r, int type)
        {
            double norm = r / 255.0;
            switch (type)
            {
                case 0: norm = norm * norm; break;
                case 1: norm = Math.Sqrt(norm); break;
                case 2: return (byte)(255 - r);
                case 3: norm = (norm - 0.5) * 1.5 + 0.5; if (norm < 0) norm = 0; if (norm > 1) norm = 1; break;
                default: return r;
            }
            return (byte)(norm * 255);
        }

        private Bitmap ApplyGradationTransform(Bitmap src, int type)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);
                    dst.SetPixel(x, y, Color.FromArgb(TransformPixel(c.R, type), TransformPixel(c.G, type), TransformPixel(c.B, type)));
                }
            return dst;
        }

        private int[] ComputeHistogram(Bitmap bmp)
        {
            int[] hist = new int[256];
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    int bright = (bmp.GetPixel(x, y).R + bmp.GetPixel(x, y).G + bmp.GetPixel(x, y).B) / 3;
                    hist[bright]++;
                }
            return hist;
        }

        private Bitmap DrawHistogram(int[] hist, int w, int h, Color color)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                int max = hist.Max();
                if (max == 0) return bmp;
                float scale = (float)h / max;
                int bw = Math.Max(1, w / 256);
                for (int i = 0; i < 256; i++)
                {
                    int bh = (int)(hist[i] * scale);
                    if (bh > h) bh = h;
                    using (SolidBrush br = new SolidBrush(color))
                        g.FillRectangle(br, i * bw, h - bh, bw, bh);
                }
                using (Pen pen = new Pen(Color.Black)) g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
            }
            return bmp;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) { MessageBox.Show("Выберите изображение 1!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            try
            {
                Bitmap orig = new Bitmap(pictureBox1.Image);
                Bitmap res = ApplyGradationTransform(orig, comboBox3.SelectedIndex);
                pictureBox3.Image = res;
                int[] h_orig = ComputeHistogram(orig), h_res = ComputeHistogram(res);
                pictureBox4.Image = DrawHistogram(h_orig, pictureBox4.Width, pictureBox4.Height, Color.Blue);
                pictureBox5.Image = DrawHistogram(h_res, pictureBox5.Width, pictureBox5.Height, Color.Red);
                pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private byte ToGrayscale(Color c) => (byte)(0.2125 * c.R + 0.7154 * c.G + 0.0721 * c.B);

        private byte[,] ImageToByteArray(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            byte[,] arr = new byte[h, w];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    arr[y, x] = ToGrayscale(bmp.GetPixel(x, y));
            return arr;
        }

        private Bitmap ByteArrayToBitmap(byte[,] data)
        {
            int h = data.GetLength(0), w = data.GetLength(1);
            Bitmap bmp = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    byte v = data[y, x];
                    bmp.SetPixel(x, y, Color.FromArgb(v, v, v));
                }
            return bmp;
        }

        private byte[,] BinarizeGavrilov(byte[,] img)
        {
            int h = img.GetLength(0), w = img.GetLength(1);
            long sum = 0;
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) sum += img[y, x];
            byte th = (byte)(sum / (w * h));
            byte[,] res = new byte[h, w];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) res[y, x] = img[y, x] <= th ? (byte)0 : (byte)255;
            return res;
        }

        private byte[,] BinarizeOtsu(byte[,] img)
        {
            int h = img.GetLength(0), w = img.GetLength(1), total = w * h;
            int[] hist = new int[256];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) hist[img[y, x]]++;
            double[] prob = new double[256];
            for (int i = 0; i < 256; i++) prob[i] = (double)hist[i] / total;
            double totalMean = 0;
            for (int i = 0; i < 256; i++) totalMean += i * prob[i];
            double maxVar = 0;
            int best = 0;
            double w1 = 0, mean1 = 0;
            for (int t = 0; t < 256; t++)
            {
                w1 += prob[t];
                if (w1 == 0) continue;
                double w2 = 1 - w1;
                if (w2 == 0) break;
                mean1 += t * prob[t];
                double mean2 = (totalMean - mean1) / w2;
                double var = w1 * w2 * (mean1 / w1 - mean2) * (mean1 / w1 - mean2);
                if (var > maxVar) { maxVar = var; best = t; }
            }
            byte th = (byte)best;
            byte[,] res = new byte[h, w];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) res[y, x] = img[y, x] <= th ? (byte)0 : (byte)255;
            return res;
        }

        private byte[,] BinarizeNiblack(byte[,] img, int ws, double k)
        {
            int h = img.GetLength(0), w = img.GetLength(1), off = ws / 2;
            byte[,] res = new byte[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int x1 = Math.Max(0, x - off), y1 = Math.Max(0, y - off);
                    int x2 = Math.Min(w - 1, x + off), y2 = Math.Min(h - 1, y + off);
                    int cnt = 0; long sum = 0, sumSq = 0;
                    for (int j = y1; j <= y2; j++)
                        for (int i = x1; i <= x2; i++)
                        {
                            byte val = img[j, i];
                            sum += val; sumSq += val * val; cnt++;
                        }
                    double mean = (double)sum / cnt;
                    double var = ((double)sumSq / cnt) - mean * mean;
                    double th = mean + k * Math.Sqrt(var);
                    res[y, x] = img[y, x] <= th ? (byte)0 : (byte)255;
                }
            }
            return res;
        }

        private byte[,] BinarizeSauvola(byte[,] img, int ws, double k)
        {
            int h = img.GetLength(0), w = img.GetLength(1), off = ws / 2;
            double R = 128.0;
            byte[,] res = new byte[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int x1 = Math.Max(0, x - off), y1 = Math.Max(0, y - off);
                    int x2 = Math.Min(w - 1, x + off), y2 = Math.Min(h - 1, y + off);
                    int cnt = 0; long sum = 0, sumSq = 0;
                    for (int j = y1; j <= y2; j++)
                        for (int i = x1; i <= x2; i++)
                        {
                            byte val = img[j, i];
                            sum += val; sumSq += val * val; cnt++;
                        }
                    double mean = (double)sum / cnt;
                    double var = ((double)sumSq / cnt) - mean * mean;
                    double th = mean * (1 + k * ((Math.Sqrt(var) / R) - 1));
                    res[y, x] = img[y, x] <= th ? (byte)0 : (byte)255;
                }
            }
            return res;
        }

        private byte[,] BinarizeWolf(byte[,] img, int ws, double a)
        {
            int h = img.GetLength(0), w = img.GetLength(1), off = ws / 2;
            byte globalMin = 255;
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) if (img[y, x] < globalMin) globalMin = img[y, x];
            double maxStd = 0;
            double[,] means = new double[h, w], stds = new double[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int x1 = Math.Max(0, x - off), y1 = Math.Max(0, y - off);
                    int x2 = Math.Min(w - 1, x + off), y2 = Math.Min(h - 1, y + off);
                    int cnt = 0; long sum = 0, sumSq = 0;
                    for (int j = y1; j <= y2; j++)
                        for (int i = x1; i <= x2; i++)
                        {
                            byte val = img[j, i];
                            sum += val; sumSq += val * val; cnt++;
                        }
                    means[y, x] = (double)sum / cnt;
                    double var = ((double)sumSq / cnt) - means[y, x] * means[y, x];
                    stds[y, x] = Math.Sqrt(var);
                    if (stds[y, x] > maxStd) maxStd = stds[y, x];
                }
            }
            double R = maxStd; if (R < 0.001) R = 1.0;
            byte[,] res = new byte[h, w];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    double th = (1 - a) * means[y, x] + a * globalMin + a * (stds[y, x] / R) * (means[y, x] - globalMin);
                    res[y, x] = img[y, x] <= th ? (byte)0 : (byte)255;
                }
            return res;
        }

        private long[,] ComputeIntegralImage(byte[,] img)
        {
            int h = img.GetLength(0), w = img.GetLength(1);
            long[,] integral = new long[h, w];
            for (int y = 0; y < h; y++)
            {
                long rowSum = 0;
                for (int x = 0; x < w; x++)
                {
                    rowSum += img[y, x];
                    integral[y, x] = rowSum + (y > 0 ? integral[y - 1, x] : 0);
                }
            }
            return integral;
        }

        private long GetSum(long[,] integral, int x1, int y1, int x2, int y2)
        {
            long a = (x1 > 0 && y1 > 0) ? integral[y1 - 1, x1 - 1] : 0;
            long b = (y1 > 0) ? integral[y1 - 1, x2] : 0;
            long c = (x1 > 0) ? integral[y2, x1 - 1] : 0;
            long d = integral[y2, x2];
            return d - b - c + a;
        }

        private byte[,] BinarizeBradley(byte[,] img, int ws, double k)
        {
            int h = img.GetLength(0), w = img.GetLength(1);
            long[,] integral = ComputeIntegralImage(img);
            int off = ws / 2;
            byte[,] res = new byte[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int x1 = Math.Max(0, x - off), y1 = Math.Max(0, y - off);
                    int x2 = Math.Min(w - 1, x + off), y2 = Math.Min(h - 1, y + off);
                    int cnt = (x2 - x1 + 1) * (y2 - y1 + 1);
                    long sum = GetSum(integral, x1, y1, x2, y2);
                    res[y, x] = (img[y, x] * cnt < sum * (1 - k)) ? (byte)0 : (byte)255;
                }
            }
            return res;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) { MessageBox.Show("Выберите изображение 1!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            try
            {
                Bitmap orig = new Bitmap(pictureBox1.Image);
                byte[,] gray = ImageToByteArray(orig);
                string method = comboBox4.SelectedItem.ToString();
                byte[,] result = null;
                int ws = (int)numericUpDown1.Value; if (ws % 2 == 0) ws++;
                double k_n = (double)numericUpDown2.Value, k_s = (double)numericUpDown3.Value, k_b = (double)numericUpDown4.Value, a_w = (double)numericUpDown5.Value;
                switch (method)
                {
                    case "1. Гаврилова": result = BinarizeGavrilov(gray); break;
                    case "2. Отсу": result = BinarizeOtsu(gray); break;
                    case "3. Ниблека": result = BinarizeNiblack(gray, ws, k_n); break;
                    case "4. Сауволы": result = BinarizeSauvola(gray, ws, k_s); break;
                    case "5. Вульфа": result = BinarizeWolf(gray, ws, a_w); break;
                    case "6. Брэдли-Рота": result = BinarizeBradley(gray, ws, k_b); break;
                    default: MessageBox.Show("Неизвестный метод!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
                }
                pictureBox3.Image = ByteArrayToBitmap(result);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка бинаризации: " + ex.Message); }
        }

        private void label12_Click(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
    }
}