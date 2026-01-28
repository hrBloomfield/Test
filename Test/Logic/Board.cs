namespace Game.Logic
{
    public class Board
    {
        public ulong Hash;
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
            if (userSide == 'w')
            {
                for (int row = 7; row >= 0; row--)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        int index = row * 8 + col;
                        Console.Write($"{gameBoard[index],3}");
                    }

                    Console.WriteLine();
                }
            }
            else if (userSide == 'b')
            {
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        int index = row * 8 + col;
                        Console.Write($"{gameBoard[index],3}");
                    }

                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Invalid userSide");
                MainGame.Main();
            }
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