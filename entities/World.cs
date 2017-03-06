using System;
using System.Collections.Generic;
using System.Linq;

namespace GOLCore {
    public class World {
        private EventHandler CommitCycle;
        public int XResolution {get; private set;}
        public int YResolution {get; private set;}
        public Cell[] Grid {get; private set;}
        public int MaxPopulation => Grid.Length;
        public int Population => Grid.Count(c=>c.IsAlive);

        public World(bool[] worldState, int width = 0, int height = 0) {
            XResolution = width > 0 ? width : (int)Math.Sqrt(worldState.Length);
            YResolution = height > 0 ? height : XResolution;
            Grid = worldState.Select(s => {
                var cell = new Cell(s);
                CommitCycle += (sender, e) => cell.CommitStateChange(false);
                return cell;
            }).ToArray();
        }

        public void Cycle() {
            Cell.ChangedStateCount = 0;

            for(int y=0, i=0; y<YResolution; y++) {
                for(int x=0; x<XResolution; x++, i++) {
                    var cycleContext = new CellCycleContext() {
                        neighborCount = GetNeighbors(Grid, x, y).Length
                    };
                    var cell = Grid[i];
                    cell.ProcessCycle(cycleContext);
                }
            }
        }

        public void TriggerCommitCycle() {
            CommitCycle?.Invoke(this, EventArgs.Empty);
        }

        private T[] GetNeighbors<T>(T[] source, int xPosition, int yPosition, int range = 1) where T : Cell {
            int
                xStart = Math.Max(xPosition - range, 0),
                xEnd = Math.Min(xPosition + range, XResolution-1),
                yStart = Math.Max(yPosition - range, 0),
                yEnd = Math.Min(yPosition + range, YResolution-1);
            var neighbors = new List<T>();
                
            for(int y = yStart; y <= yEnd; y++) {
                for(int x = xStart; x <= xEnd; x++) {
                    if(x == xPosition && y == yPosition) continue;
                    var i = x + y*XResolution;
                    var cell = source[i];
                    if(cell.IsAlive)
                        neighbors.Add(cell);
                }
            }

            return neighbors.ToArray();
        }
    }
}
