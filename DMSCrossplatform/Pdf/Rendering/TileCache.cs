using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace DMSCrossplatform.Pdf.Rendering;

public class TileCache
{
    private readonly int _capacity = 200;
    private readonly Dictionary<TileKey, WriteableBitmap> _dict = new();
    private readonly LinkedList<TileKey> _lru = new();

    public bool TryGet(TileKey key, out WriteableBitmap bmp)
    {
        if (_dict.TryGetValue(key, out bmp))
        {
            _lru.Remove(key);
            _lru.AddFirst(key);
            return true;
        }
        return false;
    }

    public void Add(TileKey key, WriteableBitmap bmp)
    {
        if (_dict.ContainsKey(key))
            return;

        _dict[key] = bmp;
        _lru.AddFirst(key);

        if (_dict.Count > _capacity)
        {
            var last = _lru.Last!.Value;
            _lru.RemoveLast();

            _dict.Remove(last);
        }
    }
}