namespace Tabalonia;


public static class ZIndexes
{
    public const int Selected = int.MaxValue;
    
    public const int PointerOver = Selected - 1;

    public const int NonSelected = Selected - 10;
}