using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapReader.MapReader;

namespace MapReader.Evaluators
{
    internal class SliderEvaluator
    {
        private const double _SkillMultiplier = 0.241;
        public static double evaluateSlider(BeatMapHitObject note)
        {
            if (!note.isSlider)
                return 1.0;

            int sliderHead = 0; // Find the slider head
            while (note.PreviousHand(sliderHead).isSlider) 
                sliderHead++;

            int sliderHeadColumn = (int)note.PreviousHand(sliderHead).startPosition.x;
            int sliderHeadRow = (int)note.PreviousHand(sliderHead).startPosition.y;

            int currentBodyColumn = (int)note.startPosition.x;
            int currentBodyRow = (int)note.startPosition.y;

            double sliderDifficulty = 1.0;

            sliderDifficulty *= 1.0 + 0.5 * Math.Pow(Math.Abs(sliderHeadColumn - currentBodyColumn), 2);
            sliderDifficulty *= 1.0 + 0.5 * Math.Pow(Math.Abs(sliderHeadRow - currentBodyRow), 2);

            return Math.Max(1.0, sliderDifficulty) * _SkillMultiplier;
        }
    }
}
