using System;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;

namespace GOLCore {
    public class Program
    {
        private static Point displayOrigin;
        
        public static void Main(string[] args)
        {
            // Output configuration
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Cli argument parsing configuration
            var cliApp = new CommandLineApplication(throwOnUnexpectedArg:false);
            var pathOption = cliApp.Option(
                "-i|--input-path",
                "Path to an image file to be used as starting world configuration",
                CommandOptionType.SingleValue);
            var cycleDelayOption = cliApp.Option(
                "-c|--cycle-delay",
                "Define the delay in millisecond between each cycle",
                CommandOptionType.SingleValue);
            cliApp.HelpOption("-?|-h|--help");

            cliApp.OnExecute(()=> {
                var cycleDelay = 750;
                var input = new bool[] {
                        false, false, false, false, false, false,
                        false, false, true, false, false, false,
                        false, true, false, true, false, false,
                        false, true, false, true, false, false,
                        false, false, false, false, false, false,
                        false, false, false, false, false, false};
                try{
                    if(pathOption.HasValue()) {
                        try{
                            input = WorldConverter.FromBitmap(pathOption.Value());
                        }
                        catch(Exception e) {
                            throw new ArgumentException("! Specified path is either invalid or file isn't bitmap", e);
                        }
                    } 
                    else
                        throw new ArgumentException(String.Format("! You may want to specify a starting configuration."));

                    if(cycleDelayOption.HasValue()) {
                        int delayValue;
                        if(int.TryParse(cycleDelayOption.Value(), out delayValue))
                            cycleDelay = delayValue;
                        else {
                            throw new ArgumentException(String.Format("! Specified delay is invalid, defaulting to {0}ms.", cycleDelay));
                        }
                    }
                }
                catch(Exception e) {
                    Console.Write(" " + e.Message);
                    cliApp.ShowHelp();
                }
                
                var cycleCount = Play(input, cycleDelay);
                Console.WriteLine("This population configuration survived for {0} cycles.", cycleCount);

                return 0;
            });
            cliApp.Execute(args);
        }

        private static int Play(bool[] initState, int cycleDelay) {
            var cycleCount = 0;
            var world = World.Initialize(initState);

            displayOrigin = new Point(Console.CursorLeft, Console.CursorTop);
            DisplayWorld(world);
            
            do {
                world.Cycle();
                cycleCount++;
                Thread.Sleep(cycleDelay);
                DisplayWorld(world);
            }
            while(world.AliveCellCount > 0);

            return cycleCount;
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

        
    }
}
