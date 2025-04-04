﻿using MapReader.MapData;
using MapReader.MapReader;

namespace MapReader.Difficulty.Evaluators
{
    internal class AngleEvaluator
    {
        private static double _AngleStrictness = Modifiers.angleLeniency; // Defines the angle strictness, Base game default is 60 Degrees and SA is 45 Degrees
        private static double _PreviousStrain = 0;

        private const double _SkillMultiplier = 1.216;

        /// <summary>
        /// Evaluates the angle difficulty of this <paramref name="note"/> of a beatmap.
        /// </summary>
        public static double evaluateDifficultyOf(BeatMapHitObject note)
        {
            if (note.PreviousHand(0) == null)
                return 0;

            double angleRating = 1.0;
            double strainTime = note.time - note.PreviousHand(0).time;
            float noteAngle = note.startPosition.rotation;
            float angleChange = Math.Abs(note.startPosition.rotation - note.PreviousHand(0).startPosition.rotation);

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
            if (angleChange > 90)
                AngleStrain = angleChange / 90 * angleStrictnessDifficulty;

            double flickPenalty = strainTime >= _PreviousStrain ? 1 : 300 / strainTime;

            angleRating *= DistanceEvaluator.evaluateDistanceOf(note) * Math.Clamp(RhythmEvaluator.evaluateStreamLengthDifficultyOf(note), 1.0, 2.0);
            angleRating *= DistanceEvaluator.evaluateLocationOf(note);
            angleRating *= DistanceEvaluator.evaluateCirclingOf(note);

            angleRating *= RhythmEvaluator.evaluateJumpComplexityOf(note) / flickPenalty;

            // We scale the angle rating slightly based on NJS as higher NJS makes harsher Angles harder to read
            angleRating *= 1 + Math.Pow(Math.Clamp((MapReader.MapReader.NJS - 23.5) / 3, 0, 0.15), 1.5);
            angleRating *= 0.125 + Math.Clamp(150 / (strainTime * 2), 0.875, 1.5);

            if (note.isSlider)
                angleRating *= SliderEvaluator.evaluateSlider(note);
            if (!note.isSlider)
                _PreviousStrain = strainTime;

            return angleRating * AngleStrain * _SkillMultiplier;
        }

        static double getRadianAngle(double noteAngle)
        {
            return Math.PI * noteAngle / 180;
        }

        public static void globalReset()
        {
            _PreviousStrain = 0;
        }
    }
}
