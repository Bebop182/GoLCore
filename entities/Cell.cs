using System;
using System.Collections.Generic;

namespace GOLCore {
    public class Cell {
        public static int ChangedStateCount {get; set;}
        private readonly Queue<bool> _stagedStates;
        
        public bool IsAlive {get; private set;}
        public bool StagedState {
            get {
                return _stagedStates.Count > 0 ? _stagedStates.Peek() : IsAlive;
            }
            private set {
                if(IsAlive == value) return;
                _stagedStates.Enqueue(value);
            }
        }
        public bool StateChanged => !IsAlive.Equals(StagedState);

        static Cell() {
            // Load configurations
            ChangedStateCount = 0;
        }

        public Cell(bool initState = false)
        {
            _stagedStates = new Queue<bool>();
            IsAlive = initState;
        }

        public void ProcessCycle(CellCycleContext cellContext) {
            ChangedStateCount++;

            //Birth conditions
            if(!IsAlive) {
                if(cellContext.neighborCount == 3) {
                    StagedState = true;
                    return;
                }
            }
            else {
                //Death conditions
                if(cellContext.neighborCount > 3){
                    StagedState = false;
                    return;
                }
                if(cellContext.neighborCount <= 1){
                    StagedState = false;
                    return;
                }
            }
            
            ChangedStateCount--;
        }

        public void CommitStateChange(bool commitAll = false) {
            if(_stagedStates.Count <= 0) return;

            IsAlive = _stagedStates.Dequeue();
            if(commitAll)
                CommitStateChange(commitAll:true);
        }

        public void AbordStateChange() {
            _stagedStates.Clear();
        }
    }

    public class CellCycleContext : EventArgs {
        public int neighborCount {get; set;}
    }
}
