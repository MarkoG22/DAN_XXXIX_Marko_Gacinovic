using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            string option = null;

            do
            {
                Console.WriteLine("\nWELCOME");
                Console.WriteLine("1. Add a song");
                Console.WriteLine("2. List of songs");
                Console.WriteLine("3. Audio player");
                Console.WriteLine("4. Exit");
                Console.Write("Please choose an option: ");
                option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        AddSong();
                        break;
                    case "2":
                        ListOfSongs();
                        break;
                    case "3":
                        break;
                    case "4": Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Please choose an option in range 1-4.");
                        break;
                }
            } while (!option.Equals("4"));

            Console.ReadLine();
        }

        static void ListOfSongs()
        {
            try
            {
                using (StreamReader sr = new StreamReader("../../Music.txt"))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void AddSong()
        {
            Console.WriteLine("Please input the name of author: ");
            string author = Console.ReadLine();

            Console.WriteLine("Song name: ");
            string songName = Console.ReadLine();

            Console.WriteLine("Song duration: (XX:XX:XX)");
            bool validDuration = TimeSpan.TryParse(Console.ReadLine(), out TimeSpan duration);

            while (validDuration == false)
            {
                Console.WriteLine("Wrong input, please enter the time in format XX:XX:XX :");
                validDuration = TimeSpan.TryParse(Console.ReadLine(), out duration);
            }

            string song = author + ": " + songName + " " + duration.ToString();

            try
            {
                using (StreamWriter sw = new StreamWriter("../../Music.txt", true))
                {
                    sw.WriteLine(song);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
