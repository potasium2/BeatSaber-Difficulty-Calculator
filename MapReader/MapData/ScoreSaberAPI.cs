using Newtonsoft.Json.Linq;

namespace MapReader.MapData
{
    internal class ScoreSaberAPI
    {
        public static int beatMapDifficulty;
        public static string ScoreSaberHashGrabber(int mapIndex, bool sorting, int sortDirection)
        {
            string scoreSaberLeaderboardsURL;
            if (sorting)
            {
                scoreSaberLeaderboardsURL = $"https://scoresaber.com/api/leaderboards?ranked=true&category=3&sort={sortDirection}&page={Math.Ceiling(((float)mapIndex + 1) / 14)}";
            }
            else
            {
                scoreSaberLeaderboardsURL = $"https://scoresaber.com/api/leaderboards?ranked=true&page={Math.Ceiling(((float)mapIndex + 1) / 14)}";
            }

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, scoreSaberLeaderboardsURL);

            HttpResponseMessage response = client.Send(request);

            if (response.IsSuccessStatusCode)
            {
                using StreamReader reader = new StreamReader(response.Content.ReadAsStream());
                string json = reader.ReadToEnd();
                JObject jsonData = JObject.Parse(json);

                var maps = jsonData.Value<JArray>("leaderboards")[mapIndex % 14];
                var mapHash = maps.Value<string>("songHash");

                beatMapDifficulty = maps.Value<JObject>("difficulty").Value<int>("difficulty");
                return mapHash;
            }
            else
            {
                Console.WriteLine(response);
            }

            return "null";
        }
    }
}
