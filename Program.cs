using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SpeedTest
{
    class Program
    {
        static Conf _conf;

        static async Task Main()
        {
            _conf = JsonConvert.DeserializeObject<Conf>(GetEnvValue("CONF"));
            using var client = new HttpClient();

            string confJs = await client.GetStringAsync("https://www.chinafy.com/js/poca/component/SpeedTestHelper.js");
            confJs = Regex.Match(confJs, @"locationsCfgMap:([\s\S]+?),[\s]*locationAreasMap").Groups[1].Value;
            var locCfg = JsonConvert.DeserializeObject<Dictionary<string, Loc>>(confJs);
            var locs = locCfg.Select(p => p.Value).ToList();
            Console.WriteLine("SpeedTest开始运行...");
            int i = 0;
            Random rand = new Random();
            while (i < _conf.RunTimes)
            {
                try
                {
                    int index = rand.Next(0, locs.Count);
                    Loc loc = locs[index];
                    locs.RemoveAt(index);

                    Console.Write($"{_conf.RunTimes}-{i + 1}: {loc.Name}...");
                    //测速
                    string result = await client.GetStringAsync($"https://{loc.UrlPrefix}.ultrasite.com/api2/web/performance/v2?url={_conf.Url}&network=Good3G");
                    Console.WriteLine(result);

                    await Task.Delay(rand.Next(10, _conf.DelaySeconds) * 1000);
                    i++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ex! {ex.Message}");
                }
            }
            Console.WriteLine("SpeedTest运行完毕");
        }

        static string GetEnvValue(string key) => Environment.GetEnvironmentVariable(key);
    }

    #region Conf

    public class Conf
    {
        public int DelaySeconds { get; set; } = 30;
        public string Url { get; set; }
        public int RunTimes { get; set; }
    }

    #endregion

    #region Loc

    class Loc
    {
        public string Name { get; set; }
        public string UrlPrefix { get; set; }
    }

    #endregion
}
