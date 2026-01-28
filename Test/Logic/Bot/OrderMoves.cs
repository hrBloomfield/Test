namespace Game.Logic.Bot
{
    public class OrderMoves
    {
        public static List<Move.moveInfo> orderMoves(List<Move.moveInfo> moves, Board board)
        {
            return moves.OrderByDescending(m =>
            {
                int score = 0;

                if (m.moveType == Move.MoveType.Castle)
                    score += 700;

                if (m.moveType == Move.MoveType.Capture || m.moveType == Move.MoveType.PromotionCapture)
                {
                    int capturedPiece = Math.Abs(board.gameBoard[m.to]);
                    int attackingPiece = Math.Abs(board.gameBoard[m.from]);

                    int victimValue = Kenith.pieceValues.ContainsKey(capturedPiece)
                        ? Kenith.pieceValues[capturedPiece]
                        : 0;
                    int attackerValue = Kenith.pieceValues.ContainsKey(attackingPiece)
                        ? Kenith.pieceValues[attackingPiece]
                        : 0;

                    score += victimValue * 10 - attackerValue;
                }

                if (m.moveType == Move.MoveType.Promotion || m.moveType == Move.MoveType.PromotionCapture)
                    score += 800;

                if (m.to == 27 || m.to == 28 || m.to == 35 || m.to == 36)
                    score += 20;

                return score;
            }).ToList();
        }
    }
}