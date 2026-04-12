using QRCoder;

public class QRCodeService
{
    public byte[] GenerateQRCode(string token, string baseUrl)
    {
        var url = $"{baseUrl}/Vote/Verify/{token}";

        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
        var qrCode = new PngByteQRCode(data);
        return qrCode.GetGraphic(
            pixelsPerModule: 20,
            darkColorRgba:  new byte[] { 0, 0, 0, 255 },
            lightColorRgba: new byte[] { 255, 255, 255, 255 }
        );
    }
}