namespace Game.Logic
{
    public class Move
    {
        protected const int moveUp = 8;
        protected const int moveDown = -8;
        protected const int moveRight = 1;
        protected const int moveLeft = -1;
        protected const int moveUpRight = 9;
        protected const int moveUpLeft = 7;
        protected const int moveDownRight = -7;
        protected const int moveDownLeft = -9;

        public enum MoveType 
        {
            Normal, 
            Capture, 
            Castle, 
            EnPassant, 
            Promotion, 
            DoubleMove,
            PromotionCapture 
        }

        public class moveInfo
        {
            public int from;
            public int to;
            public MoveType moveType;

            public moveInfo(int from, int to, MoveType moveType)
            {
                this.from = from;
                this.to = to;
                this.moveType = moveType;
            }
        }
        
        protected bool IsOpponent(int piece, bool isWhite) => isWhite ? piece < 0 : piece > 0;

        protected List<moveInfo> SlideMoves(int[] board, int currentPos, int[] directions, bool isWhite)
        {
            var moves = new List<moveInfo>();

            foreach (var dir in directions)
            {
                int pos = currentPos;
                int next = pos + dir;

                while (next >= 0 && next < 64 &&
                       Math.Abs((next % 8) - (pos % 8)) <= 1 && Math.Abs((next / 8) - (pos / 8)) <= 1)
                {
                    int piece = board[next];
                    if (piece == Pieces.noPiece)
                    {
                        moves.Add(new moveInfo(currentPos, next, MoveType.Normal));
                    }
                    else
                    {
                        if (IsOpponent(piece, isWhite))
                            moves.Add(new moveInfo(currentPos, next, MoveType.Capture));
                        break;
                    }
                    pos = next;
                    next += dir;
                }
            }

            return moves;
        }

    }
}