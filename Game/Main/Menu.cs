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
        Continue = 2,
        Restore = 3,
        Exit = 10
    }

    public class Menu
    {
        Dictionary<int, string> list;
        
        public Menu()
        {
            this.list = new Dictionary<int, string>();
            this.list.Add((int)UserAction.NewGame, "Start a new game");
            this.list.Add((int)UserAction.Continue, "Continue the last game");
            this.list.Add((int)UserAction.Restore, "Restore from save files");
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
                userAction = (UserAction)i;
                Console.Clear();
                switch (userAction)
                {
                    case UserAction.NewGame:
                    case UserAction.Exit:
                        break;
                    case UserAction.Continue:
                        var file = BaseGameSaver.GetAutoSaveFile();
                        if (file != null)
                        {
                            fileName = file.FullName;
                        }
                        break;
                    case UserAction.Restore:
                        ToConsole("Select a file from the list below:\r\n", ConsoleColor.Yellow);
                        
                        List<FileInfo> vs = BaseGameSaver
                            .LoadFiles(5)
                            .ToList();
                        int padding = vs.Max(x => x.Name.Length);
                        vs.ForEach(x => { Console.WriteLine($"{vs.IndexOf(x) + 1}:  {Path.GetFileNameWithoutExtension(x.Name).Replace(BaseGameSaver.postfixSave,string.Empty).PadRight(padding)}{x.CreationTime}"); });

                        ToConsole("\r\nEnter the number of the selected file and press <Enter>", ConsoleColor.Yellow);
                        
                        name = Console.ReadLine().Trim();
                        if (int.TryParse(name, out int index))
                        {
                            if(vs.Count >= index)
                            {
                                fileName = vs[index - 1].FullName;
                            }
                        }
                        break;
                }
                Console.Clear();
            }
            else
                ToConsole($"\r\nValue '{name}' doesn't match to any numbers from the menu! The apllication will be closed.", ConsoleColor.Red);
            return userAction;
        }


        public bool ShowSaveMenu(BaseGame baseGame)
        {
            ToConsole("\r\nSave game options.");
            ToConsole("Press <1> to save your progress to a file.\r\nPress <2> to create remote save!\r\n", ConsoleColor.Yellow);
            string key = Console.ReadLine().Trim();
            if (!string.IsNullOrEmpty(key))
            {
                ToConsole("Enter the file name to save your progress and press 'Enter'.", ConsoleColor.Yellow);
                string name = Console.ReadLine().Trim();
                //
                return BaseGameSaver.SaveToFile(baseGame, $"{name}.json", key.Equals("1", StringComparison.InvariantCultureIgnoreCase) ? SaveType.Local : SaveType.Remote);
            }
            else
                return false;
        }

    }
}
