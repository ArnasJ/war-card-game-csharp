using System.Text;
using ExhaustiveMatching;
using LanguageExt;
using static ExtensionMethods;
using static LanguageExt.Prelude;

var program =
    from rng in Eff(() => new Random())
    from trump in Suite_.all.GetRandom(rng)
    from shuffledDeck in Eff(() => PlayingDeck.deck.TransformUnsafe(_ => _.OrderBy(_ => rng.Next())) )
    let cardComparer = Card.TrumpCompare(trump)
    let playerCards = shuffledDeck.SplitAsTuples()
    let roundsWinners = War.CompareDeck(playerCards, cardComparer).ToSeq()
    let scorePiles = War.CardsToScorePiles(roundsWinners)
    select (trump, roundsWinners, scorePiles);

program
    .Map(tpl => RenderResults(tpl.trump, tpl.roundsWinners, tpl.scorePiles))
    .Bind(WriteLineEff)
    .Run()
    .Match(
        Succ: _ => {},
        Fail: error => {
            Console.Error.WriteLine(error);
            Environment.Exit(1);
        }
    );

string RenderResults(
    Suite trump,
    Seq<(Card player1, Card player2, Winner)> roundsWinners,
    (Seq<Card> scorePile1, Seq<Card> scorePile2) scorePiles
) {
    var result = CustomComparer.@int.Compare(
        scorePiles.scorePile1.Length,
        scorePiles.scorePile2.Length
    );

    var gameWinner = result switch {
        CompareResult.Lesser => Winner.Player2,
        CompareResult.Equals => Winner.Tie,
        CompareResult.Greater => Winner.Player1,
        _ => throw ExhaustiveMatch.Failed(result)
    };
    
    var sb = new StringBuilder();
    sb.AppendLine($"Trump is {trump}");
    for (var index = 0; index < roundsWinners.Count; index++) {
        var (player1, player2, winner) = roundsWinners[index];
        sb.AppendLine(War.RoundEndText(player1, player2, winner, index + 1));
    }

    sb.Append(War.GameWinnerText(gameWinner));
    
    return sb.ToString();
}