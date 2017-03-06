using System;
using System.Threading;

namespace GOLCore {
    public static class WorldDisplayExtensions {
        private static Point displayOrigin;

        static WorldDisplayExtensions() {
            displayOrigin = new Point(Console.CursorLeft, Console.CursorTop);
        }

        public static World Display(this World world, char aliveSymbol, char deadSymbol) {
            Console.SetCursorPosition(displayOrigin.X, displayOrigin.Y);

            for(int i=0; i<world.MaxPopulation; i++) {
                var cell = world.Grid[i];
                
                if(i % world.XResolution == 0) {
                    Console.CursorTop++;
                    Console.CursorLeft = displayOrigin.X;
                }

                // Console.Write(
                //     cell.IsAlive
                //     ? cell.StateChanged ? aliveSymbol : DEFAULT_ALIVE_SYMBOL
                //     : cell.StateChanged ? deadSymbol : DEFAULT_DEAD_SYMBOL);
                Console.Write(
                    cell.IsAlive
                    ? aliveSymbol
                    : deadSymbol);
            }
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