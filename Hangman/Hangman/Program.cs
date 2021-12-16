using System;
using System.Text;
using static System.Console;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace Hangman
{
    
    class Program
    {
        static string[] secretWord1 = { "Stockholm", "Oslo", "Tokyo", "Canberra", "Nairobi","obsessions" };
        static string[] secretWord2 = new string[] { };
        static char[] correctLetters = new char[] { };
        static string chosenWord = " ";
        static bool[] bools = { true, true }; //(running,firstWrong)
        

        static void Main(string[] args)
        {
            ShowFirstMenu();
            PlayGame();
        }

        //User gets to choose between loading secret words
        //from an array of strings or from an text file
        private static void ShowFirstMenu()
        {
            bool correctChoice = false;
            int readFrom = 0;

            string allText = " ";
            bool[] bools = { true, true }; //(running,firstWrong)

            //First menu
            while (!correctChoice)
            {
                WriteLine(" Welcome to Hangman");
                WriteLine("Choose one of the following.\n");
                WriteLine("1: Read in words from an array of strings.");
                WriteLine("2: Read in words from a textfile.\n");
                Write("Enter your choice: ");

                bool choice = Int32.TryParse(ReadLine(), out readFrom);
                if (choice && readFrom > 0 && readFrom < 3)
                {

                    correctChoice = true;
                    if (readFrom == 1)
                    {
                        chosenWord = ChooseSecretWord(secretWord1);
                        correctLetters = new char[chosenWord.Length];
                        correctLetters = InitializeCorrectLetters(correctLetters, chosenWord.Length);

                    }
                    else if (readFrom == 2) // File location : project directory/Data
                    {                       // File properties : build action = content, copy always to output directory  
                        try
                        {
                            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\RandomWords.txt");
                            allText = File.ReadAllText(path);
                        }
                        catch (FileNotFoundException e)
                        {
                            WriteLine("File not found: " + e.ToString());
                            bools[0] = false;
                        }
                        catch (DirectoryNotFoundException e)
                        {
                            WriteLine("Directory not found: " + e.ToString());
                        }
                        secretWord2 = allText.Split(',');
                        chosenWord = ChooseSecretWord(secretWord2);
                        correctLetters = new char[chosenWord.Length];
                        correctLetters = InitializeCorrectLetters(correctLetters, chosenWord.Length);
                    }
                }
                else
                {
                    Clear();
                    SetCursorPosition(0, 9);
                    WriteLine("Invalid input.");
                    SetCursorPosition(0, 0);
                }
            }
        }

        //User makes guesses of either a letter or the whole word
        //until he either guesses right and wins or he uses all his
        //guesses and looses
        private static void PlayGame()
        {
            bool GuessIsNew = true;
            int guessesLeft = 10;
            StringBuilder incorrectLetters = new StringBuilder(guessesLeft);
            string userInputString = " ";
            int neededGuesses = 0;

            while (bools[0])
                {

                    Clear();
                    ShowInfo();
                    if (!GuessIsNew)
                        WriteLine("You have already made that guess.\n");
                    DisplayOutput(correctLetters, guessesLeft, incorrectLetters);
                    userInputString = GetUserInput(chosenWord);
                    GuessIsNew = NewGuess(userInputString, correctLetters, incorrectLetters);
                    if (GuessIsNew) //Returns true if guess is new
                    {
                        guessesLeft--;
                        neededGuesses++;
                        if (userInputString == "!") //End program at any time with a zero
                        {
                            Clear();
                            WriteLine("Program has ended.");
                            bools[0] = false;
                        }
                        else if (userInputString.ToLower().Equals(chosenWord.ToLower())) //If user wins by guessing the right whole word
                        {
                            Clear();
                            DisplayResult(chosenWord.ToCharArray(), neededGuesses,"Win");
                            bools[0] = false;
                        }

                        else // User guesses a letter
                        {
                            if (chosenWord.ToLower().Contains(userInputString.ToLower()) && userInputString.Length == 1) //Letter exists in word
                            {
                                bools = RightGuessOfLetter(chosenWord, userInputString, correctLetters, bools, neededGuesses);
                            }

                            else //Letter does not exist in word
                            {
                                bools = WrongGuessOfLetter(chosenWord, userInputString, incorrectLetters, bools, neededGuesses);
                            }
                        }

                    }
                }
        }

        //User guesses a letter that exists in the secret word
        private static bool[] RightGuessOfLetter(string chosenWord, string userInputString,
            char[] correctLetters, bool[] bools, int neededGuesses)
        {
          
           char userInputChar = char.Parse(userInputString);
           List<int> foundIndexes = FindIndexes(userInputChar, chosenWord);
           foreach (int i in foundIndexes)
           {
              correctLetters[i] = userInputChar;
           }
           if (new string(correctLetters).ToLower().Equals(chosenWord.ToLower()))
           {
                Clear();
                DisplayResult(chosenWord.ToCharArray(), neededGuesses, "Win");
                bools[0] = false;
           }
           else if (neededGuesses == 10)
            {
                Clear();
                DisplayResult(chosenWord.ToCharArray(), neededGuesses, "Loose");
                bools[0] = false;
            }
            return bools;
        }

        //User guesses a letter that don't exists in secret word
        private static bool[] WrongGuessOfLetter(string chosenWord, string userInputString, 
            StringBuilder incorrectLetters, bool[] bools, int neededGuesses)
        {
         
            if (neededGuesses == 10)
            {
                Clear();
                DisplayResult(chosenWord.ToCharArray(), neededGuesses, "Loose");
                bools[0] = false;
            }
            else if (userInputString.Length == 1)
            {
                if (bools[1])
                {
                    incorrectLetters.Append(userInputString);
                    bools[1] = false;
                }
                else
                    incorrectLetters.Append("," + userInputString);
            }
            return bools;
        }


        private static void DisplayResult(char[] chosenWord, int neededGuesses, string result)
        {
            string winstring = "Win";
            bool win = winstring.Equals(result);

            Write("Secret word: ");
            DisplayWord(chosenWord);
            if (win)
            {
                WriteLine("\n\nCongratulations!");
                WriteLine("You guessed the right word!");
            }
            else
                WriteLine("\n\nSorry, you loose!");
            WriteLine("You needed {0} guesses", neededGuesses);

        }

        //Initializes every unguessed letter as an underscore
        private static char[] InitializeCorrectLetters(char[] correctLetters, int length)
        {
       
            char[] letters = new char[length];

            for (int i = 0; i < length; i++)
            {
                letters[i] = '_';
            }

           
            return letters;
        }

        //Checks if user has made this guess before
        private static bool NewGuess(string userInputString, char[]correctLetters, StringBuilder incorrectLetters)
        {
            bool test1 = new string(correctLetters).Contains(userInputString);
            bool test2 = incorrectLetters.ToString().Contains(userInputString);
          
            return (!test1 && !test2);
        }

        //Displays current status of the game
        private static void DisplayOutput(char[]correctLetters, int guessesLeft, StringBuilder incorrectLetters)
        {
            Write("Secret word: ");
            DisplayWord(correctLetters);
            WriteLine("\n\nGuesses left: " + guessesLeft);
            Write("Incorrect letters: " + incorrectLetters + "\n\n");
        }

        //Inserts an empty position between each letter
        private static void DisplayWord(char[]correctLetters)
        {
          
           for (int i = 0; i <correctLetters.Length; i++)
            {
                Write(correctLetters[i] + " ");
            }
        }

        //Finds all indexes of a letter in secret word
        private static List<int> FindIndexes(char userInput, string chosenWord)
        {
            List<int> indexes = new List<int>();
            char[] wordChar = new char[chosenWord.Length];

            wordChar = chosenWord.ToLower().ToCharArray();
            for (int i = 0; i < chosenWord.Length; i++)
            {
                if (wordChar[i] == userInput)
                {
                  
                    indexes.Add(i);
                }
            }


            return indexes;
        }

        //Parsing user input
        private static string GetUserInput(string chosenWord)
        {
            bool success = false;
            string input = " ";
            string sign = " ";
            decimal number;

            do
            {
                Write("Enter your guess: ");
                input = ReadLine();

                if (decimal.TryParse(input, out number))
                {
                    if (number == 0)
                    {
                        sign= "!";
                        success = true;
                    }
                    else
                    {
                        ClearLine();
                        WriteLine("Invalid input. No numbers except zero allowed.");
                     
                    }
                    
                }
                else if (!Regex.IsMatch(input, @"^[\p{L}]+$"))
                {
                    ClearLine();
                    WriteLine("Invalid input. Only letters in the alphabet allowed.");
                }
                else if (chosenWord.Length != input.Length && input.Length!=1)
                {
                    ClearLine();
                    WriteLine("Invalid input. Length of guess differs from length of secret word.");

                }
                else
                {
                    success = true;
                    sign = input;
                }
                
            } while (!success);

            return sign;
        }

        private static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
        }

        private static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        //Chooses secret word from a source
        private static string ChooseSecretWord(string[] word)
        {
            Random rnd = new Random();
            int wordIndex = rnd.Next(word.Length);
            return word[wordIndex];
        }

        private static void ShowInfo()
        {
            WriteLine("Welcome to Hangman!");
            WriteLine("You have 10 guesses to complete the word.");
            WriteLine("Either guess a letter or the whole word.");
            WriteLine("Press 0 to end program.");
            WriteLine("Good luck!");
            WriteLine("-------------------------------------------------------");
        }
    }
}
