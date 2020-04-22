using Game.GameObjects;
using Game.Weapons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Game.Main
{
    public enum SaveType
    {
        Local,
        Remote
    }

    public class GameSave
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string GameBaseContent { get; set; }
        public string MapContent { get; set; }

        public GameSave()
        {
            Date = DateTime.Now;
        }

        public GameSave(int id, DateTime date, string gameBaseContent, string mapContent)
        {
            Id = id;
            Date = date;
            GameBaseContent = gameBaseContent;
            MapContent = mapContent;
        }
    }

    public static class BaseGameSaver
    {
        static HttpClient client = new HttpClient();
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
        /// URL
        /// </summary>
        private const string urlSavePath = "http://localhost:5000/api/game";

        #region Saving

        /// <summary>
        /// Saving objects
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        /// <param name="saveType"></param>
        public static bool SaveToFile(BaseGame baseGame, string path, SaveType saveType)
        {
            bool objSaved = false, mapSaved = false;
            try
            {
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                
                switch (saveType)
                {
                    case SaveType.Local:
                        Task.WaitAll(SaveContentToFileAsync(
                            JsonConvert.SerializeObject(baseGame), $"{saveDir}\\{Path.GetFileNameWithoutExtension(path)}{postfixSave}.json"),
                            SaveContentToFileAsync(GetMapContent(baseGame), $"{saveDir}\\{Path.GetFileNameWithoutExtension(path)}{postfixMap}.json"));
                        break;
                    case SaveType.Remote:
                        Task task = SetGameSaveToServerAsync(baseGame, $"{saveDir}\\{Path.GetFileNameWithoutExtension(path)}{postfixSave}.json");
                        Task.WaitAll(task);
                        objSaved = mapSaved = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File saving error: {ex.Message}");
                return false;
            }
            return objSaved && mapSaved;
        }

        /// <summary>
        /// Game objects saving
        /// </summary>
        /// <param name="baseGame"></param>
        /// <returns></returns>
        private async static Task SaveContentToFileAsync(string content, string path)
        {
            try
            {
                using (var file = new StreamWriter(path, false))
                {
                    await file.WriteAsync(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File saving error: {ex.Message}");
            }
        }

        private static string GetMapContent(BaseGame baseGame)
        {
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
            return JsonConvert.SerializeObject(list);
        }

        #endregion

        #region Autosave

        /// <summary>
        /// Autosaving
        /// </summary>
        /// <param name="baseGame"></param>
        public static void AutoSaveToFile(BaseGame baseGame)
        {
            try
            {
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                Task.WaitAll(SaveContentToFileAsync(JsonConvert.SerializeObject(baseGame, Formatting.Indented), $"{saveDir}\\autosave{postfixSave}.json"),
                    SaveContentToFileAsync(GetMapContent(baseGame), $"{saveDir}\\autosave{postfixMap}.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File saving error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get autosave file from directory
        /// </summary>
        /// <returns></returns>
        public static FileInfo GetAutoSaveFile()
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            return di.GetFiles($"{saveDir}\\autosave{postfixSave}.json").SingleOrDefault();
        }

        #endregion

        /// <summary>
        /// Restore an object from a file
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        public static void RestoreFromFile(ref BaseGame baseGame, string path)
        {
            try
            {
                string baseGameContent = string.Empty;
                using (var file = new StreamReader(path))
                {
                    baseGameContent = file.ReadToEnd();
                }

                if (!baseGameContent.Contains(urlSavePath))
                {
                    baseGame = JsonConvert.DeserializeObject<BaseGame>(baseGameContent);
                    
                    #region ObjectInit

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
                    #endregion
                    
                    RestoreMapFromFile(ref baseGame, $"{saveDir}\\{Path.GetFileNameWithoutExtension(path).Replace(postfixSave, string.Empty)}{postfixMap}.json");
                }
                else
                {
                    GetGameRestoreFromServerById(ref baseGame, baseGameContent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Restore an object from content
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="contentBaseGame"></param>
        /// <param name="contentMap"></param>
        public static void RestoreFromContent(ref BaseGame baseGame, string contentBaseGame, string contentMap)
        {
            try
            {
                baseGame = JsonConvert.DeserializeObject<BaseGame>(contentBaseGame);
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
                RestoreMapFromContent(ref baseGame, contentMap);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Restore object cells from a file
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        private static void RestoreMapFromFile(ref BaseGame baseGame, string path)
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
                            if(baseGame.Character1?.Id == cell.Id)
                            {
                                baseGame.World.InitGameObject(baseGame.Character1, cell.PositionX, cell.PositionY);
                            }
                            if (baseGame.Character2?.Id == cell.Id)
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
        /// Restore object cells from content
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="contentMap"></param>
        private static void RestoreMapFromContent(ref BaseGame baseGame, string contentMap)
        {
            try
            {
                List<CellKeeper> list = JsonConvert.DeserializeObject<List<CellKeeper>>(contentMap);
                baseGame.World = new Map(baseGame.MapSize1, baseGame.MapSize2, Season.None);
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
                        if (baseGame.Character1?.Id == cell.Id)
                        {
                            baseGame.World.InitGameObject(baseGame.Character1, cell.PositionX, cell.PositionY);
                        }
                        if (baseGame.Character2?.Id == cell.Id)
                        {
                            baseGame.World.InitGameObject(baseGame.Character2, cell.PositionX, cell.PositionY);
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
        /// Get save files from directory
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public static List<FileInfo> LoadFiles(int counter)
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            return di.GetFiles($"{saveDir}\\*{postfixSave}.json")
                .Where(x => !x.Name.Contains("autosave"))
                .OrderByDescending(x => x.CreationTime)
                .Take(counter)
                .ToList();
        }

        /// <summary>
        /// Get save content from the url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async static Task<string> GetSaveGameContentFromServerByUrlAsync(string url)
        {
            Uri uri = new Uri(url);
            var savesAsync = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
            return await savesAsync.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Restore BaseGame using remote saved data
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="url"></param>
        public static void GetGameRestoreFromServerById(ref BaseGame baseGame, string url)
        {
            Task<string> task = GetSaveGameContentFromServerByUrlAsync(url);
            GameSave gameSave = JsonConvert.DeserializeObject<GameSave>(task.Result);
            if (gameSave != null)
                RestoreFromContent(ref baseGame, gameSave.GameBaseContent, gameSave.MapContent);
            else
                Console.WriteLine("Can't load saved game progress");
        }

        /// <summary>
        /// Send saving progress to the server and create a local file
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static async Task SetGameSaveToServerAsync(BaseGame baseGame, string path)
        {
            //Object
            string objectContent = JsonConvert.SerializeObject(baseGame);
            
            //Map Cells
            string mapContent = GetMapContent(baseGame);

            //Prepare object
            GameSave gameSave = new GameSave();
            gameSave.GameBaseContent = objectContent;
            gameSave.MapContent = mapContent;
            var json = JToken.Parse(JsonConvert.SerializeObject(gameSave));
            json["id"]?.Remove();

            Uri uri = new Uri(urlSavePath);
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Put, uri);
            //
            message.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            var n = await client.SendAsync(message);
            var id = await n.Content.ReadAsStringAsync();
            //
            await SaveUrlToFileAsync(path, $"{urlSavePath}/{id}");
        }

        /// <summary>
        /// Save url to a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async static Task SaveUrlToFileAsync(string path, string content)
        {
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            using (var file = new StreamWriter(path, false))
            {
                await file.WriteAsync(content);
            }
        }
    }
}
