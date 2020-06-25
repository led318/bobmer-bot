using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
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
        static string myId = "y2xvmwbn1tkpur93x38n";
        private static string myId1 = "uzeem6y5o57fwix75qum";
        private static string myId2 = "gm5oa7jthist6i1dxist";

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
                    .Where(x => x[0].Id != myId && x[0].Id != myId1 && x[0].Id != myId2)
                    .OrderByDescending(x => GetUserLastScore(x))
                    .ToList();

                if (File.Exists(myShortCsvPath))
                    File.Delete(myShortCsvPath);

                
                using (var streamWriter = new StreamWriter(myShortCsvPath, true))
                {
                    var top0 = orderedDict[0][0].Id;
                    var top1 = orderedDict[1][0].Id;
                    var top2 = orderedDict[2][0].Id;
                    var top3 = orderedDict[3][0].Id;

                    streamWriter.Write(GenerateScoresLine(jsonDict, myId, myId1, myId2, top0, top1, top2, top3));
                }
                

                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} processed");

                if (onlyGenerateCsv)
                    return;

                Thread.Sleep(30000);
            }
        }

        private static string GenerateScoresLine(Dictionary<string, List<ResponseObj>> jsonDict, string myId, string myId1, string myId2, string id0, string id1, string id2, string id3)
        {
            var scores = jsonDict[myId].OrderBy(x => x.Score).ToList();
            var scoresMy1 = jsonDict[myId1].OrderBy(x => x.Score).ToList();
            var scoresMy2 = jsonDict[myId2].OrderBy(x => x.Score).ToList();
            var scores0 = jsonDict[id0].OrderBy(x => x.Score).ToList();
            var scores1 = jsonDict[id1].OrderBy(x => x.Score).ToList();
            var scores2 = jsonDict[id2].OrderBy(x => x.Score).ToList();
            var scores3 = jsonDict[id3].OrderBy(x => x.Score).ToList();

            var lengths = new List<int> { scores.Count, scoresMy1.Count, scoresMy2.Count, scores0.Count, scores1.Count, scores2.Count, scores3.Count };
            var maxLength = lengths.Max();

            AddLeadingZeroes(scores, maxLength);
            AddLeadingZeroes(scoresMy1, maxLength);
            AddLeadingZeroes(scoresMy2, maxLength);
            AddLeadingZeroes(scores0, maxLength);
            AddLeadingZeroes(scores1, maxLength);
            AddLeadingZeroes(scores2, maxLength);
            AddLeadingZeroes(scores3, maxLength);

            string jsonStr = string.Empty;

            var tempScores = new int[5];
            var breakpoint = 10;

            for (var i = 0; i < scores.Count; i++)
            {
                /*
                if (i % breakpoint == 0)
                {
                    tempScores[0] = scores[i].Score;
                    tempScores[1] = scoresMy1[i].Score;
                    tempScores[2] = scores0[i].Score;
                    tempScores[3] = scores1[i].Score;
                    tempScores[4] = scores2[i].Score;
                }
                
                

                var diffScores = new int[5];
                diffScores[0] = scores[i].Score - tempScores[0];
                diffScores[1] = scoresMy1[i].Score - tempScores[1];
                diffScores[2] = scores0[i].Score - tempScores[2];
                diffScores[3] = scores1[i].Score - tempScores[3];
                diffScores[4] = scores2[i].Score - tempScores[4];
                

                jsonStr += $"{scores[i]}\t{diffScores[0]}\t{scoresMy1[i]}\t{diffScores[1]}\t{scores0}\t{diffScores[2]}\t{scores1[i]}\t{diffScores[3]}\t{scores2[i]}\t{diffScores[4]}\r\n";
                */

                
                if (i % breakpoint == 0)
                {
                    jsonStr += $"{scores[i]}\t{scoresMy1[i]}\t{scoresMy2[i]}\t{scores0[i]}\t{scores1[i]}\t{scores2[i]}\t{scores3[i]}\r\n";
                }
                
            }

            return jsonStr;
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

        private static int GetUserLastScore(List<ResponseObj> userObjList)
        {
            return userObjList.Max(x => x.Score);
        }
    }
}
