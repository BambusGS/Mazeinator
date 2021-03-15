using System;
using System.Drawing;   //Bitmap and Graphics
using System.IO;    //MemoryStream
using System.Runtime.Serialization; //Serialization
using System.Runtime.Serialization.Formatters.Binary;   //BinaryFormatter
using System.Windows.Media.Imaging; //BitmapImage

namespace Mazeinator
{
    public static class Utilities
    {
        //converts the generated Bitmap to ImageSource for the Image Display
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        //Almost automatic color conversion functions between the older Drawing.Color and the newer WPF required Windows.Media.Color
        public static System.Windows.Media.Color ConvertColor(System.Drawing.Color DColor) => System.Windows.Media.Color.FromRgb(DColor.R, DColor.G, DColor.B);

        public static System.Drawing.Color ConvertColor(System.Windows.Media.Color WMColor) => System.Drawing.Color.FromArgb(WMColor.R, WMColor.G, WMColor.B);

        //clamp function - useful, but is not built in as far as I know
        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                return min;
            else if (val.CompareTo(max) > 0)
                return max;
            else
                return val;
        }

        #region DataManipulation

        public static bool isWorking = false;

        //https://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-of-an-object-in-net
        public static bool SaveBySerializing<T>(T thing, string path)
        {
            try
            {
                isWorking = true;
                using (Stream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(writer, thing);
                }
            }
            catch (Exception)
            {
                throw new FileFormatException();
            }
            isWorking = false;
            return true;
        }

        public static T LoadFromTheDead<T>(string path)
        {
            T thing = default;
            try
            {
                isWorking = true;
                using (Stream writer = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    IFormatter formatter = new BinaryFormatter();
                    thing = (T)formatter.Deserialize(writer);
                }
            }
            catch (Exception)
            {
                throw new FileFormatException();
            }

            isWorking = false;
            return thing;
        }
    }

    #endregion DataManipulation
}