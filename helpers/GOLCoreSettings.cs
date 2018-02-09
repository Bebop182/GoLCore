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

    public class RuleSet : Singleton<RuleSet> {
        public int OverPopulationThreshold = 4; // default: 4
        public int IsolationThreshold = 1; // default: 1
        public int ParentsRequired = 3; // default: 3
        public int ProximityRange = 1; // default: 1
    }
}
