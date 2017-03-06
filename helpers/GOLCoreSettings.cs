public class GOLCoreSettings {
    public int CycleDelay {get; set;}
    public readonly char aliveSymbol;
    public readonly char deadSymbol;
    
    public GOLCoreSettings() {
        aliveSymbol = 'O';
        deadSymbol = '#';
    }
}