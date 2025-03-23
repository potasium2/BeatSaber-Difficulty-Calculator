using JoshaParity;

namespace MapReader.MapReader
{
    public class BeatMapHitObject
    {
        List<BeatMapHitObject> beatMapHitObjects;

        /// <summary>
        /// Returns whether the note is hittable with the right hand or not
        /// </summary>
        public bool rightHand;

        /// <summary>
        /// Returns whether a note is part of a slider
        /// </summary>
        public bool isSlider;

        /// <summary>
        /// Returns whether a note is the head of a slider
        /// </summary>
        public bool isSliderHead;

        /// <summary>
        /// The index of the current note
        /// </summary>
        public int noteIndex;

        /// <summary>
        /// The time, in ms, at which the first note in a given swing path appears in a beatmap based on the clockrate
        /// </summary>
        public double time;

        /// <summary>
        /// The time, in ms, at which the final note in a given swing path appears in a beatmap based on the clockrate
        /// </summary>
        public double swingEnd;

        public PositionData startPosition;

        public PositionData endPosition;

        /// <summary>
        /// The intended direction in which you must swing in order to hit a given note
        /// </summary>
        public Parity handSwingDirection;

        public BeatMapHitObject(List<BeatMapHitObject> beatMapHitObjects, bool rightHand, int noteIndex, double time, double swingEnd, 
        PositionData currentPosition, PositionData endPosition, Parity handSwingDirection, bool isSlider, bool isSliderHead, double mapBPM)
        {
            this.beatMapHitObjects = beatMapHitObjects;
            this.rightHand = rightHand;
            this.isSlider = isSlider;
            this.isSliderHead = isSliderHead;
            this.noteIndex = noteIndex;
            this.time = 60000 / (mapBPM / time);
            this.swingEnd = 60000 / (swingEnd);
            this.startPosition = currentPosition;
            this.endPosition = endPosition;
            this.handSwingDirection = handSwingDirection;
        }

        public BeatMapHitObject? GetSliderHead(BeatMapHitObject current)
        {
            if (!current.isSlider)
                return default;

            if (current.isSliderHead)
                return current;

            int index = 0;
            while (!current.Previous(index).isSliderHead)
                index++;

            return current.Previous(index);
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
