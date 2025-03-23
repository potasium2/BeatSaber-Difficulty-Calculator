using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JoshaParity;
using MapReader.Evaluators;
using MapReader.MapData;
using MapReader.MapReader;

namespace MapReader
{
    internal class PerformanceCalculator
    {
        private const double baseMultiplier = 1.0; // In the case that general PP Values feel a bit low, we multiply them by this base multiplier - Ideally it should be kept at or around 1.0

        private static int mapNoteCount;
        private static int totalHits;

        private static double peakAngleStrain;
        private static double peakStaminaStrain;

        public static double angleDifficulty;
        public static double staminaDifficulty;

        public static double anglePerformance;
        public static double staminaPerformance;
        public static double mapStarRating;

        private static List<double> angleMissDifficulty = new List<double>();
        private static List<double> staminaMissDifficulty = new List<double>();

        /// <summary>
        /// Computes the star rating of a map by computing the difficulty of notes in terms of both Technicality and Stamina requirements.
        /// </summary>
        public static double computeBeatMapStarRating(Score score, List<double> angleStrains, List<double> staminaStrains)
        {
            foreach (int miss in score.misses)
            {
                angleMissDifficulty.Add(angleStrains[miss]);
                staminaMissDifficulty.Add(staminaStrains[miss]);
            }

            // Get Peak Difficulty
            List<double> orderedAngleStrains = angleStrains.OrderByDescending(x => x).ToList();
            List<double> orderedStaminaStrains = staminaStrains.OrderByDescending(x => x).ToList();

            peakAngleStrain = orderedAngleStrains[0];
            peakStaminaStrain = orderedStaminaStrains[0];

            angleDifficulty = DifficultyCalculator.CalculateDifficultyOf(angleStrains);
            staminaDifficulty = DifficultyCalculator.CalculateDifficultyOf(staminaStrains);

            mapStarRating = angleDifficulty + staminaDifficulty + (Math.Abs(angleDifficulty - staminaDifficulty) / 2);

            // Console.WriteLine($"\nAngle Difficulty: {Math.Round(angleDifficulty, 2)} Stars");
            // Console.WriteLine($"Stamina Difficulty: {Math.Round(staminaDifficulty, 2)} Stars\n");

            return mapStarRating;
        }

        /// <summary>
        /// Computes the performance value of a map based on Skill based Star Ratings, Overall Accuracy, Modifiers, etc.
        /// </summary>
        public static (double, double) computePerformancePoints(Score score, List<double> angleStrains, List<double> staminaStrains)
        {
            mapNoteCount = score.maxAcheivableCombo;
            totalHits = score.totalHits;

            double finalMapStarRating = computeBeatMapStarRating(score, angleStrains, staminaStrains);
            anglePerformance = computeAnglePerformance(score, angleDifficulty);
            staminaPerformance = computeStaminaPerformance(score, staminaDifficulty);

            double finalPerformanceValue = anglePerformance + staminaPerformance;

            // Console.WriteLine("Angle PP: " + Math.Round(anglePerformance, 2));
            // Console.WriteLine("Stamina PP: " + Math.Round(staminaPerformance, 2) + "\n");

            return (finalMapStarRating, finalPerformanceValue);
        }

        private static double computeAnglePerformance(Score score, double angleRating)
        {
            double angleValue = 1.0;
            angleValue = convertDifficultyToPerformance(angleRating);

            // Give bonus to longer maps (May switch to Osu's Length Bonus formula if this one feels to weak)
            double lengthBonus = calculateLengthBonus((int)totalHits);
            angleValue *= lengthBonus;

            // Scale Angle Value based on NJS
            double NJSBonus = 0.0;
            if (MapReader.MapReader.NJS > 23)
                NJSBonus = 0.04 * (MapReader.MapReader.NJS - 23);

            if (MapReader.MapReader.NJS < 16)
                NJSBonus = 0.08 * (16 - MapReader.MapReader.NJS);

            angleValue *= 1.0 + NJSBonus * lengthBonus;

            // Buff GN for lower NJS
            if (Modifiers.ghostNotes)
                angleValue *= 1.0 + 0.05 * Math.Pow(28 - MapReader.MapReader.NJS, 0.35);

            // We don't care as much about swing when comparing to the given angles within a map so we just use a static curve
            angleValue *= Math.Min(1, 1.0025 * Math.Pow((score.preSwing + score.postSwing) / 50, 0.25));

            double weightedAccuracy = calculateAccuracyFromCurve(angleRating, score.NonMissAccuracy);

            if (lengthBonus < 1)
                weightedAccuracy *= lengthBonus;

            angleValue *= weightedAccuracy;

            return angleValue * calculateSRBasedMissPenalty(angleMissDifficulty, peakAngleStrain) * baseMultiplier;
        }

        private static double computeStaminaPerformance(Score score, double staminaRating)
        {
            double staminaValue = 1.0;
            staminaValue = convertDifficultyToPerformance(staminaRating);

            // Give bonus to longer maps (May switch to Osu's Length Bonus formula if this one feels to weak)
            double lengthBonus = calculateLengthBonus((int)totalHits);
            staminaValue *= lengthBonus;

            // Scale Stamina Value based on NJS
            double NJSBonus = 0.0;
            if (MapReader.MapReader.NJS > 23)
                NJSBonus = 0.04 * (MapReader.MapReader.NJS - 23);

            staminaValue *= 1.0 + NJSBonus * lengthBonus;

            // Buff GN for lower NJS
            if (Modifiers.ghostNotes)
                staminaValue *= 1.0 + 0.05 * Math.Pow(28 - MapReader.MapReader.NJS, 0.35);

            // High swing is much harder to achieve on speed maps so we give a slight buff for full swing, as well as use a more exponential curve
            staminaValue *= Math.Min(1, 1.02 * Math.Pow((score.preSwing + score.postSwing) / 50, 1.75));

            double weightedAccuracy = calculateAccuracyFromCurve(staminaRating, score.NonMissAccuracy);

            if (lengthBonus < 1)
                weightedAccuracy *= lengthBonus;

            staminaValue *= weightedAccuracy;

            return staminaValue * calculateSRBasedMissPenalty(staminaMissDifficulty, peakStaminaStrain) * baseMultiplier;
        }

        private static double convertDifficultyToPerformance(double difficulty) => Math.Pow(50 * Math.Max(1, difficulty / 0.35), 0.8);

        public static double calculateLengthBonus(int totalNoteHits) => totalNoteHits < 2000 ? 0.95 + 0.05 * (totalNoteHits / 2000.0) : 1.0 + Math.Log10(totalNoteHits / 2000.0) * 0.1;

        private static double calculateAccuracyFromCurve(double difficulty, double accuracy)
        {
            // In the event a skills difficulty rating is >9 the curve ends up wrapping around
            // This just prevents you from gaining basically free PP for doing nothing
            double difficultyCalc = Math.Min(6, difficulty);
            double accuracyCurve = accuracy < 0.95 ? 
                Math.Pow(Math.Pow(accuracy + 0.05, 2), Math.Log(10 - difficultyCalc)) :
                Math.Pow(Math.Pow(accuracy + 0.05, 3.65), Math.Log(Math.Pow(difficultyCalc, 3.65)));

            return accuracyCurve;
        }

        // Old Combo Scaling formula initially used before obtaining miss data
        private static double calculateComboScaling(int maxCombo) => maxCombo < 1 ? 0 : Math.Min(1, Math.Pow(Math.Pow(maxCombo, 0.525) / Math.Pow(mapNoteCount, 0.5), 0.7));

        /// <summary>
        /// Returns the Miss Penalty based on the difficulty strain of where a given miss occurs compared to the peak strain within the beatmap.
        /// </summary>
        private static double calculateSRBasedMissPenalty(List<double> missStrains, double peakStrainDifficulty) 
        {
            double missPenalty = 1.0;
            missStrains.Sort();
            missStrains.Reverse();

            // We weight each miss so the miss at peak difficulty gets weighted at 100% and each subsequent miss gets weighted ~33% less
            for (int i = 0; i < missStrains.Count(); i++)
            {
                double additionalMissPenalty = i > 0 ? Math.Pow(1 / (i), 3) : 0.0;
                missPenalty -= (missStrains[i] / peakStrainDifficulty) / (5 * Math.Log(i + 2));
                missPenalty -= additionalMissPenalty / 128;
            }

            // This caps the minimum miss penalty so you don't get 'rewarded' negative PP
            return Math.Max(0, missPenalty);
        }

        public static void globalReset()
        {
            peakAngleStrain = 0;
            peakStaminaStrain = 0;
            angleDifficulty = 0;
            staminaDifficulty = 0;
            mapStarRating = 0;

            anglePerformance = 0;
            staminaPerformance = 0;

            mapNoteCount = 0;
            totalHits = 0;

            angleMissDifficulty.Clear();
            staminaMissDifficulty.Clear();
        }
    }
}
