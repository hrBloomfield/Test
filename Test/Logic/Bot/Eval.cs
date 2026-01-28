namespace Game.Logic.Bot
{
    public class Eval
    {
        public static int EvaluatePosition(Board board, char sideToMove)
        {
            int score = 0;
            int whiteMaterial = 0;
            int blackMaterial = 0;
            int whitePieceCount = 0;
            int blackPieceCount = 0;

            for (int i = 0; i < 64; i++)
            {
                int piece = board.gameBoard[i];
                if (piece == Pieces.noPiece || Math.Abs(piece) == Pieces.enPassantMarker)
                    continue;

                int pieceType = Math.Abs(piece);
                int pieceValue = Kenith.pieceValues.ContainsKey(pieceType) ? Kenith.pieceValues[pieceType] : 0;
                int positionBonus = GetPositionBonus(pieceType, i, piece > 0);
                int totalValue = pieceValue + positionBonus;

                if (piece > 0)
                {
                    score += totalValue;
                    whiteMaterial += pieceValue;
                    whitePieceCount++;
                }
                else
                {
                    score -= totalValue;
                    blackMaterial += pieceValue;
                    blackPieceCount++;
                }
            }


            int totalPieces = whitePieceCount + blackPieceCount;
            if (totalPieces > 24)
                score += EvaluateDevelopment(board);

            score += EvaluateKingSafety(board, 'w') - EvaluateKingSafety(board, 'b');

            score += EvaluatePawnStructure(board);

            score += EvaluateCastling(board);

            return sideToMove == 'w' ? score : -score;
        }

        public static int EvaluateCastling(Board board)
        {
            int score = 0;


            if (board.gameBoard[6] == Pieces.king)
                score += 70;

            else if (board.gameBoard[2] == Pieces.king)
                score += 60;

            if (board.gameBoard[62] == -Pieces.king)
                score -= 70;

            else if (board.gameBoard[58] == -Pieces.king)
                score -= 60;

            return score;
        }

        public static int EvaluateDevelopment(Board board)
        {
            int score = 0;

            if (board.gameBoard[1] == Pieces.knight) score -= 20;
            if (board.gameBoard[6] == Pieces.knight) score -= 20;
            if (board.gameBoard[57] == -Pieces.knight) score += 20;
            if (board.gameBoard[62] == -Pieces.knight) score += 20;

            if (board.gameBoard[2] == Pieces.bishop) score -= 15;
            if (board.gameBoard[5] == Pieces.bishop) score -= 15;
            if (board.gameBoard[58] == -Pieces.bishop) score += 15;
            if (board.gameBoard[61] == -Pieces.bishop) score += 15;

            return score;
        }

        public static int EvaluateKingSafety(Board board, char side)
        {
            int kingPiece = (side == 'w') ? Pieces.king : -Pieces.king;
            int kingPos = Array.IndexOf(board.gameBoard, kingPiece);

            if (kingPos == -1) return -10000;

            int safety = 0;
            int[] adjacentSquares = { -9, -8, -7, -1, 1, 7, 8, 9 };

            foreach (int offset in adjacentSquares)
            {
                int pos = kingPos + offset;
                if (pos >= 0 && pos < 64)
                {
                    int piece = board.gameBoard[pos];
                    if (side == 'w' && piece > 0 && piece != Pieces.enPassantMarker)
                        safety += 5;
                    else if (side == 'b' && piece < 0 && piece != -Pieces.enPassantMarker)
                        safety += 5;
                }
            }

            return safety;
        }

        public static int EvaluatePawnStructure(Board board)
        {
            int score = 0;

            for (int file = 0; file < 8; file++)
            {
                int whitePawns = 0;
                int blackPawns = 0;

                for (int rank = 0; rank < 8; rank++)
                {
                    int pos = rank * 8 + file;
                    if (board.gameBoard[pos] == Pieces.pawn) whitePawns++;
                    if (board.gameBoard[pos] == -Pieces.pawn) blackPawns++;
                }

                if (whitePawns > 1) score -= (whitePawns - 1) * 15;
                if (blackPawns > 1) score += (blackPawns - 1) * 15;
            }

            return score;
        }

        public static int GetPositionBonus(int pieceType, int position, bool isWhite)
        {
            int index = isWhite ? position : (63 - position);

            switch (pieceType)
            {
                case Pieces.pawn:
                    return Kenith.pawnTable[index];
                case Pieces.knight:
                    return Kenith.knightTable[index];
                case Pieces.bishop:
                    return Kenith.bishopTable[index];
                case Pieces.rook:
                    return Kenith.rookTable[index];
                case Pieces.king:
                    return Kenith.kingMiddleGameTable[index];
                default:
                    return 0;
            }
        }
    }
}