using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp1
{
    // Defines the ship data type.
    public struct ship
    {
        public int column, row, direction, length;

        public ship(int column, int row, int direction, int length)
        {
            this.column = column;
            this.row = row;
            this.direction = direction;
            this.length = length;
        }
    }


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // These are the "global" variables used throughout.
        private string[,] playerShipArray = new string[10, 10];
        private string[,] aiShipArray = new string[10, 10];
        private string[,] playerHitsAndMisses = new string[10, 10];
        private string[,] aiHitsAndMisses = new string[10, 10];

        private ship[] allShips = new ship[5];
        private ship currentShip;
        private int currentShipCounter = 0;

        private bool shipPlacing = false;
        private bool gameRunning = false;

        // Change this to true to see where the opposing team ships are. There are bugs in this mode however I felt it unnecesary to fix when you are not supposed to use this.
        private bool cheat = false;

        // Each ship and their starting stats.
        private ship destroyer1 = new ship(0, 0, 0, 0);
        private ship destroyer2 = new ship(0, 0, 0, 0);
        private ship submarine1 = new ship(0, 0, 0, 1);
        private ship submarine2 = new ship(0, 0, 0, 1);
        private ship carrier1 = new ship(0, 0, 0, 2);

        // When the program runs, this is called.
        // This subroutine sets the background to white, and creates menu text.
        private void Form1_Load(object sender, EventArgs e)
        {
            Graphics GR = CreateGraphics();
            Font font = new Font(this.Font, FontStyle.Regular);
            GR.FillRectangle(Brushes.White, 0, 0, 1000, 1000);

            ship[] Ships = { destroyer1, destroyer2, submarine1, submarine2, carrier1 };
            allShips = Ships;
            currentShip = allShips[currentShipCounter];

            GR.DrawString("1: Draw", font, Brushes.Black, 20, 400);
            GR.DrawString("2: Clear", font, Brushes.Black, 100, 400);
            GR.DrawString("3: Start Game", font, Brushes.Black, 180, 400);
            GR.DrawString("4: Load Game", font, Brushes.Black, 300, 400);
        }

        //This creates the grids the game is played on. It iterates through each column and row and draws a line for each one. 
        private void Draw()
        {
            Graphics GR = CreateGraphics();
            Pen penTest = new Pen(Brushes.Black);
            Font font = new Font(Font, FontStyle.Regular);

            for (int row = 0; row < 10; row++)
            {
                GR.DrawLine(penTest, 20, 20 + row * 35, 370, 20 + row * 35);
                GR.DrawLine(penTest, 428, 20 + row * 35, 778, 20 + row * 35);
                GR.DrawString(Convert.ToString(row), font, Brushes.Black, 5, 18 + row * 35);
                GR.DrawString(Convert.ToString(row), font, Brushes.Black, 413, 18 + row * 35);
            }
            GR.DrawLine(penTest, 20, 370, 370, 370);
            GR.DrawLine(penTest, 428, 370, 778, 370);

            for (int column = 0; column < 10; column++)
            {
                GR.DrawLine(penTest, 20 + column * 35, 20, 20 + column * 35, 370);
                GR.DrawLine(penTest, 428 + column * 35, 20, 428 + column * 35, 370);
                GR.DrawString(Convert.ToString(Convert.ToChar(column + 65)), font, Brushes.Black, 31 + column * 35, 0);
                GR.DrawString(Convert.ToString(Convert.ToChar(column + 65)), font, Brushes.Black, 439 + column * 35, 0);
            }
            GR.DrawLine(penTest, 370, 20, 370, 370);
            GR.DrawLine(penTest, 778, 20, 778, 370);

            DrawX(0, 0, playerHitsAndMisses, false, false, false);

            // This should only draw if the game has started as prior there is no shots fired.
            if (gameRunning)
            {
                DrawX(0, 0, aiHitsAndMisses, false, true, true);
                DrawX(0, 0, playerHitsAndMisses, false, true, false);
                GR.FillRectangle(Brushes.White, 400, 400, 600, 450);
                GR.DrawString("Fire shots at your opponent.", font, Brushes.Black, 520, 400);
            }
            else
            {
                currentShip = allShips[currentShipCounter];
                GR.FillRectangle(Brushes.White, 350, 450, 600, 450);
                GR.DrawString("Place ships on your board using arrow keys.", font, Brushes.Black, 470, 400);
                GR.DrawString("Enter = Place, Control = Rotate", font, Brushes.Black, 470, 420);
            }
        }

        // This subroutine scans both the tempArray and the shipArray for "x" or "a" to display it on the screen.
        private void DrawX(int j, int k, string[,] hitsAndMisses, bool rotate, bool fire, bool correction)
        {
            Graphics GR = CreateGraphics();
            Font font = new Font(Font, FontStyle.Regular);
            int boardCorrection = 0;

            // This draws an input letter in a required location. This is used to display hits and misses, as well as the ships and your cursor.
            void drawLetter(Graphics GR, Font font, Brush colour, string letter, int column, int row)
            {
                GR.FillRectangle(Brushes.White, 21 + boardCorrection + column * 35, 21 + row * 35, 33, 33);
                GR.DrawString(letter, font, colour, 32 + boardCorrection + column * 35, 27 + row * 35);
            }

            // If the board being used is the enemies, the values should be "corrected" in position to the right hand board.
            if (correction)
            {
                boardCorrection = 408;
            }    

            // If you are not shooting at an opponent, display the ships and cursor on the left hand side and display any collisions of the cursor against the ships.
            if (!fire)
            {
                for (int i = 0; i <= currentShip.length; i++)
                {
                    if (rotate)
                    {
                        if (currentShip.direction == 0)
                        {
                            GR.FillRectangle(Brushes.White, 21 + currentShip.column * 35, 21 + (currentShip.row + i) * 35, 33, 33);
                        }
                        else if (currentShip.direction == 90)
                        {
                            GR.FillRectangle(Brushes.White, 21 + (currentShip.column + i) * 35, 21 + currentShip.row * 35, 33, 33);
                        }
                    }

                    if (currentShip.direction == 0)
                    {
                        if (j == -1 && i == currentShip.length)
                        {
                            GR.FillRectangle(Brushes.White, 21 + (currentShip.column + i - j) * 35, 21 + currentShip.row * 35, 33, 33);
                        }
                        else if (j == 1 && i == 0)
                        {
                            GR.FillRectangle(Brushes.White, 21 + (currentShip.column - j) * 35, 21 + currentShip.row * 35, 33, 33);
                        }
                        else
                        {
                            GR.FillRectangle(Brushes.White, 21 + (currentShip.column + i) * 35, 21 + (currentShip.row - k) * 35, 33, 33);
                        }
                        drawLetter(GR, font, Brushes.Black, "x", currentShip.column + i, currentShip.row);
                    }
                    else if (currentShip.direction == 90)
                    {
                        if (k == -1 && i == currentShip.length)
                        {
                            GR.FillRectangle(Brushes.White, 21 + currentShip.column * 35, 21 + (currentShip.row + i - k) * 35, 33, 33);
                        }
                        else if (k == 1 && i == 0)
                        {
                            GR.FillRectangle(Brushes.White, 21 + currentShip.column * 35, 21 + (currentShip.row - k) * 35, 33, 33);
                        }
                        else
                        {
                            GR.FillRectangle(Brushes.White, 21 + (currentShip.column - j) * 35, 21 + (currentShip.row + i) * 35, 33, 33);
                        }
                        drawLetter(GR, font, Brushes.Black, "x", currentShip.column, currentShip.row + i);
                    }
                }

                for (int column = 0; column < playerShipArray.GetLength(0); column++)
                {
                    for (int row = 0; row < playerShipArray.GetLength(0); row++)
                    {
                        if (playerShipArray[column, row] == "x")
                        {
                            drawLetter(GR, font, Brushes.Black, "x", column, row);
                        }
                    }
                }

                bool validate = true;
                int[] relativePositionArray = CheckShipCollision(ref validate, playerShipArray);
                if (!validate)
                {
                    foreach (int relativePosition in relativePositionArray)
                    {
                        if (currentShip.direction == 0)
                        {
                            drawLetter(GR, font, Brushes.Red, "x", currentShip.column + relativePosition, currentShip.row);
                        }
                        else if (currentShip.direction == 90)
                        {
                            drawLetter(GR, font, Brushes.Red, "x", currentShip.column, currentShip.row + relativePosition);
                        }
                    }
                }
            }
            // Else, if cheating show the opponents ships. Then draw the hits and misses on your board. Then draw the hits and misses on your opponents board.
            else
            {
                if (cheat)
                {
                    for (int column = 0; column < aiShipArray.GetLength(0); column++)
                    {
                        for (int row = 0; row < aiShipArray.GetLength(0); row++)
                        {
                            if (aiShipArray[column, row] == "x")
                            {
                                drawLetter(GR, font, Brushes.Green, "x", column, row);
                            }
                        }
                    }
                    if (j == -1 || j == 1)
                    {
                        GR.FillRectangle(Brushes.White, 21 + boardCorrection + (currentShip.column - j) * 35, 21 + currentShip.row * 35, 33, 33);
                    }
                    else
                    {
                        GR.FillRectangle(Brushes.White, 21 + boardCorrection + currentShip.column * 35, 21 + (currentShip.row - k) * 35, 33, 33);
                    }
                    drawLetter(GR, font, Brushes.Black, "x", currentShip.column, currentShip.row);
                }

                for (int column = 0; column < hitsAndMisses.GetLength(0); column++)
                {
                    for (int row = 0; row < hitsAndMisses.GetLength(0); row++)
                    {
                        if (hitsAndMisses[column, row] == "H")
                        {
                            drawLetter(GR, font, Brushes.Blue, "H", column, row);
                        }
                        else if (hitsAndMisses[column, row] == "M")
                        {
                            drawLetter(GR, font, Brushes.Red, "M", column, row);
                        }
                    }
                }

                if (j == -1 || j == 1)
                {
                    GR.FillRectangle(Brushes.White, 21 + boardCorrection + (currentShip.column - j) * 35, 21 + currentShip.row * 35, 33, 33);
                    if (hitsAndMisses[currentShip.column - j, currentShip.row] == "H")
                    {
                        drawLetter(GR, font, Brushes.Blue, "H", currentShip.column - j, currentShip.row);
                    }
                    else if (hitsAndMisses[currentShip.column - j, currentShip.row] == "M")
                    {
                        drawLetter(GR, font, Brushes.Red, "M", currentShip.column - j, currentShip.row);
                    }
                }
                else
                {
                    // This should only run on the players turn, not the opponents. This is because it will cause an error where it deletes the top left cell on the players board when being called by the ai the first time.
                    if (correction) GR.FillRectangle(Brushes.White, 21 + boardCorrection + currentShip.column * 35, 21 + (currentShip.row - k) * 35, 33, 33);

                    if (hitsAndMisses[currentShip.column, currentShip.row - k] == "H")
                    {
                        drawLetter(GR, font, Brushes.Blue, "H", currentShip.column, currentShip.row - k);
                    }
                    else if (hitsAndMisses[currentShip.column, currentShip.row - k] == "M")
                    {
                        drawLetter(GR, font, Brushes.Red, "M", currentShip.column, currentShip.row - k);
                    }
                }

                // As a correction only happens on the players turn (due to the position of the boards) and we only need to draw the cursor on the players turn, we ask whether correction is true.
                if (correction)
                {
                    drawLetter(GR, font, Brushes.Black, "x", currentShip.column, currentShip.row);
                }
            }
        }

        // This paints over everything with white to get an empty screen.
        private void ClearScreen()
        {
            Graphics GR = CreateGraphics();

            GR.FillRectangle(Brushes.White, 0, 0, 800, 390);
        }

        // This takes user arrow keys and tries to move the current "x" center point accordingly.
        private void MoveCursor(string key, bool fire, bool correction)
        {
            // If you pushed control, this equation will change the angle of your ship to 90 degrees if 0, and 0 degreed if 90.
            void Rotate()
            {
                currentShip.direction = 90 - currentShip.direction;
            }

            // Move the cursor in the correct direction. Check the validity of the new position and if it fails return the cursor to its prior position. Then draw the new position.
            void MoveX(int j, int k, bool rotate)
            {
                currentShip.column += j;
                currentShip.row += k;
                if (!ValidateShip())
                {
                    currentShip.column -= j;
                    currentShip.row -= k;

                    if (rotate)
                    {
                        Rotate();
                        rotate = false;
                    }
                }

                DrawX(j, k, aiHitsAndMisses, rotate, fire, correction);
            }

            // This asks whether the ship is valid by asking if it is within the bounds of the array.
            bool ValidateShip()
            {
                if (currentShip.column < 0 || currentShip.row < 0)
                {
                    return false;
                }
                else
                {
                    if (currentShip.direction == 0 && currentShip.column + currentShip.length > playerShipArray.GetLength(0) - 1 || currentShip.row > playerShipArray.GetLength(1) - 1)
                    {
                        return false;
                    }
                    else if (currentShip.direction == 90 && currentShip.row + currentShip.length > playerShipArray.GetLength(0) - 1 || currentShip.column > playerShipArray.GetLength(1) - 1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            switch (key)
            {
                case "down":
                    {
                        MoveX(0, 1, false);
                        break;
                    }

                case "up":
                    {
                        MoveX(0, -1, false);
                        break;
                    }

                case "left":
                    {
                        MoveX(-1, 0, false);
                        break;
                    }

                case "right":
                    {
                        MoveX(1, 0, false);
                        break;
                    }
                default:
                    {
                        Rotate();
                        MoveX(0, 0, true);
                        break;
                    }
            }
        }

        // This copies the contents of the temporary array to the shipArray, storing it perminantly.
        private void AssignArray(ref string[,] shipArray, bool ai)
        {
            bool validate = true;
            CheckShipCollision(ref validate, shipArray);

            // This asks whether the ship collided with another, and if it didn't it assigns the ship or cursor just placed into the given array.
            if (validate)
            {
                for (int i = 0; i <= currentShip.length; i++)
                {
                    if (currentShip.direction == 0)
                    {
                        shipArray[currentShip.column + i, currentShip.row] = "x";
                    }
                    else if (currentShip.direction == 90)
                    {
                        shipArray[currentShip.column, currentShip.row + i] = "x";
                    }
                }

                // Firstly makes sure the method isn't called by the ai, then if it isn't cycles through which ship is currently in use.
                if (!ai)
                {
                    // If the ship currently on is outside the amount of ships you can place, the game begins. Otherwise, cycle through to the next ship.
                    currentShipCounter++;
                    if (currentShipCounter >= allShips.Length)
                    {
                        shipPlacing = false;
                        gameRunning = true;
                        PlayGame();
                    }
                    else
                    {
                        currentShip = allShips[currentShipCounter];
                    }
                }
            }
        }

        // This checks if any ships collide.
        private int[] CheckShipCollision(ref bool validate, string[,] shipArray)
        {
            // It does this by taking every position of the ship trying to be placed and checks if it matches any already placed ships.
            int[] relativePositionArray = new int[0];
            validate = true;
            for (int i = 0; i <= currentShip.length; i++)
            {
                if (currentShip.direction == 0)
                {
                    if (shipArray[currentShip.column + i, currentShip.row] == "x")
                    {
                        validate = false;
                        relativePositionArray = relativePositionArray.Append(i).ToArray();
                    }
                }
                else if (currentShip.direction == 90)
                {
                    if (shipArray[currentShip.column, currentShip.row + i] == "x")
                    {
                        validate = false;
                        relativePositionArray = relativePositionArray.Append(i).ToArray();
                    }
                }
            }
            return relativePositionArray;
        }

        // This randomly generates the AI's ships positions, then validates them. If they can't exist, a new ship is placed with the same check.
        private void AssignAiShips()
        {
            Random randInt = new Random();
            for (int currentShipCounterAI = 0; currentShipCounterAI < allShips.Length; currentShipCounterAI++)
            {
                currentShip = allShips[currentShipCounterAI];
                
                bool validate = false;

                while (!validate) 
                {
                    int randomColumn = randInt.Next(0, aiShipArray.GetLength(0));
                    int randomRow = randInt.Next(0, aiShipArray.GetLength(1));
                    int randomDirection = randInt.Next(0, 2) * 90;

                    currentShip.column = randomColumn;
                    currentShip.row = randomRow;
                    currentShip.direction = randomDirection;

                    // This checks if the ship extends out of the array and ignores the suggested values if it does.
                    if (currentShip.direction == 0)
                    {
                        if (currentShip.column + currentShip.length >= aiShipArray.GetLength(0))
                        {
                            continue;
                        }
                    }
                    else if (currentShip.direction == 90)
                    {
                        if (currentShip.row + currentShip.length >= aiShipArray.GetLength(1))
                        {
                            continue;
                        }
                    }

                    // This checks for any collisions in the ships.
                    CheckShipCollision(ref validate, aiShipArray);
                }

                // If all validation passes, the ships are placed.
                AssignArray(ref aiShipArray, true);
            }
        }

        // Asks whether the cursor hits a ship and assigns a hit or miss accordingly.
        private void Fire(string[,] shipArray, string[,] hitsAndMisses, int x, int y)
        {
            if (shipArray[x, y] == "x")
            {
                hitsAndMisses[x, y] = "H";
            }
            else
            {
                hitsAndMisses[x, y] = "M";
            }
        }

        // This takes the AI's turn. It randomly fires making sure not to shoot the same spot twice.
        private void AiTurn()
        {
            Random randomInt = new Random();

            int randomX;
            int randomY;

            do
            {
                randomX = randomInt.Next(0, 10);
                randomY = randomInt.Next(0, 10);
            }
            while (playerHitsAndMisses[randomX, randomY] != null);

            Fire(playerShipArray, playerHitsAndMisses, randomX, randomY);
            DrawX(0, 0, playerHitsAndMisses, false, true, false);
        }

        // This either starts the ship placing, or starts the game.
        private void PlayGame()
        {
            // When the user asks to start the game, it starts them placing ships. To begin this, it draws your cursor.
            if (!gameRunning)
            {
                if (!shipPlacing)
                {
                    DrawX(0, 0, null, false, false, false);
                    shipPlacing = true;
                }
            }
            // When finishing placing ships this is called again, and initialises the game starting. It assigns the ai ships, recreates the cursor, plays a turn for the ai then draws your cursor.
            else
            {
                AssignAiShips();
                currentShip = new ship();

                AiTurn();                
                DrawX(0, 0, aiHitsAndMisses, false, true, true);
            }
        }

        // This calculates if someone won.
        private void CheckIfGameEnds()
        {
            // This counts the total number of ship coordinates.
            int CountTotalShipLengths()
            {
                int totalCount = 0;
                foreach (ship currentShip in allShips)
                {
                    totalCount += currentShip.length + 1;
                }
                return totalCount;
            }

            // Firstly it checks how many ships have been hit.
            int CountPoints(string[,] shipArray, string[,] playerHitOrMiss)
            {
                int count = 0;
                for (int column = 0; column < shipArray.GetLength(0); column++)
                {
                    for (int row = 0; row < shipArray.GetLength(1); row++)
                    {
                        if (shipArray[column, row] == "x" && playerHitOrMiss[column, row] == "H")
                        {
                            count++;
                        }
                    }
                }
                return count;
            }

            // If the ai is playing and they have the same amount of hits as things to hit, they win.
            if (CountPoints(playerShipArray, playerHitsAndMisses) == CountTotalShipLengths())
            {
                MessageBox.Show("Unfortunate, you lost.");
                Environment.Exit(1);
            }
            // If you are playing and have the same number of hits as things to hit, you win.
            else if(CountPoints(aiShipArray, aiHitsAndMisses) == CountTotalShipLengths())
            {
                MessageBox.Show("Congratulations, you won!");
                Environment.Exit(1);
            }
        }

        // This sticks all variables into a csv text file.
        private void SaveGame()
        {
            string saveData = "";

            for (int column = 0; column < aiShipArray.GetLength(0); column++)
            {
                for (int row = 0; row < aiShipArray.GetLength(1); row++)
                {
                    saveData += playerShipArray[column,row] + ",";
                    saveData += playerHitsAndMisses[column, row] + ",";
                    saveData += aiShipArray[column, row] + ",";
                    saveData += aiHitsAndMisses[column, row] + ",";
                }
            }

            saveData += currentShipCounter + ",";
            saveData += shipPlacing + ",";
            saveData += gameRunning;

            File.WriteAllText("previousGame.txt", saveData);
        }

        // This reads all variables from a csv text file.
        private void LoadGame()
        {
            string loadData = File.ReadAllText("previousGame.txt");
            string[] loadDataArray = loadData.Split(",");

            // This puts each value in correct position in the arrays they are supposed to be.
            void AssignValue(int index, int column, int row, string[,] array)
            {
                if (loadDataArray[Convert.ToInt32(index)] == "")
                {
                    array[column, row] = null;
                }
                else
                {
                    array[column, row] = loadDataArray[Convert.ToInt32(index)];
                }
            }

            for (int index = 0; index < loadDataArray.Length - 3; index++)
            {
                int row = (index / 4) % playerShipArray.GetLength(0);
                int column = (index / 4) / playerShipArray.GetLength(1);

                if (index % 4 == 0)
                {
                    AssignValue(index, column, row, playerShipArray);
                }
                else if (index % 4 == 1)
                {
                    AssignValue(index, column, row, playerHitsAndMisses);
                }
                else if (index % 4 == 2)
                {
                    AssignValue(index, column, row, aiShipArray);
                }
                else if (index % 4 == 3)
                {
                    AssignValue(index, column, row, aiHitsAndMisses);
                }
            }

            currentShipCounter = Convert.ToInt32(loadDataArray[loadDataArray.Length - 3]);
            shipPlacing = Convert.ToBoolean(loadDataArray[loadDataArray.Length - 2]);
            gameRunning = Convert.ToBoolean(loadDataArray[loadDataArray.Length - 1]);

            // This makes the cursor not show up when drawing until your turn starts.
            currentShip = new ship(0,0,0,-1);

            // This clears the screen then draws the boards and your ships.
            ClearScreen();
            Draw();
            DrawX(0, 0, playerHitsAndMisses, false, false, false);

            // This should only draw if the game has started as prior there is no shots fired.
            if (gameRunning)
            {
                DrawX(0, 0, aiHitsAndMisses, false, true, true);
                DrawX(0, 0, playerHitsAndMisses, false, true, false);
                currentShip = new ship();
            }
            else
            {
                currentShip = allShips[currentShipCounter];
            }
        }

        //This detects key inputs. I would use a switch statement if they worked for keycodes, but they don't.
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // This detects the arrow keys.
            if (e.KeyCode == Keys.D1)
            {
                Draw();
            }
            else if (e.KeyCode == Keys.D2)
            {
                ClearScreen();
            }
            else if (e.KeyCode == Keys.D3 && !gameRunning)
            {
                PlayGame();
            }
            else if (e.KeyCode == Keys.D4)
            {
                LoadGame();
            }

            // If you are placing ships, do these commands.
            if (shipPlacing)
            {
                if (e.KeyCode == Keys.Down)
                {
                    MoveCursor("down", false, false);
                }

                else if (e.KeyCode == Keys.Up)
                {
                    MoveCursor("up", false, false);
                }

                else if (e.KeyCode == Keys.Left)
                {
                    MoveCursor("left", false, false);
                }

                else if (e.KeyCode == Keys.Right)
                {
                    MoveCursor("right", false, false);
                }

                else if (e.KeyCode == Keys.ControlKey)
                {
                    MoveCursor("", false, false);
                }

                else if (e.KeyCode == Keys.Enter)
                {
                    AssignArray(ref playerShipArray, false);
                    SaveGame();
                    if (!gameRunning) DrawX(0, 0, playerHitsAndMisses, false, false, false); 
                }
            }
            // Otherwise, if the game is in progress do these instead.
            else if (gameRunning)
            {
                if (e.KeyCode == Keys.Down)
                {
                    MoveCursor("down", true, true);
                }

                else if (e.KeyCode == Keys.Up)
                {
                    MoveCursor("up", true, true);
                }

                else if (e.KeyCode == Keys.Left)
                {
                    MoveCursor("left", true, true);
                }

                else if (e.KeyCode == Keys.Right)
                {
                    MoveCursor("right", true, true);
                }

                else if (e.KeyCode == Keys.ControlKey)
                {
                    MoveCursor("", true, true);
                }

                else if (e.KeyCode == Keys.Enter)
                {
                    // Checks if the user already tried to fire there. If they have, nothing happens.
                    if (aiHitsAndMisses[currentShip.column, currentShip.row] == null)
                    {
                        Fire(aiShipArray, aiHitsAndMisses, currentShip.column, currentShip.row);
                        DrawX(0, 0, aiHitsAndMisses, false, true, true);
                        AiTurn();
                        CheckIfGameEnds();
                        SaveGame();
                    }
                }
            }
            // If neither of them are active, the player must start teh game by pushing "3".
        }
    }
}
