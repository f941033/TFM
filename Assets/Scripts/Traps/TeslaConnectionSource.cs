// TeslaConnectionSource.cs
public class TeslaConnectionSource
{
    public TeslaTowerController A { get; }
    public TeslaTowerController B { get; }

    public TeslaConnectionSource(TeslaTowerController a, TeslaTowerController b)
    {
        A = a; B = b;
    }
}
