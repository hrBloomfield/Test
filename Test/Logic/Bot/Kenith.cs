namespace Game.Logic.Bot
{
    public class Kenith
    {
        private const int MAX_DEPTH = 3;
        private const int CHECKMATE_SCORE = 100000;
        private const int STALEMATE_SCORE = 0;
        
        private static readonly Dictionary<int, int> pieceValues = new Dictionary<int, int>
        {
            { Pieces.pawn, 100 },
            { Pieces.knight, 320 },
            { Pieces.bishop, 330 },
            { Pieces.rook, 500 },
            { Pieces.queen, 900 },
            { Pieces.king, 20000 }
        };
        
        private static readonly int[] pawnTable = {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10,-20,-20, 10, 10,  5,
            5, -5,-10,  0,  0,-10, -5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5,  5, 10, 25, 25, 10,  5,  5,
            10, 10, 20, 30, 30, 20, 10, 10,
            50, 50, 50, 50, 50, 50, 50, 50,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        private static readonly int[] knightTable = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        private static readonly int[] bishopTable = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        private static readonly int[] rookTable = {
            0,  0,  0,  5,  5,  0,  0,  0,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            5, 10, 10, 10, 10, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };
        
        private static readonly int[] kingMiddleGameTable = {
            30, 40, 20,  0,  0, 20, 40, 30, 
            20, 20,  0,  0,  0,  0, 20, 20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30
        };


        public static Move.moveInfo FindBestMove(char sideToMove, Board board)
        {
            Move.moveInfo bestMove = null;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            var allMoves = Game.GenerateAllLegalMoves(sideToMove, board);
            if (allMoves.Count == 0) return null;
            
            int searchDepth = GetAdaptiveDepth(board, allMoves.Count);
            

            var orderedMoves = OrderMoves(allMoves, board);
            

            if (board.IsStartingPosition())
            {
                int topN = Math.Min(5, orderedMoves.Count);
                Random rng = new Random();
                bestMove = orderedMoves[rng.Next(topN)];
            }
            else
            {
                foreach (var move in orderedMoves)
                {
                    Board tempBoard = board.Clone();
                    ApplyMove.applyMove(tempBoard, move);

                    char opponentSide = sideToMove == 'w' ? 'b' : 'w';
                    int score = -Minimax(tempBoard, searchDepth - 1, -beta, -alpha, opponentSide);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }

                    alpha = Math.Max(alpha, score);
                }
            }

            return bestMove;
        }
        

        private static int GetAdaptiveDepth(Board board, int moveCount)
        {
            int material = CalculateMaterial(board);
            int depth = MAX_DEPTH;
            

            if (material <= 1500) 
                depth += 2;
            else if (material <= 2500) 
                depth += 1;
            

            if (moveCount <= 10) 
                depth += 2;
            
            return Math.Min(depth, MAX_DEPTH + 2);
        }
        

        private static List<Move.moveInfo> OrderMoves(List<Move.moveInfo> moves, Board board)
        {
            return moves.OrderByDescending(m => {
                int score = 0;
                
                if (m.moveType == Move.MoveType.Castle)
                    score += 700;
                
                if (m.moveType == Move.MoveType.Capture || m.moveType == Move.MoveType.PromotionCapture)
                {
                    int capturedPiece = Math.Abs(board.gameBoard[m.to]);
                    int attackingPiece = Math.Abs(board.gameBoard[m.from]);
                    
                    int victimValue = pieceValues.ContainsKey(capturedPiece) ? pieceValues[capturedPiece] : 0;
                    int attackerValue = pieceValues.ContainsKey(attackingPiece) ? pieceValues[attackingPiece] : 0;
                    
                    score += victimValue * 10 - attackerValue;
                }
                
                if (m.moveType == Move.MoveType.Promotion || m.moveType == Move.MoveType.PromotionCapture)
                    score += 800;
                
                if (m.to == 27 || m.to == 28 || m.to == 35 || m.to == 36)
                    score += 20;

                return score;
            }).ToList();
        }
        
        private static int CalculateMaterial(Board board)
        {
            int total = 0;
            for (int i = 0; i < 64; i++)
            {
                int piece = board.gameBoard[i];
                if (piece == Pieces.noPiece || Math.Abs(piece) == Pieces.enPassantMarker) 
                    continue;

                int pieceType = Math.Abs(piece);
                if (pieceValues.ContainsKey(pieceType))
                    total += pieceValues[pieceType];
            }
            return total;
        }
        
        private static int Minimax(Board board, int depth, int alpha, int beta, char sideToMove)
        {
            string gameState = Game.CheckGameState(sideToMove, board);
            if (gameState != "null")
            {
                if (gameState.Contains("wins")) 
                    return -CHECKMATE_SCORE + (MAX_DEPTH - depth);
                return STALEMATE_SCORE;
            }
            
            if (depth == 0) 
                return Quiescence(board, alpha, beta, sideToMove);

            var allMoves = Game.GenerateAllLegalMoves(sideToMove, board);
            var orderedMoves = OrderMoves(allMoves, board);

            int maxScore = int.MinValue;

            foreach (var move in orderedMoves)
            {
                Board tempBoard = board.Clone();
                ApplyMove.applyMove(tempBoard, move);

                char opponentSide = sideToMove == 'w' ? 'b' : 'w';
                int score = -Minimax(tempBoard, depth - 1, -beta, -alpha, opponentSide);

                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, score);
                
                if (alpha >= beta)
                    break;
            }

            return maxScore;
        }
        
        private static int EvaluatePosition(Board board, char sideToMove)
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
                int pieceValue = pieceValues.ContainsKey(pieceType) ? pieceValues[pieceType] : 0;
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
        
        private static int EvaluateCastling(Board board)
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
        
        private static int EvaluateDevelopment(Board board)
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
        
        private static int EvaluateKingSafety(Board board, char side)
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
        
        private static int EvaluatePawnStructure(Board board)
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
        
        private static int GetPositionBonus(int pieceType, int position, bool isWhite)
        {
            int index = isWhite ? position : (63 - position);

            switch (pieceType)
            {
                case Pieces.pawn:
                    return pawnTable[index];
                case Pieces.knight:
                    return knightTable[index];
                case Pieces.bishop:
                    return bishopTable[index];
                case Pieces.rook:
                    return rookTable[index];
                case Pieces.king:
                    return kingMiddleGameTable[index];
                default:
                    return 0;
            }
        }
        
        private static int Quiescence(Board board, int alpha, int beta, char sideToMove)
        {
            int standPat = EvaluatePosition(board, sideToMove);

            if (standPat >= beta) 
                return beta;
            
            if (standPat > alpha) 
                alpha = standPat;
            
            var captures = Game.GenerateAllLegalMoves(sideToMove, board)
                .Where(m => m.moveType == Move.MoveType.Capture || m.moveType == Move.MoveType.PromotionCapture)
                .OrderByDescending(m => {
                    int capturedPiece = Math.Abs(board.gameBoard[m.to]);
                    return pieceValues.ContainsKey(capturedPiece) ? pieceValues[capturedPiece] : 0;
                });

            foreach (var move in captures)
            {
                Board tempBoard = board.Clone();
                ApplyMove.applyMove(tempBoard, move);
                char opponent = sideToMove == 'w' ? 'b' : 'w';
                int score = -Quiescence(tempBoard, -beta, -alpha, opponent);

                if (score >= beta) 
                    return beta;
                
                if (score > alpha) 
                    alpha = score;
            }

            return alpha;
        }
    }
}