using System;
using System.Drawing;

public class WorldConverter {
    public static bool[] FromImage(string path) {
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

    private static bool IsDark(Color color) {
        var r = Convert.ToInt32(color.R);
        var g = Convert.ToInt32(color.G);
        var b = Convert.ToInt32(color.B);
        //Console.WriteLine("red: {0} green: {1} blue: {2}", r, g, b);
        
        return r<50 && g<50 && b<50;
    }
}