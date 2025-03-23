using MapReader.MapReader;

namespace MapReader.Difficulty.Evaluators
{
    internal class RhythmEvaluator
    {
        private const double _SkillMultiplier = 0.197;
        private const int _MaximumTimeToEvaluate = 3 * 1000; // 3 Seconds

        /// <summary>
        /// Evaluates the rhythmic difficulty of the current pattern.
        /// </summary>
        public static double evaluateDifficultyOf(BeatMapHitObject note)
        {
            (double, double) RhythmComplexities = (evaluateJumpComplexityOf(note), evaluateStreamComplexityOf(note));

            return Math.Max(RhythmComplexities.Item1, RhythmComplexities.Item2);
        }

        /// <summary>
        /// Calculates the length of a stream pattern and returns a bonus based on its length.
        /// </summary>
        public static double evaluateStreamLengthDifficultyOf(BeatMapHitObject note)
        {
            if (note.Previous(0) == null)
                return 1.0;

            int historicalNoteCount = note.noteIndex;
            int burstLength = 0;

            while (note.Previous(burstLength + 2) != null && burstLength < historicalNoteCount - 2 &&
                (note.Previous(burstLength + 1).time - note.Previous(burstLength + 2).time) * 0.75 < note.Previous(burstLength).time - note.Previous(burstLength + 1).time)
            {
                burstLength++;
            }

            double streamLengthBonus = burstLength > 256 ? 0.96 + 0.04 * (burstLength / 32) : 1.28 + Math.Log10(burstLength / 256);

            return Math.Max(1.0, streamLengthBonus);
            //return 0.5 * Math.Sqrt(4 + Math.Pow(burstLength / 256.0, 2.95));
        }

        /// <summary>
        /// Evaluates the rhythmic difficulty of a given stream pattern.
        /// </summary>
        public static double evaluateStreamComplexityOf(BeatMapHitObject note)
        {
            if (note.Previous(0) == null)
                return 1.0;

            int historicalNoteCount = note.noteIndex;

            double rhythmComplexity = 0;
            int previousBurstLength = 0;
            int burstLength = 0;

            int rhythmStart = 0;
            bool newBurst = false;

            while (note.Previous(rhythmStart + 2) != null && rhythmStart < historicalNoteCount - 2 && note.time - note.Previous(rhythmStart).time < _MaximumTimeToEvaluate)
                rhythmStart++;

            BeatMapHitObject lastNote = note.Previous(rhythmStart);
            BeatMapHitObject lastLastNote = note.Previous(rhythmStart + 1);

            for (int i = rhythmStart; i > 0; i--)
            {

                BeatMapHitObject currNote = note.Previous(i - 1);

                // If all three notes we're looking at are the same hand then we are looking at jumps.
                if (currNote.rightHand == lastNote.rightHand && lastNote.rightHand == lastLastNote.rightHand)
                {
                    lastLastNote = lastNote;
                    lastNote = currNote;

                    continue;
                }

                double currentStrain = currNote.time - lastNote.time;
                double previousStrain = lastNote.time - lastLastNote.time;

                double rhythmBonus = 1.0;

                // If the speed increases by a weird amount then we give it a bonus
                if (currentStrain * 1.1 < previousStrain)
                    rhythmBonus = 1.0 + 1 / 8 * Math.Abs(currentStrain - previousStrain);

                if (newBurst)
                {
                    if ((previousStrain * 1.25 < currentStrain || previousStrain < currentStrain * 0.75) == false)
                    {
                        burstLength++;
                    }
                    else
                    {
                        rhythmComplexity += burstLength > 1 ? rhythmBonus * Math.Sqrt(4 + burstLength) * Math.Sqrt(4 + previousBurstLength) : 0.0;

                        if (previousBurstLength + burstLength > 4 && (previousBurstLength + burstLength) % 2 == 0)
                            rhythmComplexity += rhythmBonus * Math.Sqrt(4 + burstLength + previousBurstLength);

                        rhythmComplexity *= burstLength > 8 ? burstLength / 4.0 : 1.0;
                        rhythmComplexity *= burstLength < 4 ? burstLength / 4.0 : 1.0;

                        previousBurstLength = burstLength;

                        if (previousStrain < currentStrain * 0.75) // Speed Decreasing
                            newBurst = false;

                        if (!newBurst)
                            burstLength = 1;
                    }
                }
                else if (previousStrain * 1.25 < currentStrain && currentStrain > 0.01625) // Speed Increasing
                {
                    // Bonus for up note starting bursts
                    if (note.handSwingDirection == JoshaParity.Parity.Backhand)
                        rhythmComplexity += rhythmBonus * Math.Sqrt(4 + previousBurstLength);

                    newBurst = true;
                    burstLength = 1;
                }

                lastLastNote = lastNote;
                lastNote = currNote;
            }

            return (1.0 + Math.Cbrt(rhythmComplexity)) * _SkillMultiplier;
        }

        /// <summary>
        /// Evaluates the rhythmic difficulty of a given jump pattern.
        /// </summary>
        public static double evaluateJumpComplexityOf(BeatMapHitObject note)
        {
            if (note.Previous(0) == null)
                return 1.0;

            int historicalNoteCount = note.noteIndex;

            double rhythmComplexity = 0;
            int previousBurstLength = 0;
            int burstLength = 0;

            int rhythmStart = 0;
            bool newBurst = false;

            while (note.PreviousHand(rhythmStart + 2) != null && rhythmStart < historicalNoteCount - 2 && note.time - note.PreviousHand(rhythmStart).time < _MaximumTimeToEvaluate)
                rhythmStart++;

            BeatMapHitObject lastNote = note.PreviousHand(rhythmStart);
            BeatMapHitObject lastLastNote = note.PreviousHand(rhythmStart + 1);

            for (int i = rhythmStart; i > 0; i--)
            {
                BeatMapHitObject currNote = note.PreviousHand(i - 1);

                double currentStrain = (currNote.time - lastNote.time) * 2;
                double previousStrain = (lastNote.time - lastLastNote.time) * 2;

                double rhythmBonus = 1.0;

                // If the speed increases by a weird amount then we give it a bonus
                if (currentStrain * 1.1 < previousStrain)
                    rhythmBonus = 1.0 + 1 / 8 * Math.Abs(currentStrain - previousStrain);

                if (newBurst)
                {
                    if ((previousStrain * 1.25 < currentStrain || previousStrain < currentStrain * 0.75) == false)
                    {
                        burstLength++;
                    }
                    else
                    {
                        rhythmComplexity += rhythmBonus * Math.Sqrt(4 + burstLength) * Math.Sqrt(4 + previousBurstLength);

                        if (burstLength % 2 == 1)
                            rhythmComplexity += rhythmBonus * Math.Sqrt(4 + burstLength);

                        rhythmComplexity *= burstLength > 8 ? burstLength / 4.0 : 1.0;
                        rhythmComplexity *= burstLength < 4 ? burstLength / 4.0 : 1.0;

                        previousBurstLength = burstLength;

                        if (previousStrain < currentStrain * 0.75) // Speed Decreasing
                            newBurst = false;

                        burstLength = 1;
                    }
                }
                else if (previousStrain * 1.25 < currentStrain) // Speed Increasing
                {
                    newBurst = true;
                    burstLength = 1;
                }

                lastLastNote = lastNote;
                lastNote = currNote;
            }

            return (1.0 + Math.Cbrt(rhythmComplexity)) * _SkillMultiplier;
        }
    }
}
