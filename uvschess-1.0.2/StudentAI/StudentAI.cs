using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        
        #region IChessAI Members that are implemented by the Student
        public int numberOfRows = 8;
        public DecisionTree dTree;
        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "StudentAI (Debug)"; }
#else
            get { return "StudentAI"; }
#endif
        }

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="myColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            if (dTree == null)
                dTree = new DecisionTree(board);
            List<ChessMove> lst = getAllMoves(board, myColor);
            ChessMove chosen = null;
            int Best = 0;
            foreach (ChessMove move in lst)
            {
                if (move.ValueOfMove >= Best)
                {
                    chosen = move;
                    Best = move.ValueOfMove;
                }
            }
            return chosen;
        }

        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            //throw (new NotImplementedException());
            return true;
            
        }

        public bool isChecked(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor)
        {
            x += dx;
            y += dy;

            if (!(x > -1 && x < numberOfRows && y > -1 && y < numberOfRows))
                return false;

            if (board[x,y] == ChessPiece.Empty) // if space is empty recurse
                return isChecked(x, y, dx, dy, board, curColor);
            
            else if (board[x,y] < ChessPiece.Empty && curColor == ChessColor.Black) //if piece is black and I'm black not in check
                return false;
            else if (board[x,y] > ChessPiece.Empty && curColor == ChessColor.White) //if piece is white and I'm white not in check
                return false;
         
            else if (dx == dy || -dx == dy) //bishop or queen
            {
                if (board[x,y] == ChessPiece.WhiteBishop || board[x,y] == ChessPiece.WhiteQueen || board[x,y] == ChessPiece.BlackBishop || board[x,y] == ChessPiece.BlackQueen)
                    return true;
                return false;
            }

            else //rook or queen
            {
                if (board[x,y] == ChessPiece.WhiteRook || board[x,y] == ChessPiece.WhiteQueen || board[x,y] == ChessPiece.BlackRook || board[x,y] == ChessPiece.BlackQueen)
                    return true;
                return false;
            }

        }
        
        public bool isKnightChecking(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor)
        {
            x += dx;
            y += dy;

            if (!(x > -1 && x < numberOfRows && y > -1 && y < numberOfRows))
                return false;

            if (board[x,y] == ChessPiece.BlackKnight && curColor == ChessColor.White)
                return true;
            if (board[x,y] == ChessPiece.WhiteKnight && curColor == ChessColor.Black)
                return true;

            return false;
        }

        public bool IsInCheck(ChessBoard board, ChessColor curColor) 
        {
            for (int x = 0; x < numberOfRows; x++)
            {
                for (int y = 0; y < numberOfRows; y++)
                {
                    if ((board[x,y] == ChessPiece.BlackKing && curColor == ChessColor.Black) || (board[x,y] == ChessPiece.WhiteKing && curColor == ChessColor.White))
                    {
                        //check for pawn
                        if (curColor == ChessColor.Black)//white can only move in negative direction, they are white
                        {
                            if ((x-1 >= 0) && (board[x-1, y+1] == ChessPiece.WhitePawn))
                                return true;
                            else if ((x+1 < numberOfRows) && (board[x+1, y+1] == ChessPiece.WhitePawn))
                                return true;
                        }
                        else //black can only move in positive direction, they are black
                        {
                            if ((x-1 >= 0) && (board[x-1, y-1] == ChessPiece.BlackPawn))
                                return true;
                            else if ((x+1 < numberOfRows) && (board[x+1, y-1] == ChessPiece.BlackPawn))
                                return true;
                        }

                        //check for knight
                        if (isKnightChecking(x, y, 2, 3, board, curColor))
                            return true;
                        else if (isKnightChecking (x, y, 3, 2, board, curColor))
                            return true;
                        else if (isKnightChecking(x, y, -2, -3, board, curColor))
                            return true;
                        else if (isKnightChecking (x, y, -3, -2, board, curColor))
                            return true;
                        else if (isKnightChecking(x, y, 2, -3, board, curColor))
                            return true;
                        else if (isKnightChecking (x, y, 3, -2, board, curColor))
                            return true;
                        else if (isKnightChecking(x, y, -2, 3, board, curColor))
                            return true;
                        else if (isKnightChecking (x, y, -3, 2, board, curColor))
                            return true;

                        //check for rook, bishop and queen
                        for (int dx = -1; dx < 2; dx++)//this will check for rook, bishop and queen
                        {
                            for (int dy = -1; dy < 2; dy++)
                            {
                                if (dx != 0 && dy != 0)
                                {
                                    if (isChecked(x, y, dx, dy, board, curColor))
                                        return true;
                                }
                            }
                        }
                    }
                }
            } 
            return false;
        }

        public int rateMove(ChessBoard board) //black is negative and white is positive
        {
            int val = 0;

            for (int x = 0; x < numberOfRows; x++)
            {
                for (int y = 0; y < numberOfRows; y++)
                {
                    if (board[x, y] == ChessPiece.Empty)
                        val += 0;
                    else if (board[x, y] == ChessPiece.BlackPawn)
                        val -= 1;
                    else if (board[x, y] == ChessPiece.WhitePawn)
                        val += 1;
                    else if (board[x, y] == ChessPiece.BlackBishop)
                        val -= 3;
                    else if (board[x, y] == ChessPiece.WhiteBishop)
                        val += 3;
                    else if (board[x, y] == ChessPiece.BlackKnight)
                        val -= 3;
                    else if (board[x, y] == ChessPiece.WhiteKnight)
                        val += 3;
                    else if (board[x, y] == ChessPiece.BlackRook)
                        val -= 5;
                    else if (board[x, y] == ChessPiece.WhiteRook)
                        val += 5;
                    else if (board[x, y] == ChessPiece.BlackQueen)
                        val -= 9;
                    else if (board[x, y] == ChessPiece.WhiteQueen)
                        val += 9;
                }
            }
            return val;
        }

        public List<ChessMove> knightAndKingCheck(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor, List<ChessMove> lst)
        {
            x += dx;
            y += dy;

            if (x > 7 || x < 0 || y > 7 || y < 0)
                return lst;
            if (board[x, y].Equals(ChessPiece.Empty))
            {
                ChessBoard newBoard = board.Clone();//creates new board state
                ChessMove newMove = new ChessMove(new ChessLocation(x - dx, y - dy), new ChessLocation(x, y));
                newBoard.MakeMove(newMove);
                if (!IsInCheck(newBoard, curColor))
                {
                    newMove.ValueOfMove = rateMove(newBoard);
                    lst.Add(newMove);
                }
                
                return lst;
            }
            else
            {
                if (curColor == ChessColor.Black) //is black
                {
                    if (board[x, y] < ChessPiece.Empty) //piece is black
                    {
                        return lst;
                    }
                    else
                    {
                        ChessBoard newBoard = board.Clone();//creates new board state
                        ChessMove newMove = new ChessMove(new ChessLocation(x - dx, y - dy), new ChessLocation(x, y));
                        newBoard.MakeMove(newMove);
                        if (!IsInCheck(newBoard, curColor))
                        {
                            newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                        return lst;
                    }
                }
                else //is white
                {
                    if (board[x, y] > ChessPiece.Empty) //piece is white
                    {
                        return lst;
                    }
                    else
                    {
                        ChessBoard newBoard = board.Clone();//creates new board state
                        ChessMove newMove = new ChessMove(new ChessLocation(x - dx, y - dy), new ChessLocation(x, y));
                        newBoard.MakeMove(newMove);
                        if (!IsInCheck(newBoard, curColor))
                        {
                            newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                        return lst;
                    }
                }
            }
        }

        public List<ChessMove> rookAndBishopCheck(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor, List<ChessMove> lst)
        {
            x += dx;
            y += dy;

            if (x > 7 || x < 0 || y > 7 || y < 0)
                return lst;
            if (board[x, y].Equals(ChessPiece.Empty))
            {
                ChessBoard newBoard = board.Clone();//creates new board state
                ChessMove newMove = new ChessMove(new ChessLocation(x - dx, y - dy), new ChessLocation(x, y));
                newBoard.MakeMove(newMove);
                if (!IsInCheck(newBoard, curColor))
                {
                    newMove.ValueOfMove = rateMove(newBoard);
                    lst.Add(newMove);
                }
                return rookAndBishopCheck(x,y,dx,dy,newBoard,curColor,lst);
            }
            else
            {
                if (curColor == ChessColor.Black) //is black
                {
                    if (board[x, y] < ChessPiece.Empty) //piece is black
                    {
                        return lst;
                    }
                    else
                    {
                        ChessBoard newBoard = board.Clone();//creates new board state
                        ChessMove newMove = new ChessMove(new ChessLocation(x - dx, y - dy), new ChessLocation(x, y));
                        newBoard.MakeMove(newMove);
                        if (!IsInCheck(newBoard, curColor))
                        {
                            newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                            return lst;
                    }
                }
                else //is white
                {
                    if (board[x, y] > ChessPiece.Empty) //piece is white
                    {
                        return lst;
                    }
                    else
                    {
                        ChessBoard newBoard = board.Clone();//creates new board state
                        ChessMove newMove = new ChessMove(new ChessLocation(x - dx, y - dy), new ChessLocation(x, y));
                        newBoard.MakeMove(newMove);
                        if (!IsInCheck(newBoard, curColor))
                        {
                            newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                            return lst;
                    }
                }
            }
        }

        public List<ChessMove> pawnCheck(int x, int y, ChessBoard board, ChessColor curColor, List<ChessMove> lst)
        {
            if (curColor == ChessColor.Black)
            {
                if (y == 1) //check move 2 spots
                {
                    if (board[x, y + 1] == ChessPiece.Empty && board[x, y + 2] == ChessPiece.Empty)
                    {
                        ChessBoard newBoard = board.Clone();//creates new board state
                        ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y+2));
                        newBoard.MakeMove(newMove);
                        if (!IsInCheck(newBoard, curColor))
                        {
                            newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                    }
                }

                if (board[x, y + 1] == ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y + 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if (x+1 < numberOfRows && board[x + 1, y + 1] > ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y + 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if (x - 1 > -1 && board[x - 1, y + 1] > ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y + 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }
            }
            else //if white
            {
                if (y == 6) //check move 2 spots
                {
                    if (board[x, y - 1] == ChessPiece.Empty && board[x, y - 2] == ChessPiece.Empty)
                    {
                        ChessBoard newBoard = board.Clone();//creates new board state
                        ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y - 2));
                        newBoard.MakeMove(newMove);
                        if (!IsInCheck(newBoard, curColor))
                        {
                            newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                    }
                }

                if (board[x, y - 1] == ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y - 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if (x + 1 < numberOfRows && board[x + 1, y - 1] < ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y - 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if (x - 1 > -1 && board[x - 1, y - 1] < ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y - 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }
            }
            return lst;
        }

        public List<ChessMove> getAllMoves(ChessBoard board, ChessColor curColor)
        {
            List<ChessMove>  moveList = new List<ChessMove>();
            List<ChessBoard> boardList = new List<ChessBoard>();
            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfRows; j++)
                {
                    if (curColor == ChessColor.White)
                    {
                        switch (board[i, j])
                        {
                            case ChessPiece.WhiteBishop:
                                moveList = rookAndBishopCheck(i, j, -1, -1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 1, 1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 1, -1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, -1, 1, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteKing:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (dx != 0 && dy != 0)
                                            moveList = knightAndKingCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteRook:
                                moveList = rookAndBishopCheck(i, j, -1, 0, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 0, 1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 1, 0, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 0, -1, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteQueen:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (dx != 0 && dy != 0)
                                            moveList = rookAndBishopCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteKnight:
                                moveList = knightAndKingCheck(i, j, 3, 2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, 2, 3, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -3, 2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, 3, -2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -3, -2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -2, -3, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -2, 3, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, 2, -3, board, curColor, moveList);
                                break;
                            case ChessPiece.WhitePawn:
                                moveList = pawnCheck(i, j, board, curColor, moveList);
                                break;
                        }
                    }

                    else
                    {
                        switch (board[i, j])
                        {
                            case ChessPiece.BlackBishop:
                                moveList = rookAndBishopCheck(i, j, -1, -1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 1, 1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 1, -1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, -1, 1, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackKing:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (dx != 0 && dy != 0)
                                            moveList = knightAndKingCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackRook:
                                moveList = rookAndBishopCheck(i, j, -1, 0, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 0, 1, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 1, 0, board, curColor, moveList);
                                moveList = rookAndBishopCheck(i, j, 0, -1, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackQueen:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (dx != 0 && dy != 0)
                                            moveList = rookAndBishopCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackKnight:
                                moveList = knightAndKingCheck(i, j, 3, 2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, 2, 3, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -3, 2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, 3, -2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -3, -2, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -2, -3, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, -2, 3, board, curColor, moveList);
                                moveList = knightAndKingCheck(i, j, 2, -3, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackPawn:
                                moveList = pawnCheck(i, j, board, curColor, moveList);
                                break;
                        }
                    }
                }
            }

            return moveList;
        }
        #endregion
















        #region IChessAI Members that should be implemented as automatic properties and should NEVER be touched by students.
        /// <summary>
        /// This will return false when the framework starts running your AI. When the AI's time has run out,
        /// then this method will return true. Once this method returns true, your AI should return a 
        /// move immediately.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public AIIsMyTurnOverCallback IsMyTurnOver { get; set; }

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AILoggerCallback Log { get; set; }

        /// <summary>
        /// Call this method to catch profiling information. The framework subscribes to this event
        /// and will print out the profiling stats in your log window.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="key"></param>
        public AIProfiler Profiler { get; set; }

        /// <summary>
        /// Call this method to tell the framework what decision print out debug information. The framework subscribes to this event
        /// and will provide a debug window for your decision tree.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AISetDecisionTreeCallback SetDecisionTree { get; set; }
        #endregion
    }
}
