using System;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;

namespace GOLCore {
    //todo: refactor console output cell animations
    //todo: refactor error handling to allow display of multiple error messages
    //todo: write/load all settings to/from a config file
    //todo: add interactivity to a world cycle
    //todo: i18n
    public class Program
    {
        public const char DEFAULT_ALIVE_SYMBOL = 'O';
        public const char DEFAULT_DEAD_SYMBOL = '#';
        
        private static Point displayOrigin;
        
        public static void Main(string[] args)
        {
            // Output configuration
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            // Cli argument parsing configuration
            var cliApp = new CommandLineApplication(throwOnUnexpectedArg:false) {
                Name="GOLCore",
                FullName="Game of Life Core",
                Description="Conway's Game of life in console form, you can load up an image file as the starting configuration and watch it evolve\nAuthor: Bebop182",
            };
            var pathOption = cliApp.Option(
                "-i|--input-path",
                "Path to an image file to be used as starting world configuration",
                CommandOptionType.SingleValue);
            var cycleDelayOption = cliApp.Option(
                "-d|--delay",
                "Define the delay in millisecond between each cycle",
                CommandOptionType.SingleValue);
            var clearOption = cliApp.Option(
                "-c|--clear",
                "Clear console output to only display world",
                CommandOptionType.NoValue);
            cliApp.HelpOption("-?|-h|--help");

            cliApp.OnExecute(()=> {
                var cycleDelay = 750;
                
                World world = null;
                try{
                    if(clearOption.HasValue())
                        Console.Clear();

                    if(pathOption.HasValue()) {
                        try{
                            world = WorldConverter.FromBitmap(pathOption.Value());
                        }
                        catch(Exception e) {
                            throw new ArgumentException("! Specified path is either invalid or file isn't bitmap", e);
                        }
                    } 
                    else {
                        throw new ArgumentException(String.Format("! You may want to specify a starting configuration."));
                    }

                    if(cycleDelayOption.HasValue()) {
                        int delayValue;
                        if(int.TryParse(cycleDelayOption.Value(), out delayValue))
                            cycleDelay = delayValue;
                        else
                            throw new ArgumentException(String.Format("! Specified delay is invalid, defaulting to {0}ms.", cycleDelay));
                    }
                }
                catch(Exception e) {
                    Console.Write(e.Message);
                    cliApp.ShowHelp();
                    if(world == null) {
                        // Initialize with default configuration
                        var input = new bool[] {
                            false, false, false, false, false, false,
                            false, false, true, false, false, false,
                            false, true, false, true, false, false,
                            false, true, false, true, false, false,
                            false, false, false, false, false, false,
                            false, false, false, false, false, false};
                        world = World.Initialize(input);
                    }
                }
                
                var cycleCount = Play(world, cycleDelay);
                Console.WriteLine("This population configuration survived for {0} cycles.", cycleCount);
                if(world.Population > 0)
                    Console.WriteLine("Although some subsist, they will stagnate forever.", cycleCount);

                return 0;
            });
            cliApp.Execute(args);

            Console.CursorVisible = false;
        }

        private static int Play(World world, int cycleDelay) {
            var cycleCount = 0;

            displayOrigin = new Point(Console.CursorLeft, Console.CursorTop);
            DisplayWorld(world);
            
            while(world.Population > 0 && Cell.ChangedStateCount > 0) {
                world.Cycle();
                cycleCount++;
                Thread.Sleep(cycleDelay-400);
                DisplayWorld(world, true);
            }            

            return cycleCount;
        }

        private static void DisplayWorld(World world, bool anim = false) {
            if(anim) {
                Display(world, '#', ' ');
                Thread.Sleep(200);
                Display(world, 'o', '#');
                Thread.Sleep(200);
            }
            
            Display(world, 'O', '#');
            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void Display(World world, char aliveSymbol, char deadSymbol) {
            Console.SetCursorPosition(displayOrigin.X, displayOrigin.Y);
            for(int i=0; i<world.Grid.Length; i++) {
                if(i % world.XResolution == 0) {
                    Console.CursorTop++;
                    Console.CursorLeft = displayOrigin.X;
                }
                var cell = world.Grid[i];

                Console.Write(
                    cell.IsAlive
                    ? cell.StateChanged ? aliveSymbol : DEFAULT_ALIVE_SYMBOL
                    : cell.StateChanged ? deadSymbol : DEFAULT_DEAD_SYMBOL);
            }
        }
    }
}
