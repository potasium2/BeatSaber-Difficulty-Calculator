using JoshaParity;
using MapReader.MapData;
using MapReader.MapReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapReader.Evaluators
{
    internal class AngleEvaluator
    {
        private static double _AngleStrictness = Modifiers.angleLeniency; // Defines the angle strictness, Base game default is 60 Degrees and SA is 45 Degrees
        private static int _AngleStrainCount = 0;
        private static double _PreviousStrain = 0;

        private const int _MaximumAngleStrainBonus = 10;

        private const double _SkillMultiplier = 1.042;

        /// <summary>
        /// Evaluates the angle difficulty of this <paramref name="note"/> of a beatmap.
        /// </summary>
        public static double evaluateDifficultyOf(BeatMapHitObject note)
        {
            if (note.NextHand(0) == null)
                return 0;

            double angleRating = 1.0;
            double strainTime = note.NextHand(0).time - note.time;
            float noteAngle = note.startPosition.rotation;
            float angleChange = note.startPosition.rotation - note.NextHand(0).startPosition.rotation;

            // Dot Notes require substantially less difficulty to hit them in angle so we give them a slight nerf
            double angleDifficulty = _AngleStrictness;
            double angleStrictnessDifficulty = 0.5 * Math.Pow(45 / angleDifficulty, 1.0 / 2.0) * Math.Pow(150.0 / strainTime, 1.0 / 4.0);

            if (strainTime == 0)
                return 0;

            // Negative Value Angles require less leaning, Positive Value Angles require more leaning
            angleRating *= 0.5 - 0.5 * Math.Sin(1.0 / 2.0 * getRadianAngle(Math.Min(0, noteAngle))); // Angles that require more leaning get buffed more
            angleRating *= 0.5 + 0.5 * Math.Pow(Math.Sin(1.0 / 2.0 * getRadianAngle(Math.Max(0, noteAngle))), 2); // Angles that require virtually no lean get buffed less

            angleRating *= 0.5 + Math.Pow(Math.Sin(getRadianAngle(angleChange)), 2) * angleStrictnessDifficulty;

            double AngleStrain = 1.0;
            if (Math.Abs(angleChange) > 90)
                AngleStrain = Math.Abs(angleChange) / 90 * angleStrictnessDifficulty;

            double flickPenalty = strainTime >= _PreviousStrain ? 1 : 300 / strainTime;

            angleRating *= DistanceEvaluator.evaluateDistanceOf(note) * Math.Clamp(RhythmEvaluator.evaluateStreamLengthDifficultyOf(note), 1.0, 2.0);
            angleRating *= DistanceEvaluator.evaluateLocationOf(note);
            angleRating *= DistanceEvaluator.evaluateCirclingOf(note);

            angleRating *= RhythmEvaluator.evaluateJumpComplexityOf(note) / flickPenalty;
            angleRating *= SliderEvaluator.evaluateSlider(note);

            // We scale the angle rating slightly based on NJS as higher NJS makes harsher Angles harder to read
            angleRating *= 1 + Math.Pow(Math.Clamp((MapReader.MapReader.NJS - 23.5) / 3, 0, 0.15), 1.5);
            angleRating *= 0.125 + Math.Clamp(150 / (strainTime * 2), 0.875, 1.5);

            if (!note.isSlider)
                _PreviousStrain = strainTime;

            return angleRating * AngleStrain * _SkillMultiplier;
        }

        static double getRadianAngle(double noteAngle)
        {
            return (Math.PI * noteAngle) / 180;
        }

        public static void globalReset()
        {
            _AngleStrainCount = 0;
            _PreviousStrain = 0;
        }
    }
}
