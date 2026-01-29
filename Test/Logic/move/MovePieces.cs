namespace Game.Logic
{
    public static class MovePieces
    {
        public static List<Move.moveInfo> GetLegalMoves(int[] board, int position)
        {
            int piece = board[position];
            bool isWhite = piece > 0;
            List<Move.moveInfo> legalMoves = new();

            legalMoves = Math.Abs(piece) switch
            {
                Pieces.pawn   => new Pawn(isWhite).GenerateLegalMoves(board, position),
                Pieces.rook   => new Rook(isWhite).GenerateLegalMoves(board, position),
                Pieces.knight => new Knight(isWhite).GenerateLegalMoves(board, position),
                Pieces.bishop => new Bishop(isWhite).GenerateLegalMoves(board, position),
                Pieces.queen  => new Queen(isWhite).GenerateLegalMoves(board, position),
                Pieces.king   => new King(isWhite).GenerateLegalMoves(board, position)
            };

            return legalMoves;

        }
    }
}