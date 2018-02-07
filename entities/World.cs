using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GOLCore.Structures;

namespace GOLCore {
    public class World {
        private Action CycleCommit = delegate {};

        public string Name {get; set;}
        public uint Age {get; private set;}

        public readonly int Width;
        public readonly int Height;
        public readonly ReadOnlyCollection<Cell> CellGrid;
        // Store precomputed cells coordinates
        public readonly Dictionary<Cell, Point> CellsCoordinates;
        
        public int MaxPopulation => CellGrid.Count;
        public int CurrentPopulation => CellGrid.Count(c=>c.IsAlive);

        public World(bool[] worldState, int width = 0, int height = 0) {
            Name = String.Empty;
            Age = 1;
            Width = width > 0 ? width : (int)Math.Sqrt(worldState.Length);
            Height = height > 0 ? height : Width;

            CellsCoordinates = new Dictionary<Cell, Point>();

            var grid = worldState.Select((state, index) => {
                var cell = new Cell(state);

                // Find coordinates in the grid
                var x = index % Width;
                var y = index / Width;

                CellsCoordinates.Add(cell, new Point(x, y));

                CycleCommit += cell.Commit;
                
                return cell;
            }).ToArray();

            CellGrid = new ReadOnlyCollection<Cell>(grid);
        }

        public World (int width, int height) : this(new bool[width*height], width, height) {
        }

        public World(bool[] worldState, Size size) : this(worldState, size.Width, size.Height) {
        }

        public void Cycle() {
            // var cells = CellGrid
            // .Where(c=> c.IsAlive || c.IsAwake)
            // .Select(c => {c.IsAwake = false; return c;})
            // .ToList();

            var cells = CellGrid;

            Console.WriteLine("Population {0} / Awake {1}", CurrentPopulation, cells.Count);

            foreach(var cell in cells) {
                var coord = CellsCoordinates[cell];
                var neighbors = GetNeighbors(coord.X, coord.Y);
                cell.ProcessCycle(aliveNeighborCount:neighbors.Count(n=>n.IsAlive));

                // If cell is alive next cycle, awake neighbors
                /*if(cell.StagedState)
                    neighbors.Select( n => {n.IsAwake = true; return n;} );*/
            }

            CycleCommit();

            Age++;
        }

        private List<Cell> GetNeighbors(int xPosition, int yPosition, int range = 1) {
            int
                xStart = Math.Max(xPosition - range, 0),
                xEnd = Math.Min(xPosition + range, Width-1),
                yStart = Math.Max(yPosition - range, 0),
                yEnd = Math.Min(yPosition + range, Height-1);
            var neighbors = new List<Cell>();
            var cell = CellGrid[xPosition + yPosition*Width];
                
            for(int y = yStart; y <= yEnd; y++) {
                for(int x = xStart; x <= xEnd; x++) {
                    if(x == xPosition && y == yPosition) continue;

                    var index = x + y*Width;
                    neighbors.Add(CellGrid[index]);
                }
            }

            return neighbors;
        }

        public void TriggerCycleCommit() {
            CycleCommit?.Invoke();
        }
    }
}
