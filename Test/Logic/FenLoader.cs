namespace Game.Logic
{
    public static class FenLoader
    {
        public static void ReadFenAndLoad(string fen, Board board)
        {
            var pieceTypeForFen = new Dictionary<char, int>()
            {
                ['k'] = Pieces.king, ['q'] = Pieces.queen, ['b'] = Pieces.bishop,
                ['r'] = Pieces.rook, ['n'] = Pieces.knight, ['p'] = Pieces.pawn
            };

            string fenBoard = fen.Split(' ')[0];

            int file = 0;
            int rank = 7;

            foreach (char currentFenDigit in fenBoard)
            {

                if (currentFenDigit == '/')
                {
                    file = Pieces.noPiece;
                    rank--;
                }
                else if (char.IsDigit(currentFenDigit))
                {
                    file += (int)char.GetNumericValue(currentFenDigit);
                }
                else
                {

                    int pieceColour;
                    int piece;
                    pieceColour = char.IsUpper(currentFenDigit) ? Pieces.white : Pieces.black;
                    piece = pieceTypeForFen[char.ToLower(currentFenDigit)];
                    
                    int squareIndex = rank * 8 + file;
                    board.gameBoard[squareIndex] = piece * pieceColour;

                    file++;
                    
                }
            }
        }
    }
}