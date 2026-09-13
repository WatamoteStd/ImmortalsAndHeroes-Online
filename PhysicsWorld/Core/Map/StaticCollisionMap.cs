namespace PhysWorld.Map;

public class StaticCollisionMap
{
    
    public ushort Width {get; private set;}
    public ushort Height {get; private set;}
    public byte[] Data {get; private set;}


    /// <summary>
    /// Creates static collision map (layer 0). Minimal size 10
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public StaticCollisionMap(ushort width, ushort height, byte[]? data = null)
    {

        if (width <= 10 || height <= 10)
        {
            Width = 10;
            Height = 10;
        }
        else
        {
            Height = height;
            Width = width;
        }

        int requiredSize = (Width * Height + 7) / 8;

        if (data is null)
        {
            Data = new byte[requiredSize];
        }
        else
        {
            if (data.Length < requiredSize)
                throw new ArgumentException("Map array is too small. It's can be damaged");

            Data = data;
        }

    }

}