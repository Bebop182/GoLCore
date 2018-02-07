using System;
using System.Linq;
using System.Collections.Generic;

namespace GOLCore {
    public class Cell {
        private bool _currentState;
        private bool _nextState;
        private bool _isAwake;

        public bool IsAwake {
            get { return _isAwake || IsAlive; }
            set { _isAwake = value; }
        }
        public bool IsAlive => _currentState;
        public bool Evolved { get; private set; }

        public List<Cell> Neighbors;

        public Cell(bool initState = false)
        {
            _nextState = initState;
            _currentState = _nextState;
        }

        public void OnCycleStart() {
            Evolved = false;
            if(IsAlive)
                WakeNeighbors(true);
        }

        public void OnProcessCycle() {
            if(!IsAwake) return;

            var aliveNeighborCount = Neighbors.Count(c=>c.IsAlive);
            
            if(!IsAlive) {
                //Birth conditions            
                if(aliveNeighborCount == 3) {
                    _nextState = true;
                    return;
                }
            }
            else {
                //Death conditions
                if(aliveNeighborCount > 3){
                    _nextState = false;
                    return;
                }
                if(aliveNeighborCount <= 1){
                    _nextState = false;
                    return;
                }
            }
        }

        public void OnCycleEnd() {
            if(!IsAwake) return;

            if(_nextState != _currentState) {
                _currentState = _nextState;
                Evolved = true;
            }

            IsAwake = false;
        }

        private void WakeNeighbors(bool wake) {
            foreach(var neighbor in Neighbors) {
                neighbor.IsAwake = wake;
            }
        }
    }
}
