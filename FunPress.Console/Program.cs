using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using Image = System.Drawing.Image;
using SystemConsole = System.Console;

namespace FunPress.Console
{
    internal class Program
    {
        private static void Main()
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "template_1.JPG");
            var imageToAttach = Path.Combine(Directory.GetCurrentDirectory(), "Images", "IMG_3245.JPG");
            var generatedImage = Path.Combine(Directory.GetCurrentDirectory(), "generated_image.jpg");

            if (!File.Exists(templatePath))
            {
                SystemConsole.WriteLine("Template path does not exist!");
                SystemConsole.ReadLine();

                return;
            }

            if (!File.Exists(imageToAttach))
            {
                SystemConsole.WriteLine("Template path does not exist!");
                SystemConsole.ReadLine();

                return;
            }

            var result = CombineImages(templatePath, imageToAttach, generatedImage);

            if (!result)
            {
                SystemConsole.WriteLine("Image does not generated!");
                SystemConsole.ReadLine();

                return;
            }

            if (!File.Exists(generatedImage))
            {
                SystemConsole.WriteLine("Generated image does not exist!");
                SystemConsole.ReadLine();

                return;
            }

            //Print(generatedImage);

            SystemConsole.ReadLine();
        }

        private static bool CombineImages(string backgroundImagePath, string overlayImagePath, string generatedImagePath)
        {
            try
            {
                SystemConsole.WriteLine("Start generating image...");

                var backgroundImage = new Bitmap(backgroundImagePath);
                var overlayImage = new Bitmap(overlayImagePath);

                var resizedOverlayImage = new Bitmap(overlayImage, new Size(3300, 1650));
                overlayImage.Dispose();

                var combinedImage = (Bitmap)backgroundImage.Clone();
                backgroundImage.Dispose();

                using (var combinedImageGraphics = Graphics.FromImage(combinedImage))
                {
                    var bmp = new Bitmap(resizedOverlayImage.Width, resizedOverlayImage.Height);
                    using (var resizedOverlayImageGraphics = Graphics.FromImage(bmp))
                    {
                        var colorMatrix = new ColorMatrix(
                            new[]
                            {
                                new[] { 0.3f, 0.3f, 0.3f, 0f, 0f },
                                new[] { 0.59f, 0.59f, 0.59f, 0f, 0f },
                                new[] { 0.11f, 0.11f, 0.11f, 0f, 0f },
                                new[] { 0f, 0f, 0f, 1f, 0f },
                                new[] { 0f, 0f, 0f, 0f, 1f }
                            });

                        var attributes = new ImageAttributes();
                        attributes.SetColorMatrix(colorMatrix);

                        resizedOverlayImageGraphics.DrawImage(resizedOverlayImage, 
                            new Rectangle(0, 0, resizedOverlayImage.Width, resizedOverlayImage.Height),
                            0, 
                            0, 
                            resizedOverlayImage.Width, 
                            resizedOverlayImage.Height,
                            GraphicsUnit.Pixel, 
                            attributes);
                    }

                    combinedImageGraphics.DrawImage(bmp, 130, 770);
                    bmp.Dispose();
                }

                combinedImage.Save(generatedImagePath);

                resizedOverlayImage.Dispose();
                combinedImage.Dispose();

                SystemConsole.WriteLine("Image generated...");

                return true;
            }
            catch (Exception exception)
            {
                SystemConsole.WriteLine(exception);

                return false;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static void Print(string imagePath)
        {
            try
            {
                SystemConsole.WriteLine("Prepare printer...");

                var name = string.Empty;

                foreach (var printer in PrinterSettings.InstalledPrinters.Cast<string>().Where(printer => string.IsNullOrWhiteSpace(name)))
                {
                    name = printer;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    SystemConsole.WriteLine("There is no printers!");

                    return;
                }

                SystemConsole.WriteLine($"Printer name: {name}");
                SystemConsole.WriteLine($"Image path: {imagePath}");

                var printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = name;
                printDocument.PrintPage += (sender, args) =>
                {
                    var image = Image.FromFile(imagePath);

                    args.Graphics.DrawImage(image, 0, 0, args.PageBounds.Width, args.PageBounds.Height);

                    image.Dispose();
                };

                printDocument.Print();

                SystemConsole.WriteLine("Printing!");
            }
            catch (Exception exception)
            {
                SystemConsole.WriteLine(exception);
            }
        }
    }
}
