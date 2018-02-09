using System;
using System.Threading;
using GOLCore.Structures;

namespace GOLCore {
    public static class WorldDisplayExtensions {
        public static readonly Point displayOrigin;

        static WorldDisplayExtensions() {
            displayOrigin = new Point(Console.CursorLeft, Console.CursorTop);
        }

        public static World Display(this World world) {
            world.Print(c=> {
                Console.Write(c.IsAlive ? 'O' : ' ');
            });
            return world;
        }

        public static void Print(this World world, Action<Cell> printCell) {
            HomeCursor();

            for(int i=0; i<world.MaxPopulation; i++) {
                var cell = world.CellGrid[i];
                
                if(i % world.Width == 0) {
                    Console.CursorTop++;
                    Console.CursorLeft = displayOrigin.X;
                }
                printCell?.Invoke(cell);
            }
        }

        public static void Print(bool[] worldState, Size size, Action<bool> printState) {
            HomeCursor();

            for(int i=0; i<worldState.Length; i++) {
                var cell = worldState[i];
                
                if(i % size.Width == 0) {
                    Console.CursorTop++;
                    Console.CursorLeft = displayOrigin.X;
                }
                printState?.Invoke(cell);
            }
        }

        public static World ShowEndScreen(this World world) {
            var step = (int)Math.Clamp(2000/(float)world.MaxPopulation, 1f, float.MaxValue);
            world.Print(c=>{
                Console.Write('#');
                Thread.Sleep(step);
            });
            return world;
        }

        public static World WaitFor(this World world, int milliseconds) {
            Thread.Sleep(milliseconds);
            return world;
        }

        public static World Jump(this World world, int nbLine){
            while(nbLine > 0) {
                Console.WriteLine();
                nbLine--;
            }
            return world;
        }

        public static void HomeCursor() {
            Console.SetCursorPosition(displayOrigin.X, displayOrigin.Y);
        }

        public static int Clamp(int value, int min, int max) {
            value = Math.Max(value, min);
            return Math.Min(value, max);
        }
    }
}