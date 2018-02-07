using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;

namespace GOLCore
{
    public static class WorldHelper {
        public static World Import(string path) {
            // var width = 100;
            // var height = 15;
            // var worldState = GenerateWorldState(width, height);
            
            // return new World(worldState, width, height);
            using(var image = Image.Load(path)) {
                var worldState = new bool[image.Width*image.Height];

                for(int y=0, i=0; y<image.Height; y++) {
                    for(int x=0; x<image.Width; x++, i++) {
                        worldState[i] = image[x, y].R >= 0x5 ? true : false;
                    }
                }
                return new World(worldState, image.Width, image.Height);
            }
        }

        public static void Export(this World world, string path) {
            using(var image = new Image<Rgba32>(world.Width, world.Height)) {
                for(int y=0, i=0; y<image.Height; y++) {
                    for(int x=0; x<image.Width; x++, i++) {
                        image[x, y] = world.CellGrid[i].IsAlive ? Rgba32.White : Rgba32.Black;
                    }
                }
                image.Save(path);
            }
        }

        private static bool[] GenerateWorldState(int width, int height) {
            var rand = new Random();
            
            return Enumerable
                .Range(0, width*height)
                .Select(e => rand.Next(100) >= 92 ? true : false )
                .ToArray();
        }

        // public static World FromBitmap(Bitmap image, string name) {
        //     var worldState = new bool[image.Width*image.Height];
        //     for(int y=0, i=0; y<image.Height; y++) {
        //         for(int x=0; x<image.Width; x++, i++) {
        //             var pixel = image.GetPixel(x, y);
        //             worldState[i] = !pixel.IsDark();
        //         }
        //     }

        //     var world = new World(worldState, image.Width, image.Height);
        //     world.Name = String.IsNullOrEmpty(name) ? String.Empty : name;

        //     image.Dispose();
        //     return world;
        // }

        // public static World FromBitmap(string path) {
        //     var fullPath = Path.GetFullPath(path);
        //     Bitmap image = null;
        //     using(var fs = new FileStream(fullPath, FileMode.Open)) {
        //         image = new Bitmap(fs);
        //     }
        //     var name = Path.GetFileNameWithoutExtension(fullPath);
        //     return FromBitmap(image, name);
        // }

        // public static Bitmap ToBitmap(this World world) {
        //     var image = new Bitmap(world.XResolution, world.YResolution);
        //     for(int y=0, i=0; y<image.Height; y++) {
        //         for(int x=0; x<image.Width; x++, i++) {
        //             image.SetPixel(x, y, world.CellGrid[i].IsAlive ? Color.White : Color.Black);
        //         }
        //     }
        //     return image;
        // }

        // public static bool IsDark(this Color color, int threshold = 50) {
        //     var r = Convert.ToInt32(color.R);
        //     var g = Convert.ToInt32(color.G);
        //     var b = Convert.ToInt32(color.B);
            
        //     return r<threshold && g<threshold && b<threshold;
        // }

        // public static ImageFormat GetImageFormat(string path) {
        //     // Check provided path extension to figure out ImageFormat
        //     // return Png by default
        //     var fullPath = Path.GetFullPath(path);
        //     var isFile = Path.HasExtension(fullPath);

        //     ImageFormat format = ImageFormat.Png;
        //     if(!isFile) return format;
        //     try {
        //         var imageFormatConverter = new ImageFormatConverter();
        //         var ext = Path.GetExtension(fullPath).TrimStart('.');
        //         format = (ImageFormat)imageFormatConverter.ConvertFromString(ext);
        //     }
        //     catch(Exception) {
        //         Console.WriteLine("! Desired image format defaulting to " + format);
        //     }
        //     return format;
        // }
    }
}
