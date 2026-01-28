namespace Game.Logic
{
    public static class Pieces
    {
        public const int noPiece = 0;
        public const int pawn = 1;
        public const int knight = 2;
        public const int bishop = 3;
        public const int queen = 4;
        public const int rook = 5;
        public const int king = 6;
        
        public const int enPassantMarker = 10;
        
        public const int white = 1;
        public const int black = -1;
    }
    
    public static class PieceHelpers
    {
        public static bool IsWhite(int piece)
        {
            return piece > 0;
        }

        public static bool IsBlack(int piece)
        {
            return piece < 0;
        }
    }
}