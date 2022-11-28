using ExhaustiveMatching;

public enum CompareResult { Lesser, Equals, Greater }

public static class CompareResult_ {
    public static CompareResult FromInt(int i) =>
    i switch {
        <= -1 => CompareResult.Lesser,
        0 => CompareResult.Equals,
        _ => CompareResult.Greater
    };
}

public static class CompareResultsExts {
    public static int ToInt (this CompareResult result) =>
        result switch {
            CompareResult.Lesser => -1,
            CompareResult.Equals => 0,
            CompareResult.Greater => 1,
            _ => throw ExhaustiveMatch.Failed(result)
        };

    public static CompareResult AsCompareResult(this int i) =>
        CompareResult_.FromInt(i);
}

public interface ICustomComparer<in A> : IComparer<A> {
    CompareResult Compare(A a1, A a2);
}

record class LambdaCustomComparer<A>(Func<A, A, CompareResult> cmp) : ICustomComparer<A> {
    public CompareResult Compare(A a1, A a2) => cmp(a1, a2);
    
    int IComparer<A>.Compare(A? x, A? y) => Compare(x!, y!).ToInt();
}

public static class CustomComparer {
    public static readonly ICustomComparer<int> @int =
        FromCompareTo<int>((i1, i2) => i1.CompareTo(i2));

    public static ICustomComparer<A> FromCompareTo<A>(Func<A, A, int> compareTo) {
        return new LambdaCustomComparer<A>((a1, a2) => compareTo(a1, a2).AsCompareResult());
    }
}

public static class ComparerExts {
    public static ICustomComparer<A> AndThen<A>(this ICustomComparer<A> comparer1, ICustomComparer<A> comparer2) =>
        new LambdaCustomComparer<A>((a1, a2) => {
            var result = comparer1.Compare(a1, a2);
            return result switch {
                CompareResult.Equals => comparer2.Compare(a1, a2),
                _ => result
            };
        });

    public static ICustomComparer<Whole> ContraMap<Whole, Part>(
        this ICustomComparer<Part> partCustomComparer, Func<Whole, Part> extractor
    ) =>
        new LambdaCustomComparer<Whole>((whole1, whole2) =>
            partCustomComparer.Compare(extractor(whole1), extractor(whole2)));
}