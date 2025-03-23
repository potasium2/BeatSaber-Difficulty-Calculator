using MapReader.MapReader;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapReader.MapData
{
    internal class Score
    {
        protected static double _RawAccuracy = 1;
        protected static double _PreSwing = 70;
        protected static double _PostSwing = 30;
        protected static double _NonMissAccuracy = 1.0;
        protected static double _ScoreAccuracy;
        protected static int _MaxAcheivableCombo;
        protected static int _TotalHits;
        protected static List<int> _Misses = new List<int>();

        public double rawAccuracy => _RawAccuracy;
        public double preSwing => _PreSwing;
        public double postSwing => _PostSwing;
        public double NonMissAccuracy => _NonMissAccuracy;
        public double ScoreAccuracy => _ScoreAccuracy;
        public int maxAcheivableCombo => _MaxAcheivableCombo;
        public int totalHits => _TotalHits;
        public List<int> misses => _Misses;

        public Score(double preSwing, double postSwing, double accuracy, List<int> misses, int objectCount)
        {
            _RawAccuracy = 15 * (accuracy / 15);
            _PreSwing = preSwing;
            _PostSwing = postSwing;
            _Misses = misses;
            _MaxAcheivableCombo = objectCount;
            _TotalHits = objectCount - misses.Count();

            double missPenalty = misses.Count() * 115.0 / objectCount;
            _NonMissAccuracy = (_RawAccuracy + _PreSwing + _PostSwing - missPenalty) / 115;
            _ScoreAccuracy = CalculateAccuracy(_RawAccuracy + _PreSwing + _PostSwing, misses.Count(), objectCount);
        }

        public static double CalculateAccuracy(double baseAccuracy, int misses, int objectCount)
        {
            double maxScore = (objectCount - 13) * 8 * 115 + 5611;
            double rawScore = (objectCount - 13) * 8 * baseAccuracy + 5611;
            double missedScore = 115 * 36 * misses;

            return (rawScore - missedScore) / maxScore;
        }

        public override string ToString()
        {
            return $"Preswing: {_PreSwing}, Postswing: {_PostSwing}, Total Hit Accuracy: {_NonMissAccuracy})";
        }

        public static void Reset()
        {
            _RawAccuracy = 15;
            _PreSwing = 70;
            _PostSwing = 30;
            _NonMissAccuracy = 1.0;
        }
    }
}
