using MapReader.Other;

namespace MapReader.Difficulty
{
    internal class DifficultyCalculator
    {
        private const double DifficultyMuiltiplier = 1.0; // This value helps keep Star Rating values and PP Values much closer to were people would expect them to be - Ideally it should be kept at or around 1.0

        /// <summary>
        /// Calculates the raw difficulty of a skill based on the peak strains of that given skillset
        /// </summary>
        /// <param name="peaks"></param>
        /// <returns></returns>
        public static double CalculateDifficultyOf(List<double> peaks)
        {
            double difficulty = 0.0;
            int ReducedSectionCount = 36;
            double ReducedStrainBaseline = 0.75;
            double decayWeight = 0.95;
            double weight = 1.0;


            List<double> strains = peaks.OrderByDescending(x => x).ToList();

            for (int i = 0; i < Math.Min(strains.Count, 10); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((float)i / ReducedSectionCount, 0, 1)));
                strains[i] *= Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale);
            }

            foreach (double strain in strains.OrderByDescending(x => x))
            {
                difficulty += strain * weight;
                weight *= decayWeight;
            }

            return difficulty * DifficultyMuiltiplier;
        }
    }
}
