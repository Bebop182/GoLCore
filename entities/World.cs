using System;
using System.Collections.Generic;
using System.Linq;

namespace GOLCore {
    public delegate void CycleEventHandler();

    public class World {
        public event CycleEventHandler OnCycle;
        public int XResolution {get; private set;}
        public int YResolution {get; private set;}
        public Cell[] Grid {get; private set;}
        public int MaxPopulation => Grid.Length;
        public int Population => Grid.Count(c=>c.IsAlive);
        public bool[] State => Grid.Select(c=>c.IsAlive).ToArray();

        public static World Initialize(bool[] worldState, int width = 0, int height = 0) {
            var world = new World();
            world.XResolution = width > 0 ? width : (int)Math.Sqrt(worldState.Length);
            world.YResolution = height > 0 ? height : world.XResolution;
            world.Grid = worldState.Select(s => new Cell(s)).ToArray();
            return world;
        }

        public void Cycle() {
            Cell.ChangedStateCount = 0;
            for(int y=0, i=0; y<YResolution; y++) {
                for(int x=0; x<XResolution; x++, i++) {
                    var cell = Grid[i];
                    var neighbors = GetNeighbors(x, y);
                    OnCycle += ()=>cell.OnCycle(new CellCycleContext() { neighborCount = neighbors.Length });
                }
            }
            TriggerOnCycle();
            OnCycle = null;
        }

        private Cell[] GetNeighbors(int xPos, int yPos, int range = 1) {
            var neighbors = new List<Cell>();
            for(int y = yPos-range, yEnd = yPos+range; y<=yEnd; y++) {
                if(y < 0) continue;
                if(y >= YResolution) break;
                for(int x = xPos-range, xEnd = xPos+range; x<=xEnd; x++) {
                    if(x == xPos && y == yPos) continue;
                    if(x < 0) continue;
                    if(x >= XResolution) break;

                    var cell = Grid[x + y*XResolution];
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
