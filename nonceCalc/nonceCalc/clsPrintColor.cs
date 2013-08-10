using System;
using System.Globalization;

namespace nonceCalc
{
    /// <summary>
    /// Bietet einfachen Zugriff um Text farbig darzustellen in der Konsole.
    /// </summary>
    public static class PrintColor
    {
        /// <summary>
        /// Gibt die Konsolenfarben zurück, hexadezimal sortiert, von 0-F
        /// </summary>
        public static ConsoleColor[] ColorRow
        {
            get
            {
                ConsoleColor[] c = new ConsoleColor[16];
                for (int i = 0; i < 16; i++)
                {
                    c[i] = (ConsoleColor)i;
                }
                return c;
            }
        }

        /// <summary>
        /// Schreibt eine Zeile mit farbigem Text, inklusive Zeilenumbruch
        /// </summary>
        /// <param name="text">Zu schreibender Text</param>
        /// <param name="mapF">Vordergrundfarben</param>
        /// <param name="mapB">Hintergrundfarben</param>
        public static void printColorL(string text, string mapF, string mapB)
        {
            printColor(text, mapF, mapB);
            Console.WriteLine();
        }

        /// <summary>
        /// Schreibt eine Zeile mit farbigem Text
        /// </summary>
        /// <param name="text">Zu schreibender Text</param>
        /// <param name="mapF">Vordergrundfarben</param>
        /// <param name="mapB">Hintergrundfarben</param>
        public static void printColor(string text, string mapF, string mapB)
        {
            int color=0;

            mapF = mapF.ToUpper().Replace(' ', '_');
            mapB = mapB.ToUpper().Replace(' ', '_');

            if (text.Length != mapF.Length || mapF.Length != mapB.Length)
            {
                throw new Exception("Alle Parameter müssen die selbe Textlänge aufweisen");
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (mapF[i] != '_')
                {
                    if (!int.TryParse(mapF.Substring(i, 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out color))
                    {
                        throw new Exception(string.Format("Vordergrundfarbe an Position {0} ungültig. Ist: '{1}'", i, mapF[i]));
                    }
                }
                if (mapB[i] != '_')
                {
                    if (!int.TryParse(mapB.Substring(i, 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out color))
                    {
                        throw new Exception(string.Format("Hintergrundfarbe an Position {0} ungültig. Ist: '{1}'", i, mapF[i]));
                    }
                }
            }


            ConsoleColor[] C = new ConsoleColor[] { Console.ForegroundColor, Console.BackgroundColor };
            for (int i = 0; i < text.Length; i++)
            {
                if (mapF[i] != '_')
                {
                    Console.ForegroundColor = (ConsoleColor)int.Parse(mapF.Substring(i, 1), NumberStyles.HexNumber);
                }
                if (mapB[i] != '_')
                {
                    Console.BackgroundColor= (ConsoleColor)int.Parse(mapB.Substring(i, 1), NumberStyles.HexNumber);
                }
                Console.Write(text[i]);
            }
            Console.ForegroundColor = C[0];
            Console.BackgroundColor = C[1];
        }

    }
}
