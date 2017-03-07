using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;

namespace GOLCore {
    //todo: refactor console output cell animations
    //todo: write/load all settings to/from a config file
    //todo: add interactivity to a world cycle
    //todo: i18n

    // Conway's Game of life in console form,
    // you can load up an image file as the starting configuration and
    // watch it expand or die
    public class Program
    {
        public static void Main(string[] args)
        {
            // Output configuration
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            // Application settings
            var cycleDelay = 750;

            // Cli argument parsing configuration
            var cliApp = new CommandLineApplication(throwOnUnexpectedArg:false) {
                Name="GOLCore",
                FullName="Game of Life Core",
                Description="Conway's Game of life in console form, "
                + "you can load up an image file as the starting configuration "
                + "and watch it expand or die\nAuthor: Bebop182"};
            
            // Cli options declaration
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

            // Cli execution
            cliApp.OnExecute(()=> {
                World world = null;
                var warnings = new List<Exception>();
                try {
                    if(clearOption.HasValue())
                        Console.Clear();

                    if(cycleDelayOption.HasValue()) {
                        int delayValue;
                        if(int.TryParse(cycleDelayOption.Value(), out delayValue))
                            cycleDelay = delayValue;
                        else warnings.Add(new ArgumentException(String.Format("! Specified delay is invalid, defaulting to {0}ms.", cycleDelay)));
                    }

                    if(pathOption.HasValue()) {
                        try {
                            world = WorldConverter.FromBitmap(pathOption.Value());
                        }
                        catch (Exception e) {
                            warnings.Add(new ArgumentException("! Specified path is either invalid or file isn't bitmap", e));
                        }
                    } 
                    else throw new ArgumentException("! You may want to specify a starting configuration.");
                }
                catch(Exception e) {
                    Console.WriteLine(e.Message);
                    foreach(var exception in warnings) {
                        Console.WriteLine(exception.Message);
                    }
                    Console.WriteLine();
                    cliApp.ShowHelp();
                    
                    if(world == null) {
                        // Initialize with default configuration
                        var input = new bool[] {
                            false, false, false, false, false, false,
                            false, false, true, false, false, false,
                            false, true, true, true, false, false,
                            false, true, false, true, false, false,
                            false, true, false, true, false, false,
                            false, false, false, false, false, false};
                        // var input = new bool[] {
                        //     false, false, false, false, false, false,
                        //     false, false, true, false, false, false,
                        //     false, false, true, false, false, false,
                        //     false, false, true, false, false, false,
                        //     false, false, false, false, false, false,
                        //     false, false, false, false, false, false};
                        world = new World(input);
                    }
                }
                
                var cycleCount = Play(world, cycleDelay);
                Console.WriteLine("This population configuration survived for {0} cycles.", cycleCount);
                if(world.Population > 0)
                    Console.WriteLine("Although some subsist, they will stagnate forever.");

                return 0;
            });
            cliApp.Execute(args);

            Console.CursorVisible = false;
        }

        private static int Play(World world, int cycleDelay) {
            var cycleCount = 0;
            var finalDelay = Math.Max(cycleDelay-300, 0);

            do {
                world.WaitFor(finalDelay)
                    .Display('O', ' ')
                    .Jump(3);
                world.Cycle();
                cycleCount++;
                world.TriggerCommitCycle();
            } while(world.Population > 0 && Cell.ChangedStateCount > 0);

            return cycleCount;
        }
    }
}
