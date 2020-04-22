using System;
using System.Threading.Tasks;
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
                        game.Start(false);
                        break;
                    case UserAction.Restore:
                        if (!string.IsNullOrEmpty(fileRestore))
                        {
                            game = new BaseGame(16, 14);
                            try
                            {
                                BaseGameSaver.RestoreFromFile(ref game, fileRestore);
                                if(game != null)
                                    game.Start(true);
                                else
                                    Console.WriteLine("Failed to restore saved game! Try another file.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Restoring: {ex.Message}");
                            }
                        }
                        else
                            Console.WriteLine("The file name is empty!");
                        break;
                    case UserAction.Continue:
                        if (!string.IsNullOrEmpty(fileRestore))
                        {
                            game = new BaseGame(16, 14);
                            try
                            {
                                BaseGameSaver.RestoreFromFile(ref game, fileRestore);
                                if (game != null)
                                    game.Start(true);
                                else
                                    Console.WriteLine("Failed to restore autosaved game! Try to restore from saves or start a new game!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Continue playing: {ex.Message}");
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
