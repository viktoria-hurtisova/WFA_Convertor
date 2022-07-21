using System;
using System.Drawing;
using System.Threading;
using System.Drawing.Imaging;
namespace WFA_Lib
{
    static public class ImageManipulator
    {
        public static double[,] Shrink(double[,] inputImage, int newWidth, int newHeight)
        {
            if (inputImage.GetLength(0) < newWidth || inputImage.GetLength(1) < newHeight)
                throw new ArgumentException("New dimensions are bigger than the input picure");

            var newImage = new double[newWidth, newHeight];
            int averageBoxWidth = inputImage.GetLength(1) / newHeight;
            int averageBoxHeight = inputImage.GetLength(0) / newWidth;
            double averageValue;

            for (int i = 0; i < newWidth; i++)
            {
                for (int j = 0; j < newHeight; j++)
                {
                    averageValue = Average(inputImage, averageBoxWidth, averageBoxHeight, i * averageBoxWidth, j * averageBoxHeight);
                    newImage[i, j] = averageValue;
                }
            }
            return newImage;
        }

        private static double Average(double[,] inputFile, int width, int height, int x, int y)
        {
            double value = 0;

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    value += inputFile[i, j];
                }
            }
            value = value / (width * height);

            return value;
        }

        public static Bitmap ConvertToGreyscale(Bitmap inputImage)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(inputImage.Width, inputImage.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(inputImage, new Rectangle(0, 0, inputImage.Width, inputImage.Height),
               0, 0, inputImage.Width, inputImage.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;

        }

        public static Bitmap ConvertToBilevel(Bitmap inputImage)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(inputImage.Width, inputImage.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
               });

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            attributes.SetThreshold(0.5f); // Change this threshold as needed
            var rc = new Rectangle(0, 0, inputImage.Width, inputImage.Height);
            g.DrawImage(inputImage, rc, 0, 0, inputImage.Width, inputImage.Height, GraphicsUnit.Pixel, attributes);

            return newBitmap;
        }

        public static Bitmap ArrayToImage(Color[,] input)
        {
            int width = input.GetLength(1);
            int height = input.GetLength(0);

            Bitmap image = new Bitmap(width, height);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = image.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    rgbValues[i * bmpData.Stride + j * 3 + 2] = input[i, j].R;
                    rgbValues[i * bmpData.Stride + j * 3 + 1] = input[i, j].G;
                    rgbValues[i * bmpData.Stride + j * 3] = input[i, j].B;
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            image.UnlockBits(bmpData);
            return image;
        }

        public static Color[,] ToSquare(Bitmap inputImage)
        {
            int maxDim = Math.Max(inputImage.Height, inputImage.Width);
            int power = (int)Math.Ceiling(Math.Log2(maxDim));
            int size = (int)Math.Pow(2, power);
            Color[,] output = new Color[size, size];

            var rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);
            var bmpData = inputImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < inputImage.Height; i++)
            {
                for (int j = 0; j < inputImage.Width; j++)
                {
                    output[i, j] = Color.FromArgb(rgbValues[i * bmpData.Stride + j * 3 + 2], rgbValues[i * bmpData.Stride + j * 3 + 1], rgbValues[i * bmpData.Stride + j * 3]);
                }
            }

            for (int i = inputImage.Height; i < size; i++)
            {
                for (int j = inputImage.Width; j < size; j++)
                {
                    output[i, j] = Color.Black;
                }
            }

            inputImage.UnlockBits(bmpData);
            return output;
        }

        public static double[,] ConcatenateColourComponents(Bitmap input)
        {
            double[,] output = new double[input.Height * 2, input.Width * 2];
            var rect = new Rectangle(0, 0, input.Width, input.Height);
            var bmpData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < input.Height; i++)
            {
                for (int j = 0; j < input.Width; j++)
                {
                    output[i, j] = rgbValues[i * bmpData.Stride + j * 3 + 2];
                    output[i, j + input.Width] = rgbValues[i * bmpData.Stride + j * 3 + 1];
                    output[i + input.Height, j] = rgbValues[i * bmpData.Stride + j * 3];
                }
            }
            input.UnlockBits(bmpData);
            return output;
        }

        public static double[,] ConcatenateColourComponents(Color[,] input)
        {
            if (input.GetLength(0) != input.GetLength(1))
                throw new ArgumentException("Input image should be square");

            int dim = input.GetLength(1);
            double[,] output = new double[input.GetLength(0) * 2, input.GetLength(1) * 2];
            
            
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    output[i, j] = input[i,j].R;
                    output[i, j + dim] = input[i,j].G;
                    output[i + dim, j + dim] = input[i,j].B;
                }
            }
            return output;
        }

        public static Bitmap VisualizeConcatenatedColours(double[,] input)
        {
            int width = input.GetLength(1);
            int height = input.GetLength(0);
            byte red, green, blue;

            Bitmap image = new Bitmap(width, height);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = image.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] rgbValues = new byte[bytes];



            for (int i = 0; i < height / 2; i++)
            {
                for (int j = 0; j < width / 2; j++)
                {
                    red = (byte)input[i, j];
                    green = (byte)input[i, j + width / 2];
                    blue = (byte)input[i + height / 2, j];

                    rgbValues[i * bmpData.Stride + j * 3 + 2] = red;
                    rgbValues[i * bmpData.Stride + j * 3 + width * 3 / 2 + 1] = green;
                    rgbValues[i * bmpData.Stride + height / 2 * bmpData.Stride + j * 3] = blue;

                }
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            image.UnlockBits(bmpData);
            return image;
        }

        public static double SquareError(Bitmap img1, Bitmap img2)
        {
            if (img1.Width != img2.Width || img1.Height != img2.Height)
                throw new ArgumentException("The picures are not the same size");
            var rect = new Rectangle(0, 0, img1.Width, img2.Width);

            var bmpData1 = img1.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var bmpData2 = img2.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            IntPtr ptr1 = bmpData1.Scan0;
            IntPtr ptr2 = bmpData2.Scan0;
            int bytes1 = Math.Abs(bmpData1.Stride) * bmpData1.Height;
            byte[] rgbValues1 = new byte[bytes1];
            byte[] rgbValues2 = new byte[bytes1];

            System.Runtime.InteropServices.Marshal.Copy(ptr1, rgbValues1, 0, bytes1);
            System.Runtime.InteropServices.Marshal.Copy(ptr2, rgbValues2, 0, bytes1);

            double error = 0;

            for (int i = 0; i < bmpData1.Width; i++)
            {
                for (int j = 0; j < bmpData1.Height; j++)
                {
                    error += Math.Pow(rgbValues1[i + j] - rgbValues2[i + j], 2);

                }
            }
            img1.UnlockBits(bmpData1);
            img2.UnlockBits(bmpData2);
            return error;
        }
    }


}
