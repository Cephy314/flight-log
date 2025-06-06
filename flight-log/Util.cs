using System.Drawing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Imaging.Effects;

namespace flight_log;
#pragma warning disable CA1416
public static class Util
{
    
    /// <summary>
    /// Loads an image from the specified file path, processes it to convert it to grayscale, inverts the colors,
    /// and applies levels adjustment.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Memory Stream of image.</returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static MemoryStream LoadAndProcessImage(string filePath)
    {
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file {filePath} does not exist.");
        }

        using var image = new Bitmap(filePath);
        
        var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);

        // Get the address of the first pixel data
        IntPtr ptr = data.Scan0;
        
        // Declare array to hold the bytes of the bitmap
        var bytes = Math.Abs(data.Stride) * image.Height;
        var rgbValues = new byte[bytes];
        
        // Copy the RGB values into the array
        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
        

        for (var i = 0; i < rgbValues.Length; i += 4)
        {
            // Convert to grayscale
            var gray = (int)(0.299 * rgbValues[i + 2] + 0.587 * rgbValues[i + 1] + 0.114 * rgbValues[i]);
            rgbValues[i] = (byte)gray;     // Blue
            rgbValues[i + 1] = (byte)gray; // Green
            rgbValues[i + 2] = (byte)gray; // Red
            
            // Invert colors
            rgbValues[i] = (byte)(255 - rgbValues[i]);     // Blue
            rgbValues[i + 1] = (byte)(255 - rgbValues[i + 1]); // Green
            rgbValues[i + 2] = (byte)(255 - rgbValues[i + 2]); // Red
            
            // Apply levels adjustment
            //var level = Levels(rgbValues[i], 132, 0, 75);     // Blue
            rgbValues[i] = Levels(rgbValues[i], 132, 0, 75);     // Blue;
            rgbValues[i + 1] = Levels(rgbValues[i + 1], 132, 0, 75);     // Blue;
            rgbValues[i + 2] = Levels(rgbValues[i + 2], 132, 0, 75);     // Blue;
        }
        
        // Copy the modified RGB values back to the bitmap
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
        // Unlock the bits
        image.UnlockBits(data);
        
#if DEBUG
        image.Save("inverted_image.png", ImageFormat.Png); // Save inverted image for debugging
#endif
        

        // Save to memory stream
        var memoryStream = new MemoryStream();
        image.Save(memoryStream, ImageFormat.Bmp);
        memoryStream.Position = 0; // Reset stream position

        return memoryStream;
    }

    private static byte Levels(byte color, int whitePoint, int blackPoint, int gamma)
    {
        // scale color value from the 0-255 range to the new range defined by whitePoint and blackPoint
        var r = (255 * (color - blackPoint)) / (whitePoint - blackPoint);
        
        // Apply gamma correction
        r = (int)(Math.Pow(r / 255.0, gamma / 100.0) * 255);
        
        // Clamp the value to the 0-255 range
        if (r < 0) r = 0;
        if (r > 255) r = 255;
        

        return (byte)r;
    }
    
#pragma warning restore CA1416
}