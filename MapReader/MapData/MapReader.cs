using MapReader.Evaluators;
using MapReader.MapData;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;
using JoshaParity;
using Microsoft.VisualBasic.ApplicationServices;
using System.Linq;
using MapReader.Scoring_Data;
using MapReader.Other;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Newtonsoft.Json;
using System;

namespace MapReader.MapReader
{
    public class MapReader
    {
        protected virtual int Version => 20250212;

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

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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
            catch (Exception e)
            {
                Console.WriteLine("Error, could not find beat map");
                return;
            }

            BeatmapDifficultyRank difficulty = usingScoreSaberAPI ? (BeatmapDifficultyRank)ScoreSaberAPI.beatMapDifficulty : (BeatmapDifficultyRank)beatMapDifficulty;
            DiffAnalysis mapDifficulty = beatMap.GetDiffAnalysis(difficulty);
            List<SwingData> beatMapNotes = mapDifficulty.GetSwingData();

            beatmapName = beatMap.MapInfo._songName;
            BPM = beatMap.MapInfo._beatsPerMinute * Modifiers.speedMultiplier;

            // Console.WriteLine($"Successfully Fetched beatmap {beatmapName} by {beatMap.MapInfo._songAuthorName}");
            // Console.WriteLine($"Score data:" +
            //     $"\nAccuracy: {rawAccuracy}" +
            //     $"\nPre-Swing: {preSwing}, Post-Swing: {postSwing}" +
            //     $"\nMiss Count: {misses.Count()}");

            int numberOfDifficulties = beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps.Length;

            // Refactor this PLEASE
            NJS = difficulty == BeatmapDifficultyRank.Easy ? beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps[Math.Max(0, numberOfDifficulties - 5)]._noteJumpMovementSpeed :
                difficulty == BeatmapDifficultyRank.Normal ? beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps[Math.Max(0, numberOfDifficulties - 4)]._noteJumpMovementSpeed :
                difficulty == BeatmapDifficultyRank.Hard ? beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps[Math.Max(0, numberOfDifficulties - 3)]._noteJumpMovementSpeed :
                difficulty == BeatmapDifficultyRank.Expert ? beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps[Math.Max(0, numberOfDifficulties - 2)]._noteJumpMovementSpeed :
                beatMap.MapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps[Math.Max(0, numberOfDifficulties - 1)]._noteJumpMovementSpeed;

            NJS *= Modifiers.speedMultiplier;

            // Console.WriteLine("\n" + BPM + "bpm");
            // Console.WriteLine(NJS + "njs");

            // Sort by note beat
            beatMapNotes.Sort((a, b) =>
            {
                float result = a.swingStartBeat - b.swingStartBeat;
                if (result < 0) return -1;
                else if (result > 0) return 1;
                else return 0;
            });

            List<BeatMapHitObject> notes = new List<BeatMapHitObject>();
            List<double> timePoints = new List<double>();

            for (int currentNote = 0; currentNote < beatMapNotes.Count; currentNote++)
            {
                notes.Add(new BeatMapHitObject(notes, beatMapNotes[currentNote].rightHand, notes.Count(), beatMapNotes[currentNote].swingStartBeat,
                    beatMapNotes[currentNote].swingEndBeat, beatMapNotes[currentNote].startPos, beatMapNotes[currentNote].endPos, beatMapNotes[currentNote].swingParity, false, BPM));

                timePoints.Add(beatMapNotes[currentNote].swingStartBeat / BPM);

                if (beatMapNotes[currentNote].notes.Count > 1)
                {
                    for (int currnetSliderNote = 1; currnetSliderNote < beatMapNotes[currentNote].notes.Count; currnetSliderNote++)
                    {
                        PositionData currentSliderNotePosition = new();
                        currentSliderNotePosition.x = beatMapNotes[currentNote].notes[currnetSliderNote].x;
                        currentSliderNotePosition.y = beatMapNotes[currentNote].notes[currnetSliderNote].y;
                        currentSliderNotePosition.rotation = beatMapNotes[currentNote].startPos.rotation;

                        notes.Add(new BeatMapHitObject(notes, beatMapNotes[currentNote].rightHand, notes.Count(), beatMapNotes[currentNote].swingStartBeat,
                            beatMapNotes[currentNote].swingEndBeat, currentSliderNotePosition, beatMapNotes[currentNote].endPos, beatMapNotes[currentNote].swingParity, true, BPM));

                        timePoints.Add(beatMapNotes[currentNote].swingStartBeat / BPM);
                    }
                }
            }

            Score score = new Score(preSwing, postSwing, rawAccuracy, misses, beatMapNotes.Count());

            (List<double>, List<double>) relevantMapStrains = CalculateStrainsFromNotes.CalculateStrains(notes);
            List<double> angleStrains = relevantMapStrains.Item1;
            List<double> staminaStrains = relevantMapStrains.Item2;

            GetStrainPoints.GetStrains(timePoints, angleStrains, staminaStrains);

            (double, double) starRatingAndPP = PerformanceCalculator.computePerformancePoints(score, angleStrains, staminaStrains);
            Console.WriteLine($"Final Star Rating for {beatmapName}: {Math.Round(starRatingAndPP.Item1, 2)} Stars");
            Console.WriteLine($"Final Performance Value for {Math.Round(score.ScoreAccuracy * 100, 2)}% with {score.misses.Count()}x Misses : {Math.Round(starRatingAndPP.Item2, 2)}pp\n");
            
            stopwatch.Stop();

            float elapsedTime = stopwatch.ElapsedMilliseconds;
            // Console.WriteLine($"Calculated beatmap difficulty in {Math.Round(elapsedTime / 1000, 2)} Seconds");

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