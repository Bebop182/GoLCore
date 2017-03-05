using System;
using System.Drawing;

namespace GOLCore {
    public class WorldConverter {
        public static bool[] FromBitmap(string path) {
            var image = new Bitmap(path);

            var worldState = new bool[image.Width*image.Height];
            for(int y=0; y<image.Height; y++) {
                for(int x=0; x<image.Width; x++) {
                    var pixel = image.GetPixel(x, y);
                    worldState[x+y*image.Height] = !IsDark(pixel);
                }
            }
            
            return worldState;
        }

        private static bool IsDark(Color color, int threshold = 50) {
            var r = Convert.ToInt32(color.R);
            var g = Convert.ToInt32(color.G);
            var b = Convert.ToInt32(color.B);
            
            return r<threshold && g<threshold && b<threshold;
        }
    }
}
