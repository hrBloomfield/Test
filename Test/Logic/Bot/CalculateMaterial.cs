namespace Game.Logic.Bot
{
    public class CalculateMaterial
    {
        public static int calculateMaterial(Board board)
        {
            int total = 0;
            for (int i = 0; i < 64; i++)
            {
                int piece = board.gameBoard[i];
                if (piece == Pieces.noPiece || Math.Abs(piece) == Pieces.enPassantMarker)
                    continue;

                int pieceType = Math.Abs(piece);
                if (Kenith.pieceValues.ContainsKey(pieceType))
                    total += Kenith.pieceValues[pieceType];
            }
            
            return total;
        }
    }
}