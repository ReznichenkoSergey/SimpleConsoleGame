using Game.GameObjects;
using Game.Weapons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.Main
{
    public static class BaseGameSaver
    {
        /// <summary>
        /// A postfix of BaseGame object
        /// </summary>
        public const string postfixSave = "_save";
        /// <summary>
        /// A postfix of World object
        /// </summary>
        private const string postfixMap = "_map";
        /// <summary>
        /// Directory inside the root application directory for file saving
        /// </summary>
        private const string saveDir = "Saves";

        /// <summary>
        /// Saving objects
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        public static bool SaveToFile(BaseGame baseGame, string path)
        {
            try
            {
                if(!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                using (var file = new StreamWriter($"{saveDir}\\{Path.GetFileNameWithoutExtension(path)}{postfixSave}.json", false))
                {
                    file.Write(JsonConvert.SerializeObject(baseGame, Formatting.Indented));
                }

                List<CellKeeper> list = new List<CellKeeper>();
                for (int i = 0; i < baseGame.World.WorldHeight; i++)
                {
                    for (int j = 0; j < baseGame.World.WorldWidth; j++)
                    {
                        if (baseGame.World.Cells[i, j].GameObject != null)
                        {
                            list.Add(new CellKeeper(baseGame.World.Cells[i, j].GameObject.Id, i, j));
                        }
                    }
                }
                SaveMapToFile(list, $"{saveDir}\\{Path.GetFileNameWithoutExtension(path)}{postfixMap}.json");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File saving error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cells' objects saving
        /// </summary>
        /// <param name="list"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool SaveMapToFile(List<CellKeeper> list, string path)
        {
            try
            {
                using (var file = new StreamWriter(path, false))
                {
                    file.Write(JsonConvert.SerializeObject(list, Formatting.Indented));
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File saving error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Object restoring
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        public static void RestoreFromFile(ref BaseGame baseGame, string path)
        {
            try
            {
                using (var file = new StreamReader(path))
                {
                    baseGame = (BaseGame)JsonConvert.DeserializeObject(file.ReadToEnd(), typeof(BaseGame));
                }
                var lines = baseGame.GameObjects.Select(x => x.Name.Split(" ")[0]).Distinct();
                for (int i = 0; i < baseGame.GameObjects.Count; i++)
                {
                    GameObject x = baseGame.GameObjects[i];
                    switch (x.Name.Split(" ")[0])
                    {
                        case "Friend":
                            Bot botFriend = new Bot(x.Name, true);
                            botFriend.Id = x.Id;
                            baseGame.GameObjects[i] = botFriend;
                            break;
                        case "Enemy":
                            Bot botEnemy = new Bot($"Enemy {x}", false);
                            botEnemy.Id = x.Id;
                            baseGame.GameObjects[i] = botEnemy;
                            break;
                        case "Heart":
                            Heart heart = new Heart();
                            heart.Id = x.Id;
                            baseGame.GameObjects[i] = heart;
                            break;
                        case "Knife":
                            Knife knife = new Knife();
                            (knife as GameObject).Id = x.Id;
                            baseGame.GameObjects[i] = knife;
                            break;
                        case "Sword":
                            Sword sword = new Sword();
                            (sword as GameObject).Id = x.Id;
                            baseGame.GameObjects[i] = sword;
                            break;

                    }
                };
                RestoreMapFromFile(ref baseGame, $"{saveDir}\\{Path.GetFileNameWithoutExtension(path).Replace(postfixSave, string.Empty)}{postfixMap}.json");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Cells' objects restoring
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        public static void RestoreMapFromFile(ref BaseGame baseGame, string path)
        {
            try
            {
                using (var file = new StreamReader(path))
                {
                    List<CellKeeper> list = (List<CellKeeper>)JsonConvert.DeserializeObject(file.ReadToEnd(), typeof(List<CellKeeper>));
                    baseGame.World= new Map(baseGame.MapSize1, baseGame.MapSize2, Season.None);
                    baseGame.World.GenerateMap();
                    foreach (CellKeeper cell in list)
                    {
                        var m = baseGame.GameObjects.Where(x => x.Id == cell.Id).SingleOrDefault();
                        //не Character
                        if (m != null)
                        {
                            baseGame.World.InitGameObject(m, cell.PositionX, cell.PositionY);
                        }
                        else
                        {
                            //Character
                            if(baseGame.Character1.Id == cell.Id)
                            {
                                baseGame.World.InitGameObject(baseGame.Character1, cell.PositionX, cell.PositionY);
                            }
                            if (baseGame.Character2.Id == cell.Id)
                            {
                                baseGame.World.InitGameObject(baseGame.Character2, cell.PositionX, cell.PositionY);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get saves from save directory
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public static List<FileInfo> LoadFiles(int counter)
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            return di.GetFiles($"{saveDir}\\*{postfixSave}.json")
                .OrderByDescending(x => x.CreationTime)
                .Take(counter)
                .ToList();
        }

    }
}
