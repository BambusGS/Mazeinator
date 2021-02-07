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
                throw new Exception();
            }
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
                throw new Exception();
            }

            isWorking = false;
            return thing;
        }
    }
}