namespace DoTs
{
    [System.Flags]
    public enum Layer
    {
        Default = 0,
        Enemy = 1,
        Building = 1 << 1,
    }
}