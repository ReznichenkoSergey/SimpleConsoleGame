using System;

using Game.Main;

namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.Unicode;

                Menu menu = new Menu();
                UserAction userAction = menu.ShowStartMenu(out string fileRestore);

                BaseGame game = null;
                switch (userAction)
                {
                    case UserAction.NewGame:
                        Console.Write("Enter player name: ");
                        string name = Console.ReadLine();
                        game = new BaseGame(16, 14);
                        game.Start();
                        break;
                    case UserAction.Restore:
                        if (!string.IsNullOrEmpty(fileRestore))
                        {
                            game = new BaseGame(16, 14);
                            try
                            {
                                BaseGameSaver.RestoreFromFile(ref game, fileRestore);
                                game.Start();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Restoring: {ex.Message}");
                            }
                        }
                        else
                            Console.WriteLine("The file name is empty!");
                        break;
                    case UserAction.Exit:
                        break;
                }
                /*
                Console.Write("Enter player name: ");
                string name = Console.ReadLine();
                BaseGame game = new BaseGame(16, 14);
                game.Start();*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }
    }
}
