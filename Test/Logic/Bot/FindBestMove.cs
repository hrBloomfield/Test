namespace Game.Logic.Bot
{
    public class FindBestMove
    {
        public static Move.moveInfo findBestMove(char sideToMove, Board board)
        {
            Move.moveInfo bestMove = null;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            var allMoves = Game.GenerateAllLegalMoves(sideToMove, board);
            if (allMoves.Count == 0) return null;

            int searchDepth = GetAdaptiveDepth.getAdaptiveDepth(board, allMoves.Count);


            var orderedMoves = OrderMoves.orderMoves(allMoves, board);


            if (board.IsStartingPosition())
            {
                int topN = Math.Min(5, orderedMoves.Count);
                Random rng = new Random();
                bestMove = orderedMoves[rng.Next(topN)];
            }
            else
            {
                foreach (var move in orderedMoves)
                {
                    Board tempBoard = board.Clone();
                    ApplyMove.applyMove(tempBoard, move);

                    char opponentSide = sideToMove == 'w' ? 'b' : 'w';
                    int score = -Minimax.minimax(tempBoard, searchDepth - 1, -beta, -alpha, opponentSide);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }

                    alpha = Math.Max(alpha, score);
                }
            }

            return bestMove;
        }
    }
}