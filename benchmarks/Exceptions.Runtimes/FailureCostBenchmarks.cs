using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Exceptions.Runtimes;

/// <summary>
/// The cost of reporting ONE failure from <see cref="Depth"/> frames down, either by throwing
/// (caught at the top) or by returning a value that every frame passes back up.
/// </summary>
[MemoryDiagnoser]
public class FailureCostBenchmarks
{
    [Params(1, 10, 50)]
    public int Depth { get; set; }

    [Benchmark]
    public bool Throw()
    {
        try
        {
            ThrowAt(Depth);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    [Benchmark]
    public bool Return() => FailAt(Depth) < 0;

    // Not in tail position (+ 1), and never inlined, so every level is a real stack frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ThrowAt(int depth) =>
        depth <= 1 ? throw new InvalidOperationException("Customer blocked.") : ThrowAt(depth - 1) + 1;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int FailAt(int depth)
    {
        if (depth <= 1) return -1;
        var inner = FailAt(depth - 1);
        return inner < 0 ? inner : inner + 1;
    }
}
