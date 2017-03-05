namespace GOLCore {
    public class Cell {
        public static int ChangedStateCount {get; set;}
        
        public bool IsAlive {get; private set;}
        public bool StateChanged {get; private set;}

        static Cell() {
            // Load configurations
            ChangedStateCount = 0;
        }

        public Cell(bool initState = false) {
            IsAlive = initState;
            StateChanged = false;
        }

        public void OnCycle(CellCycleContext cellContext) {
            StateChanged = true;
            ChangedStateCount++;

            //Birth conditions
            if(!IsAlive) {
                if(cellContext.neighborCount == 3) {
                    IsAlive = true;
                    return;
                }
            }
            else {
                //Death conditions
                if(cellContext.neighborCount > 3){
                    IsAlive = false;
                    return;
                }
                if(cellContext.neighborCount <= 1){
                    IsAlive = false;
                    return;
                }
            }
            
            ChangedStateCount--;
            StateChanged = false;
        }
    }

    public class CellCycleContext {
        public int neighborCount {get; set;}
    }
}
