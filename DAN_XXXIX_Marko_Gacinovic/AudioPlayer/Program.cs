using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioPlayer
{
    class Program
    {
        static Dictionary<int, string> songs = new Dictionary<int, string>();

        static void Main(string[] args)
        {
            string option = null;

            do
            {
                Console.WriteLine("\nWelcome to Audio player");
                Console.WriteLine("1. Add a song");
                Console.WriteLine("2. List of songs");                
                Console.WriteLine("3. Play a song");
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
                        PlaySong();
                        break;
                    case "4":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Please choose an option in range 1-3.");
                        break;
                }
            } while (!option.Equals("4"));

            Console.ReadLine();
        }        

        static void AddSong()
        {
            Console.WriteLine("Please input the name of author: ");
            string author = Console.ReadLine();

            Console.WriteLine("Song name: ");
            string songName = Console.ReadLine();

            string format = "hh\\:mm\\:ss";

            Console.WriteLine("Song duration: (hh:mm:ss)");
            bool validDuration = TimeSpan.TryParseExact(Console.ReadLine(), format, CultureInfo.CurrentCulture, out TimeSpan duration);            

            while (validDuration == false)
            {
                Console.WriteLine("Wrong input, please enter the time in format (hh:mm:ss):");
                validDuration = TimeSpan.TryParseExact(Console.ReadLine(), format, CultureInfo.CurrentCulture, out duration);
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

        static void PlaySong()
        {
            try
            {
                using (StreamReader sr = new StreamReader("../../Music.txt"))
                {
                    string line;
                    int index = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine((++index).ToString() + ". " +line);
                        songs[index] = line;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Please enter the song's number you want to play: ");
            bool validNum = int.TryParse(Console.ReadLine(), out int num);

            while (!validNum)
            {
                Console.WriteLine("Wrong input, please try again.");
                validNum = int.TryParse(Console.ReadLine(), out num);
            }                      

            if (songs.Keys.Contains(num))
            {
                string[] splitSong = songs[num].Split(' ');
                string songName = splitSong[1];
                TimeSpan songDuration = TimeSpan.Parse(splitSong[2]);

                Console.WriteLine("{0} song starting at {1}", songName, DateTime.Now.ToString("HH:mm:ss tt"));

                int duration = songDuration.Seconds;

                do
                {
                    Thread.Sleep(1000);
                    duration--;
                    Console.WriteLine("Song is still playing...");
                } while (duration>0);                
            }
            else
            {
                Console.WriteLine("Song does not exist.");
                return;
            }
        }
    }
}
