namespace Game.Logic
{
    public class Game
    {
        // Track game history for repetition detection
        private static List<string> positionHistory = new List<string>();
        private static int halfMoveClock = 0; // Moves since last pawn move or capture

        public static void ResetGameHistory()
        {
            positionHistory.Clear();
            halfMoveClock = 0;
        }

        public static void RecordMove(Board board, Move.moveInfo move, bool isPawnMove, bool isCapture)
        {
            // Reset half-move clock on pawn move or capture
            if (isPawnMove || isCapture)
            {
                halfMoveClock = 0;
            }
            else
            {
                halfMoveClock++;
            }

            // Record position for repetition detection
            string position = BoardToString(board);
            positionHistory.Add(position);
        }

        private static string BoardToString(Board board)
        {
            // Create a string representation of the board position
            return string.Join(",", board.gameBoard);
        }

        public static string CheckGameState(char sideToMove, Board board)
        {
            string winner = "null";

            bool kingInCheck = IsKingInCheck(sideToMove, board);
            var moves = GenerateAllLegalMoves(sideToMove, board);

            // Checkmate or Stalemate
            if (moves.Count == 0)
            {
                if (kingInCheck)
                {
                    // Checkmate - opponent wins
                    winner = sideToMove == 'w' ? "black wins by checkmate" : "white wins by checkmate";
                }
                else
                {
                    // Stalemate - no legal moves but not in check
                    winner = "draw by stalemate";
                }
            }
            // Fifty-move rule
            else if (CheckFiftyMoveRule())
            {
                winner = "draw by fifty move rule";
            }
            // Threefold repetition
            else if (CheckThreeFoldRepetition())
            {
                winner = "draw by threefold repetition";
            }
            // Insufficient material
            else if (CheckInsufficientMaterial(board))
            {
                winner = "draw by insufficient material";
            }

            return winner;
        }

        public static List<Move.moveInfo> GenerateAllLegalMoves(char sideToMove, Board board)
        {
            var allMoves = new List<Move.moveInfo>();

            for (int i = 0; i < board.gameBoard.Length; i++)
            {
                int piece = board.gameBoard[i];
                if (piece == Pieces.noPiece) continue;

                bool isWhitePiece = piece > 0;
                if ((sideToMove == 'w' && !isWhitePiece) || (sideToMove == 'b' && isWhitePiece))
                    continue;
                
                List<Move.moveInfo> legalMoves = GetLegalMovesForPiece(sideToMove, board, i);
                allMoves.AddRange(legalMoves);
            }

            return allMoves;
        }

        public static bool IsKingInCheck(char sideToMove, Board board)
        {
            int kingPiece = (sideToMove == 'w') ? Pieces.white * Pieces.king : Pieces.black * Pieces.king;
            int kingPosition = Array.IndexOf(board.gameBoard, kingPiece);

            if (kingPosition == -1) return true; // king missing

            char opponentSide = (sideToMove == 'w') ? 'b' : 'w';

            // Scan all opponent pieces
            for (int i = 0; i < board.gameBoard.Length; i++)
            {
                int piece = board.gameBoard[i];
                if (piece == Pieces.noPiece) continue;

                bool isWhitePiece = piece > 0;
                if ((opponentSide == 'w' && !isWhitePiece) || (opponentSide == 'b' && isWhitePiece))
                    continue;

                var pseudoMoves = MovePieces.GetLegalMoves(board.gameBoard, i);
                foreach (var move in pseudoMoves)
                {
                    if (move.to == kingPosition)
                        return true;
                }
            }

            return false;
        }

        public static bool WillKingBeInCheck(char sideToMove, Board board, Move.moveInfo move)
        {
            Board tempBoard = board.Clone();
            tempBoard.gameBoard[move.to] = tempBoard.gameBoard[move.from];
            tempBoard.gameBoard[move.from] = Pieces.noPiece;

            return IsKingInCheck(sideToMove, tempBoard);
        }

        public static List<Move.moveInfo> GetLegalMovesForPiece(char sideToMove, Board board, int pieceIndex)
        {
            List<Move.moveInfo> moves = MovePieces.GetLegalMoves(board.gameBoard, pieceIndex);
            List<Move.moveInfo> legalMoves = new List<Move.moveInfo>();

            foreach (var move in moves)
            {
                if (!WillKingBeInCheck(sideToMove, board, move))
                    legalMoves.Add(move);
            }
            
            return legalMoves;
        }
        
        public static bool CheckFiftyMoveRule()
        {
            // The fifty-move rule: If no pawn has moved and no capture has been made 
            // in the last 50 moves by each player (100 half-moves), it's a draw
            return halfMoveClock >= 100;
        }

        public static bool CheckThreeFoldRepetition()
        {
            if (positionHistory.Count < 3) return false;

            // Get the current position (last in history)
            string currentPosition = positionHistory[positionHistory.Count - 1];
            int repetitionCount = 0;

            // Count how many times this position has appeared
            foreach (string position in positionHistory)
            {
                if (position == currentPosition)
                {
                    repetitionCount++;
                    if (repetitionCount >= 3)
                        return true;
                }
            }

            return false;
        }

        public static bool CheckInsufficientMaterial(Board board)
        {
            // Count pieces
            int whiteKnights = 0, blackKnights = 0;
            int whiteBishops = 0, blackBishops = 0;
            int whitePawns = 0, blackPawns = 0;
            int whiteRooks = 0, blackRooks = 0;
            int whiteQueens = 0, blackQueens = 0;

            for (int i = 0; i < 64; i++)
            {
                int piece = board.gameBoard[i];
                int pieceType = Math.Abs(piece);

                if (piece > 0) // White
                {
                    switch (pieceType)
                    {
                        case Pieces.pawn: whitePawns++; break;
                        case Pieces.knight: whiteKnights++; break;
                        case Pieces.bishop: whiteBishops++; break;
                        case Pieces.rook: whiteRooks++; break;
                        case Pieces.queen: whiteQueens++; break;
                    }
                }
                else if (piece < 0) // Black
                {
                    switch (pieceType)
                    {
                        case Pieces.pawn: blackPawns++; break;
                        case Pieces.knight: blackKnights++; break;
                        case Pieces.bishop: blackBishops++; break;
                        case Pieces.rook: blackRooks++; break;
                        case Pieces.queen: blackQueens++; break;
                    }
                }
            }

            // If there are pawns, rooks, or queens, there's sufficient material
            if (whitePawns > 0 || blackPawns > 0 || 
                whiteRooks > 0 || blackRooks > 0 || 
                whiteQueens > 0 || blackQueens > 0)
            {
                return false;
            }

            // King vs King
            if (whiteKnights == 0 && whiteBishops == 0 && 
                blackKnights == 0 && blackBishops == 0)
            {
                return true;
            }

            // King and Knight vs King
            if ((whiteKnights == 1 && whiteBishops == 0 && blackKnights == 0 && blackBishops == 0) ||
                (blackKnights == 1 && blackBishops == 0 && whiteKnights == 0 && whiteBishops == 0))
            {
                return true;
            }

            // King and Bishop vs King
            if ((whiteBishops == 1 && whiteKnights == 0 && blackKnights == 0 && blackBishops == 0) ||
                (blackBishops == 1 && blackKnights == 0 && whiteKnights == 0 && whiteBishops == 0))
            {
                return true;
            }

            // King and Bishop vs King and Bishop (same color bishops)
            // This is complex to check properly, so we'll skip it for now

            return false;
        }
    }
}