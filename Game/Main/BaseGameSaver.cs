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
        /// Сохранение в файл объекта
        /// </summary>
        /// <param name="baseGame"></param>
        /// <param name="path"></param>
        public static bool SaveToFile(BaseGame baseGame, string path)
        {
            try
            {
                using (var file = new StreamWriter(path, false))
                {
                    file.Write(JsonConvert.SerializeObject(baseGame));
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
        /// Загрузка объекта из файла
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<FileInfo> LoadFiles(int counter)
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            return di.GetFiles("*_save.json")
                .OrderByDescending(x => x.CreationTime)
                .Take(counter)
                .ToList();
        }

    }
}
