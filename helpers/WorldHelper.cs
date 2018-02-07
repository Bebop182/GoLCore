using System;
using System.Collections.Generic;
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
    }
}
