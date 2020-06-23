using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace BobmermanParser
{
    class Program
    {
        static string url = $"https://botchallenge.cloud.epam.com/codenjoy-balancer/rest/score/day/2020-06-{DateTime.Now.Date.Day}";

        static string jsonPath = $"c:/temp/bomberman/a-stat-{DateTime.Now.ToShortDateString().Replace('/', '_')}.txt";
        //static string jsonPath = $"c:/temp/bomberman/a-stat-6_19_2020.txt";
        static string myCsvPath = $"c:/temp/bomberman/a-stat-my-{DateTime.Now.ToShortDateString().Replace('/', '_')}.csv";
        static string myShortCsvPath = $"c:/temp/bomberman/a.csv";
        //static string myId = "y2xvmwbn1tkpur93x38n";
        private static string myId = "uzeem6y5o57fwix75qum";

        static void Main(string[] args)
        {
            var onlyGenerateCsv = true;
            var getNewData = false;

            while (true)
            {
                var jsonDict = new Dictionary<string, List<ResponseObj>>();

                if (!File.Exists(jsonPath))
                    File.Create(jsonPath);

                using (var streamReader = new StreamReader(jsonPath))
                {
                    var jsonStr = streamReader.ReadToEnd();

                    if (!string.IsNullOrEmpty(jsonStr))
                    {
                        jsonDict = JsonConvert.DeserializeObject<Dictionary<string, List<ResponseObj>>>(jsonStr);
                    }
                }

                if (getNewData)
                {
                    using (var client = new HttpClient())
                    {
                        var response = client.GetStringAsync(url).Result;

                        var data = JsonConvert.DeserializeObject<List<ResponseObj>>(response);

                        var newDict = data.ToDictionary(d => d.Id, d => d);

                        foreach (var newItem in newDict)
                        {
                            if (!jsonDict.ContainsKey(newItem.Key))
                            {
                                jsonDict[newItem.Key] = new List<ResponseObj>();
                            }

                            jsonDict[newItem.Key].Add(newItem.Value);
                        }
                    }

                    using (var streamWriter = new StreamWriter(jsonPath))
                    {
                        var jsonStr = JsonConvert.SerializeObject(jsonDict);
                        streamWriter.Write(jsonStr);
                    }
                }

                var orderedDict = jsonDict.Values
                    .ToList()
                    .Where(x => x[0].Id != myId)
                    .OrderByDescending(x => GetUserLastScore(x))
                    .ToList();

                if (File.Exists(myShortCsvPath))
                    File.Delete(myShortCsvPath);

                using (var streamWriter = new StreamWriter(myShortCsvPath, true))
                {
                    var top0 = orderedDict[0][0].Id;
                    var top1 = orderedDict[1][0].Id;
                    var top2 = orderedDict[2][0].Id;

                    streamWriter.Write(GenerateScoresLine(jsonDict, myId, top0, top1, top2));
                }

                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} processed");

                if (onlyGenerateCsv)
                    return;

                Thread.Sleep(30000);
            }
        }

        private static void AddLeadingZeroes(List<ResponseObj> list, int maxLength)
        {
            if (list.Count == maxLength)
                return;

            var itemsToAdd = maxLength - list.Count;

            for (int i = 0; i < itemsToAdd; i++)
            {
                list.Insert(0, new ResponseObj());
            }


        }

        private static string GenerateScoresLine(Dictionary<string, List<ResponseObj>> jsonDict, string id, string id0, string id1, string id2)
        {
            var scores = jsonDict[id].OrderBy(x => x.Score).ToList();
            var scores0 = jsonDict[id0].OrderBy(x => x.Score).ToList();
            var scores1 = jsonDict[id1].OrderBy(x => x.Score).ToList();
            var scores2 = jsonDict[id2].OrderBy(x => x.Score).ToList();


            var lengths = new List<int> {scores.Count, scores0.Count, scores1.Count, scores2.Count};
            var maxLength = lengths.Max();

            AddLeadingZeroes(scores, maxLength);
            AddLeadingZeroes(scores0, maxLength);
            AddLeadingZeroes(scores1, maxLength);
            AddLeadingZeroes(scores2, maxLength);

            string jsonStr = string.Empty;

            var tempScores = new int[4];
            var breakpoint = 10;

            for (var i = 0; i < scores.Count; i++)
            {
                /*
                if (i % breakpoint == 0)
                {
                    tempScores[0] = scores[i].Score;
                    tempScores[1] = scores0[i].Score;
                    tempScores[2] = scores1[i].Score;
                    tempScores[3] = scores2[i].Score;
                }
                
                

                var diffScores = new int[4];
                diffScores[0] = scores[i].Score - tempScores[0];
                diffScores[1] = scores0[i].Score - tempScores[1];
                diffScores[2] = scores1[i].Score - tempScores[2];
                diffScores[3] = scores2[i].Score - tempScores[3];
                

                jsonStr += $"{scores[i]}\t{diffScores[0]}\t{scores0}\t{diffScores[1]}\t{scores1[i]}\t{diffScores[2]}\t{scores2[i]}\t{diffScores[3]}\r\n";
                */
                
               ///*
                if (i % breakpoint == 0)
                {
                    jsonStr += $"{scores[i]}\t{scores0[i]}\t{scores1[i]}\t{scores2[i]}\r\n";
                }
                //*/
            }

            return jsonStr;
        }

        private static int GetUserLastScore(List<ResponseObj> userObjList)
        {
            return userObjList.Max(x => x.Score);
        }
    }
}
