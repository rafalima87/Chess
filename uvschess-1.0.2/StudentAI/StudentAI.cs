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
        public int maxLevels = 2;
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

        //will return best move of list, if passed in color as white highest positive value is best move, if black is color passed in, the most negative value move is returned
        //color of player making the move is irrelevant, only value of move, so you can check best move for you of oppenent's possible moves
        public ChessMove BestMove(List<ChessMove> moves, ChessColor curColor)
        {
            int best = (curColor == ChessColor.White ? -1000000000 : 1000000000);
            List<ChessMove> bestList = new List<ChessMove>();
            foreach (ChessMove move in moves)
            {
                if (move.ValueOfMove == best)
                {
                    bestList.Add(move);
                }
                else if (curColor == ChessColor.White)
                {
                    if (move.ValueOfMove > best)
                    {
                        best = move.ValueOfMove;
                        bestList.Clear();
                        bestList.Add(move);
                    }
                }
                else//if I'm black
                {
                    if (move.ValueOfMove < best)
                    {
                        best = move.ValueOfMove;
                        bestList.Clear();
                        bestList.Add(move);
                    }
                }
            }

            Random rand = new Random();
           
            return bestList[rand.Next(bestList.Count)];
        }

        //returns maximum value move  (+ is good if you are white, - if you are black)
        //choose my best move given current state
        public int MaxValue(DecisionTree dt, ChessMove move, ChessBoard board, ChessColor myColor, int counter, int maximum, int minimum)
        {
            if (counter == maxLevels)
                return RateMove(board);
            List<ChessMove> allMyMoves = GetAllMoves(board, myColor, false);

            // Go through all of my opponent moves and add them to the decision tree
            foreach (ChessMove myCurMove in allMyMoves)
            {
                ChessBoard boardAfterMyCurMove = board.Clone();
                boardAfterMyCurMove.MakeMove(myCurMove);
                dt.AddChild(boardAfterMyCurMove, myCurMove);
                dt = dt.LastChild;
                myCurMove.ValueOfMove = MinValue(dt, myCurMove, boardAfterMyCurMove, myColor, counter, maximum, minimum);
                if (myColor == ChessColor.White)
                {
                    if (myCurMove.ValueOfMove >= minimum)
                        return myCurMove.ValueOfMove;
                    maximum = (maximum > myCurMove.ValueOfMove ? maximum : myCurMove.ValueOfMove);
                }
                else
                {
                    if (myCurMove.ValueOfMove <= minimum)
                        return myCurMove.ValueOfMove;
                    maximum = (maximum < myCurMove.ValueOfMove ? maximum : myCurMove.ValueOfMove);
                }

                dt = dt.Parent;
            }

            if (allMyMoves.Count > 0)
            {
                dt.BestChildMove = BestMove(allMyMoves, myColor);
                return dt.BestChildMove.ValueOfMove;
            }

            if (move.Flag == ChessFlag.Check  || IsInCheck(board, myColor))
            {
                move.Flag = ChessFlag.Checkmate;
                return (myColor == ChessColor.White ? -1000000000 : 1000000000);
            }

            move.Flag = ChessFlag.Stalemate;
            return (myColor == ChessColor.White ? 1000 : -1000);
        }

        //returns minimum value move  (+ is good if you are white, - if you are black)
        //opponent will choose the best move for them, which is worth the least for me
        public int MinValue(DecisionTree dt, ChessMove move, ChessBoard board, ChessColor myColor, int counter, int maximum, int minimum)
        {
            counter++;
            if (counter > maxLevels)
                return RateMove(board);
            ChessColor oppColor = (myColor == ChessColor.White ? ChessColor.Black : ChessColor.White);
            List<ChessMove> allOppMoves = GetAllMoves(board, oppColor, false);
                
            // Go through all of my opponent moves and add them to the decision tree
            foreach (ChessMove oppCurMove in allOppMoves)
            {
                ChessBoard boardAfterOppCurMove = board.Clone();
                boardAfterOppCurMove.MakeMove(oppCurMove);
                dt.AddChild(boardAfterOppCurMove, oppCurMove);
                dt = dt.LastChild;
                oppCurMove.ValueOfMove = MaxValue(dt, oppCurMove, boardAfterOppCurMove, myColor, counter, maximum, minimum);
                if (myColor == ChessColor.White)
                {
                    if (oppCurMove.ValueOfMove <= maximum)
                        return oppCurMove.ValueOfMove;
                    minimum = (minimum < oppCurMove.ValueOfMove ? minimum : oppCurMove.ValueOfMove);
                }
                else
                {
                    if (oppCurMove.ValueOfMove >= maximum)
                        return oppCurMove.ValueOfMove;
                    minimum = (minimum > oppCurMove.ValueOfMove ? minimum : oppCurMove.ValueOfMove);
                }
                dt = dt.Parent;
            }

            if (allOppMoves.Count > 0)
            {
                dt.BestChildMove = BestMove(allOppMoves, oppColor);
                return dt.BestChildMove.ValueOfMove;
            }

            if (move.Flag == ChessFlag.Check || IsInCheck(board, oppColor))
            {
                move.Flag = ChessFlag.Checkmate;
                return (myColor == ChessColor.White ? 1000000000 : -1000000000);
            }
            move.Flag = ChessFlag.Stalemate;
            return (myColor == ChessColor.White ? -1000 : 1000);
        }

        //will add all moves to decision tree then recurse to find best option
        public void FigureOutDecisionTree(List<ChessMove> moves, ChessBoard board, ChessColor curColor)
        {
            //if (dTree == null)
            // {
            dTree = new DecisionTree(board);
            //}
            SetDecisionTree(dTree);

            int maximum = curColor == ChessColor.White ? -100000000 : 100000000;
            int minimum = -maximum;

            // Go through all of my moves, add them to the decision tree
            // Then go through each of these moves and generate all of my
            // opponents moves and add those to the decision tree as well.
            foreach (ChessMove myCurMove in moves)
            {
                ChessBoard boardAfterMyCurMove = board.Clone();
                boardAfterMyCurMove.MakeMove(myCurMove);
                if (IsInCheck(boardAfterMyCurMove, (curColor == ChessColor.White ? ChessColor.Black : ChessColor.White)))
                {
                    myCurMove.Flag = ChessFlag.Check;
                }

                // Add the new move and board to the decision tree
                dTree.AddChild(boardAfterMyCurMove, myCurMove);

                // Descend the decision tree to the last child added so we can 
                // add all of the opponents response moves to our move.
                dTree = dTree.LastChild;

                myCurMove.ValueOfMove = MinValue(dTree, myCurMove, boardAfterMyCurMove, curColor, 0, maximum, minimum);

                dTree = dTree.Parent;
            }
            dTree.BestChildMove = BestMove(moves, curColor);
        }

/*      This version will go 1 level deep only
        //will add all moves to decision tree then recurse to find best option
        public void FigureOutDecisionTree(List<ChessMove> moves, ChessBoard board, ChessColor curColor)
        {
            //if (dTree == null)
            // {
            dTree = new DecisionTree(board);
            //}
            SetDecisionTree(dTree);

            // Go through all of my moves, add them to the decision tree
            // Then go through each of these moves and generate all of my
            // opponents moves and add those to the decision tree as well.
            for (int ix = 0; ix < moves.Count; ix++)
            {
                ChessMove myCurMove = moves[ix];
                ChessBoard boardAfterMyCurMove = board.Clone();
                boardAfterMyCurMove.MakeMove(myCurMove);

                // Add the new move and board to the decision tree
                dTree.AddChild(boardAfterMyCurMove, myCurMove);

                // Descend the decision tree to the last child added so we can 
                // add all of the opponents response moves to our move.
                dTree = dTree.LastChild;

                // Get all of the opponents response moves to my move
                ChessColor oppColor = (curColor == ChessColor.White ? ChessColor.Black : ChessColor.White);
                List<ChessMove> allOppMoves = getAllMoves(boardAfterMyCurMove, oppColor, false);

                // Go through all of my opponent moves and add them to the decision tree
                foreach (ChessMove oppCurMove in allOppMoves)
                {
                    ChessBoard boardAfterOppCurMove = boardAfterMyCurMove.Clone();
                    boardAfterOppCurMove.MakeMove(oppCurMove);
                    oppCurMove.ValueOfMove = rateMove(boardAfterOppCurMove);
                    dTree.AddChild(boardAfterOppCurMove, oppCurMove);
                }

                if (allOppMoves.Count > 0)
                {
                    dTree.BestChildMove = bestMove(allOppMoves, oppColor);
                    myCurMove.ValueOfMove = dTree.BestChildMove.ValueOfMove;
                    if (IsInCheck (boardAfterMyCurMove, oppColor))
                        myCurMove.Flag = ChessFlag.Check;
                }

                else
                {
                    if (IsInCheck (boardAfterMyCurMove, oppColor))
                    {
                        myCurMove.Flag = ChessFlag.Checkmate;
                        myCurMove.ValueOfMove += (curColor == ChessColor.White ? 1000000000 : -1000000000);
                    }
                    else
                    {
                        myCurMove.Flag = ChessFlag.Stalemate;
                        myCurMove.ValueOfMove += (curColor == ChessColor.White ? -10 : 10);
                    }

                }
                

                // All of the opponents response moves have been added to this childs move, 
                // so return to the parent so we can do the loop again for our next move.
                dTree = dTree.Parent;
            }
            dTree.BestChildMove = bestMove(moves, curColor);
        }*/

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="myColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            List <ChessMove> moves = GetAllMoves(board, myColor, false);
            FigureOutDecisionTree(moves, board, myColor);

            ChessMove chosen = dTree.BestChildMove;
            
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
            List<ChessMove> lst = GetAllMoves(boardBeforeMove, colorOfPlayerMoving, true);
            foreach (ChessMove x in lst)
            {
                if(moveToCheck.Equals(x))
                    return true;
            }
            return false;            
        }

        public bool IsChecked(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor)
        {
            int newx = x + dx;
            int newy = y + dy;

            if (newx < 0 || newx > 7 || newy < 0 || newy > 7)
                return false;

            if (board[newx, newy] == ChessPiece.Empty) // if space is empty recurse
            {
                if (dx > 0)
                    dx++;
                else if (dx < 0)
                    dx--;

                if (dy > 0)
                    dy++;
                else if (dy < 0)
                    dy--;

                return IsChecked(x, y, dx, dy, board, curColor);
            }

            if (dx == dy || -dx == dy) //bishop or queen
            {
                if (((board[newx, newy] == ChessPiece.WhiteBishop || board[newx, newy] == ChessPiece.WhiteQueen) && curColor == ChessColor.Black) || ((board[newx, newy] == ChessPiece.BlackBishop || board[newx, newy] == ChessPiece.BlackQueen) && curColor == ChessColor.White))
                    return true;
            }

            else //rook or queen
            {
                if (((board[newx, newy] == ChessPiece.WhiteRook || board[newx, newy] == ChessPiece.WhiteQueen) && curColor == ChessColor.Black) || ((board[newx, newy] == ChessPiece.BlackRook || board[newx, newy] == ChessPiece.BlackQueen) && curColor == ChessColor.White))
                    return true;
            }
            return false;
        }
        
        public bool IsKnightChecking(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor)
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

        public bool IsKingChecking(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor)
        {
            x += dx;
            y += dy;

            if (!(x > -1 && x < numberOfRows && y > -1 && y < numberOfRows))
                return false;

            if (board[x, y] == ChessPiece.BlackKing && curColor == ChessColor.White)
                return true;
            if (board[x, y] == ChessPiece.WhiteKing && curColor == ChessColor.Black)
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
                            if ((x+1 < numberOfRows) && (board[x+1, y+1] == ChessPiece.WhitePawn))
                                return true;
                        }
                        else //black can only move in positive direction, they are black
                        {
                            if ((x-1 >= 0) && (board[x-1, y-1] == ChessPiece.BlackPawn))
                                return true;
                            if ((x+1 < numberOfRows) && (board[x+1, y-1] == ChessPiece.BlackPawn))
                                return true;
                        }

                        //check for knight
                        if (IsKnightChecking(x, y, 2, 1, board, curColor))
                            return true;
                        if (IsKnightChecking (x, y, 1, 2, board, curColor))
                            return true;
                        if (IsKnightChecking(x, y, -2, -1, board, curColor))
                            return true;
                        if (IsKnightChecking (x, y, -1, -2, board, curColor))
                            return true;
                        if (IsKnightChecking(x, y, 2, -1, board, curColor))
                            return true;
                        if (IsKnightChecking (x, y, 1, -2, board, curColor))
                            return true;
                        if (IsKnightChecking(x, y, -2, 1, board, curColor))
                            return true;
                        if (IsKnightChecking (x, y, -1, 2, board, curColor))
                            return true;

                        //check for rook, bishop and queen and king
                        for (int dx = -1; dx < 2; dx++)//this will check for rook, bishop and queen and king
                        {
                            for (int dy = -1; dy < 2; dy++)
                            {
                                if (!(dx == 0 && dy == 0))
                                {
                                    if (IsChecked(x, y, dx, dy, board, curColor))
                                        return true;
                                    if (IsKingChecking(x, y, dx, dy, board, curColor))
                                        return true;
                                }
                            }
                        }
                        return false;
                    }
                }
            } 
            return false;
        }

        public int RateMove(ChessBoard board) //black is negative and white is positive
        {
            int val = 0;

            for (int x = 0; x < numberOfRows; x++)
            {
                for (int y = 0; y < numberOfRows; y++)
                {
                    if (board[x, y] == ChessPiece.Empty)
                        val += 0;
                    else if (board[x, y] == ChessPiece.BlackPawn)
                    {
                        val -= 3;
                        if (y == 4)
                            val -= 1;
                        else if (y == 5)
                            val -= 2;
                        else if (y == 6)
                            val -= 3;
                    }
                    else if (board[x, y] == ChessPiece.WhitePawn)
                    {
                        val += 3;
                        if (y == 3)
                            val += 1;
                        else if (y == 2)
                            val += 2;
                        else if (y == 1)
                            val += 3;
                    }
                    else if (board[x, y] == ChessPiece.BlackBishop)
                        val -= 9;
                    else if (board[x, y] == ChessPiece.WhiteBishop)
                        val += 9;
                    else if (board[x, y] == ChessPiece.BlackKnight)
                        val -= 9;
                    else if (board[x, y] == ChessPiece.WhiteKnight)
                        val += 9;
                    else if (board[x, y] == ChessPiece.BlackRook)
                        val -= 15;
                    else if (board[x, y] == ChessPiece.WhiteRook)
                        val += 15;
                    else if (board[x, y] == ChessPiece.BlackQueen)
                        val -= 27;
                    else if (board[x, y] == ChessPiece.WhiteQueen)
                        val += 27;
                }
            }
     //       if (IsInCheck(board, ChessColor.White))
     //          val -= 50;
     //       else if (IsInCheck(board, ChessColor.Black))
     //           val += 50;

            return val;
        }

        public List<ChessMove> KnightAndKingCheck(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor, List<ChessMove> lst)
        {
            int newx = x + dx;
            int newy = y + dy;

            if (newx > 7 || newx < 0 || newy > 7 || newy < 0)
                return lst;
            if (board[newx, newy].Equals(ChessPiece.Empty))
            {
                ChessBoard newBoard = board.Clone();//creates new board state
                ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(newx, newy));
                newBoard.MakeMove(newMove);
                if (!IsInCheck(newBoard, curColor))
                {
                    //newMove.ValueOfMove = rateMove(newBoard);
                    lst.Add(newMove);
                }
                
                return lst;
            }
            if (curColor == ChessColor.Black) //is black
            {
                if (board[newx, newy] < ChessPiece.Empty) //piece is black
                {
                    return lst;
                }
                else
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(newx, newy));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        //newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                    return lst;
                }
            }

            if (board[newx, newy] > ChessPiece.Empty) //piece is white
            {
                return lst;
            }
            else
            {
                ChessBoard newBoard = board.Clone();//creates new board state
                ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(newx, newy));
                newBoard.MakeMove(newMove);
                if (!IsInCheck(newBoard, curColor))
                {
                    //newMove.ValueOfMove = rateMove(newBoard);
                    lst.Add(newMove);
                }
                return lst;
            }
        }

        public List<ChessMove> RookAndBishopCheck(int x, int y, int dx, int dy, ChessBoard board, ChessColor curColor, List<ChessMove> lst)
        {
            int newx = x + dx;
            int newy = y + dy;

            if (newx > 7 || newx < 0 || newy > 7 || newy < 0)
                return lst;
            if (board[newx, newy].Equals(ChessPiece.Empty))
            {
                ChessBoard newBoard = board.Clone();//creates new board state
                ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(newx, newy));
                newBoard.MakeMove(newMove);
                if (!IsInCheck(newBoard, curColor))
                {
                    //newMove.ValueOfMove = rateMove(newBoard);
                    lst.Add(newMove);
                }
                if (dx > 0)
                    dx++;
                else if (dx < 0)
                    dx--;

                if (dy > 0)
                    dy++;
                else if (dy < 0)
                    dy--;

                return RookAndBishopCheck(x,y,dx,dy,board,curColor,lst);
            }
            if (curColor == ChessColor.Black) //is black
            {
                if (board[newx, newy] < ChessPiece.Empty) //piece is black
                {
                    return lst;
                }
                else
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(newx, newy));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        //newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                    return lst;
                }
            }
            //is white
            if (board[newx, newy] > ChessPiece.Empty) //piece is white
            {
                return lst;
            }
            else
            {
                ChessBoard newBoard = board.Clone();//creates new board state
                ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(newx, newy));
                newBoard.MakeMove(newMove);
                if (!IsInCheck(newBoard, curColor))
                {
                    //newMove.ValueOfMove = rateMove(newBoard);
                    lst.Add(newMove);
                }
                return lst;
            }
        }

        public List<ChessMove> PawnCheck(int x, int y, ChessBoard board, ChessColor curColor, List<ChessMove> lst)
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
                            //newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                    }
                }

                if ((y + 1 < 8) && board[x, y + 1] == ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y + 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        //newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if ((x + 1 < 8) && (y + 1 < 8) && board[x + 1, y + 1] > ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y + 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        //newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if (x - 1 > -1 && (y + 1 < 8) && board[x - 1, y + 1] > ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y + 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                       // newMove.ValueOfMove = rateMove(newBoard);
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
                            //newMove.ValueOfMove = rateMove(newBoard);
                            lst.Add(newMove);
                        }
                    }
                }

                if ((y - 1 > -1) && board[x, y - 1] == ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x, y - 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        //newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if ((x + 1 < 8) && (y - 1 > -1) && board[x + 1, y - 1] < ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x + 1, y - 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        //newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }

                if ((x - 1 > -1) && (y - 1 > -1) && board[x - 1, y - 1] < ChessPiece.Empty)
                {
                    ChessBoard newBoard = board.Clone();//creates new board state
                    ChessMove newMove = new ChessMove(new ChessLocation(x, y), new ChessLocation(x - 1, y - 1));
                    newBoard.MakeMove(newMove);
                    if (!IsInCheck(newBoard, curColor))
                    {
                        //newMove.ValueOfMove = rateMove(newBoard);
                        lst.Add(newMove);
                    }
                }
            }
            return lst;
        }

        public List<ChessMove> GetAllMoves(ChessBoard board, ChessColor curColor, bool checkChildren)
        {
            List<ChessMove>  moveList = new List<ChessMove>();
            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfRows; j++)
                {
                    if (curColor == ChessColor.White)
                    {
                        switch (board[i, j])
                        {
                            case ChessPiece.WhiteBishop:
                                moveList = RookAndBishopCheck(i, j, -1, -1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 1, 1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 1, -1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, -1, 1, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteKing:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (!(dx == 0 && dy == 0))
                                            moveList = KnightAndKingCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteRook:
                                moveList = RookAndBishopCheck(i, j, -1, 0, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 0, 1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 1, 0, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 0, -1, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteQueen:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (!(dx == 0 && dy == 0))
                                            moveList = RookAndBishopCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.WhiteKnight:
                                moveList = KnightAndKingCheck(i, j, 1, 2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, 2, 1, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -1, 2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, 1, -2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -1, -2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -2, -1, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -2, 1, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, 2, -1, board, curColor, moveList);
                                break;
                            case ChessPiece.WhitePawn:
                                moveList = PawnCheck(i, j, board, curColor, moveList);
                                break;
                        }
                    }

                    else
                    {
                        switch (board[i, j])
                        {
                            case ChessPiece.BlackBishop:
                                moveList = RookAndBishopCheck(i, j, -1, -1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 1, 1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 1, -1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, -1, 1, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackKing:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (!(dx == 0 && dy == 0))
                                            moveList = KnightAndKingCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackRook:
                                moveList = RookAndBishopCheck(i, j, -1, 0, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 0, 1, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 1, 0, board, curColor, moveList);
                                moveList = RookAndBishopCheck(i, j, 0, -1, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackQueen:
                                for (int dx = -1; dx < 2; dx++)
                                    for (int dy = -1; dy < 2; dy++)
                                        if (!(dx == 0 && dy == 0))
                                            moveList = RookAndBishopCheck(i, j, dx, dy, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackKnight:
                                moveList = KnightAndKingCheck(i, j, 1, 2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, 2, 1, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -1, 2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, 1, -2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -1, -2, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -2, -1, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, -2, 1, board, curColor, moveList);
                                moveList = KnightAndKingCheck(i, j, 2, -1, board, curColor, moveList);
                                break;
                            case ChessPiece.BlackPawn:
                                moveList = PawnCheck(i, j, board, curColor, moveList);
                                break;
                        }
                    }
                }
            }
            
            if (checkChildren)
            {
                CheckNext(board, curColor, moveList);
            }
            return moveList;
        }

        public void CheckNext(ChessBoard board, ChessColor curColor, List<ChessMove> moveList)
        {
            foreach (ChessMove move in moveList)
            {
                ChessBoard newBoard = board.Clone();
                newBoard.MakeMove(move);
                if (curColor == ChessColor.Black)
                {
                    List<ChessMove> newList = GetAllMoves(newBoard, ChessColor.White, false);
                    if (IsInCheck(newBoard, ChessColor.White))
                    {
                        if (newList.Count == 0)
                        {
                            move.Flag = ChessFlag.Checkmate;
                            move.ValueOfMove = -1000000000;
                        }
                        else
                        {
                            move.Flag = ChessFlag.Check;
                        }
                    }
                    else if (newList.Count == 0)
                    {
                        move.Flag = ChessFlag.Stalemate;
                        move.ValueOfMove += 10;
                    }
                }
                else// if I'm white
                {
                    List<ChessMove> newList = GetAllMoves(newBoard, ChessColor.Black, false);
                    if (IsInCheck(newBoard, ChessColor.Black))
                    {
                        if (newList.Count == 0)
                        {
                            move.Flag = ChessFlag.Checkmate;
                            move.ValueOfMove = 1000000000;
                        }
                        else
                        {
                            move.Flag = ChessFlag.Check;
                        }
                    }
                    else if (newList.Count == 0)
                    {
                        move.Flag = ChessFlag.Stalemate;
                        move.ValueOfMove -= 10;
                    }
                }
            }
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
