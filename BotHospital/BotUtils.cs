using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BotHospital
{
    public class BotUtils
    {
        public static string GetDayPhase()
        {
            var hour = DateTime.Now.Hour;
            if (hour >= 0 && hour < 12)
            {
                return "Buenos días";
            }
            else if (hour >= 12 && hour < 18)
            {
                return "Buenas tardes";
            }
            else
            {
                return "Buenas noches";
            }
        }
        
        public static bool ContainsWord(string input, List<string> wordList)
        {
            input = input.ToLower();

            foreach (string word in wordList)
            {
                if (input.Contains(word.ToLower()))
                {
                    return true;
                }
            }

            return false;

        }

        public static string GetSimilarString(string input, List<string> wordList)
        {
            input = input.ToLower();
            foreach (string word in wordList)
            {
                if (input.Contains(word.ToLower()))
                {
                    return word;
                }
            }

            return "";
        }

        public static string GetEmailFromString(string input)
        {
            // Utilizar una expresión regular para buscar una dirección de correo electrónico en el texto
            Match match = Regex.Match(input, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Value; // Devolver la dirección de correo encontrada
            }
            else
            {
                return ""; // No se pudo capturar la dirección de correo
            }
        }
    }
}
