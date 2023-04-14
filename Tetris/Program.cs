using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;

namespace Tetris
{
    class Tetromino
    {
        public Tetromino()
        {
            _position = new int[4, 2];
        }

        public Tetromino(Tetromino original)
        {
            _position = new int[4, 2];
            Color = original.Color;
            RotationState = original.RotationState;
            for (int i = 0; i < 4; i++)
            {
                _position[i, 0] = original._position[i, 0];
                _position[i, 1] = original._position[i, 1];
            }
        }

        private int[,] _position;
        
        public int[,] Position
        {
            get => _position;

            set
            {
                if (value == null)
                {
                    _position = null;
                }

                else
                {
                    _position = new int[value.GetLength(0),value.GetLength(1)];
                    
                    for (int i = 0; i < 4; i++)
                    {
                        _position[i, 0] = value[i, 0];
                        _position[i, 1] = value[i, 1];
                    }
                }
            }
        }
        
        public ConsoleColor Color { get; set; }
        
        public int RotationState { get; set; }
        
        public bool Move(int x, int y, ConsoleColor[,] playingField)
        {
            //making sure the new position is valid
            for (int i = 0; i < 4; i++)
            {
                if (Position[i, 0] + x < 0 || Position[i, 0] + x > 9 || Position[i, 1] + y < 0 ||
                    playingField[Position[i, 0] + x, Position[i, 1] + y] != ConsoleColor.White)
                {
                    return false;
                }
            }

            //moving the position of tet
            for (int i = 0; i < 4; i++)
            {
                Position[i, 0] += x;
                Position[i, 1] += y;
            }

            return true;
        }
        
        public void Generate(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Yellow: //O piece
                    Position = new [,]
                    {
                        { 4, 21 },
                        { 5, 21 },
                        { 4, 20 },
                        { 5, 20 }
                    };
                    break;
                
                case ConsoleColor.Cyan: //I piece
                    Position = new [,]
                    {
                        { 6, 20 },
                        { 5, 20 },
                        { 4, 20 },
                        { 3, 20 }
                    };
                    break;
                
                case ConsoleColor.Blue: //J piece
                    Position = new [,]
                    {
                        { 3, 21 },
                        { 3, 20 },
                        { 4, 20 },
                        { 5, 20 }
                    };
                    break;
                
                case ConsoleColor.DarkRed: //L piece
                    Position = new [,]
                    {
                        { 5, 21 },
                        { 3, 20 },
                        { 4, 20 },
                        { 5, 20 }
                    };
                    break;
                
                case ConsoleColor.Green: //S piece
                    Position = new [,]
                    {
                        { 4, 21 },
                        { 5, 21 },
                        { 4, 20 },
                        { 3, 20 }
                    };
                    break;
                
                case ConsoleColor.Red: //Z piece
                    Position = new [,]
                    {
                        { 3, 21 },
                        { 4, 21 },
                        { 4, 20 },
                        { 5, 20 }
                    };
                    break;
                
                case ConsoleColor.Magenta: //T piece
                    Position = new [,]
                    {
                        { 4, 21 },
                        { 3, 20 },
                        { 4, 20 },
                        { 5, 20 }
                    };
                    break;
            }

            RotationState = 0;
            Color = color;
        }
        
        public void Rotate(bool clockwise, ConsoleColor[,] playingField)
        {
            Tetromino rotated = new Tetromino(this);

            //_playerList.Add(new Task(PlayWavFile, "se_game_rotate.wav"));
            //_playerList[_playerList.Count-1].Start();

            (int, int)[] kickValues = new (int, int)[5];
            
            switch (rotated.Color)
            {
                //O piece does nothing
                case ConsoleColor.Yellow:
                    return;
                    
                //I piece is special because no rotation coordinates
                case ConsoleColor.Cyan:
                {
                    (int, int)[,] kickValuesTable =
                    {
                        { (0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2) },
                        { (0, 0), (-1, 0), (2, 0), (1, -2), (2, -1) },
                        { (0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2) },
                        { (0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1) }
                    };
                    for (int i = 0; i < 5; i++)
                    {
                        kickValues[i] = kickValuesTable[(rotated.RotationState + (clockwise ? 0 : 1)) % 4, i];
                    }

                    //projecting the moving piece to a 4x4 matrix for calculation purposes and swapping x with y
                    int[,] unitPos = new int[4, 2];
                    int[,] unitPosCopy = new int[4, 2];
                    for (int i = 0; i < 4; i++)
                    {
                        unitPosCopy[i, 1 - rotated.RotationState % 2] = 1 + (rotated.RotationState < 2 ? 1 : 0);
                        unitPosCopy[i, rotated.RotationState % 2] = rotated.Position[i, rotated.RotationState % 2] - rotated.Position[3, rotated.RotationState % 2];

                        unitPos[i, rotated.RotationState % 2] = 1 + (rotated.RotationState < 2 ? 1 : 0);
                        unitPos[i, 1 - rotated.RotationState % 2] = rotated.Position[i, rotated.RotationState % 2] - rotated.Position[3, rotated.RotationState % 2];
                    }

                    if (clockwise)
                    {
                        rotated.RotationState = (rotated.RotationState + 1) % 4;

                        if (rotated.RotationState % 2 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                unitPos[i, 1] += 1 - rotated.RotationState;
                            }
                        }
                    }

                    else
                    {
                        if (rotated.RotationState % 2 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                unitPos[i, 0] += 1 - rotated.RotationState;
                            }
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            kickValues[i].Item1 *= -1;
                            kickValues[i].Item2 *= -1;
                        }

                        rotated.RotationState = (rotated.RotationState + 3) % 4;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        rotated.Position[i, 0] += unitPos[i, 0] - unitPosCopy[i, 0];
                        rotated.Position[i, 1] += unitPos[i, 1] - unitPosCopy[i, 1];
                    }

                    break;
                }

                //J, L, T, S, Z pieces
                //the third block is the center of rotation for these
                default:
                {
                    (int, int)[,] kickValuesTable =
                    {
                        { (0, 0), (-1, 0), (-1, 1),  (0, -2), (-1, -2) },
                        { (0, 0), (1, 0),  (1, -1),  (0, 2),  (1, 2)   },
                        { (0, 0), (1, 0),  (1, 1),   (0, -1), (1, -2)  },
                        { (0, 0), (-1, 0), (-1, -1), (0, 2),  (-1, 2)  }
                    };
                    for (int i = 0; i < 5; i++)
                    {
                        kickValues[i] = kickValuesTable[(rotated.RotationState + (clockwise ? 0 : 1)) % 4, i];
                    }

                    int[,] unitPos = new int[4, 2];
                    int[,] unitPosCopy = new int[4, 2];
                    for (int i = 0; i < 4; i++)
                    {
                        unitPosCopy[i, 0] = rotated.Position[i, 0] - rotated.Position[2, 0];
                        unitPosCopy[i, 1] = rotated.Position[i, 1] - rotated.Position[2, 1];
                        
                        unitPos[i, 0] = rotated.Position[i, 0] - rotated.Position[2, 0];
                        unitPos[i, 1] = rotated.Position[i, 1] - rotated.Position[2, 1];
                    }

                    if (clockwise)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            (unitPos[i, 0], unitPos[i, 1]) = (unitPos[i, 1], -unitPos[i, 0]);   //(x, y) = (y, -x)
                        }

                        rotated.RotationState = (rotated.RotationState + 1) % 4;
                    }

                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            (unitPos[i, 0], unitPos[i, 1]) = (-unitPos[i, 1], unitPos[i, 0]);
                        }

                        rotated.RotationState = (rotated.RotationState + 3) % 4;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        rotated.Position[i, 0] += unitPos[i, 0] - unitPosCopy[i, 0];
                        rotated.Position[i, 1] += unitPos[i, 1] - unitPosCopy[i, 1];
                    }

                    break;
                }
            }
            
            for(int i = 0; i < 5; i++)
            {
                if (rotated.Move(kickValues[i].Item1, kickValues[i].Item2, playingField))
                {
                    break;
                }

                if (i == 4)
                {
                    return;
                }
            }

            Position = rotated.Position;
            RotationState = rotated.RotationState;
        }
        
        public int MoveDown(ref ConsoleColor[,] playingField, int lockDelay, int lockDelayTimer)
        {
            //making sure the new position is valid
            for (int i = 0; i < 4; i++)
            {
                if (Position[i, 1] - 1 < 0 || playingField[Position[i, 0], Position[i, 1] - 1] != ConsoleColor.White)
                {
                    //if not, set down tetromino, unless lockdelay is still active
                    if (lockDelayTimer > lockDelay)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            playingField[Position[j, 0], Position[j, 1]] = Color;
                        }

                        for (int j = 0; j < 4; j++)
                        {
                            if (Position[j, 1] > 19)
                            {
                                return -2;
                            }
                        }

                        return 0;
                    }

                    return 1;
                }
            }
            
            //if yes, move down
            Move(0, -1, playingField);
            return -1;
        }
    }
    
    internal class Program
    {
        //private static List<Task> _playerList = new List<Task>();

        private static int UpdateGhostPiece(ConsoleColor[,] playingField, ref Tetromino ghostPiece, Tetromino moving, bool justSetDown, int left, int bottom)
        {
            Tetromino copy = new Tetromino(ghostPiece);
            ghostPiece = new Tetromino(moving);

            int hardDropPoints = 0;
            while (ghostPiece.Move(0, -1, playingField))
            {
                hardDropPoints += 2;
            }
            
            for (int i = 0; i < 4; i++)
            {
                //check if the spot needs to be overwritten
                if (copy.Position[i, 1] < 20 && !justSetDown &&
                    !PositionContainsBlock(ghostPiece.Position, copy.Position[i, 0], copy.Position[i, 1]) &&
                    !PositionContainsBlock(moving.Position, copy.Position[i, 0], copy.Position[i, 1]))
                {
                    WriteColorAt(copy.Position[i, 0], copy.Position[i, 1], ConsoleColor.White, left, bottom);
                }

                if (ghostPiece.Position[i, 1] < 20 &&
                    (!PositionContainsBlock(copy.Position, ghostPiece.Position[i, 0], ghostPiece.Position[i, 1]) || justSetDown) &&
                    !PositionContainsBlock(moving.Position, ghostPiece.Position[i, 0], ghostPiece.Position[i, 1]))
                {
                    WriteColorAt(ghostPiece.Position[i, 0], ghostPiece.Position[i, 1], ConsoleColor.DarkGray, left, bottom);
                }
            }

            return hardDropPoints;
        }

        //only works for Tetrominoes in spawn position
        private static void DrawTetrominoIsolated(int left, int top, Tetromino tet, bool gray = false)
        {
            for (int row = 0; row < 2; row++)
            {
                Console.SetCursorPosition(left, top + row);
                for (int i = 3; i < 7; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (tet.Position[j, 0] == i && tet.Position[j, 1] == 21 - row)
                        {
                            Console.BackgroundColor = (ConsoleColor)tet.Color;
                            if (gray)
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                            }

                            break;
                        }
                    }

                    Console.Write("  ");
                    Console.ResetColor();
                }
            }
        }

        static bool ClearLine(int line, ref ConsoleColor[,] playingField, int left, int bottom)
        {
            //check if line is filled
            for (int i = 0; i < 10; i++)
            {
                if (playingField[i, line] == ConsoleColor.White)
                {
                    return false;
                }
            }
            
            //if no, move all lines above down
            bool emptyLine = false;

            for (int i = line; i < 20 || !emptyLine; i++)
            {
                emptyLine = true;
                
                for (int j = 0; j < 10; j++)
                {
                    if (playingField[j, i] != ConsoleColor.White)
                    {
                        emptyLine = false;
                    }
                    
                    WriteColorAt(j, i, (ConsoleColor) playingField[j, i + 1], left, bottom);
                    playingField[j, i] = playingField[j, i + 1];
                }
            }

            return true;
        }

        static bool PositionContainsBlock(int[,] pos, int x, int y)
        {
            for (int i = 0; i < pos.GetLength(0); i++)
            {
                if (pos[i, 0] == x && pos[i, 1] == y)
                {
                    return true;
                }
            }

            return false;
        }

        static void WriteColorAt(int x, int y, ConsoleColor color, int left, int bottom)
        {
            Console.SetCursorPosition(left + 2*x, bottom - y);
            Console.BackgroundColor = color;
            Console.Write("  ");
            Console.ResetColor();
            Console.SetCursorPosition(0, bottom + 2);
        }

        static ConsoleColor GrabBagElement(ref List<ConsoleColor> bag, Random rnd)
        {
            if (bag.Count == 0)
            {
                bag = new List<ConsoleColor> { ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.DarkRed, ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Magenta };
                //shuffle elements
                int n = 7;
                while (n > 1)
                {
                    n--;
                    int k = rnd.Next(n + 1);
                    (bag[n], bag[k]) = (bag[k], bag[n]);
                }
            }

            ConsoleColor value = bag[0];
            bag.RemoveAt(0);
            return value;
        }
        
        static bool ListContains(List<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Main(string[] args)
        {
            int leftPlayingField;
            int bottomPlayingField;
            bool canHold = true;
            Tetromino moving = new Tetromino();
            Tetromino copy;
            Tetromino held = new Tetromino();
            held.Position = null;
            Tetromino[] next = new Tetromino[5];
            Tetromino ghostPiece = new Tetromino();
            Random rnd = new Random();
            List<ConsoleColor> tetrominoBag = new List<ConsoleColor>();
            int score = 0;
            int hardDropPoints;
            bool justSetDown = true;
            rnd = new Random();
            ConsoleColor[,] playingField = new ConsoleColor[10, 24];    //main playing matrix that keeps track of where blocks have been set and what color they are
            bool softDropping = false;
            int frameCount = 0;
            int secondCount = 0;
            int framesPerLine = 100;
            int lockDelay = 50;
            int lockDelayTimer = 0;
            bool breakOutOfGame = false;
            
            void AfterSetDown()
            {
                List<int> uniqueLines = new List<int>();
                int clearedLines = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (!ListContains(uniqueLines, copy.Position[i, 1]))
                    {
                        uniqueLines.Add(copy.Position[i, 1]);
                    }
                }

                uniqueLines.Sort();
                foreach (int i in uniqueLines)
                {
                    if (ClearLine(i - clearedLines, ref playingField, leftPlayingField, bottomPlayingField))
                    {
                        clearedLines++;
                    }
                }

                moving = new Tetromino(next[0]);
                for (int i = 0; i < 4; i++)
                {
                    next[i] = new Tetromino(next[i + 1]);
                }
                next[4].Generate(GrabBagElement(ref tetrominoBag, rnd));

                for (int j = 0; j < 5; j++)
                {
                    DrawTetrominoIsolated(leftPlayingField + 23, bottomPlayingField - 13 + 3 * j, next[j]);
                }
            
                canHold = true;
                if (held.Position != null)
                {
                    DrawTetrominoIsolated(leftPlayingField + 23, bottomPlayingField - 18, held);
                }

                switch (clearedLines)
                {
                    case 1:
                        score += 100;
                        break;
                    
                    case 2:
                        score += 400;
                        break;
                
                    case 3:
                        score += 900;
                        break;
                
                    case 4:
                        score += 2000;
                        break;
                }

                if (clearedLines > 0)
                {
                    Console.SetCursorPosition(leftPlayingField + 8, bottomPlayingField - 22);
                    Console.Write(score.ToString("D8"));
                }
                
                hardDropPoints = UpdateGhostPiece(playingField, ref ghostPiece, moving, true, leftPlayingField, bottomPlayingField);
                if (justSetDown)
                {
                    justSetDown = false;
                }
            }
            
            Console.CursorVisible = false;
            Console.Clear();

            if (Console.WindowWidth < 35 || Console.WindowHeight < 27)
            {
                Console.SetCursorPosition(Math.Abs((Console.WindowWidth / 2 - 18) % Console.WindowWidth), Console.WindowHeight/2);
                Console.Write("Window size must be at least 35x27.");
                while (Console.WindowWidth < 35 || Console.WindowHeight < 27) {}
                Thread.Sleep(200);
                Console.Clear();
            }
            
            moving.Generate(GrabBagElement(ref tetrominoBag, rnd));
            ghostPiece = new Tetromino(moving);
            for (int i = 0; i < 5; i++)
            {
                next[i] = new Tetromino();
                next[i].Generate(GrabBagElement(ref tetrominoBag, rnd));
            }

            //setting whole playing field to white, being empty
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    playingField[i, j] = ConsoleColor.White;
                }
            }

            //drawing field for the first time
            Console.WriteLine("\u2554\u2550\u2550\u2550\u2550\u2557  \u2554\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2557\n" +
                              "\u2551TIME\u2551  \u2551 POINTS \u2551\n" + 
                              "\u25510000\u2551  \u255100000000\u2551\n" +
                              "\u255A\u2550\u2550\u2550\u2550\u255D  \u255A\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255D\n" +
                              "\u2554\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2557" +
                              "\u2554\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2557");

            //start drawing playing field and save it's position
            leftPlayingField = Console.CursorLeft + 1; //change 1 to how many characters right of the border the playing field is, if you want to change the GUI
            bottomPlayingField = Console.CursorTop + 19;
            for (int i = 0; i < 20; i++)
            {
                Console.Write("\u2551");
                Console.BackgroundColor = ConsoleColor.White;
                for (int j = 0; j < 20; j++)
                {
                    Console.Write(" ");
                }
                Console.ResetColor();
                Console.Write("\u2551\u2551");
                //box for upcoming tetrominoes on the right
                for (int j = 0; j < 10; j++)
                {
                    Console.Write(" ");
                }
                Console.WriteLine("\u2551");
            }
            Console.WriteLine("\u255A\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255D" +
                              "\u255A\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255D");
            
            //detailing box on the right
            Console.SetCursorPosition(leftPlayingField + 22, bottomPlayingField - 19);
            Console.Write("HELD PIECE");
            Console.SetCursorPosition(leftPlayingField + 21, bottomPlayingField - 16);
            Console.Write("\u255A\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255D");
            Console.SetCursorPosition(leftPlayingField + 21, bottomPlayingField - 15);
            Console.Write("\u2554\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2557");
            Console.SetCursorPosition(leftPlayingField + 22, bottomPlayingField - 14);
            Console.Write("   NEXT   ");
            for (int i = 0; i < 5; i++)
            {
                DrawTetrominoIsolated(leftPlayingField + 23, bottomPlayingField - 13 + 3 * i, next[i]);
            }

            //making the theme play
            SoundPlayer theme = new(AppDomain.CurrentDomain.BaseDirectory + "theme.wav");
            theme.PlayLooping();
            
            hardDropPoints = UpdateGhostPiece(playingField, ref ghostPiece, moving, justSetDown, leftPlayingField, bottomPlayingField);
            justSetDown = false;
            
            while (true)
            {
                Thread.Sleep(10);
                
                if (frameCount % 99 == 0)
                {
                    if (secondCount < 9999)
                    {
                        secondCount++;
                    }

                    Console.SetCursorPosition(leftPlayingField, bottomPlayingField - 22);
                    Console.Write(secondCount.ToString("D4"));
                }

                if (frameCount % framesPerLine == 0)
                {
                    copy = new Tetromino(moving);
                    switch (moving.MoveDown(ref playingField, lockDelay, lockDelayTimer))
                    {

                        case -2: //returns if a block was set down above the screen
                        {
                            breakOutOfGame = true;
                            break;
                        }

                        case -1: //returns if the Tetromino was moved down
                        {
                            //reset lock delay timer
                            lockDelayTimer = 0;

                            //removing color of old position from playing field and adding new color
                            for (int i = 0; i < 4; i++)
                            {
                                //check if the spot needs to be overwritten
                                if (copy.Position[i, 1] < 20 &&
                                    !PositionContainsBlock(moving.Position, copy.Position[i, 0],
                                        copy.Position[i, 1]))
                                {
                                    WriteColorAt(copy.Position[i, 0], copy.Position[i, 1], ConsoleColor.White,
                                        leftPlayingField, bottomPlayingField);
                                }

                                if (moving.Position[i, 1] < 20 &&
                                    !PositionContainsBlock(copy.Position, moving.Position[i, 0],
                                        moving.Position[i, 1]))
                                {
                                    WriteColorAt(moving.Position[i, 0], moving.Position[i, 1],
                                        moving.Color, leftPlayingField, bottomPlayingField);
                                }
                            }

                            break;
                        }

                        case 0: //returns if the Tetromino was set down
                        {
                            AfterSetDown();
                            break;
                        }
                    }

                    if (breakOutOfGame)
                    {
                        break;
                    }

                    if (softDropping)
                    {
                        score += 1;
                        Console.SetCursorPosition(leftPlayingField + 8, bottomPlayingField - 22);
                        Console.Write(score.ToString("D8"));
                    }
                }

                framesPerLine = secondCount switch
                {
                    >= 200 => 15,
                    >= 60 => 60 - 10 * (secondCount - 60) / 31,
                    _ => 60 + (secondCount - 60) * (secondCount - 60) / 90
                };

                softDropping = false;

                //input check
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.C:
                        {
                            if (canHold)
                            {
                                canHold = false;
                                copy = new Tetromino(moving);

                                //checking if it was executed for the first time
                                if (held.Position == null)
                                {
                                    held.Generate(moving.Color);
                                    moving = new Tetromino(next[0]);
                                    for (int i = 0; i < 4; i++)
                                    {
                                        next[i] = new Tetromino(next[i + 1]);
                                    }

                                    next[4].Generate(GrabBagElement(ref tetrominoBag, rnd));

                                    //clean up ghostpiece
                                    for (int i = 0; i < 4; i++)
                                    {
                                        WriteColorAt(ghostPiece.Position[i, 0], ghostPiece.Position[i, 1], ConsoleColor.White, leftPlayingField, bottomPlayingField);
                                    }
                                    
                                    hardDropPoints = UpdateGhostPiece(playingField, ref ghostPiece, moving, true, leftPlayingField, bottomPlayingField);

                                    for (int i = 0; i < 5; i++)
                                    {
                                        DrawTetrominoIsolated(leftPlayingField + 23, bottomPlayingField - 13 + 3 * i,
                                            next[i]);
                                    }
                                }

                                else
                                {
                                    ConsoleColor colorCopy = moving.Color;
                                    moving = new Tetromino(held);
                                    held.Generate(colorCopy);
                                }

                                //clean up position of previously moving and ghost piece
                                for (int i = 0; i < 4; i++)
                                {
                                    if (copy.Position[i, 1] < 20)
                                    {
                                        WriteColorAt(copy.Position[i, 0], copy.Position[i, 1], ConsoleColor.White,
                                            leftPlayingField, bottomPlayingField);
                                    }

                                    WriteColorAt(ghostPiece.Position[i, 0], ghostPiece.Position[i, 1],
                                        ConsoleColor.White, leftPlayingField, bottomPlayingField);
                                }

                                DrawTetrominoIsolated(leftPlayingField + 23, bottomPlayingField - 18, held, true);

                                justSetDown = true;
                            }

                            UpdateGhostPiece(playingField, ref ghostPiece, moving, justSetDown, leftPlayingField,
                                bottomPlayingField);
                            if (justSetDown)
                            {
                                justSetDown = false;
                            }

                            break;
                        }

                        //harddrop
                        case ConsoleKey.Spacebar:
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (moving.Position[i, 1] < 20)
                                {
                                    WriteColorAt(moving.Position[i, 0], moving.Position[i, 1], ConsoleColor.White, leftPlayingField, bottomPlayingField);
                                }
                            }

                            for (int i = 0; i < 4; i++)
                            {
                                if (ghostPiece.Position[i, 1] < 20)
                                {
                                    WriteColorAt(ghostPiece.Position[i, 0], ghostPiece.Position[i, 1], moving.Color, leftPlayingField, bottomPlayingField);
                                }
                            }

                            moving = new Tetromino(ghostPiece);
                            score += hardDropPoints;
                            Console.SetCursorPosition(leftPlayingField + 8, bottomPlayingField - 22);
                            Console.Write(score.ToString("D8"));

                            copy = new Tetromino(moving);
                            if (moving.MoveDown(ref playingField, lockDelay, lockDelay + 1) == -2)
                            {
                                breakOutOfGame = true;
                            }
                            
                            AfterSetDown();

                            break;
                        }

                        case ConsoleKey.LeftArrow:
                        {
                            copy = new Tetromino(moving);
                            if (moving.Move(-1, 0, playingField))
                            {
                                //removing color of old position from playing field and adding new color
                                for (int i = 0; i < 4; i++)
                                {
                                    //check if the spot needs to be overwritten
                                    if (copy.Position[i, 1] < 20 &&
                                        !PositionContainsBlock(moving.Position, copy.Position[i, 0], copy.Position[i, 1]))
                                    {
                                        WriteColorAt(copy.Position[i, 0], copy.Position[i, 1], ConsoleColor.White,
                                            leftPlayingField, bottomPlayingField);
                                    }

                                    if (moving.Position[i, 1] < 20 &&
                                        !PositionContainsBlock(copy.Position, moving.Position[i, 0], moving.Position[i, 1]))
                                    {
                                        WriteColorAt(moving.Position[i, 0], moving.Position[i, 1],
                                            moving.Color, leftPlayingField, bottomPlayingField);
                                    }
                                }
                            }

                            hardDropPoints = UpdateGhostPiece(playingField, ref ghostPiece, moving, justSetDown, leftPlayingField, bottomPlayingField);
                            break;
                        }
                        
                        case ConsoleKey.RightArrow:
                        {
                            copy = new Tetromino(moving);
                            if (moving.Move(1, 0, playingField))
                            {
                                //removing color of old position from playing field and adding new color
                                for (int i = 0; i < 4; i++)
                                {
                                    //check if the spot needs to be overwritten
                                    if (copy.Position[i, 1] < 20 &&
                                        !PositionContainsBlock(moving.Position, copy.Position[i, 0], copy.Position[i, 1]))
                                    {
                                        WriteColorAt(copy.Position[i, 0], copy.Position[i, 1], ConsoleColor.White,
                                            leftPlayingField, bottomPlayingField);
                                    }

                                    if (moving.Position[i, 1] < 20 &&
                                        !PositionContainsBlock(copy.Position, moving.Position[i, 0], moving.Position[i, 1]))
                                    {
                                        WriteColorAt(moving.Position[i, 0], moving.Position[i, 1],
                                            moving.Color, leftPlayingField, bottomPlayingField);
                                    }
                                }
                            }

                            hardDropPoints = UpdateGhostPiece(playingField, ref ghostPiece, moving, justSetDown, leftPlayingField, bottomPlayingField);
                            break;
                        }
                        
                        case ConsoleKey.DownArrow:
                            softDropping = true;
                            framesPerLine = 1;
                            break;
                        
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.X:
                        {
                            copy = new Tetromino(moving);
                            moving.Rotate(true, playingField);
                            //painting
                            for (int i = 0; i < 4; i++)
                            {
                                //check if the spot needs to be overwritten
                                if (copy.Position[i, 1] < 20 &&
                                    !PositionContainsBlock(moving.Position, copy.Position[i, 0], copy.Position[i, 1]))
                                {
                                    WriteColorAt(copy.Position[i, 0], copy.Position[i, 1], ConsoleColor.White, leftPlayingField, bottomPlayingField);
                                }

                                if (moving.Position[i, 1] < 20 &&
                                    !PositionContainsBlock(copy.Position, moving.Position[i, 0], moving.Position[i, 1]))
                                {
                                    WriteColorAt(moving.Position[i, 0], moving.Position[i, 1], moving.Color, leftPlayingField, bottomPlayingField);
                                }
                            }

                            hardDropPoints = UpdateGhostPiece(playingField, ref ghostPiece, moving, justSetDown, leftPlayingField, bottomPlayingField);
                            break;
                        }

                        case ConsoleKey.Z:
                        case ConsoleKey.Y:
                        {
                            copy = new Tetromino(moving);
                            moving.Rotate(false, playingField);
                            //painting
                            for (int i = 0; i < 4; i++)
                            {
                                //check if the spot needs to be overwritten
                                if (copy.Position[i, 1] < 20 &&
                                    !PositionContainsBlock(moving.Position, copy.Position[i, 0], copy.Position[i, 1]))
                                {
                                    WriteColorAt(copy.Position[i, 0], copy.Position[i, 1], ConsoleColor.White, leftPlayingField, bottomPlayingField);
                                }

                                if (moving.Position[i, 1] < 20 &&
                                    !PositionContainsBlock(copy.Position, moving.Position[i, 0], moving.Position[i, 1]))
                                {
                                    WriteColorAt(moving.Position[i, 0], moving.Position[i, 1], moving.Color, leftPlayingField, bottomPlayingField);
                                }
                            }
                            
                            hardDropPoints = UpdateGhostPiece(playingField, ref ghostPiece, moving, justSetDown, leftPlayingField, bottomPlayingField);
                            break;
                        }
                    }
                }

                if (breakOutOfGame)
                {
                    break;
                }

                frameCount++;
                lockDelayTimer++;
            }
            
            Console.SetCursorPosition(leftPlayingField + 6, bottomPlayingField - 9);
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < 9; i++)
            {
                Console.BackgroundColor = (ConsoleColor)playingField[3 + i/2, 9];
                Console.Write("GAME OVER"[i]);
            }
            
            theme.Stop();
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, bottomPlayingField + 2);
            Console.WriteLine("Enter anything to close the program.");
            Thread.Sleep(3000);
            Console.ReadKey(true);
        }
    }
}