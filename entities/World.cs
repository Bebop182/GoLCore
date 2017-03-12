using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GOLCore.Structures;

namespace GOLCore {
    public class World {
        private EventHandler CommitCycle = delegate {};

        public string Name {get; set;}
        public readonly ReadOnlyCollection<Cell> CellGrid;
        public readonly int XResolution;
        public readonly int YResolution;
        public int MaxPopulation => CellGrid.Count;
        public int CurrentPopulation => CellGrid.Count(c=>c.IsAlive);

        public World(bool[] worldState, int width = 0, int height = 0) {
            Name = String.Empty;
            XResolution = width > 0 ? width : (int)Math.Sqrt(worldState.Length);
            YResolution = height > 0 ? height : XResolution;

            var grid = worldState.Select(state => {
                var cell = new Cell(state);
                CommitCycle += (sender, e) => cell.CommitStateChange(commitAll:false);
                return cell;
            }).ToArray();

            CellGrid = new ReadOnlyCollection<Cell>(grid);
        }

        public World(bool[] worldState, Size size) : this(worldState, size.Width, size.Height) {
        }

        public void Cycle() {
            Cell.ChangedStateCount = 0;

            for(int y=0, i=0; y<YResolution; y++) {
                for(int x=0; x<XResolution; x++, i++) {
                    // There will be more properties in the future
                    var cycleContext = new CellCycleContext() {
                        neighborCount = GetNeighbors(x, y).Count,
                    };
                    CellGrid[i].ProcessCycle(cycleContext);
                }
            }
        }

        private List<Cell> GetNeighbors(int xPosition, int yPosition, int range = 1) {
            int
                xStart = Math.Max(xPosition - range, 0),
                xEnd = Math.Min(xPosition + range, XResolution-1),
                yStart = Math.Max(yPosition - range, 0),
                yEnd = Math.Min(yPosition + range, YResolution-1);
            var neighbors = new List<Cell>();
                
            for(int y = yStart; y <= yEnd; y++) {
                for(int x = xStart; x <= xEnd; x++) {
                    if(x == xPosition && y == yPosition) continue;

                    var index = x + y*XResolution;
                    var cell = CellGrid[index];
                    if(cell.IsAlive)
                        neighbors.Add(cell);
                }
            }

            return neighbors;
        }

        public void TriggerCommitCycle() {
            CommitCycle?.Invoke(this, EventArgs.Empty);
        }
    }
}
