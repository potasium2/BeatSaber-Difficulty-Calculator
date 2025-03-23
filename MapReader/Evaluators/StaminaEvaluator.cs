using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JoshaParity;
using MapReader.MapReader;

namespace MapReader.Evaluators
{
    internal class StaminaEvaluator
    {
        private const double _SkillMultiplier = 0.211;
        private static double _LongestDoubleLength = 0;
        private static double _PreviousHandStrainTime = 0;

        /// <summary>
        /// Evaluates the stamina required to hit this <paramref name="note"/>.
        /// </summary>
        public static double evaluateDifficultyOf(BeatMapHitObject note)
        {
            bool noteIsDouble = false;
            double sameHandStrainTime = note.NextHand(0) == null ? 0 : note.NextHand(0).time - note.time;
            double oppositeHandStrainTime = note.NextOpposite(0) == null ? 0 : note.NextOpposite(0).time - note.time;

            if (note.isSlider)
                sameHandStrainTime = _PreviousHandStrainTime;

            if (sameHandStrainTime == 0)
                return 0;

            if (oppositeHandStrainTime <= 0.01625 && sameHandStrainTime < 150)
            {
                noteIsDouble = true;
                _LongestDoubleLength++;
            }
            else
            {
                _LongestDoubleLength = 0;
            }

            // Derive Stamina Rating with base bonus
            double staminaRating = sameHandStrainTime <= oppositeHandStrainTime / 2 ? 
                0.5 * Math.Clamp(Math.Pow(150 / (sameHandStrainTime * 2), 1.0), 1, 2) : 
                0.5 * Math.Clamp(Math.Pow(150 / (oppositeHandStrainTime * 2), 1.0), 1, 2);

            if (sameHandStrainTime > 200)
            {
                staminaRating *= Math.Min(1, 200 / (sameHandStrainTime * 2));
            }

            if (_LongestDoubleLength > 4)
                staminaRating *= Math.Sqrt(4 + (_LongestDoubleLength / 2));

            // Apply relevant distance and rhythm strains
            if (noteIsDouble)
                staminaRating *= DistanceEvaluator.evaluateDistanceOf(note) / 1.25;
            else
                staminaRating *= DistanceEvaluator.evaluateDistanceOf(note);

            staminaRating *= DistanceEvaluator.evaluateLocationOf(note);
            staminaRating *= DistanceEvaluator.evaluateMovementOf(note);

            if (!noteIsDouble)
            {
                staminaRating *= RhythmEvaluator.evaluateDifficultyOf(note);
                staminaRating *= Math.Clamp(RhythmEvaluator.evaluateStreamLengthDifficultyOf(note), 1.0, 2.0);
            }

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
