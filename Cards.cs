using ExhaustiveMatching;
using LanguageExt;

public enum Suite {
    Hearts, Diamonds, Clubs, Spades
}

public static class SuiteExts {
    public static string AsString(this Suite suite) =>
        suite switch {
            Suite.Hearts => "H",
            Suite.Diamonds => "D",
            Suite.Clubs => "C",
            Suite.Spades => "S",
            _ => throw ExhaustiveMatch.Failed(suite)
        };
}

public static class Suite_ {
    public static readonly IMarked<Arr<Suite>, NonEmpty> all =
        Enum.GetValues<Suite>()
            .ToArr()
            .Mark(default(NonEmpty));

    public static ICustomComparer<Suite> TrumpCompare(Suite trump) =>
        new LambdaCustomComparer<Suite>(
            (suite1, suite2) => {
                if (suite1 == suite2) return CompareResult.Equals;
                if (suite1 == trump) return CompareResult.Greater;
                if (suite2 == trump) return CompareResult.Lesser;
                return CompareResult.Equals;
            });
}

public enum Rank {
    _2, _3, _4, _5, _6, _7, _8, _9, _10, Jack, Queen, King, Ace
}

public static class Rank_ {
    public static readonly ICustomComparer<Rank> Compare =
        CustomComparer.@int.ContraMap((Rank rank) => rank.AsInt());
}

public static class RankExts {
    public static int AsInt(this Rank rank) =>
        rank switch {
            Rank._2 => 2,
            Rank._3 => 3,
            Rank._4 => 4,
            Rank._5 => 5,
            Rank._6 => 6,
            Rank._7 => 7,
            Rank._8 => 8,
            Rank._9 => 9,
            Rank._10 => 10,
            Rank.Jack => 11,
            Rank.Queen => 12,
            Rank.King => 13,
            Rank.Ace => 14,
            _ => throw ExhaustiveMatch.Failed(rank)
        };

    public static string AsString(this Rank rank) =>
        rank switch {
            Rank._2 => "2",
            Rank._3 => "3",
            Rank._4 => "4",
            Rank._5 => "5",
            Rank._6 => "6",
            Rank._7 => "7",
            Rank._8 => "8",
            Rank._9 => "9",
            Rank._10 => "10",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => throw ExhaustiveMatch.Failed(rank)
        };
}

public record Card(Suite Suite, Rank rank) {
    public static ICustomComparer<Card> TrumpCompare(Suite trump) =>
        Suite_.TrumpCompare(trump)
            .ContraMap((Card card) => card.Suite)
            .AndThen(Rank_.Compare.ContraMap((Card card) => card.rank));
}

public static class PlayingDeck {
    public static readonly IMarked<Arr<Card>, Even> deck =
        Enum.GetValues<Suite>()
            .SelectMany(_ => Enum.GetValues<Rank>(), (suite, rank) => new Card(suite, rank))
            .ToArr()
            .Mark(default(Even));
}