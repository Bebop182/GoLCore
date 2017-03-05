using System;
using System.Drawing;

namespace GOLCore {
    public static class WorldConverter {
        public static World FromBitmap(string path) {
            var image = new Bitmap(path);

            var worldState = new bool[image.Width*image.Height];
            for(int y=0, i=0; y<image.Height; y++) {
                for(int x=0; x<image.Width; x++, i++) {
                    var pixel = image.GetPixel(x, y);
                    worldState[i] = pixel.IsDark();
                }
            }
            var world = World.Initialize(worldState, image.Width, image.Height);
            image.Dispose();
            return world;
        }

        public static bool IsDark(this Color color, int threshold = 50) {
            var r = Convert.ToInt32(color.R);
            var g = Convert.ToInt32(color.G);
            var b = Convert.ToInt32(color.B);
            
            return r<threshold && g<threshold && b<threshold;
        }
    }
}
