using System;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using K8Workerino.Models;
using System.Linq;
using System.IO;

namespace K8Workerino
{
    class Program
    {

        public static class ProgHelpers
        {
            //Helpers
            //things needed for obvious reasons
            public static IConfigurationRoot Configuration { get; set; }
            public static string connstring = "";
            public static string owapiprovider = "";
            public static int sleeptimems = 5000;
        }

        private ksBotSQLContext db = new ksBotSQLContext();

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            ProgHelpers.Configuration = builder.Build();

            ProgHelpers.connstring = ProgHelpers.Configuration["Settings:ConnectionString"];
            ProgHelpers.sleeptimems = Convert.ToInt32(ProgHelpers.Configuration["Settings:SleepTimeMS"]);
            ProgHelpers.owapiprovider = ProgHelpers.Configuration["Settings:OWAPIProvider"];

            //Print out Settings
            Console.WriteLine("CONNSTRING: " + ProgHelpers.connstring);
            Console.WriteLine("SLEEPTIME-MS: " + ProgHelpers.sleeptimems);
            Console.WriteLine("OWAPIURL: " + ProgHelpers.owapiprovider);
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("STARTING KITSUN8'S WORKERINO OWSTATS UPDATER");
            Console.WriteLine("------------" + DateTime.Now.ToString() + "------------");

            //All good, go!
            Program program = new Program();
            program.RunGo().Wait();
        }
        public async Task RunGo()
        {
            using (var dbgo = new ksBotSQLContext())
            {
                var users = dbgo.User.Where(x => x.BtagId != null).ToList();
                try
                {
                    if (users.Count > 0)
                    {
                        //for (var iz = 0; iz < users.Count; iz++)
                        foreach (var n in users)
                        {
                            Console.WriteLine(n.DiscordId + "------" + n.BtagId);
                            System.Threading.Thread.Sleep(ProgHelpers.sleeptimems);
                            await OWstats(n.DiscordId, n.BtagId);

                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //ex
                }
            }

        }

        public async Task OWstats(string i, string st)
        {
            string stnew = "";
            stnew = st.Replace("#", "-");
            string url = ProgHelpers.owapiprovider + stnew + "/stats";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    try
                    {
                        var responseText = await client.GetStringAsync(url);
                        dynamic data = responseText;
                        JObject o = JObject.Parse(data);

                        double killavg = 0;
                        double healavg = 0;
                        double deathavg = 0;
                        double winrate = 0;
                        double timeplayed = 0;
                        string avatarurl = "";

                        int sr = 0;

                        if (o.SelectToken("eu.stats.competitive.overall_stats.comprank") != null)
                        {
                            sr = (int)o.SelectToken("eu.stats.competitive.overall_stats.comprank");
                        }
                        if (o.SelectToken("eu.stats.competitive.average_stats.eliminations_avg") != null)
                        {
                            killavg = (double)o.SelectToken("eu.stats.competitive.average_stats.eliminations_avg");
                        }
                        if (o.SelectToken("eu.stats.competitive.average_stats.healing_done_avg") != null)
                        {
                            healavg = (double)o.SelectToken("eu.stats.competitive.average_stats.healing_done_avg");
                        }
                        if (o.SelectToken("eu.stats.competitive.average_stats.deaths_avg") != null)
                        {
                            deathavg = (double)o.SelectToken("eu.stats.competitive.average_stats.deaths_avg");
                        }
                        if (o.SelectToken("eu.stats.competitive.overall_stats.win_rate") != null)
                        {
                            winrate = (double)o.SelectToken("eu.stats.competitive.overall_stats.win_rate");
                        }
                        if (o.SelectToken("eu.stats.competitive.overall_stats.avatar") != null)
                        {
                            avatarurl = (string)o.SelectToken("eu.stats.competitive.overall_stats.avatar");
                        }
                        if (o.SelectToken("eu.stats.competitive.game_stats.time_played") != null)
                        {
                            timeplayed = (double)o.SelectToken("eu.stats.competitive.game_stats.time_played");
                        }


                        using (var db = new ksBotSQLContext())
                        {
                            var result = db.User.SingleOrDefault(b => b.DiscordId == i);
                            if (result != null)
                            {
                                //result.Apihero1 = hero1;
                                //result.Apihero2 = hero2;
                                //result.Apihero3 = hero3;
                                result.ApicurrentSr = sr;
                                result.ApikillAvg = killavg;
                                result.ApideathAvg = deathavg;
                                result.ApihealAvg = healavg;
                                result.ApiavatarUrl = avatarurl;
                                result.ApiwinRate = winrate;
                                result.ApitimePlayed = timeplayed;

                                await db.SaveChangesAsync();
                                Console.WriteLine("OWAPI-UPDATE OK");
                                return;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("OWAPI 404 EX");
                    }

                }
            }
            catch (Exception)
            {
                Console.WriteLine("OWAPI-UPDATE -- EX");
                //return null;
            }

        }
    }
}