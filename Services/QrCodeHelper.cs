using System;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace GymFinanceBillingWpf.Services;

public static class QrCodeHelper
{
    public static BitmapImage GenerateQrCode(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            data = "N/A";
        }

        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
            
            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(qrCodeAsPngByteArr))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            bitmap.Freeze();
            return bitmap;
        }
    }
}
