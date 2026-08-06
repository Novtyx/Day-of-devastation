using Unity.Collections;

internal class NativeHashMap<T1, T2>
{
    private int v;
    private Allocator persistent;

    public NativeHashMap(int v, Allocator persistent)
    {
        this.v = v;
        this.persistent = persistent;
    }
}