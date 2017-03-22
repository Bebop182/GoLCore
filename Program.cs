using System;
using System.Drawing.Imaging;
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
            
            var cliApp = ConfigureApplication();
            
            cliApp.Execute(args);
        }

        private static CommandLineApplication ConfigureApplication() {
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
            var playCommand = cliApp.Command("play", (command) => {
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
                command.HelpOption("-?|-h|--help");
                #endregion
                
                command.OnExecute(() => {
                    Console.CursorVisible = false;
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

                    var cycleCount = Play(world, cycleDelay, maxCycle, silent);

                    if(cycleCount == maxCycle)
                        Console.WriteLine("This population configuration still holds {0} individuals after {1} cycles.", world.CurrentPopulation, cycleCount);
                    else {
                        Console.WriteLine("This population configuration survived for {0} cycles.", cycleCount);
                        if(world.CurrentPopulation > 0)
                            Console.WriteLine("Although some subsist, they will stagnate forever.");
                    }

                    // If provided output result to path
                    if(outputPathOption.HasValue()) {
                        SaveWorld(world, outputPathOption.Value(), cycleCount);                        
                    }

                    return 0;
                });
            }, throwOnUnexpectedArg:false);
            var editCommand = cliApp.Command("edit", (command) => {
                command.Description = "Allows you to create and edit image file to be used as starting configuration in the play command.";
                Console.CursorVisible = true;
                var outputPathArgument = command.Argument(
                    "Output path",
                    "Path to world save location",
                    multipleValues:false);
                var widthOption = command.Option(
                    "-x|--width",
                    "Width of the world to be created",
                    CommandOptionType.SingleValue);
                var heightOption = command.Option(
                    "-y|--height",
                    "Height of the world to be created",
                    CommandOptionType.SingleValue);
                command.HelpOption("-?|-h|--help");
                
                command.OnExecute(() => {
                    // Check output path
                    var outputPath = Path.GetFullPath(outputPathArgument.Value);
                    // Check size
                    Size worldSize = Size.Null;
                    int
                        width = 0,
                        height = 0;
                    
                    if(widthOption.HasValue())
                        int.TryParse(widthOption.Value(), out width);
                    if(heightOption.HasValue())
                        int.TryParse(heightOption.Value(), out height);
                    
                    if(width <= 0)
                        width = 20;
                    if(height <= 0)
                        height = width;

                    worldSize = new Size(width, height);
                    var world = Edit(worldSize);

                    using(var image = world.ToBitmap())
                    using(var fs = new FileStream(outputPath, FileMode.OpenOrCreate)) {
                        image.Save(fs, ImageFormat.Png);
                    }

                    return 0;
                });
            });

            // Cli execution
            cliApp.OnExecute(() => {
                cliApp.ShowHelp();

                return 0;
            });

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
                world.TriggerCommitCycle();
                cycleCount++;
            } while(world.CurrentPopulation > 0 && Cell.ChangedStateCount > 0 && cycleCount < maxCycle);

            if(!silent) {
                world.ShowEndScreen()
                    .Jump(displayMargin);
            }
            return cycleCount;
        }

        private static World Edit(Size size) {
            var worldState = new bool[size.Area()];

            var finishEdit = false;

            // Set Cursor
            var origin = WorldDisplayExtensions.displayOrigin;
            
            // Display world
            WorldDisplayExtensions.Print(worldState, size, (state)=> {
                Console.Write(state ? 'O' : '-');
            });
            Console.SetCursorPosition(origin.X + size.Width/2, origin.Y + size.Height/2);

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
                        if(Console.CursorLeft < size.Width-1)
                            Console.CursorLeft++;
                        break;
                    case ConsoleKey.DownArrow:
                        if(Console.CursorTop < origin.Y+size.Height)
                            Console.CursorTop++;
                        break;
                    case ConsoleKey.Enter:
                        var index = Console.CursorLeft + (Console.CursorTop - origin.Y)*size.Width;
                        worldState[index] = !worldState[index];
                        Console.Write(worldState[index] ? 'O' : '-');
                        Console.CursorLeft--;
                        break;
                    case ConsoleKey.Escape:
                        finishEdit = true;
                        Console.SetCursorPosition(0, origin.Y + size.Height);
                        break;
                }
            }
            while(!finishEdit);

            return new World(worldState, size);
        }

        private static void SaveWorld(World world, string path, int cycleCount) {
            var outputPath = Path.GetFullPath(path);
            var parentDirectory = Path.GetDirectoryName(outputPath);

            var fileName = Path.GetFileName(outputPath);
            var format = WorldConverter.GetImageFormat(outputPath);
            
            if(fileName == string.Empty)
                fileName = String.Format("{0}_{1}",
                    String.IsNullOrEmpty(world.Name) ? "gol" : world.Name,
                    cycleCount);

            fileName = Path.ChangeExtension(fileName, format.ToString().ToLower());
            outputPath = Path.Combine(parentDirectory, fileName);
            
            using(var fs = new FileStream(outputPath, FileMode.OpenOrCreate)) {
                var image = world.ToBitmap();
                image.Save(fs, format);
            }
        }
    }
}
