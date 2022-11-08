 using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Tetris
{
    internal class Program
    {
        private const int Rows = 22;
        private const int Columns = 10;
        private static int _time;
        private static int _points;
        private static Timer _secondsTimer;
        private static Timer _moveTimer;
        private static bool _gameOver = false;
        private static bool _printing;
        //private static bool _moving = false;
        private static bool _held;
        private static int[,] _playField = new int[Rows, Columns];
        private static Piece _movingPiece = new Piece(0);
        private static Piece _heldPiece = new Piece(0);
        private static Piece[] _nextPieces = {new Piece(0), new Piece(0), new Piece(0), new Piece(0), new Piece(0)};
        private static int _linesCleared;

        private static readonly Dictionary<int, int> ColorCodes = new Dictionary<int, int>() //translates piece color to Console.Color color
        {
            { 0, 15 },
            { 1, 14 },
            { 2, 14 },
            { 3, 11 },
            { 4, 11 },
            { 5, 9 },
            { 6, 9 },
            { 7, 4 },
            { 8, 4 },
            { 9, 10 },
            { 10, 10 },
            { 11, 12 },
            { 12, 12 },
            { 13, 13 },
            { 14, 13 },
            { 16, 7 }
        };

        public struct Piece //stores positions of all blocks and the color of a piece
        {
            public int[,] Blocks;
            public int Color;

            public Piece(int color)
            {
                Blocks = new[,] {{0, 0}, {0, 0}, {0, 0}, {0, 0}} ;
                Color = color;
            }

            public Piece(Piece tet) //copy constructor
            {
                this.Blocks = new int[4, 2];
                
                for (int i = 0; i < 4; i++)
                {
                    this.Blocks[i, 0] = tet.Blocks[i, 0];
                    this.Blocks[i, 1] = tet.Blocks[i, 1];
                }

                this.Color = tet.Color;
            }
        }

        /*
        static int LowestX(Piece tet) //returns x value of lowest block in a piece
        {
            int res = 0;

            for (int i = 0; i < 4; i++)
            {
                if (tet.Blocks[i, 0] > res)
                {
                    res = tet.Blocks[i, 0];
                }
            }

            return res;
        }

        static Piece LowestPos(Piece tet) //returns position of Piece if it dropped down straight
        {
            Piece pos = new Piece(tet) { Color = 16 };

            for(int i = 21 - LowestX(tet); Move(i, 0, ref pos, false) == 1; i--) {}
            
            return pos;
        }
        */

        static void ApplyPieceColor(Piece tet, int color)   //takes positions of piece and sets corresponding spots on the playing field to it's color
        {
            for (int i = 0; i < 4; i++)
            {
                _playField[tet.Blocks[i, 0], tet.Blocks[i, 1]] = color;
            }
        }
        
        static void HoldPiece() //swaps the current held piece with the moving piece
        {
            if (!_held) //make sure it's only done once before spawning a new piece
            {
                _held = true;
                Piece temp = new Piece(_movingPiece); //store moving piece
                
                if (_heldPiece.Color != 0)
                {
                    GeneratePiece(ref _movingPiece, ColorCodes[_movingPiece.Color]);    //just resets positions
                    GeneratePiece(ref _heldPiece, ColorCodes[_heldPiece.Color]);
                    (_movingPiece, _heldPiece) = (_heldPiece, _movingPiece);    //swap moving piece and held piece
                }
                else //only do on first exec
                {
                    _heldPiece = _movingPiece;
                    GeneratePiece(ref _heldPiece, ColorCodes[_heldPiece.Color]);
                    SpawnPiece();
                }

                ApplyPieceColor(temp, 0);   //clearing spot of where the original piece was
                ApplyPieceColor(_movingPiece, _movingPiece.Color);

                WriteGame();
            }
        }

        static void ClearLine(int row)  //moves all rows above row down once
        {
            for (int i = row - 1; i > Rows - 20; i--)
            {
                for (int j = 0; j < Columns; j++)
                {
                    _playField[i + 1, j] = _playField[i, j];
                }
            }
        }
        
        static void LineClearCheck(int row) //checks if a line can be cleared
        {
            for (int i = 0; i < Columns; i++)
            {
                if (_playField[row, i] % 2 != 1)
                {
                    return;
                }
            }

            ClearLine(row);
            _linesCleared += 1;
        }
        
        static void WritePieceLine(int row, Piece tet)  //writes one row of a piece (with nice formatting)
        {
            Console.Write("\u2551\u2001");
            for (int p = 3; p < 7; p++)
            {
                for (int q = 0; q < 4; q++)
                {
                    if (tet.Blocks[q, 1] == p && tet.Blocks[q, 0] == Rows - 22 + row)
                    {
                        Console.BackgroundColor = (ConsoleColor) ColorCodes[tet.Color];
                        break;
                    }
                }

                Console.Write("\u2001\u2001");
                Console.ResetColor();
            }

            Console.Write("\u2551");
        }
        
        static void TimeCounter(Object source, ElapsedEventArgs e)  //gets called every second
        {
            _time += 1;
        }

        static void WriteCharacters(char c, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Console.Write(c);
            }
        }

        /*
        public static bool CheckCritical(int[,] playField)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (playField[i, j] % 2 == 1) //all solid blocks will be uneven numbers
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        */

        static void SetBlock()  //sets a block down
        {
            _held = false;
            
            for (int i = 0; i < 4; i++)
            {
                _playField[_movingPiece.Blocks[i, 0], _movingPiece.Blocks[i, 1]] -= 1;
                LineClearCheck(_movingPiece.Blocks[i, 0]);
            }

            switch (_linesCleared)
            {
                case 1:
                    _points += 40;
                    break;
                case 2:
                    _points += 100;
                    break;
                case 3:
                    _points += 300;
                    break;
                case 4:
                    _points += 1200;
                    break;
            }

            _linesCleared = 0;

            SpawnPiece();
            WriteGame();
        }

        static int Move(int x, int y, ref Piece tet, bool write = true) //moves the moving piece x down and y right, returns 1 if illegal
        {
            //while (_moving) {} //wait until not moving

            //make sure that it's not trying to move in an illegal way
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    if (tet.Blocks[i, 1] + y == 10 || tet.Blocks[i, 1] + y == -1 ||
                        (_playField[tet.Blocks[i, 0] + x, tet.Blocks[i, 1] + y] != 0 &&
                         _playField[tet.Blocks[i, 0] + x, tet.Blocks[i, 1] + y] % 2 == 1))
                    {
                        //this would be an illegal move, therefore exit out of the method
                        return 1;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    return 1;
                }
            }

            //clear previous position
            ApplyPieceColor(tet, 0);

            //set new position
            for (int i = 0; i < 4; i++)
            {
                tet.Blocks[i, 0] += x;
                tet.Blocks[i, 1] += y;

                _playField[tet.Blocks[i, 0], tet.Blocks[i, 1]] = tet.Color;
            }

            if (write)
            {
                WriteGame();
            }

            return 0;
        }

        static void MoveDown(Object source, ElapsedEventArgs e) //moves a piece down once
        {
            //check if any blocks are in the way
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    if (_playField[_movingPiece.Blocks[i, 0] + 1, _movingPiece.Blocks[i, 1]] != 0 &&
                        _playField[_movingPiece.Blocks[i, 0] + 1, _movingPiece.Blocks[i, 1]] % 2 == 1)
                    {
                        //if yes, set down block
                        SetBlock();
                        return;
                    }
                }
                catch (IndexOutOfRangeException) //handles the case of the block being in the bottom row
                {
                    SetBlock();
                    return;
                }
            }

            //if no blocks are in the way, move down
            Move(1, 0, ref _movingPiece);
        }

        private static void SpawnPiece() //spawns a random piece
        {
            //setting the moving piece as next queued up piece
            _movingPiece = _nextPieces[0];
            
            //moving all remaining pieces ahead in queue
            for (int i = 0; i < 4; i++)
            {
                _nextPieces[i] = _nextPieces[i + 1];
            }
            
            GeneratePiece(ref _nextPieces[4]);

            ApplyPieceColor(_movingPiece, _movingPiece.Color);
        }

        private static void GeneratePiece(ref Piece tet, int num = -1)
        {
            if (num == -1)
            {
                Random random = new Random();
                num = random.Next(0, 7);
            }
            
            switch (num)
            {
                case 0: //O piece
                {
                    tet.Blocks = new [,]
                    {
                        { Rows-22, 4 },
                        { Rows-22, 5 },
                        { Rows-21, 4 },
                        { Rows-21, 5 },
                    };
                    break;
                }
                case 1: //I piece
                {
                    tet.Blocks = new [,]
                    {
                        { Rows-21, 3 },
                        { Rows-21, 4 },
                        { Rows-21, 5 },
                        { Rows-21, 6 },
                    };
                    break;
                }
                case 2: //J piece
                {
                    tet.Blocks = new [,]
                    {
                        { Rows-22, 3 },
                        { Rows-21, 3 },
                        { Rows-21, 4 },
                        { Rows-21, 5 },
                    };
                    break;
                }
                case 3: //L piece
                {
                    tet.Blocks = new [,]
                    {
                        { Rows-21, 3 },
                        { Rows-21, 4 },
                        { Rows-21, 5 },
                        { Rows-22, 5 },
                    };
                    break;
                }
                case 4: //S piece
                {
                    tet.Blocks = new [,]
                    {
                        { Rows-21, 3 },
                        { Rows-21, 4 },
                        { Rows-22, 4 },
                        { Rows-22, 5 },
                    };
                    break;
                }
                case 5: //Z piece
                {
                    tet.Blocks = new [,]
                    {
                        { Rows-22, 3 },
                        { Rows-22, 4 },
                        { Rows-21, 4 },
                        { Rows-21, 5 },
                    };
                    break;
                }
                case 6: //T piece
                {
                    tet.Blocks = new [,]
                    {
                        { Rows-22, 4 },
                        { Rows-21, 3 },
                        { Rows-21, 4 },
                        { Rows-21, 5 },
                    };
                    break;
                }
            }

            tet.Color = 2 * (num + 1);
        }

        private static void WriteGame()
        {
            if (!_printing) //if this would get called twice at the same time bad things would happen
            {
                _printing = true;

                const char square = '\u2001';
                const char horizontalBorder = '\u2550';
                const char verticalBorder = '\u2551';
                const char topLeftBorder = '\u2554';
                const char bottomLeftBorder = '\u255A';
                const char bottomRightBorder = '\u255D';
                const char topRightBorder = '\u2557';
                //bool critical;
                int i = Rows - 20;  //only print the last 20 rows
                Console.Clear();

                /* NOT NEEDED ANYMORE
                //check if there's a solid block in the first two rows
                critical = CheckCritical(playField);
                if (critical)
                {
                    i = 0;
                }
                */

                //just a bunch of fancy text, ignore this for debugging
                Console.Write(topLeftBorder);
                WriteCharacters(horizontalBorder, 4);
                Console.Write(topRightBorder + "" + square + "" + square + "" + topLeftBorder);
                WriteCharacters(horizontalBorder, 8);
                Console.WriteLine(topRightBorder);
                Console.WriteLine(verticalBorder + "TIME" + verticalBorder + "" + square + "" + square + "" +
                                  verticalBorder + "" + square + "" + square + "SCORE" + square + "" + verticalBorder);
                Console.WriteLine(verticalBorder + _time.ToString("D4") + verticalBorder + "" + square + "" + square +
                                  "" + verticalBorder + _points.ToString("D8") + verticalBorder);
                Console.Write(bottomLeftBorder);
                WriteCharacters(horizontalBorder,4);
                Console.Write(bottomRightBorder + "" + square + "" + square + "" + bottomLeftBorder);
                WriteCharacters(horizontalBorder,8);
                Console.WriteLine(bottomRightBorder);
                Console.Write(topLeftBorder);
                WriteCharacters(horizontalBorder,20);
                Console.Write(topRightBorder + "" + topLeftBorder);
                WriteCharacters(horizontalBorder,9);
                Console.WriteLine(topRightBorder);

                for (; i < Rows; i++)
                {
                    Console.Write(verticalBorder);
                    for (int j = 0; j < Columns; j++)
                    {
                        Console.BackgroundColor = (ConsoleColor) ColorCodes[_playField[i, j]];
                        Console.Write(square + "" + square);
                        Console.ResetColor();
                    }

                    Console.Write(verticalBorder);
                    
                    switch (i)  //for all the things to the right of the playing field
                    {
                        case (Rows - 20):
                            Console.Write(verticalBorder);
                            WriteCharacters('\u2000', 3);
                            Console.Write("HELD");
                            WriteCharacters('\u2000', 2);
                            Console.Write(verticalBorder);
                            break;
                        
                        case (Rows - 19):
                            WritePieceLine(0, _heldPiece);
                            break; 
                        
                        case (Rows - 18):
                            WritePieceLine(1, _heldPiece);
                            break;
                        
                        case (Rows - 17):
                            Console.Write(bottomLeftBorder);
                            WriteCharacters(horizontalBorder, 9);
                            Console.Write(bottomRightBorder);
                            break;
                        
                        case(Rows - 16):
                            Console.Write(topLeftBorder);
                            WriteCharacters(horizontalBorder,9);
                            Console.Write(topRightBorder);
                            break;
                        
                        case(Rows - 15):
                            Console.Write(verticalBorder + "" + square + "UPCOMING" + verticalBorder);
                            break;
                        
                        case(Rows - 14):
                        case(Rows - 11):
                        case(Rows - 8):
                        case(Rows - 5):
                        case(Rows - 2):
                            WritePieceLine(0, _nextPieces[(i-Rows+14)/3]);
                            break;
                        
                        case(Rows - 13):
                        case(Rows - 10):
                        case(Rows - 7):
                        case(Rows - 4):
                        case(Rows - 1):
                            WritePieceLine(1, _nextPieces[(i-Rows+14)/3]);
                            break;
                        
                        case(Rows - 12):
                        case(Rows - 9):
                        case(Rows - 6):
                        case(Rows - 3):
                            Console.Write(verticalBorder);
                            WriteCharacters(square, 9);
                            Console.Write(verticalBorder);
                            break;
                    }

                    Console.WriteLine();
                }

                //more fancy text
                Console.Write(bottomLeftBorder);
                WriteCharacters(horizontalBorder,20);
                Console.Write(bottomRightBorder + "" + bottomLeftBorder);
                WriteCharacters(horizontalBorder, 9);
                Console.WriteLine(bottomRightBorder);

                _printing = false;
            }
        }

        public static void Main(string[] args)
        {
            Console.Clear();
            ConsoleKeyInfo input;

            //populating playing field
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    _playField[i, j] = 0;
                }
            }

            //spawning in the first 6 pieces so that all 5 next pieces can be shown
            for (int i = 0; i < 6; i++)
            {
                SpawnPiece();
            }
            
            WriteGame();
            //making the theme play
            string musicPath = AppDomain.CurrentDomain.BaseDirectory + "tetris_theme.wav";
            SoundPlayer player = new SoundPlayer(musicPath);
            player.PlayLooping();

            //making timers
            //timer that counts seconds
            _secondsTimer = new Timer();
            _secondsTimer.Interval = 1000;
            _secondsTimer.Elapsed += TimeCounter;
            _secondsTimer.Enabled = true;
            //timer that moves pieces down
            _moveTimer = new Timer();
            _moveTimer.Interval = 500;
            _moveTimer.Elapsed += MoveDown;
            _moveTimer.Enabled = true;

            //input loop
            while (!_gameOver)
            {
                Thread.Sleep(50);
                input = Console.ReadKey(true);
                switch (input.Key)
                {
                    case ConsoleKey.LeftArrow:
                        Move(0, -1, ref _movingPiece);
                        break;
                    case ConsoleKey.RightArrow:
                        Move(0, 1, ref _movingPiece);
                        break;
                    case ConsoleKey.DownArrow:
                        MoveDown(null, null);
                        break;
                    case ConsoleKey.C:
                        HoldPiece();
                        break;
                }
            }
        }
    }
}
