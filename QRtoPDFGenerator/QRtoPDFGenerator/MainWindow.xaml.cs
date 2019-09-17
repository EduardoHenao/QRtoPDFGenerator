namespace QRtoPDFGenerator
{
    using Microsoft.Win32;
    using PdfSharp.Drawing;
    using PdfSharp.Pdf;
    using QRCoder;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // algorithm variables
        private static string SERIES = "AAA";
        private static int QUANTITY = 1;
        private static int STARTING_POINT = 0;
        private static int STARTING_POINT_DEFAULT_VALUE = 0;
        private static int DIGITS = 3;
        private static int DIGITS_DEFAULT_VALUE = 3;
        private static string SEPARATOR = "-";


        // preview variables
        private string resultStart = SERIES + SEPARATOR + STARTING_POINT.ToString("D" + DIGITS);
        private string resultEnd = string.Empty;

        // wpf variables
        private bool endVisible = false;

        //QR lib params
        private int PIXELS_PER_MODULE = 6;
        private static int QR_TAGS_QUANTITY = 5;

        //PDF
        private static int LINES_PER_PAGE = 6;
        private static int PIXELS_BETWEEN_QR = 100;
        private static int OFFSETY_PIXELS_STRING = 20;
        private static int OFFSETY_PIXELS_QR = 100;
        private static int RECT_PIXELS_HEIGHT = 20;

        public MainWindow()
        {
            InitializeComponent();
            this.InitializeAlgoVariables();
        }

        private void InitializeAlgoVariables()
        {
            this.DigitsTextBox.Text = DIGITS.ToString();
            this.QuantityTextBox.Text = QUANTITY.ToString();
            this.StartingNumberTextBox.Text = STARTING_POINT.ToString();
            this.DigitsTextBox.Text = DIGITS.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.GeneratePdfButton.IsEnabled = false;

            // this click will call the generation functions from a worker thread
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var qRContainerList = this.generateQR();
            var document = this.generatePDF(qRContainerList);
            this.saveDocument(document);

            //fire changes for ui components
            this.GeneratePdfButton.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { GeneratePdfButton.IsEnabled = true; }));
        }

        private List<QRContainer> generateQR()
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            List<QRContainer> qRContainers = new List<QRContainer>();

            //qr generation loop
            for (int i = STARTING_POINT; i < STARTING_POINT + QUANTITY; i++)
            {
                string code = this.GenerateCode(i); // reuse the preview function
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(PIXELS_PER_MODULE);
                MemoryStream strm = new MemoryStream();
                qrCodeImage.Save(strm, System.Drawing.Imaging.ImageFormat.Png);
                qRContainers.Add(new QRContainer
                {
                    code = code,
                    bitmap = XImage.FromStream(strm)
            });
            }

            return qRContainers;
        }

        private PdfDocument generatePDF(List<QRContainer> qRContainers)
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Series {this.GenerateCode(0)} to {this.GenerateCode(QUANTITY - 1)}";

            // Create an empty page
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("SansSerif", 10, XFontStyle.BoldItalic);

            // Draw the text
            var lines = 0;
            var offsetY = 0;
            foreach (var qRContainer in qRContainers)
            {
                if (lines >= LINES_PER_PAGE)
                {
                    lines = 0;
                    offsetY = 0;
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                }

                //draw code
                gfx.DrawString(qRContainer.code, font, XBrushes.Black, new XRect(0, offsetY, page.Width, RECT_PIXELS_HEIGHT), XStringFormats.Center);
                offsetY += OFFSETY_PIXELS_STRING;

                //draw qr bitmap
                for (int i = 0; i < QR_TAGS_QUANTITY; i++)
                {
                    gfx.DrawImage(qRContainer.bitmap, new XPoint(i*PIXELS_BETWEEN_QR, offsetY));
                }

                offsetY += OFFSETY_PIXELS_QR;

                //draw line
                gfx.DrawLine(XPens.DarkGray, 0, offsetY, page.Width, offsetY);

                lines++;
            }

            return document;
        }

        public void saveDocument(PdfDocument document)
        {
            // Save the document...
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog().Value == true)
            {
                try
                {
                    document.Save(saveFileDialog1.FileName);
                    Process.Start(saveFileDialog1.FileName);
                }
                catch (System.IO.IOException)
                {
                    //do nothing, the file is already opened, so we catch this
                }
            }
        }

        private void RecalculatePreview()
        {
            this.resultStart = this.GenerateCode(0);
            this.resultEnd = this.GenerateCode(QUANTITY - 1);
            this.endVisible = QUANTITY > 1;
            this.setVisibilityWithQuantity();
        }

        private string GenerateCode(int offset)
        {
            string digit = (STARTING_POINT + offset).ToString("D" + DIGITS);
            if (string.IsNullOrEmpty(SERIES)) return digit;
            return SERIES + SEPARATOR + digit;
        }

        private void setVisibilityWithQuantity()
        {
            if (this.ResultadoDesdeText != null && this.ResultadoHastaText != null)
            {
                this.ResultadoDesdeText.Text = this.resultStart;
                this.ResultadoHastaText.Text = this.resultEnd;

                Visibility visibility = Visibility.Hidden;
                if (this.endVisible) visibility = Visibility.Visible;

                this.ResultadoHastaText.Visibility = visibility;
                this.ResultadoHastaTitle.Visibility = visibility;
            }
        }

        private void DigitsChanged(object sender, TextChangedEventArgs e)
        {
            var digitsTextBox = sender as TextBox;
            if (int.TryParse(digitsTextBox.Text, out int digits))
            {
                if (digits < 0)
                {
                    DIGITS = DIGITS_DEFAULT_VALUE;
                }
                else DIGITS = digits;
            }
            this.RecalculatePreview();
        }

        private void SeriesChanged(object sender, TextChangedEventArgs e)
        {
            var serie = sender as TextBox;
            SERIES = serie.Text;
            this.RecalculatePreview();
        }

        private void StartingPointChanged(object sender, TextChangedEventArgs e)
        {
            var startingPointTextBox = sender as TextBox;
            if (int.TryParse(startingPointTextBox.Text, out int startingPoint))
            {
                if (startingPoint < 0)
                {
                    STARTING_POINT = STARTING_POINT_DEFAULT_VALUE;
                }
                else STARTING_POINT = startingPoint;
            }
            this.RecalculatePreview();
        }

        private void QuantityChanged(object sender, TextChangedEventArgs e)
        {
            var quantityTextBox = sender as TextBox;
            if (int.TryParse(quantityTextBox.Text, out int quantity))
            {
                if (quantity < 0)
                {
                    QUANTITY = 0;
                }
                else QUANTITY = quantity;
            }
            this.RecalculatePreview();
        }

        private void SeparatorChanged(object sender, TextChangedEventArgs e)
        {
            var separatorTextBox = sender as TextBox;
            SEPARATOR = separatorTextBox.Text;
            this.RecalculatePreview();
        }
    }
}
