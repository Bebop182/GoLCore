﻿using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace GOLCore {
    //todo: write/load all settings to/from a config file
    //todo: error handling
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

            var cliApp = ConfigureApplication();
            
            cliApp.Execute(args);
        }

        private static CommandLineApplication ConfigureApplication() {
            // Application settings
            var cycleDelay = 500;
            var maxCycle = 10000;
            var displayMargin = 2;

            // Cli argument parsing configuration
            var cliApp = new CommandLineApplication(throwOnUnexpectedArg:false) {
                Name="GOLCore",
                FullName="Game of Life Core",
                Description="Conway's Game of life in console form, "
                + "you can load up an image file as the starting configuration "
                + "and watch it expand or die\nAuthor: Bebop182"};
            
            // Cli options declaration
            cliApp.HelpOption("-?|-h|--help");

            var playCommand = cliApp.Command("play", (command)=>{
                // Command initialization
                command.Description = "Run a game of life from an image file.";
                #region Command options
                var inputPathArgument = command.Argument(
                    "Input path",
                    "Path to an image file to be used as starting world configuration",
                    multipleValues:false);
                var outputPathOption = command.Option(
                    "-o|--output-path",
                    "Path to the desired save location of the output image",
                    CommandOptionType.SingleValue);
                var cycleDelayOption = command.Option(
                    "-d|--delay",
                    "Define the delay in millisecond between each cycle",
                    CommandOptionType.SingleValue);
                var maxCycleOption = command.Option(
                    "-m|--max-cycle",
                    "Define the limit of cycle to process",
                    CommandOptionType.SingleValue);
                var silentOption = command.Option(
                    "-s|--silent",
                    "Disable world output to display and ignore cycle delay option",
                    CommandOptionType.NoValue);
                var clearOption = command.Option(
                    "-c|--clear",
                    "Clear console output to only display world",
                    CommandOptionType.NoValue);
                #endregion
                
                command.OnExecute(() => {
                    // Check input path
                    var inputPath = Path.GetFullPath(inputPathArgument.Value);
                    // Initialize world
                    var world = WorldConverter.FromBitmap(inputPath);
                    // Check configuration
                    var silent = silentOption.HasValue();
                    if(!silent && cycleDelayOption.HasValue())
                        cycleDelay = int.Parse(cycleDelayOption.Value());
                    if(maxCycleOption.HasValue())
                        maxCycle = int.Parse(maxCycleOption.Value());

                    if(clearOption.HasValue())
                        Console.Clear();
                    // Cycle logic
                    var cycleCount = 0;
                    do {
                        if(!silent) {
                            world.WaitFor(cycleDelay)
                                .Display()
                                .Jump(displayMargin);
                            Console.WriteLine("Cycle n°{0}", cycleCount+1);
                        }
                        world.Cycle();
                        world.TriggerCommitCycle();
                        cycleCount++;
                    } while(world.CurrentPopulation > 0 && Cell.ChangedStateCount > 0 && cycleCount < maxCycle);

                    if(!silent) {
                        world.ShowEndScreen()
                            .Jump(displayMargin);
                    }

                    if(cycleCount == maxCycle)
                        Console.WriteLine("This population configuration still holds {0} individuals after {1} cycles.", world.CurrentPopulation, cycleCount);
                    else {
                        Console.WriteLine("This population configuration survived for {0} cycles.", cycleCount);
                        if(world.CurrentPopulation > 0)
                            Console.WriteLine("Although some subsist, they will stagnate forever.");
                    }

                    // If provided output result to path
                    if(outputPathOption.HasValue()) {
                        // todo: Figure out format from path
                        var outputPath = Path.GetFullPath(outputPathOption.Value());
                        var parentDirectory = Path.GetDirectoryName(outputPath);

                        var fileName = Path.GetFileName(outputPath);
                        var format = WorldConverter.GetImageFormat(outputPath);
                        
                        if(fileName == string.Empty) 
                            fileName = "golCore_output";

                        fileName = Path.ChangeExtension(fileName, format.ToString().ToLower());
                        outputPath = Path.Combine(parentDirectory, fileName);
                        
                        using(var fs = new FileStream(outputPath, FileMode.OpenOrCreate)) {
                            var image = world.ToBitmap();
                            image.Save(fs, format);
                        }
                    }

                    return 0;
                });
            }, throwOnUnexpectedArg:false);

            // Cli execution
            cliApp.OnExecute(()=> {
                playCommand.ShowHelp();
                return 0;
            });

            return cliApp;
        }
    }
}
