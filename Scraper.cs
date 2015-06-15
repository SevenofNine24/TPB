using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Scraper
    {
        private string c_url;
        public Scraper(string consturl = null)
        {
            c_url = consturl;
        }
        public Dictionary<string,string> scrape(string arg_url = null)
        {
            var output = new Dictionary<string, string>()
            {
                {"title",null},
                {"type",null},
                {"description",null},
                {"magnet",null},
                {"size",null},
                {"uploaded",null},
                {"infohash",null}
            };
            if (string.IsNullOrEmpty(c_url) && string.IsNullOrEmpty(arg_url))
                throw new ScraperException("URL is not defined.");
            Console.WriteLine("Scraper: scraping URL " + c_url ?? arg_url);
            string html = GetHTML(c_url ?? arg_url);
            if (html != null)
            {
                output["title"] = new Regex(@"<div id=""title"">\s*([^<]*)").Match(html).Groups[1].Value.Trim().Replace("'", "\'");
                output["type"] = new Regex(@"<dt>Type:<\/dt>\s*<dd><a.*"">([^<]*)").Match(html).Groups[1].Value.Trim().Replace("&gt;",">");
                output["description"] = new Regex(@"<div class=""nfo"">\s*<pre>([\s\S]*)</pre>").Match(html).Groups[1].Value.Trim().Replace("'","\'");
                output["magnet"] = new Regex(@"(magnet:\?[^""]*)").Match(html).Groups[1].Value.Trim();
                output["size"] = new Regex(@"<dt>Size:<\/dt>\s*<dd>.*?\(([0-9]*)").Match(html).Groups[1].Value.Trim();
                output["uploaded"] = new Regex(@"<dt>Uploaded:<\/dt>\s*<dd>([^<]*)").Match(html).Groups[1].Value.Trim();
                output["infohash"] = new Regex(@"<dt>Info Hash:<\/dt>\s*<dd><\/dd>\s*([^<]*)").Match(html).Groups[1].Value.Trim();
            }
            return output;
        }
        public static int most_recent()
        {
            string html = GetHTML("https://thepiratebay.la/recent");
            return Int32.Parse(new Regex(@"<div class=""detName"">[^\d]*(\d*)").Match(html).Groups[1].Value);
        }
        private static string GetHTML(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.UserAgent = "sevenscraper";
            StreamReader reader;
            try
            {
                reader = new StreamReader(request.GetResponse().GetResponseStream());
            } 
            catch
            {
                return null;
            }
            return reader.ReadToEnd();
        }
    }
    class ScraperException : Exception
    {
        public ScraperException():base() { }
        public ScraperException(string message) : base(message) { }
    }
}
