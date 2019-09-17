namespace QRtoPDFGenerator
{
    using PdfSharp.Drawing;
    using QRCoder;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

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
        private int PIXELS_PER_MODULE = 10;

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
            //TODO move this logic to a worker thread
            var qRContainerList = this.generateQR();
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
                qRContainers.Add(new QRContainer
                {
                    code = code,
                    bitmap = qrCodeImage
                });
            }

            return qRContainers;
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
