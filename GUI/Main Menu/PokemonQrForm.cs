using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HebiKaio.Core.Profiles;
using QRCoder;
using ZXing;
using ZXing.Common;

namespace GUI;

internal sealed class PokemonQrForm : Form
{
    public PokemonQrForm(ProfileTransferService transfers, OwnedPokemon pokemon)
    {
        Text = $"Share {pokemon.Nickname ?? pokemon.SpeciesName}"; ClientSize = new Size(560, 640); StartPosition = FormStartPosition.CenterParent;
        var generator = new QRCodeGenerator(); using var data = generator.CreateQrCode(transfers.ExportPokemonQrPayload(pokemon.Id), QRCodeGenerator.ECCLevel.L);
        var png = new PngByteQRCode(data).GetGraphic(7); using var stream = new MemoryStream(png); using var source = Image.FromStream(stream);
        var image = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, Image = new Bitmap(source), Margin = new Padding(20) };
        Controls.Add(image); Controls.Add(new Label { Dock = DockStyle.Top, Height = 65, Padding = new Padding(10), TextAlign = ContentAlignment.MiddleCenter, Text = "Scan this code from Receive / Import on the other computer." });
        Controls.Add(new Button { Dock = DockStyle.Bottom, Height = 42, Text = "Close", DialogResult = DialogResult.OK });
    }

    public static string ReadPayload(string path)
    {
        using var bitmap = new Bitmap(path); var rgb = new byte[bitmap.Width * bitmap.Height * 3]; var offset = 0;
        for (var y = 0; y < bitmap.Height; y++) for (var x = 0; x < bitmap.Width; x++) { var color = bitmap.GetPixel(x, y); rgb[offset++] = color.R; rgb[offset++] = color.G; rgb[offset++] = color.B; }
        var source = new RGBLuminanceSource(rgb, bitmap.Width, bitmap.Height, RGBLuminanceSource.BitmapFormat.RGB24);
        var result = new BarcodeReaderGeneric { AutoRotate = true, Options = new DecodingOptions { TryHarder = true } }.Decode(source);
        return result?.Text ?? throw new InvalidDataException("No readable QR code was found in that image.");
    }
}
