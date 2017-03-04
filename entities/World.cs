using System;
using System.Collections.Generic;
using System.Linq;

namespace GOLCore {
    public delegate void CycleEventHandler();

    public class World {
        public event CycleEventHandler OnCycle;

        public int Resolution {get; private set;}
        public Cell[] Grid {get; private set;}
        public int AliveCellCount => Grid.Count(c=>c.IsAlive);
        public bool[] State => Grid.Select(c=>c.IsAlive).ToArray();

        private World(bool[] worldState) {
            //todo: better checks
            Grid = worldState.Select(s=>new Cell(){IsAlive = s}).ToArray();
            Resolution = (int)Math.Sqrt(Grid.Length);
        }

        public static World Initialize(bool[] worldState) {
            return new World(worldState);
        }

        public void Cycle() {
            for(int y=0; y<Resolution; y++) {
                // get cell
                for(int x=0; x<Resolution; x++) {
                    var cell = Grid[x+ y*Resolution];
                    var neighbors = GetNeighbors(x, y);
                    OnCycle += ()=>cell.OnCycle(new CellCycleContext() {neighborCount = neighbors.Length});
                }
            }
            TriggerOnCycle();
        }

        private Cell[] GetNeighbors(int xPos, int yPos, int range = 1) {
            var neighbors = new List<Cell>();
            for(int y = yPos-range, yEnd = yPos+range ; y<=yEnd; y++) {
                if(y < 0) continue;
                if(y >= Resolution) break;
                for(int x = xPos-range, xEnd = xPos+range; x<=xEnd; x++) {
                    if(x == xPos && y == yPos) continue;
                    if(x < 0) continue;
                    if(x >= Resolution) break;

                    var cell = Grid[x + y*Resolution];
                    if(cell.IsAlive)
                        neighbors.Add(cell);
                }
            }
            return neighbors.ToArray();
        }

        private void TriggerOnCycle() {
            if(OnCycle != null)
                OnCycle.Invoke();
        }
    }
}
