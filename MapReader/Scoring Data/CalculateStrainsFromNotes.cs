using MapReader.MapReader;
using MapReader.Difficulty.Evaluators;

namespace MapReader.Scoring_Data
{
    internal class CalculateStrainsFromNotes
    {
        static List<double> angleStrains = new List<double>();
        static List<double> staminaStrains = new List<double>();
        public static (List<double>, List<double>) CalculateStrains(List<BeatMapHitObject> notes)
        {
            for (int i = 0; i < notes.Count(); i++)
            {
                double angleStrain = AngleEvaluator.evaluateDifficultyOf(notes[i]);
                double staminaStrain = StaminaEvaluator.evaluateDifficultyOf(notes[i]);

                angleStrains.Add(angleStrain);
                staminaStrains.Add(staminaStrain);
            }

            return (angleStrains, staminaStrains);
        }

        public static void resetStrains()
        {
            angleStrains.Clear();
            staminaStrains.Clear();
        }
    }
}
