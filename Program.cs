using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {
        static SQL sql = new SQL("test.sqlite");
        static int index = last_torrent(sql);
        static int threads = 8;
        static void Main(string[] args)
        {
            while (true)
            {
                if (args.Length > 1)
                    if (args[0] == "-threads")
                        if (!Int32.TryParse(args[1], out threads)) {
                            Console.WriteLine("Please define a thread number.");
                            Environment.Exit(1);
                        }
                        else ;
                    else Console.WriteLine("Unknown argument.");
                else ;
                Console.WriteLine("Running on {0} threads.", threads);
                int most_recent = Scraper.most_recent();
                while (index <= most_recent - threads)
                {
                    var results = new List<Dictionary<string, string>>();
                    Task[] tasks = new Task[threads];
                    for (int i = 1; i <= threads; i++)
                    {
                        string url = "http://thepiratebay.la/torrent/" + index + "/";
                        var sr = new Scraper(url);
                        tasks[i - 1] = Task.Factory.StartNew(() =>
                        {
                            results.Add(sr.scrape());
                        });
                        index++;
                    }
                    Task.WaitAll(tasks);
                    Task.Factory.StartNew(() => last_torrent(sql, index));
                    Console.WriteLine("Program: storing scraped results");
                    Task.Factory.StartNew(() => store_results(sql, results));
                    Console.Clear();
                }
                Thread.Sleep(10000);
            }
            //Console.ReadLine();
        }
        static int last_torrent(SQL db_handle, int set = -1)
        {
            if (set == -1)
            {
                db_handle.query("SELECT size FROM torrents WHERE uploaded = 'last_torrent'");
                db_handle.result.Read();
                return Int32.Parse(db_handle.result["size"].ToString());
            }
            else
            {
                db_handle.query("UPDATE torrents SET size = '" + set + "' WHERE uploaded = 'last_torrent'");
                return set;
            }
        }
        static void store_results(SQL db_handle, List<Dictionary<string,string>> results)
        {
            foreach (Dictionary<string, string> dict in results)
            {
                if (dict.ContainsValue(null)) continue;
                sql.query(string.Format(
                    "INSERT INTO torrents (id,title,type,description,magnet,size,uploaded,infohash) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                    index, dict["title"], dict["type"], dict["description"], dict["magnet"], dict["size"], dict["uploaded"], dict["infohash"]
                ));
                sql.free();
            }
        }
    }
}
