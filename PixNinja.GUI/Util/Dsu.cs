using System.Linq;

namespace PixNinja.GUI.Util;

public class Dsu
{
    private int[] _pa;

    public Dsu(int size)
    {
        _pa = Enumerable.Range(0, size).ToArray();
    }

    public int Find(int elem)
    {
        return _pa[elem] == elem ? elem : _pa[elem] = Find(_pa[elem]);
    }

    public void Union(int a, int b)
    {
        if (a == b) return;
        _pa[Find(a)] = Find(b);
    }
}
