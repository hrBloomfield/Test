using Game.Logic.Bot;

namespace Game.Logic
{
    public class MakingMoves : MainGame
    {
        public static char sideToMove = 'w';
        private static int enPassantSquare = -1;
        private static bool whiteKingMoved = false;
        private static bool blackKingMoved = false;
        private static bool whiteKingsideRookMoved = false;
        private static bool whiteQueensideRookMoved = false;
        private static bool blackKingsideRookMoved = false;
        private static bool blackQueensideRookMoved = false;
        
        // Timer support
        private static Timer gameTimer = null;
        private static bool useTimer = false;

        public static void ResetGameState()
        {
            sideToMove = 'w';
            enPassantSquare = -1;
            whiteKingMoved = false;
            blackKingMoved = false;
            whiteKingsideRookMoved = false;
            whiteQueensideRookMoved = false;
            blackKingsideRookMoved = false;
            blackQueensideRookMoved = false;
            Game.ResetGameHistory();
        }

        public static void InitializeTimer(bool enableTimer, int timePerSideInSeconds = 600)
        {
            useTimer = enableTimer;
            if (useTimer)
            {
                gameTimer = new Timer(timePerSideInSeconds);
                gameTimer.OnTimeExpired += OnTimerExpired;
                Console.WriteLine($"Timer enabled: {timePerSideInSeconds / 60} minutes per side");
            }
        }

        private static void OnTimerExpired(char side)
        {
            Console.Clear();
            Console.WriteLine($"\n{(side == 'w' ? "White" : "Black")}'s time has expired!");
            Console.WriteLine($"{(side == 'w' ? "Black" : "White")} wins on time!");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void HandleMoves(Board board)
        {
            if (useTimer && gameTimer != null)
            {
                gameTimer.DisplayTimers();
                Console.WriteLine();
            }

            Console.WriteLine($"It is {(sideToMove == 'w' ? "White" : "Black")}'s turn.");

            string result = Game.CheckGameState(sideToMove, board);
            if (result != "null")
            {
                if (useTimer && gameTimer != null)
                    gameTimer.Stop();
                    
                Console.WriteLine($"Game over: {result}");
                Console.ReadKey();
                return;
            }

            if (useTimer && gameTimer != null)
            {
                gameTimer.Start(sideToMove);
            }

            bool isBotTurn = false;
            if (MainGame.userGameMode == "1")
                isBotTurn = (userSide == 'w' && sideToMove == 'b') || (userSide == 'b' && sideToMove == 'w');
            else if (userGameMode == "3")
                isBotTurn = true;

            if (isBotTurn)
            {
                Move.moveInfo botMove = FindBestMove.findBestMove(sideToMove, board);
                if (botMove != null)
                {
                    Console.WriteLine($"Kenith moves from {botMove.from} to {botMove.to}");

                    if (botMove.moveType == Move.MoveType.Promotion ||
                        botMove.moveType == Move.MoveType.PromotionCapture)
                    {
                        ExecuteMove(board, botMove);
                        int colour = sideToMove == 'w' ? Pieces.white : Pieces.black;
                        board.gameBoard[botMove.to] = Pieces.queen * colour;
                    }
                    else
                    {
                        ExecuteMove(board, botMove);
                    }

                    if (useTimer && gameTimer != null)
                    {
                        gameTimer.Stop();
                    }

                    sideToMove = sideToMove == 'w' ? 'b' : 'w';
                    
                    if (userGameMode == "3")
                    {
                        Thread.Sleep(500);
                    }
                }

                return;
            }

            // Human player's turn
            Console.WriteLine("Piece Coordinate to move: ");
            
            Console.Write("Piece to move: ");
            string fromInput = Console.ReadLine();

            if (!Sq.TryParse(fromInput, out int userPieceSelection))
            {
                Console.WriteLine("Invalid square.");
                Console.ReadKey();
                return;
            }
            int usersPiece = board.gameBoard[userPieceSelection];
            if (usersPiece == Pieces.noPiece)
            {
                Console.WriteLine("No piece on that square.");
                Console.ReadKey();
                return;
            }

            if ((sideToMove == 'w' && !PieceHelpers.IsWhite(usersPiece)) ||
                (sideToMove == 'b' && !PieceHelpers.IsBlack(usersPiece)))
            {
                Console.WriteLine("Wrong colour. Try again.");
                Console.ReadKey();
                return;
            }

            List<Move.moveInfo> moves = Game.GetLegalMovesForPiece(sideToMove, board, userPieceSelection);

            Console.WriteLine("Legal moves:");
            if (moves.Count > 0)
            {
                foreach (var move in moves)
                {
                    Console.WriteLine($"{Sq.ToAlgebraic(move.from)} -> {Sq.ToAlgebraic(move.to)} ({move.moveType})");
                }

                Console.WriteLine("Move to where?");
                Console.Write("Move to: ");
                string toInput = Console.ReadLine();

                if (!Sq.TryParse(toInput, out int toIndex))
                {
                    Console.WriteLine("Invalid destination.");
                    Console.ReadKey();
                    return;
                }

                Move.moveInfo selectedMove = moves.Find(m => m.to == toIndex);

                if (selectedMove != null)
                {
                    if (selectedMove.moveType == Move.MoveType.Promotion ||
                        selectedMove.moveType == Move.MoveType.PromotionCapture)
                        PromotePawn(board, selectedMove);
                    else
                        ExecuteMove(board, selectedMove);

                    if (useTimer && gameTimer != null)
                    {
                        gameTimer.Stop();
                    }

                    sideToMove = sideToMove == 'w' ? 'b' : 'w';
                    board.PrintBoard(userSide);
                }
                else
                {
                    Console.WriteLine("Illegal move");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("No legal moves.");
                Console.ReadKey();
                return;
            }
        }

        public static void ExecuteMove(Board board, Move.moveInfo move)
        {
            int movingPiece = board.gameBoard[move.from];
            int capturedPiece = board.gameBoard[move.to];
            bool isPawnMove = Math.Abs(movingPiece) == Pieces.pawn;
            bool isCapture = capturedPiece != Pieces.noPiece || move.moveType == Move.MoveType.EnPassant;

            // Track king and rook moves for castling rights
            if (Math.Abs(movingPiece) == Pieces.king)
            {
                if (PieceHelpers.IsWhite(movingPiece))
                    whiteKingMoved = true;
                else
                    blackKingMoved = true;
            }
            else if (Math.Abs(movingPiece) == Pieces.rook)
            {
                // White rooks
                if (move.from == 0) whiteQueensideRookMoved = true;
                else if (move.from == 7) whiteKingsideRookMoved = true;
                // Black rooks
                else if (move.from == 56) blackQueensideRookMoved = true;
                else if (move.from == 63) blackKingsideRookMoved = true;
            }

            // Clear old en passant marker
            if (enPassantSquare != -1)
            {
                board.gameBoard[enPassantSquare] = Pieces.noPiece;
                enPassantSquare = -1;
            }

            // Handle castling
            if (move.moveType == Move.MoveType.Castle)
            {
                bool kingSide = move.to > move.from;

                board.gameBoard[move.to] = movingPiece;
                board.gameBoard[move.from] = Pieces.noPiece;

                if (kingSide)
                {
                    int rookFrom = move.from + 3;
                    int rookTo = move.from + 1;

                    board.gameBoard[rookTo] = board.gameBoard[rookFrom];
                    board.gameBoard[rookFrom] = Pieces.noPiece;
                }
                else
                {
                    int rookFrom = move.from - 4;
                    int rookTo = move.from - 1;

                    board.gameBoard[rookTo] = board.gameBoard[rookFrom];
                    board.gameBoard[rookFrom] = Pieces.noPiece;
                }

                Game.RecordMove(board, move, isPawnMove, isCapture);
                return;
            }

            // Handle en passant
            if (move.moveType == Move.MoveType.EnPassant)
            {
                int capturedPawnSquare = PieceHelpers.IsWhite(movingPiece) ? move.to - 8 : move.to + 8;
                board.gameBoard[capturedPawnSquare] = Pieces.noPiece;
            }

            // Normal move
            board.gameBoard[move.to] = movingPiece;
            board.gameBoard[move.from] = Pieces.noPiece;

            // Double pawn move create en passant marker
            if (move.moveType == Move.MoveType.DoubleMove)
            {
                enPassantSquare = PieceHelpers.IsWhite(movingPiece) ? move.to - 8 : move.to + 8;

                board.gameBoard[enPassantSquare] = PieceHelpers.IsWhite(movingPiece)
                    ? Pieces.enPassantMarker
                    : Pieces.black * Pieces.enPassantMarker;
            }

            // Record move for game history (50-move rule and repetition detection)
            Game.RecordMove(board, move, isPawnMove, isCapture);
        }

        public static void PromotePawn(Board board, Move.moveInfo move)
        {
            string userChoiceForPromotion;

            ExecuteMove(board, move);
            Console.WriteLine("Promoting Pawn Options \n1: Queen\n2: Rook\n3: Bishop\n4: Knight\nEnter your choice:");
            userChoiceForPromotion = Console.ReadLine();

            switch (userChoiceForPromotion)
            {
                case "1":
                case "queen":
                    board.gameBoard[move.to] = sideToMove == 'w' ? Pieces.queen : Pieces.black * Pieces.queen;
                    break;
                case "2":
                case "rook":
                    board.gameBoard[move.to] = sideToMove == 'w' ? Pieces.rook : Pieces.black * Pieces.rook;
                    break;
                case "3":
                case "bishop":
                    board.gameBoard[move.to] = sideToMove == 'w' ? Pieces.bishop : Pieces.black * Pieces.bishop;
                    break;
                case "4":
                case "knight":
                    board.gameBoard[move.to] = sideToMove == 'w' ? Pieces.knight : Pieces.black * Pieces.knight;
                    break;
                default:
                    Console.WriteLine("Invalid choice, promoting to Queen");
                    board.gameBoard[move.to] = sideToMove == 'w' ? Pieces.queen : Pieces.black * Pieces.queen;
                    break;
            }
        }
    }
}
