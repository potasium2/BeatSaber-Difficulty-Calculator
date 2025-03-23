using MapReader.MapReader;

namespace MapReader.Difficulty.Evaluators
{
    internal class StaminaEvaluator
    {
        private const double _SkillMultiplier = 0.257;

        private const double _MinimumDoubleTime = 0.01625;
        private const double _DoubleStrainBonus = 0.215;

        private static double _LongestDoubleLength = 0;
        private static double _PreviousHandStrainTime = 0;

        /// <summary>
        /// Evaluates the stamina required to hit this <paramref name="note"/>.
        /// </summary>
        public static double evaluateDifficultyOf(BeatMapHitObject note)
        {
            if (note.PreviousHand(0) == null)
                return 0;

            bool noteIsDouble = false;
            double sameHandStrainTime = note.PreviousHand(0) == null ? 0 : note.time - note.PreviousHand(0).time;
            double oppositeHandStrainTime = note.PreviousOpposite(0) == null ? 0 : note.time - note.PreviousOpposite(0).time;

            if (note.isSlider)
                sameHandStrainTime = _PreviousHandStrainTime;

            if (sameHandStrainTime == 0)
                return 0;

            // Derive Stamina Rating with base bonus
            double staminaRating = sameHandStrainTime <= oppositeHandStrainTime / 2 ?
                0.5 * Math.Clamp(Math.Pow(150 / (sameHandStrainTime * 2), 1.0), 1, 2) :
                0.5 * Math.Clamp(Math.Pow(150 / (oppositeHandStrainTime * 2), 1.0), 1, 2);

            if (sameHandStrainTime > 200)
                staminaRating *= Math.Min(1, 200 / (sameHandStrainTime * 2));
            
            if (oppositeHandStrainTime <= _MinimumDoubleTime)
                noteIsDouble = true;
            
            staminaRating *= DistanceEvaluator.evaluateLocationOf(note);
            staminaRating *= DistanceEvaluator.evaluateMovementOf(note);
            
            if (noteIsDouble)
            {
                staminaRating *= DistanceEvaluator.evaluateDistanceOf(note) / 1.35;
                staminaRating *= Math.Min(1, sameHandStrainTime * _DoubleStrainBonus);
            }
            else
            {
                staminaRating *= DistanceEvaluator.evaluateDistanceOf(note);
                staminaRating *= RhythmEvaluator.evaluateDifficultyOf(note);
                staminaRating *= Math.Clamp(RhythmEvaluator.evaluateStreamLengthDifficultyOf(note), 1.0, 2.0);
            }
            
            if (note.isSlider)
                staminaRating *= SliderEvaluator.evaluateSlider(note);

            if (!note.isSlider)
                _PreviousHandStrainTime = sameHandStrainTime;

            return staminaRating * _SkillMultiplier;
        }

        public static void globalReset()
        {
            _LongestDoubleLength = 0;
            _PreviousHandStrainTime = 0;
        }
    }
}
