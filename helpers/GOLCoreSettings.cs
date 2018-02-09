namespace GOLCore {
    public class ApplicationSettings {
        public int CycleRate {get; set;} = 500; // in ms
        public int MaxCycles {get; set;} = 2000;
        public readonly char aliveSymbol;
        public readonly char deadSymbol;
        
        public ApplicationSettings() {
            aliveSymbol = 'O';
            deadSymbol = ' ';
        }
    }

}