using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Battle_City
{
    // Клас, який відповідає за управління найкращим часом гри
    public static class BestTimeManager
    {
        private static readonly string filePath = "best_time.txt"; // Шлях до файлу з найкращим часом

        // Метод для зчитування найкращого часу гри з файлу
        public static TimeSpan ReadBestTime()
        {
            if (File.Exists(filePath)) // Перевірка існування файлу
            {
                string bestTimeStr = File.ReadAllText(filePath); // Зчитування часу з файлу
                if (TimeSpan.TryParseExact(bestTimeStr, @"mm\:ss", null, out TimeSpan bestTime)) // Парсинг часу
                {
                    return bestTime; // Повернення найкращого часу
                }
            }
            return TimeSpan.MaxValue; // Повернення максимального значення часу, якщо файл не знайдено або час невірного формату
        }

        // Метод для запису нового найкращого часу у файл
        public static void WriteBestTime(TimeSpan newBestTime)
        {
            // Форматування часу у вигляді "хвилини:секунди"
            string formattedTime = newBestTime.ToString(@"mm\:ss");
            File.WriteAllText(filePath, formattedTime); // Запис нового найкращого часу у файл
        }
    }
}
