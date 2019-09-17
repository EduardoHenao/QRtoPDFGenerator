namespace QRtoPDFGenerator
{
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
        private static int DIGITS = 3;
        private static string SEPARATOR = "-";


        // preview variables
        private string resultStart = SERIES + SEPARATOR + STARTING_POINT.ToString("D" + DIGITS);
        private string resultEnd = string.Empty;

        // wpf variables
        private bool endVisible = false;

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
            //TODO: after having all the events coded
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

                if (this.endVisible)
                {
                    this.ResultadoHastaText.Visibility = Visibility.Visible;
                    this.ResultadoHastaTitle.Visibility = Visibility.Visible;
                }
                else
                {
                    this.ResultadoHastaText.Visibility = Visibility.Hidden;
                    this.ResultadoHastaTitle.Visibility = Visibility.Hidden;
                }
            }
        }

        private void DigitsChanged(object sender, TextChangedEventArgs e)
        {
            var digitsTextBox = sender as TextBox;
            if (int.TryParse(digitsTextBox.Text, out int digits))
            {
                if (digits < 0)
                {
                    DIGITS = 3;
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
                    STARTING_POINT = 0;
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
