using System.Numerics;
using MapReader.MapReader;

namespace MapReader.Difficulty.Evaluators
{
    internal class SliderEvaluator
    {
        private const double _SkillMultiplier = 0.241;
        public static double evaluateSlider(BeatMapHitObject note)
        {
            if (note.Previous(0) == null)
                return 1.0;

            BeatMapHitObject sliderHead = note.GetSliderHead(note);

            Vector2 sliderStartPosition = new Vector2(sliderHead.startPosition.x, sliderHead.startPosition.y);
            Vector2 sliderEndPosition = new Vector2(sliderHead.endPosition.x, sliderHead.endPosition.y);

            int sliderHeadColumn = (int)sliderStartPosition.X;
            int sliderHeadRow = (int)sliderStartPosition.Y;

            int currentBodyColumn = (int)note.startPosition.x;
            int currentBodyRow = (int)note.startPosition.y;

            double sliderDifficulty = 1.0;

            sliderDifficulty *= 1.0 + 0.5 * Math.Pow(Math.Abs(sliderHeadColumn - currentBodyColumn), 2);
            sliderDifficulty *= 1.0 + 0.5 * Math.Pow(Math.Abs(sliderHeadRow - currentBodyRow), 2);

            return Math.Max(1.0, sliderDifficulty) * _SkillMultiplier;
        }
    }
}
