namespace Game.Logic
{
    public static class MovePieces
    {
        public static List<Move.moveInfo> GetLegalMoves(int[] board, int position)
        {
            int piece = board[position];
            bool isWhite = piece > 0;
            List<Move.moveInfo> legalMoves = new();

            switch (Math.Abs(piece))
            {
                case Pieces.pawn:
                    legalMoves = new Pawn(isWhite).GenerateLegalMoves(board, position);
                    break;
                case Pieces.rook:
                    legalMoves = new Rook(isWhite).GenerateLegalMoves(board, position);
                    break;
                case Pieces.knight:
                    legalMoves = new Knight(isWhite).GenerateLegalMoves(board, position);
                    break;
                case Pieces.bishop:
                    legalMoves = new Bishop(isWhite).GenerateLegalMoves(board, position);
                    break;
                case Pieces.queen:
                    legalMoves = new Queen(isWhite).GenerateLegalMoves(board, position);
                    break;
                case Pieces.king:
                    legalMoves = new King(isWhite).GenerateLegalMoves(board, position);
                    break;
            }

            return legalMoves;

        }
    }
}