using JoshaParity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapReader.MapReader
{
    public class BeatMapHitObject
    {
        List<BeatMapHitObject> beatMapHitObjects;

        public bool rightHand;

        /// <summary>
        /// Returns whether a note is part of a slider body
        /// </summary>
        public bool isSlider;

        public int noteIndex;

        public double time;

        public double swingEnd;

        public PositionData startPosition;

        public PositionData endPosition;

        public Parity handSwingDirection;

        public BeatMapHitObject(List<BeatMapHitObject> beatMapHitObjects, bool rightHand, int noteIndex, double time, double swingEnd, 
        PositionData currentPosition, PositionData endPosition, Parity handSwingDirection, bool isSlider, double mapBPM)
        {
            this.beatMapHitObjects = beatMapHitObjects;
            this.rightHand = rightHand;
            this.isSlider = isSlider;
            this.noteIndex = noteIndex;
            this.time = 60000 / (mapBPM / time);
            this.swingEnd = 60000 / (swingEnd);
            this.startPosition = currentPosition;
            this.endPosition = endPosition;
            this.handSwingDirection = handSwingDirection;
        }

        public BeatMapHitObject? Previous(int lastIndex)
        {
            int index = noteIndex - (lastIndex + 1);
            return index >= 0 && index < beatMapHitObjects.Count() ? beatMapHitObjects[index] : default;
        }

        public BeatMapHitObject? PreviousHand(int lastIndex)
        {
            int index = noteIndex - (lastIndex + 1);
            while (index >= 0)
            {
                if (beatMapHitObjects[index].rightHand == beatMapHitObjects[noteIndex].rightHand)
                    return beatMapHitObjects[index];

                index--;
            }

            return default;
        }

        public BeatMapHitObject? PreviousOpposite(int lastIndex)
        {
            int index = noteIndex - (lastIndex + 1);
            while (index >= 0)
            {
                if (beatMapHitObjects[index].rightHand != beatMapHitObjects[noteIndex].rightHand)
                    return beatMapHitObjects[index];

                index--;
            }

            return default;
        }

        public BeatMapHitObject? Next(int nextIndex)
        {
            int index = noteIndex + (nextIndex + 1);
            return index >= 0 && index < beatMapHitObjects.Count() ? beatMapHitObjects[index] : default;
        }

        public BeatMapHitObject? NextHand(int nextIndex)
        {
            int index = noteIndex + (nextIndex + 1);
            while (index < beatMapHitObjects.Count())
            {
                if (beatMapHitObjects[index].rightHand == beatMapHitObjects[noteIndex].rightHand)
                    return beatMapHitObjects[index];

                index++;
            }

            return default;
        }

        public BeatMapHitObject? NextOpposite(int nextIndex)
        {
            int index = noteIndex + (nextIndex + 1);
            while (index < beatMapHitObjects.Count())
            {
                if (beatMapHitObjects[index].rightHand != beatMapHitObjects[noteIndex].rightHand)
                    return beatMapHitObjects[index];

                index++;
            }

            return default;
        }

        public override string ToString()
        {
            return $"Right Hand: {rightHand}, Current Time: {time}, Position: {startPosition.x}x {startPosition.y}y {startPosition.rotation}deg, Swing Direction: {handSwingDirection}";
        }
    }
}
