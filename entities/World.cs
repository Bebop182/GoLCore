using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GOLCore.Structures;

namespace GOLCore {
    public class World {
        private Action CycleStart, CycleProcess, CycleEnd;

        public string Name {get; set;}
        public uint Age {get; private set;}

        public Size Size {get;}
        public int Width => Size.Width;
        public int Height => Size.Height;
        public readonly ReadOnlyCollection<Cell> CellGrid;
        // Cache cells coordinates
        public readonly Dictionary<Cell, Point> CellsCoordinates;
        
        public int MaxPopulation => CellGrid.Count;
        public int CurrentPopulation => CellGrid.Count(c=>c.IsAlive);
        public int CycleChangeCount => CellGrid.Count(c=>c.Evolved);
        public bool[] WorldState => CellGrid.Select(c=>c.IsAlive).ToArray();

        public World(bool[] worldState, int width, int height) {
            width = Math.Clamp(width, 0, int.MaxValue);
            height = Math.Clamp(height, 0, int.MaxValue);

            Size = new Size(width, height);
            Name = String.Empty;
            Age = 0;         

            CellsCoordinates = new Dictionary<Cell, Point>();

            var cells = worldState.Select((state, index) => {
                var cell = new Cell(state);

                // Find coordinates in the grid
                var x = index % Width;
                var y = index / Width;

                // Cache coordinates
                CellsCoordinates.Add(cell, new Point(x, y));

                CycleStart += cell.OnCycleStart;
                CycleProcess += cell.OnProcessCycle;
                CycleEnd += cell.OnCycleEnd;
                
                return cell;
            }).ToList();

            CellGrid = new ReadOnlyCollection<Cell>(cells);

            // Cache neighbors
            foreach(var cell in cells) {
                var n = GetNeighbors(CellsCoordinates[cell]);
                cell.Neighbors = n;
            }
        }

        public World (int width, int height) : this(new bool[width*height], width, height) {
        }

        public World(bool[] worldState, Size size) : this(worldState, size.Width, size.Height) {
        }

        public void Cycle() {
            TriggerCycleStart();

            TriggerCycleProcess();

            TriggerCycleEnd();

            //Console.WriteLine("Population {0} / Awake {1}", CurrentPopulation, cells.Count);

            Age++;
        }

        private List<Cell> GetNeighbors(int xPosition, int yPosition, int range = 1) {
            var neighbors = new List<Cell>();
            
            int
                xStart = Math.Max(xPosition - range, 0),
                xEnd = Math.Min(xPosition + range, Width-1),
                yStart = Math.Max(yPosition - range, 0),
                yEnd = Math.Min(yPosition + range, Height-1);
            //var cell = CellGrid[xPosition + yPosition*Width];
                
            for(int y = yStart; y <= yEnd; y++) {
                for(int x = xStart; x <= xEnd; x++) {
                    if(x == xPosition && y == yPosition) continue;

                    var index = x + y*Width;
                    var cell = CellGrid[index];
                    neighbors.Add(cell);
                }
            }

            return neighbors;
        }

        private List<Cell> GetNeighbors(Point coordinates, int range = 1) {
            return GetNeighbors(coordinates.X, coordinates.Y, range);
        }

        public void TriggerCycleStart() {
            CycleStart?.Invoke();
        }

        public void TriggerCycleProcess() {
            CycleProcess?.Invoke();
        }

        public void TriggerCycleEnd() {
            CycleEnd?.Invoke();
        }
    }
}
