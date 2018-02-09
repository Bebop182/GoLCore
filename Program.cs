using System;
using System.Collections.Generic;
using System.IO;
using GOLCore.Structures;
using Microsoft.Extensions.CommandLineUtils;

namespace GOLCore {
    //todo: error handling
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
            List<Exception> warnings;
            var cliApp = ConfigureApplication(out warnings);

            cliApp.Execute(args);

            Console.WriteLine();
            foreach(var exception in warnings) {
                Console.WriteLine("!!! " + exception.Message);
            }
        }

        private static CommandLineApplication ConfigureApplication(out List<Exception> warningOutputs) {
            var warnings = new List<Exception>();

            // Application settings
            var cycleDelay = 500;
            var maxCycle = 2000;

            // Cli argument parsing configuration
            var cliApp = new CommandLineApplication(throwOnUnexpectedArg:false) {
                Name="GOLCore",
                FullName="Game of Life Core",
                Description="Conway's Game of life in console form, "
                + "you can load up an image file as the starting configuration "
                + "and watch it expand or die\nAuthor: Bebop182"};

            // Cli main options declaration
            cliApp.HelpOption("-?|-h|--help");

            // Cli commands declaration
            var playCommand = cliApp.Command("play", (playcmd) => {
                playcmd.Description = "Run a game of life from an image file.";
                #region Command options
                var inputPathArgument = playcmd.Argument(
                    "Input path",
                    "Path to an image file to be used as starting world configuration",
                    multipleValues:false);
                var outputPathOption = playcmd.Option(
                    "-o|--output-path",
                    "Path to the desired save location of the output image",
                    CommandOptionType.SingleValue);
                var cycleDelayOption = playcmd.Option(
                    "-d|--delay",
                    "Define the delay in millisecond between each cycle",
                    CommandOptionType.SingleValue);
                var maxCycleOption = playcmd.Option(
                    "-m|--max-cycle",
                    "Define the limit of cycle to process",
                    CommandOptionType.SingleValue);
                var silentOption = playcmd.Option(
                    "-s|--silent",
                    "Disable world output to display and ignore cycle delay option",
                    CommandOptionType.NoValue);
                var clearOption = playcmd.Option(
                    "-c|--clear",
                    "Clear console output to only display world",
                    CommandOptionType.NoValue);
                playcmd.HelpOption("-?|-h|--help");
                #endregion
                
                playcmd.OnExecute(() => {
                    Console.CursorVisible = false;
                    string inputPath = String.Empty;
                    World world = null;
                    try {
                        // Check input path
                        inputPath = Path.GetFullPath(inputPathArgument.Value);
                        // Initialize world
                        world = WorldHelper.Import(inputPath);
                    }
                    catch(Exception e) {
                        warnings.Add(e);
                        playcmd.ShowHelp();
                        return -2;
                    }

                    // Check configuration
                    var silent = silentOption.HasValue();

                    try {
                        if(!silent && cycleDelayOption.HasValue())
                            cycleDelay = int.Parse(cycleDelayOption.Value());
                        if(maxCycleOption.HasValue())
                            maxCycle = int.Parse(maxCycleOption.Value());
                    }
                    catch(Exception e) {
                        warnings.Add(e);
                        //playcmd.ShowHelp();
                        //return -2;
                    }
                    

                    if(clearOption.HasValue())
                        Console.Clear();

                    var cycleCount = Play(world, cycleDelay, maxCycle, silent);

                    if(cycleCount == maxCycle)
                        Console.WriteLine("This population configuration still holds {0} individuals after {1} cycles.", world.CurrentPopulation, cycleCount);
                    else {
                        Console.WriteLine("This population configuration survived for {0} cycles.", cycleCount);
                        if(world.CurrentPopulation > 0)
                            Console.WriteLine("Although some subsist, they will stagnate forever.");
                    }

                    // If provided output result to path
                    if(!outputPathOption.HasValue()) return 0;

                    try{
                        SaveWorld(world, outputPathOption.Value());                        
                    }
                    catch(Exception e) {
                        warnings.Add(e);
                    }

                    return 0;
                });
            }, throwOnUnexpectedArg:false);

            var editCommand = cliApp.Command("edit", (editcmd) => {
                editcmd.Description = "Allows you to create and edit image file to be used as starting configuration in the play command.";
                Console.CursorVisible = true;
                
                var outputPathArgument = editcmd.Argument(
                    "Output path",
                    "Path to world save location",
                    multipleValues:false);
                var widthOption = editcmd.Option(
                    "-x|--width",
                    "Width of the world to be created",
                    CommandOptionType.SingleValue);
                var heightOption = editcmd.Option(
                    "-y|--height",
                    "Height of the world to be created",
                    CommandOptionType.SingleValue);
                editcmd.HelpOption("-?|-h|--help");
                
                editcmd.OnExecute(() => {
                    string outputPath = String.Empty;
                    try {
                        // Check output path
                        outputPath = Path.GetFullPath(outputPathArgument.Value);
                    }
                    catch(Exception e) {
                        warnings.Add(e);
                        editcmd.ShowHelp();
                        return -1;
                    }
                    
                    // Check size options
                    Size worldSize = Size.Null;
                    int
                        width = 0,
                        height = 0;
                    
                    if(!(widthOption.HasValue() && int.TryParse(widthOption.Value(), out width)))
                        warnings.Add(new ArgumentException("! Specify width using -x, defaulting..."));
                    if(!(heightOption.HasValue() && int.TryParse(heightOption.Value(), out height)))
                        warnings.Add(new ArgumentException("! Specify height using -y, defaulting..."));
                    
                    if(width <= 0)
                        width = 20;
                    if(height <= 0)
                        height = width/2;

                    // Try to load
                    World world;
                    try {
                        world = WorldHelper.Import(outputPath);
                    } catch {
                        world = new World(width, height);
                    }

                    world = Edit(world);

                    try{
                        WorldHelper.Export(world, outputPath);
                    } catch(Exception e) {
                        warnings.Add(e);
                        return -1;
                    }

                    return 0;
                });
            });

            // Cli execution
            cliApp.OnExecute(() => {
                cliApp.ShowHelp();

                return 0;
            });

            warningOutputs = warnings;

            return cliApp;
        }

        private static int Play(World world, int cycleDelay, int maxCycle, bool silent) {
            // Cycle logic
            var cycleCount = 0;
            var displayMargin = 2;
            
            do {
                if(!silent) {
                    world.WaitFor(cycleDelay)
                        .Display()
                        .Jump(displayMargin);
                    Console.WriteLine("Cycle n°{0}", cycleCount+1);
                }
                world.Cycle();
                
                cycleCount++;
            } while(world.CurrentPopulation > 0 && cycleCount < maxCycle && world.CycleChangeCount > 0);
            // todo: Add stop when no changes

            if(!silent) {
                world.ShowEndScreen()
                    .Jump(displayMargin);
            }
            return cycleCount;
        }

        private static World Edit(World world) {
            var worldState = world.WorldState;

            var finishEdit = false;

            // Set Cursor
            var origin = WorldDisplayExtensions.displayOrigin;
            
            // Display world
            WorldDisplayExtensions.Print(worldState, world.Size, (state)=> {
                Console.Write(state ? 'O' : '-');
            });
            Console.SetCursorPosition(origin.X + world.Width/2, origin.Y + world.Height/2);

            // Read movement keys
            ConsoleKeyInfo keyPressed;
            do {
                keyPressed = Console.ReadKey(true);
                switch(keyPressed.Key) {
                    case ConsoleKey.LeftArrow:
                        if(Console.CursorLeft > 0)
                            Console.CursorLeft--;
                        break;
                    case ConsoleKey.UpArrow:
                        if(Console.CursorTop > origin.Y+1)
                            Console.CursorTop--;
                        break;
                    case ConsoleKey.RightArrow:
                        if(Console.CursorLeft < world.Width-1)
                            Console.CursorLeft++;
                        break;
                    case ConsoleKey.DownArrow:
                        if(Console.CursorTop < origin.Y+world.Height)
                            Console.CursorTop++;
                        break;
                    case ConsoleKey.Enter:
                        var index = Console.CursorLeft + (Console.CursorTop-1 - origin.Y) * world.Width;
                        worldState[index] = !worldState[index];
                        Console.Write(worldState[index] ? 'O' : '-');
                        Console.CursorLeft--;
                        break;
                    case ConsoleKey.Escape:
                        finishEdit = true;
                        Console.SetCursorPosition(0, origin.Y + world.Height);
                        break;
                }
            }
            while(!finishEdit);

            return new World(worldState, world.Size);
        }
        private static void SaveWorld(World world, string path) {
            var outputPath = Path.GetFullPath(path);
            var parentDirectory = Path.GetDirectoryName(outputPath);

            var fileName = Path.GetFileName(outputPath);
            var format = "png";
            
            // Generate filename if not provided
            if(fileName == string.Empty) {
                fileName = String.Format("{0}_{1}.{2}",
                    String.IsNullOrEmpty(world.Name) ? "gol" : world.Name,
                    world.Age,
                    format);
            }

            // Save file
            WorldHelper.Export(world, outputPath);

            // fileName = Path.ChangeExtension(fileName, format.ToString().ToLower());
            // outputPath = Path.Combine(parentDirectory, fileName);
            
            // using(var fs = new FileStream(outputPath, FileMode.OpenOrCreate)) {
            //     var image = world.ToBitmap();
            //     image.Save(fs, format);
            // }
        }
    }
}
