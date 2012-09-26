using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        public int numberOfRows = 8;
        #region IChessAI Members that are implemented by the Student

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
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            throw (new NotImplementedException());
            
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
            throw (new NotImplementedException());
            
        }
        public bool IsInCheck(ChessBoard board, ChessColor curColor) {
            return false;
        }

        public List<ChessBoard> rookAndBishopCheck(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor, List<ChessBoard> lst)
        {
            x += dx;
            y += dy;

            if (x > 7 || x < 0 || y > 7 || y < 0)
                return lst;
            if (board[x, y].Equals(ChessPiece.Empty))
            {
                ChessBoard newBoard = board.Clone();//creates new board state
                newBoard.MakeMove(new ChessMove(new ChessLocation(x - dx, x - dy), new ChessLocation(x, y)));
                if (!IsInCheck(newBoard, curColor))
                {
                    lst.Add(newBoard);
                    return rookAndBishopCheck(x,y,dx,dy,newBoard,curColor,lst);
                }
                else
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
                        newBoard.MakeMove(new ChessMove(new ChessLocation(x - dx, x - dy), new ChessLocation(x, y)));
                        if (!IsInCheck(newBoard, curColor))
                        {
                            lst.Add(newBoard);
                            return lst;
                        }
                        else
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
                        newBoard.MakeMove(new ChessMove(new ChessLocation(x - dx, x - dy), new ChessLocation(x, y)));
                        if (!IsInCheck(newBoard, curColor))
                        {
                            lst.Add(newBoard);
                            return lst;
                        }
                        else
                            return lst;
                    }
                }
            }
        }
        public List<ChessMove> getAllMoves(ChessBoard board, ChessColor curColor)
        {
            List<ChessMove> retList = new List<ChessMove>();
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
                                boardList = rookAndBishopCheck(i, j, -1, -1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 1, 1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 1, -1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, -1, 1, board, curColor, boardList);
                                break;
                            case ChessPiece.WhiteKing:
                                break;
                            case ChessPiece.WhiteRook:
                                boardList = rookAndBishopCheck(i, j, -1, 0, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 0, 1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 1, 0, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 0, -1, board, curColor, boardList);
                                break;
                            case ChessPiece.WhiteQueen:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (dx != 0 && dy != 0)
                                            boardList = rookAndBishopCheck(i, j, dx, dy, board, curColor, boardList);
                                break;

                        }
                    }

                    else
                    {
                        switch (board[i, j])
                        {
                            case ChessPiece.BlackBishop:
                                boardList = rookAndBishopCheck(i, j, -1, -1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 1, 1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 1, -1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, -1, 1, board, curColor, boardList);
                                break;
                            case ChessPiece.BlackKing:
                                break;
                            case ChessPiece.BlackRook:
                                boardList = rookAndBishopCheck(i, j, -1, 0, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 0, 1, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 1, 0, board, curColor, boardList);
                                boardList = rookAndBishopCheck(i, j, 0, -1, board, curColor, boardList);
                                break;
                            case ChessPiece.BlackQueen:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (dx != 0 && dy != 0)
                                            boardList = rookAndBishopCheck(i, j, dx, dy, board, curColor, boardList);
                                break;

                        }
                    }
                }
            }
            return retList;
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
