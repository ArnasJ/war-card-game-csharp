using System.Text;
using ExhaustiveMatching;
using LanguageExt;

public enum Winner {
    Player2 = -1,
    Tie,
    Player1
}

public static class War {
    public static (Seq<Card> scorePile1, Seq<Card> scorePile2) CardsToScorePiles(
        IEnumerable<(Card player1, Card player2, Winner winner)> cards
    ) {
        return cards.Aggregate(
            (player1Score :Seq<Card>.Empty, player2Score :Seq<Card>.Empty),
            (current, tpl) => tpl.winner switch {
                Winner.Player2 => (
                    current.player1Score,
                    current.player2Score.Add(tpl.player1).Add(tpl.player2)
                ),
                Winner.Tie => (
                    current.player1Score.Add(tpl.player1),
                    current.player2Score.Add(tpl.player2)
                ),
                Winner.Player1 => (
                    current.player1Score.Add(tpl.player1).Add(tpl.player2),
                    current.player2Score
                ),
                _ => throw ExhaustiveMatch.Failed(tpl.winner)
            }
        );
    }

    public static IEnumerable<(Card player1, Card player2, Winner)> CompareDeck(
        IEnumerable<(Card player1, Card player2)> deck, ICustomComparer<Card> comparer
    ) =>
        deck.Select(
            tpl => {
                var result = comparer.Compare(tpl.player1, tpl.player2);
                var winner =  comparer.Compare(tpl.player1, tpl.player2) switch {
                    CompareResult.Lesser => Winner.Player2,
                    CompareResult.Equals => Winner.Tie,
                    CompareResult.Greater => Winner.Player1,
                    _ => throw ExhaustiveMatch.Failed(result)
                };

                return (tpl.player1, tpl.player2, winner);
            }
        );

    public static string RoundEndText(Card p1, Card p2, Winner winner, int roundNumber) {
        var winnerText =  winner switch {
            Winner.Player2 => "Player 2 wins",
            Winner.Tie => "It is a tie",
            Winner.Player1 => "Player 1 wins",
            _ => throw ExhaustiveMatch.Failed(winner)
        };

        var sb = new StringBuilder();
        sb.Append($"Round {roundNumber}: P1 has {ShowCard(p1)}, P2 has {ShowCard(p2)}. ");
        sb.Append(winnerText);
        return sb.ToString();
    }

    public static string GameWinnerText(Winner winner) =>
        winner switch {
            Winner.Player2 => "Player 2 is game winner",
            Winner.Tie => "Game ended in a tie",
            Winner.Player1 => "Player 1 is game winner",
            _ => throw ExhaustiveMatch.Failed(winner)
        };
    
    public static string ShowRank(Rank rank) => rank.AsString();
    public static string ShowSuite(Suite suite) => suite.AsString();
    public static string ShowCard(Card card) => $"{ShowSuite(card.Suite)}{ShowRank(card.rank)}";
}