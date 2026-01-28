namespace Game.Logic
{
    public class Board
    {
        public int[] gameBoard;

        public Board()
        {
            gameBoard = new int[64];
        }

        public Board Clone()
        {
            var copy = new Board();
            copy.gameBoard = (int[])this.gameBoard.Clone();
            return copy;
        }

        public void PrintBoard(char userSide)
        {
            int startRow = userSide == 'w' ? 7 : 0;
            int step = userSide == 'w' ? -1 : 1;

            for (int i = 0; i < 8; i++)
            {
                int row = startRow + i * step;
                int rankLabel = row + 1;
                
                Console.Write($"{rankLabel} ");

                for (int col = 0; col < 8; col++)
                {
                    int index = row * 8 + col;
                    Console.Write($" {UnicodePieces.ToChar(gameBoard[index])} ");
                }
                
                Console.WriteLine();
            }
            
            Console.WriteLine("   a  b  c  d  e  f  g  h");
        }

        public bool IsStartingPosition()
        {
            int[] startingBoard =
            {
                Pieces.rook, Pieces.knight, Pieces.bishop,
                Pieces.queen, Pieces.king, Pieces.bishop,
                Pieces.knight, Pieces.rook,
                Pieces.pawn, Pieces.pawn, Pieces.pawn,
                Pieces.pawn, Pieces.pawn, Pieces.pawn,
                Pieces.pawn, Pieces.pawn,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                Pieces.black * Pieces.pawn, Pieces.black * Pieces.pawn,
                Pieces.black * Pieces.pawn, Pieces.black * Pieces.pawn,
                Pieces.black * Pieces.pawn, Pieces.black * Pieces.pawn,
                Pieces.black * Pieces.pawn, Pieces.black * Pieces.pawn,
                Pieces.black * Pieces.rook, Pieces.black * Pieces.knight,
                Pieces.black * Pieces.bishop, Pieces.black * Pieces.queen,
                Pieces.black * Pieces.king, Pieces.black * Pieces.bishop,
                Pieces.black * Pieces.knight, Pieces.black * Pieces.rook
            };

            for (int i = 0; i < 64; i++)
            {
                if (gameBoard[i] != startingBoard[i])
                    return false;
            }

            return true;
        }
    }
}
