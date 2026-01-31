namespace Game.Logic.Bot
{
    public class Quiescence
    {
        public static int quiescence(Board board, int alpha, int beta, char sideToMove)
        {
            int baselineEval = Eval.EvaluatePosition(board, sideToMove);

            if (baselineEval >= beta) 
                return beta;
            
            if (baselineEval > alpha) 
                alpha = baselineEval;
            
            var captures = Game.GenerateAllLegalMoves(sideToMove, board)
                .Where(m => m.moveType == Move.MoveType.Capture || m.moveType == Move.MoveType.PromotionCapture)
                .OrderByDescending(m => {
                    int capturedPiece = Math.Abs(board.gameBoard[m.to]);
                    return Kenith.pieceValues.ContainsKey(capturedPiece) ? Kenith.pieceValues[capturedPiece] : 0;
                });

            foreach (var move in captures)
            {
                Board tempBoard = board.Clone();
                ApplyMove.applyMove(tempBoard, move);
                char opponent = sideToMove == 'w' ? 'b' : 'w';
                int score = -quiescence(tempBoard, -beta, -alpha, opponent);

                if (score >= beta) 
                    return beta;
                
                if (score > alpha) 
                    alpha = score;
            }

            return alpha;
        }
    }
}