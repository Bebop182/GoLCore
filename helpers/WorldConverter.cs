using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace GOLCore {
    public static class WorldConverter {
        public static World FromBitmap(string path) {
            var fullPath = Path.GetFullPath(path);
            Bitmap image = null;
            using(var fs = new FileStream(fullPath, FileMode.Open)) {
                image = new Bitmap(fs);
            }
            var worldState = new bool[image.Width*image.Height];

            for(int y=0, i=0; y<image.Height; y++) {
                for(int x=0; x<image.Width; x++, i++) {
                    var pixel = image.GetPixel(x, y);
                    worldState[i] = !pixel.IsDark();
                }
            }

            var world = new World(worldState, image.Width, image.Height);
            image.Dispose();
            
            return world;
        }

        public static Bitmap ToBitmap(this World world, string path, ImageFormat format) {
            var image = new Bitmap(world.XResolution, world.YResolution);
            for(int y=0, i=0; y<image.Height; y++) {
                for(int x=0; x<image.Width; x++, i++) {
                    image.SetPixel(x, y, world.CellGrid[i].IsAlive ? Color.White : Color.Black);
                }
            }
            var fullPath = Path.GetFullPath(path);
            using(var fs = new FileStream(fullPath, FileMode.OpenOrCreate)) {
                image.Save(fs, format);
            }
            return image;
        }

        public static bool IsDark(this Color color, int threshold = 50) {
            var r = Convert.ToInt32(color.R);
            var g = Convert.ToInt32(color.G);
            var b = Convert.ToInt32(color.B);
            
            return r<threshold && g<threshold && b<threshold;
        }
    }
}
