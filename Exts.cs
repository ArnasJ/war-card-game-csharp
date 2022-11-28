using LanguageExt;
using static LanguageExt.Prelude;

public static class ExtensionMethods {
    public static Either<
        (Seq<A> a1, Seq<A> a2),
        (Seq<A> a1, Seq<A> a2, A remainder)
    > Split<A>(this IEnumerable<A> enumerable) {
        using var enumerator = enumerable.GetEnumerator();

        var firstHalf = new List<A>();
        var secondHalf = new List<A>();
        while (enumerator.MoveNext()) {
            var firstElement = enumerator.Current;

            if (enumerator.MoveNext()) {
                var secondElement = enumerator.Current;
                firstHalf.Add(firstElement);
                secondHalf.Add(secondElement);
            }
            else {
                return Right((firstHalf.ToSeq(), secondHalf.ToSeq(), firstElement));
            }
        }

        return Left((firstHalf.ToSeq(), secondHalf.ToSeq()));
    }

    public static (Seq<A> a1, Seq<A> a2) Split<A>(this IMarked<IEnumerable<A>, Even> enumerable) =>
        enumerable.data
            .Split()
            .Match(
                _ => throw new Exception("Collection has to be Even"),
                tpl => tpl
            );

    public static Seq<(A, A)> SplitAsTuples<A>(this IMarked<IEnumerable<A>, Even> enumerable) {
        var (a1, a2) = enumerable.Split();
        return a1.Zip(a2, (aa1, aa2) => (aa1, aa2));
    }
        
    public static Eff<A> GetRandom<A>(this IMarked<Arr<A>, NonEmpty> array, Random rng) =>
        Eff(() => {
            var arr = array.data;
            return arr[rng.Next(arr.Length)];    
        });
    
    public static Eff<Unit> WriteLineEff(string str) =>
        Eff(() => {
            Console.WriteLine(str);
            return Unit.Default;
        });
}

public interface IMarked<out Data, Marker> {
    Data data { get; }
}

public static class MarkedExts {
    public static IMarked<Data2, Marker> TransformUnsafe<Marker, Data, Data2>(
        this IMarked<Data, Marker> marked, Func<Data, Data2> transformer
    ) =>
        new Marked<Data2, Marker>(transformer(marked.data));
}

public record Marked<Data, Marker>(Data data) : IMarked<Data, Marker>;
public static class Marked {
    public static IMarked<Data, Marker> Mark<Data, Marker>(this Data data, in Marker marker) =>
        new Marked<Data, Marker>(data);
}

public readonly struct NonEmpty { };
public readonly struct Even { };
