using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace MapReader.Other
{
    internal class GetStrainPoints
    {
        public static void GetStrains(List<double> timePoints, List<double> currentAngleRating, List<double> currentStaminaRating)
        {
            string strainFilePath = @$"D:\Desktop V2\folder of folders v3\cool code\Astrella's Accuracy Revamp & Changes Ranked PP System\star rating algorithims\MapReader\draw strains\Strain Graphs\{MapReader.MapReader.beatmapName} strains.txt";
            if (!File.Exists(strainFilePath))
            {
                try
                {
                    File.Create(strainFilePath);
                    using (StreamWriter writer = File.CreateText(strainFilePath))
                    {
                        writer.WriteLine($"[{String.Join(", ", timePoints)}]");
                        writer.WriteLine($"[{String.Join(", ", currentAngleRating)}]");
                        writer.WriteLine($"[{String.Join(", ", currentStaminaRating)}]");
                    }
                }
                catch
                {
                    // Console.WriteLine("bruh");
                }
            }
            else
            {
                File.Delete(strainFilePath);
                using (StreamWriter writer = File.CreateText(strainFilePath))
                {
                    writer.WriteLine($"[{String.Join(", ", timePoints)}]");
                    writer.WriteLine($"[{String.Join(", ", currentAngleRating)}]");
                    writer.WriteLine($"[{String.Join(", ", currentStaminaRating)}]");
                }
            }

            //string ppFilePath = @$"D:\Desktop V2\folder of folders v3\cool code\Astrella's Accuracy Revamp & Changes Ranked PP System\star rating algorithims\MapReader\draw strains\Strain Graphs\{MapReader.MapReader.beatmapName} performance.txt";
            //if (!File.Exists(ppFilePath))
            //{
            //    try
            //    {
            //        File.Create(ppFilePath);
            //        using (StreamWriter writer = File.CreateText(ppFilePath))
            //        {
            //            writer.WriteLine($"[{String.Join(", ", timePoints)}]");
            //            writer.WriteLine($"[{String.Join(", ", performancePoints)}]");
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("bruh");
            //    }
            //}
            //else
            //{
            //    File.Delete(ppFilePath);
            //    using (StreamWriter writer = File.CreateText(ppFilePath))
            //    {
            //        writer.WriteLine($"[{String.Join(", ", timePoints)}]");
            //        writer.WriteLine($"[{String.Join(", ", performancePoints)}]");
            //    }
            //}
        }
    }
}
