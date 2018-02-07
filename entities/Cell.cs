using System;
using System.Collections.Generic;

namespace GOLCore {
    public class Cell {
        private bool _state;
        public bool StagedState {get; private set;}
        public bool IsAlive => _state;

        private bool _isAwake = true;
        public bool IsAwake {
            get {
                return _isAwake || IsAlive;
            }
            set {
                _isAwake = value;
            }
        }

        public Cell(bool initState = false)
        {
            _state = initState;
            StagedState = _state;
        }

        public void ProcessCycle(int aliveNeighborCount) {
            StagedState = _state;
            
            if(!IsAlive) {
                //Birth conditions            
                if(aliveNeighborCount == 3) {
                    StagedState = true;
                    return;
                }
            }
            else {
                //Death conditions
                if(aliveNeighborCount > 3){
                    StagedState = false;
                    return;
                }
                if(aliveNeighborCount <= 1){
                    StagedState = false;
                    return;
                }
            }
        }

        public void Commit() {
            //if(!IsAwake) return;
            _state = StagedState;
        }
    }
}
