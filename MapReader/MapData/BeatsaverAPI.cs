using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MapReader.MapData
{
    internal class BeatsaverAPI
    {
        public static string MapDownloader(string mapHash)
        {
            string beatSaverMapURL = $"https://api.beatsaver.com/maps/hash/{mapHash}";
            string zipFileLocation = @$"C:\Users\BigGuy\Desktop\prog\BS-Star-Rating-Tool\MapReader\Test Maps\{mapHash}.zip";
            string unzippedFileLocation = @$"C:\Users\BigGuy\Desktop\prog\BS-Star-Rating-Tool\MapReader\Test Maps\{mapHash}";

            if (File.Exists(unzippedFileLocation))
                return unzippedFileLocation;

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, beatSaverMapURL);

            HttpResponseMessage response = client.Send(request);

            // Get whether or not beatmap exists
            if (response.IsSuccessStatusCode)
            {
                // Get Beatsaver Json Data
                using StreamReader reader = new StreamReader(response.Content.ReadAsStream());
                string json = reader.ReadToEnd();
                JObject jsonData = JObject.Parse(json);

                // Get Beatsaver Map download URL
                var mapDownloadURL = jsonData.Value<JArray>("versions")[0].Value<string>("downloadURL");

                // Download Map .zip
                WebClient downloadMap = new WebClient();
                downloadMap.DownloadFile(mapDownloadURL, zipFileLocation);
            
                // Unzip beatmap file
                try
                {
                    ZipFile.ExtractToDirectory(zipFileLocation, unzippedFileLocation);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Map data already exists, continuing");
                }

                return unzippedFileLocation;
            }
            else
            {
                Console.WriteLine(response);
            }

            return "null";
        }
    }
}
