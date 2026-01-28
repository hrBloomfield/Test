namespace Game.Logic.Bot
{
    public class ApplyMove
    {
        public static void applyMove(Board board, Move.moveInfo move)
        {
            int movingPiece = board.gameBoard[move.from];
            
            for (int i = 0; i < 64; i++)
            {
                if (Math.Abs(board.gameBoard[i]) == Pieces.enPassantMarker)
                    board.gameBoard[i] = Pieces.noPiece;
            }
            
            if (move.moveType == Move.MoveType.Castle)
            {
                bool kingSide = move.to > move.from;
                board.gameBoard[move.to] = movingPiece;
                board.gameBoard[move.from] = Pieces.noPiece;

                if (kingSide)
                {
                    int rookFrom = move.from + 3;
                    int rookTo = move.from + 1;
                    board.gameBoard[rookTo] = board.gameBoard[rookFrom];
                    board.gameBoard[rookFrom] = Pieces.noPiece;
                }
                else
                {
                    int rookFrom = move.from - 4;
                    int rookTo = move.from - 1;
                    board.gameBoard[rookTo] = board.gameBoard[rookFrom];
                    board.gameBoard[rookFrom] = Pieces.noPiece;
                }
                return;
            }
            
            if (move.moveType == Move.MoveType.EnPassant)
            {
                int capturedPawnSquare = PieceHelpers.IsWhite(movingPiece) ? move.to - 8 : move.to + 8;
                board.gameBoard[capturedPawnSquare] = Pieces.noPiece;
            }
            
            if (move.moveType == Move.MoveType.Promotion || move.moveType == Move.MoveType.PromotionCapture)
            {
                int colour = PieceHelpers.IsWhite(movingPiece) ? Pieces.white : Pieces.black;
                board.gameBoard[move.to] = Pieces.queen * colour;
                board.gameBoard[move.from] = Pieces.noPiece;
                return;
            }
            
            board.gameBoard[move.to] = movingPiece;
            board.gameBoard[move.from] = Pieces.noPiece;
            
            if (move.moveType == Move.MoveType.DoubleMove)
            {
                int enPassantSquare = PieceHelpers.IsWhite(movingPiece) ? move.to - 8 : move.to + 8;
                board.gameBoard[enPassantSquare] = PieceHelpers.IsWhite(movingPiece) ? 
                    Pieces.enPassantMarker : Pieces.black * Pieces.enPassantMarker;
            }
        }
    }
}