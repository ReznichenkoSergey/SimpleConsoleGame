using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

using static Game.Extensions;

namespace Game.Main
{
    public enum UserAction
    {
        NewGame = 1,
        Restore = 2,
        Exit = 3
    }

    public class Menu
    {
        Dictionary<int, string> list;
        private const string prefix = "_save";

        public Menu()
        {
            this.list = new Dictionary<int, string>();
            this.list.Add((int)UserAction.NewGame, "Start a new game");
            this.list.Add((int)UserAction.Restore, "Restore Game");
            this.list.Add((int)UserAction.Exit, "Exit");
        }

        public UserAction ShowStartMenu(out string fileName)
        {
            fileName = string.Empty;
            UserAction userAction = UserAction.Exit;
            ToConsole("Enter the action number from the list below:\r\n", ConsoleColor.Yellow);
            list
                .Select(x => new { Number = $"{x.Key}: {x.Value}" })
                .ToList()
                .ForEach(x => { ToConsole(x.Number); });

            ToConsole("\r\nTap <Enter> to confirm your choice.\r\n", ConsoleColor.Yellow);
            
            string name = Console.ReadLine();
            if(int.TryParse(name, out int i))
            {
                switch (i)
                {
                    case 1:
                        userAction = UserAction.NewGame;
                        Console.Clear();
                        break;
                    case 2:
                        userAction = UserAction.Restore;

                        Console.Clear();
                        ToConsole("Choose a file from the list below:\r\n", ConsoleColor.Yellow);
                        
                        List<FileInfo> vs = BaseGameSaver.LoadFiles(5);
                        int padding = vs.Max(x => x.Name.Length);
                        vs.ForEach(x => { Console.WriteLine($"{vs.IndexOf(x) + 1}:  {Path.GetFileNameWithoutExtension(x.Name).PadRight(padding)}{x.CreationTime}"); });

                        ToConsole("\r\nInput a number of the file and press <Enter>.", ConsoleColor.Yellow);
                        
                        name = Console.ReadLine().Trim();
                        if (int.TryParse(name, out int index))
                        {
                            if(vs.Count >= index)
                            {
                                fileName = vs[index - 1].FullName;
                            }
                        }
                        Console.Clear();
                        break;
                    case 3:
                        userAction = UserAction.Exit;
                        break;
                }
            }
            else
                ToConsole($"\r\nValue '{name}' doesn't match to any numbers from the menu! The apllication will be closed.", ConsoleColor.Red);
            return userAction;
        }


        public bool ShowSaveMenu(BaseGame baseGame)
        {
            ToConsole("\r\nPress 'Y' to save your progress!\r\n", ConsoleColor.Yellow);
            string name = Console.ReadLine().Trim().ToLower();
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals("y"))
                {
                    Console.Clear();
                    ToConsole("Enter the file name to save your progress and press 'Enter'.", ConsoleColor.Yellow);
                    name = Console.ReadLine().Trim();
                    if(!string.IsNullOrEmpty(name))
                        return BaseGameSaver.SaveToFile(baseGame, $"{name}{prefix}.json");
                }
            }
            return false;
        }
    }
}
