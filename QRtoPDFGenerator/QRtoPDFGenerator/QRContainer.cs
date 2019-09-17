using System;

namespace QRtoPDFGenerator
{
    using PdfSharp.Drawing;

    class QRContainer
    {
        public string code { get; set; }
        public XImage bitmap { get; set; }
    }
}
