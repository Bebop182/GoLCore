using System;
using System.Threading;
using GOLCore;

namespace ConsoleApplication
{
    public class Program
    {
        private static Point displayOrigin;
        
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var cycleCount = 0;

            displayOrigin = new Point(Console.CursorLeft, Console.CursorTop);
            var input = new bool[] {
                false, false, false, false, false, false,
                false, false, true, false, false, false,
                false, true, false, true, false, false,
                false, true, false, true, false, false,
                false, false, false, false, false, false,
                false, false, false, false, false, false,
            };
            var world = World.Initialize(input);
            DisplayWorld(world);
            do {
                world.Cycle();
                Thread.Sleep(750);
                DisplayWorld(world);
                cycleCount++;   
            }
            while(world.AliveCellCount > 0);
            Console.WriteLine("This population configuration survived for " + cycleCount + " cycles.");
        }

        private static void DisplayWorld(World world) {
            Console.SetCursorPosition(displayOrigin.X, displayOrigin.Y);
            var worldState = world.State;
            for(int i=0; i<world.Grid.Length; i++) {
                if(i % world.Resolution == 0) {
                    Console.CursorTop++;
                    Console.CursorLeft = displayOrigin.X;
                }
                Console.Write(world.State[i] ? 'O' : '#');
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        private struct Point {
            public int X;
            public int Y;
            public Point(int x, int y) {
                X = x;
                Y = y;
            }
        }
    }
}
