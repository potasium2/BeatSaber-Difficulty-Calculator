using MapReader.MapData;
using JoshaParity;
using MapReader.Scoring_Data;
using MapReader.Difficulty.Evaluators;
using MapReader.Difficulty;

namespace MapReader.MapReader
{
    public class MapReader
    {
        protected virtual int Version => 20250322;

        // Default Map Values
        public static string beatmapName = "";
        public static double BPM = 100;
        public static double NJS = 12;

        private static double preSwing = 70; // Max 70 Points
        private static double postSwing = 30; // Max 30 Points
        private static double rawAccuracy = 11.55; // Max 15 Points
        private static List<int> misses = new List<int>(); // Indicies of where Misses occur

        static bool usingScoreSaberAPI = true;
        static int beatMapDifficulty = 7;

        public static void MapReaderCalculator(int mapIndex, bool sorting)
        {
            // Takes a Beatmap Hash
            string beatMapCode = ScoreSaberAPI.ScoreSaberHashGrabber(mapIndex, sorting, 0);
            //beatMapCode = "64B81F99B76742B3C70EAC3BDA4230ED55E0D2AD";

            MapAnalyser beatMap;
            try
            {
                string beatMapLocation = BeatsaverAPI.MapDownloader(beatMapCode);

                // This is to prevent "Gathering BPM Events" from printing to console. (Thanks Josh!)
                TextWriter backupOut = Console.Out;
                Console.SetOut(TextWriter.Null);

                beatMap = new MapAnalyser(beatMapLocation);
                Console.SetOut(backupOut);
            }
            catch
            {
                Console.WriteLine("Error, could not find beat map");
                return;
            }

            BeatmapDifficultyRank difficulty = usingScoreSaberAPI ? (BeatmapDifficultyRank)ScoreSaberAPI.beatMapDifficulty : (BeatmapDifficultyRank)beatMapDifficulty;
            DiffAnalysis mapDifficulty = beatMap.GetDiffAnalysis(difficulty);
            List<SwingData> beatMapNotes = mapDifficulty.GetSwingData();

            beatmapName = beatMap.MapInfo._songName;
            BPM = beatMap.MapInfo._beatsPerMinute * Modifiers.speedMultiplier;

            int numberOfDifficulties = beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps.Length;

            NJS = beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps[Math.Max(0, (((int)difficulty - 1) / 2) - numberOfDifficulties)]._noteJumpMovementSpeed;
            NJS *= Modifiers.speedMultiplier;

            // Sort by note beat
            beatMapNotes.Sort((a, b) =>
            {
                float result = a.swingStartBeat - b.swingStartBeat;
                if (result < 0) return -1;
                else if (result > 0) return 1;
                else return 0;
            });

            List<BeatMapHitObject> notes = new List<BeatMapHitObject>();

            for (int currentNote = 0; currentNote < beatMapNotes.Count; currentNote++)
            {
                bool isSlider = false;
                bool isSliderHead = false;
                if (beatMapNotes[currentNote].notes.Count > 1)
                {
                    isSlider = true;
                    isSliderHead = true;
                }

                notes.Add(new BeatMapHitObject(notes, beatMapNotes[currentNote].rightHand, notes.Count(), beatMapNotes[currentNote].swingStartBeat,
                    beatMapNotes[currentNote].swingEndBeat, beatMapNotes[currentNote].startPos, beatMapNotes[currentNote].endPos, beatMapNotes[currentNote].swingParity, isSlider, isSliderHead, BPM));

                if (isSlider)
                {
                    for (int currnetSliderNote = 1; currnetSliderNote < beatMapNotes[currentNote].notes.Count; currnetSliderNote++)
                    {
                        isSliderHead = false;

                        PositionData currentSliderNotePosition = new();
                        currentSliderNotePosition.x = beatMapNotes[currentNote].notes[currnetSliderNote].x;
                        currentSliderNotePosition.y = beatMapNotes[currentNote].notes[currnetSliderNote].y;
                        currentSliderNotePosition.rotation = beatMapNotes[currentNote].startPos.rotation;

                        notes.Add(new BeatMapHitObject(notes, beatMapNotes[currentNote].rightHand, notes.Count(), beatMapNotes[currentNote].swingStartBeat,
                            beatMapNotes[currentNote].swingEndBeat, currentSliderNotePosition, beatMapNotes[currentNote].endPos, beatMapNotes[currentNote].swingParity, isSlider, isSliderHead, BPM));
                    }
                }
            }

            Score score = new Score(preSwing, postSwing, rawAccuracy, misses, beatMapNotes.Count());

            (List<double>, List<double>) relevantMapStrains = CalculateStrainsFromNotes.CalculateStrains(notes);
            (double, double) starRatingAndPP = PerformanceCalculator.computePerformancePoints(score, relevantMapStrains.Item1, relevantMapStrains.Item2);

            Console.WriteLine($"Final Star Rating for {beatmapName}: {Math.Round(starRatingAndPP.Item1, 2)} Stars");
            Console.WriteLine($"Final Performance Value for {Math.Round(score.ScoreAccuracy * 100, 2)}% with {score.misses.Count()}x Misses : {Math.Round(starRatingAndPP.Item2, 2)}pp\n");

            Reset();
        }

        /// <summary>
        /// Resets all variables across all evaluators
        /// </summary>
        public static void Reset()
        {
            PerformanceCalculator.globalReset();
            StaminaEvaluator.globalReset();
            AngleEvaluator.globalReset();
            DistanceEvaluator.globalReset();
            CalculateStrainsFromNotes.resetStrains();
        }
    }
}