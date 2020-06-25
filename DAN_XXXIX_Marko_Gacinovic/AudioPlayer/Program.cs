using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace AudioPlayer
{
    class Program
    {
        // ManualResetEvent for playing song and adverts at the same time
        static readonly ManualResetEvent manual = new ManualResetEvent(false);

        // AutoResetEvent for playing song
        static readonly AutoResetEvent auto = new AutoResetEvent(true);

        // locker for threads
        static readonly object locker = new object();

        // dictionary for songs
        static Dictionary<int, string> songs = new Dictionary<int, string>();

        // random object for displaying adverts
        static Random rnd = new Random();

        // delegate for stopping the song
        public delegate void Stop(string message);

        // variable for song's duration
        static int duration = 0;

        static void Main(string[] args)
        {
            string option = null;

            // loop for the Main Menu
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
                        
                        lock (locker)
                        {
                            // monitor.wait for waiting thread to finish method
                            Monitor.Wait(locker);
                        }
                        break;
                    case "4":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Wrong input, please choose an option in range 1-4.");
                        break;
                }
            } while (!option.Equals("4"));

            Console.ReadLine();
        }        

        /// <summary>
        /// method for adding a new song
        /// </summary>
        static void AddSong()
        {
            // inputs and validations
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

            // song string for file
            string song = author + ": " + songName + " " + duration.ToString();

            try
            {
                // writing to the file
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

        /// <summary>
        /// method for displaying songs from the file
        /// </summary>
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

        /// <summary>
        /// method for choosing a song to play
        /// </summary>
        static void PlaySong()
        {
            // AutoResetEvent waiting the song to finish
            auto.WaitOne();

            try
            {
                // reading songs from the file and writing them into dictionary
                using (StreamReader sr = new StreamReader("../../Music.txt"))
                {
                    string line;

                    // variable for dictionary keys
                    int index = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine((++index).ToString() + ". " + line);
                        songs[index] = line;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // inputs and validations
            Console.Write("Please enter the song's number you want to play (For stopping the song while playing, please press 'Esc'): ");
            bool validNum = int.TryParse(Console.ReadLine(), out int num);

            while (!validNum)
            {
                Console.WriteLine("Wrong input, please try again.");
                validNum = int.TryParse(Console.ReadLine(), out num);
            }

            // creating ang starting the threads for playing song and displaying adverts
            Thread song = new Thread(() => PlayingSong(num));
            Thread advert = new Thread(() => Advertisement());
            song.Start();
            advert.Start();
        }

        /// <summary>
        /// method for playing the song
        /// </summary>
        /// <param name="num"></param>
        static void PlayingSong(int num)
        {           
            // checking if the song exists
            if (songs.Keys.Contains(num))
            {
                // splitting strings to get name and duration
                string[] splitSong = songs[num].Split(' ');
                string songName = splitSong[1];
                TimeSpan songDuration = TimeSpan.Parse(splitSong[(splitSong.Length-1)]);

                Console.WriteLine("\nSong '{0}' starting at {1}", songName, DateTime.Now.ToString("HH:mm:ss tt"));                
                
                // calculating duration of the song in seconds 
                duration = songDuration.Hours*60*60 + songDuration.Minutes*60 + songDuration.Seconds;

                // ManualResetEvent setting for adverts to start
                manual.Set();

                Console.WriteLine("For stopping the song, please press 'Esc'.\n");

                // loop for displaying while the song is playing
                do
                {
                    Thread.Sleep(1000);
                    duration--;
                    Console.WriteLine("Song is still playing...");

                    // stopping the song with delegate
                    if (Console.KeyAvailable)
                    {
                        StoppingSong(CallDelegate);
                        duration = 0;
                        auto.Set();
                        return;
                    }

                } while (duration > 0);

                Console.WriteLine("\nSong just finished.");

                auto.Set();

                manual.Reset();                
            }
            else
            {
                Console.WriteLine("Song does not exist.");
                auto.Set();

                lock (locker)
                {
                    // notifying the method is finished
                    Monitor.Pulse(locker);
                }
            }
        }

        /// <summary>
        /// method for displaying advertisements
        /// </summary>
        static void Advertisement()
        {   
            try
            {
                // reading adverts from the file and adding them to the list
                using (StreamReader sr = new StreamReader("../../Advertisements.txt"))
                {
                    string line;

                    List<string> adverts = new List<string>();

                    while ((line = sr.ReadLine()) != null)
                    {
                        adverts.Add(line);
                    }

                    // waiting the song to start playing
                    manual.WaitOne();

                    // displaying the advertisements
                    do
                    {
                        Thread.Sleep(200);
                        Console.WriteLine(adverts[rnd.Next(0,5)]);
                    } while (duration > 0);
                    
                    lock (locker)
                    {
                        // notifying the method is finished
                        Monitor.Pulse(locker);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // method for displaying the message in delegate
        static void CallDelegate(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// method for delegate to stop the song
        /// </summary>
        /// <param name="song"></param>
        static void StoppingSong(Stop song)
        {
            string message = "\nThe song is stopped, you can play another one.";

            song(message);
        }
    }
}
