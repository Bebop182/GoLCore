using System;
using System.Threading;

namespace GOLCore {
    public static class WorldDisplayExtensions {
        private static readonly Point displayOrigin;

        static WorldDisplayExtensions() {
            displayOrigin = new Point(Console.CursorLeft, Console.CursorTop);
        }

        public static World Display(this World world) {
            //var backgroundColor = Console.BackgroundColor;

            world.Print(c=> {
                // Console.BackgroundColor = c.IsAlive
                // ? ConsoleColor.White
                // : ConsoleColor.Black;
                var symbol = c.IsAlive
                ? 'O'
                : ' ';
                Console.Write(symbol);
            });

            //Console.BackgroundColor = backgroundColor;
            return world;
        }

        public static void Print(this World world, Action<Cell> printCell) {
            Console.SetCursorPosition(displayOrigin.X, displayOrigin.Y);

            for(int i=0; i<world.MaxPopulation; i++) {
                var cell = world.CellGrid[i];
                
                if(i % world.XResolution == 0) {
                    Console.CursorTop++;
                    Console.CursorLeft = displayOrigin.X;
                }
                printCell?.Invoke(cell);
            }
        }

        public static World ShowEndScreen(this World world) {
            world.Print(c=>{
                Console.Write('#');
                Thread.Sleep(10);
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
    }
}