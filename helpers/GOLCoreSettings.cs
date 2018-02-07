public class GOLCoreSettings {
    public int CycleRate {get; set;}
    public readonly char aliveSymbol;
    public readonly char deadSymbol;
    
    public GOLCoreSettings() {
        aliveSymbol = 'O';
        deadSymbol = '#';
    }
}