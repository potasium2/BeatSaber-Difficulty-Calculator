using JoshaParity;
using MapReader.MapData;
using MapReader.MapReader;

namespace MapReader.Difficulty.Evaluators
{
    internal class DistanceEvaluator
    {
        private static int _WidthStrain = 0;

        private static float _RightHandMovementStrain = 0;
        private static float _LeftHandMovementStrain = 0;

        private static float _RightHandLinearity = 0;
        private static float _LeftHandLinearity = 0;

        private static double _AngleStrictness = Modifiers.angleLeniency;

        /// <summary>
        /// Evaluates the difficulty in distance between this <paramref name="note"/> and the next <paramref name="note"/>.
        /// </summary>
        public static double evaluateDistanceOf(BeatMapHitObject note)
        {
            if (note.isSlider)
                return 1.0;

            double widthBonus = 1.0;

            float currColumn = note.startPosition.x;
            float nextColumn = note.NextHand(0).startPosition.x;
            double strainTime = note.NextHand(0) == null ? 0 : note.NextHand(0).time - note.time;

            if (strainTime == 0)
                return 0;

            // Bonus for 4 Wide patterns
            if (Math.Abs(currColumn - nextColumn) > 2)
            {
                _WidthStrain++;
                widthBonus *= 1.5 + Math.Pow(Math.Sin(Math.PI / 2 * _WidthStrain / 15), 2) / 2;
            }
            else
                _WidthStrain = _WidthStrain > 0 ? _WidthStrain - 1 : 0;

            return widthBonus;
        }

        /// <summary>
        /// Evaluates the difficulty of a pattern based on movement.
        /// </summary>
        public static double evaluateMovementOf(BeatMapHitObject note)
        {
            if (note.isSlider)
                return 1.0;

            double movementBonus = 1.0;

            if (note.NextHand(0) == null)
                return 1.0;

            if (note.PreviousHand(1) == null)
                return 1.0;

            double nextHand = note.time - note.NextHand(0).time;

            float lastDownColumn = note.PreviousHand(1).handSwingDirection == Parity.Forehand ? note.PreviousHand(1).startPosition.x : note.PreviousHand(0).startPosition.x;
            float lastUpRow = note.PreviousHand(1).handSwingDirection == Parity.Backhand ? note.PreviousHand(1).startPosition.y : note.PreviousHand(0).startPosition.y;
            float currRow = note.startPosition.y;
            float currColumn = note.startPosition.x;

            // Extended Movement Bonus
            // We only want to return bonus for patterns that aren't close to edges
            if (note.handSwingDirection == Parity.Forehand)
            {
                if (note.rightHand)
                {
                    if (nextHand < 150 && lastDownColumn < 3 && currColumn < 3 && lastDownColumn != currColumn)
                    {
                        _RightHandMovementStrain++;
                        movementBonus *= _RightHandMovementStrain > 2 ?
                            1.0 + Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(25, _RightHandMovementStrain) / 25), 2) / 2 : 1.0;

                        _RightHandLinearity = 0;
                    }
                    else if (lastDownColumn == currColumn)
                    {
                        _RightHandLinearity++;
                        movementBonus *= _RightHandLinearity >= 1 ? Math.Pow(_RightHandLinearity, -0.15) : 1.0;

                        _RightHandMovementStrain = 0;
                    }
                    else
                    {
                        _RightHandMovementStrain = 0;
                        _RightHandLinearity = 0;
                    }

                    if (note.startPosition.x < 2 && note.startPosition.y > 0 && Math.Abs(note.startPosition.rotation) == 45)
                    {
                        movementBonus *= 1.0 + Math.Pow(Math.Sin(Math.PI / 4 * note.startPosition.y), 2);
                    }
                }
                else
                {
                    if (nextHand < 150 && lastDownColumn > 0 && currColumn > 0 && lastDownColumn != currColumn)
                    {
                        _LeftHandMovementStrain++;
                        movementBonus *= _LeftHandMovementStrain > 2 ?
                            1.0 + Math.Pow(Math.Sin(Math.PI / 2 * Math.Min(25, _LeftHandMovementStrain) / 25), 2) / 2 : 1.0;

                        _LeftHandLinearity = 0;
                    }
                    else if (lastDownColumn == currColumn)
                    {
                        _LeftHandLinearity++;
                        movementBonus *= _LeftHandLinearity >= 1 ? Math.Pow(_LeftHandLinearity, -0.15) : 1.0;

                        _LeftHandMovementStrain = 0;
                    }
                    else
                    {
                        _LeftHandMovementStrain = 0;
                        _LeftHandLinearity = 0;
                    }

                    if (note.startPosition.x > 1 && note.startPosition.y > 0 && Math.Abs(note.startPosition.rotation) == 45)
                    {
                        movementBonus *= 1.0 + Math.Pow(Math.Sin(Math.PI / 4 * note.startPosition.y), 2);
                    }
                }
            }

            // Give a bonus for patterns that consistently make you adjust your up swing
            if (note.handSwingDirection == Parity.Backhand)
            {
                if (nextHand < 150 && currRow != lastUpRow)
                {
                    movementBonus *= 1.0 + Math.Abs(currRow - lastUpRow) / 8;
                }
            }

            return movementBonus;
        }

        /// <summary>
        /// Evaluates the difficulty of a pattern based on its location and distance.
        /// </summary>
        public static double evaluateLocationOf(BeatMapHitObject note)
        {
            if (note.isSlider)
                return 1.0;

            double distanceBonus = 1.0;

            if (note.NextHand == null)
                return 1.0;

            float noteColumn = note.startPosition.x;
            float nextColumn = note.NextHand(0).startPosition.x;

            float noteRow = note.startPosition.y;
            float nextRow = note.NextHand(0).startPosition.y;

            float currentAngle = note.startPosition.rotation;
            float angleChange = note.NextHand(0).startPosition.rotation;

            double strainTime = note.NextHand(0).time - note.time;

            if (note.handSwingDirection == Parity.Forehand && noteColumn < 2 && note.rightHand)
            {
                if ((currentAngle == 0 || currentAngle == -45) && strainTime < 75)
                    distanceBonus *= noteColumn == 0 ? 10.0 / 8.0 : 9.0 / 8.0;
            }

            if (note.handSwingDirection == Parity.Forehand && noteColumn > 1 && !note.rightHand)
            {
                if ((currentAngle == 0 || currentAngle == -45) && strainTime < 75)
                    distanceBonus *= noteColumn == 3 ? 10.0 / 8.0 : 9.0 / 8.0;
            }

            if (note.handSwingDirection == Parity.Backhand && noteColumn < 2 && note.rightHand)
            {
                if ((currentAngle == 0 || currentAngle == -45) && strainTime < 75)
                    distanceBonus *= noteColumn == 0 ? 11.0 / 8.0 : 10.0 / 8.0;
                else if (noteColumn == 0)
                    distanceBonus *= 5.0 / 3.0;
            }

            if (note.handSwingDirection == Parity.Backhand && noteColumn > 1 && !note.rightHand)
            {
                if ((currentAngle == 0 || currentAngle == -45) && strainTime < 75)
                    distanceBonus *= noteColumn == 3 ? 11.0 / 8.0 : 10.0 / 8.0;
                else if (noteColumn == 3)
                    distanceBonus *= 5.0 / 3.0;
            }

            // Give a small buff to inline circling patterns
            if (Math.Abs(noteColumn - nextColumn) == 0 && Math.Abs(noteRow - nextRow) != 0 && currentAngle % 180 != 0)
                distanceBonus *= currentAngle == 0 ? 1 :
                    Math.Pow(Math.Sin(45 * Math.PI / 180), 2) / 4 + 1;

            return distanceBonus;
        }

        /// <summary>
        /// Evaluates whether a pattern requires a circling motion in order to accurately hit it.
        /// </summary>
        public static double evaluateCirclingOf(BeatMapHitObject note)
        {
            if (note.isSlider)
                return 1.0;

            double handCirclingBonus = 1.0;
            double circlingBonus = 1.5;

            if (note.NextHand(0) == null)
                return 1.0;

            float noteColumn = note.startPosition.x;
            float nextColumn = note.NextHand(0).startPosition.x;
            float distance = Math.Abs(noteColumn - nextColumn);

            float noteRow = note.startPosition.y;
            float nextRow = note.NextHand(0).startPosition.y;

            float currentAngle = note.startPosition.rotation;
            float nextAngle = note.NextHand(0).startPosition.rotation;
            float angleChange = note.startPosition.rotation - note.NextHand(0).startPosition.rotation;

            if (noteRow != nextRow)
            {
                // nextColumn > noteColumn means next note is to the right, nextColumn < noteColumn means next note is to the left
                if ((note.rightHand && note.handSwingDirection == Parity.Forehand || !note.rightHand && note.handSwingDirection == Parity.Backhand) && currentAngle > 0)
                    handCirclingBonus *= currentAngle % 180 == 0 || nextAngle % 180 == 0 ? 1.0 : nextAngle < currentAngle ? 1.0 :
                    nextColumn > noteColumn ? 1 + Math.Pow(distance, circlingBonus) / 4.0 * (Math.Pow(Math.Sin((currentAngle - angleChange) * Math.PI / 180), 2) / 4 + 1) : 1.0;

                if ((note.rightHand && note.handSwingDirection == Parity.Backhand || !note.rightHand && note.handSwingDirection == Parity.Forehand) && currentAngle > 0)
                    handCirclingBonus *= currentAngle % 180 == 0 || nextAngle % 180 == 0 ? 1.0 : nextAngle < currentAngle ? 1.0 :
                    nextColumn < noteColumn ? 1 + Math.Pow(distance, circlingBonus) / 4.0 * (Math.Pow(Math.Sin((currentAngle - angleChange) * Math.PI / 180), 2) / 4 + 1) : 1.0;


                if ((note.rightHand && note.handSwingDirection == Parity.Forehand || !note.rightHand && note.handSwingDirection == Parity.Backhand) && currentAngle < 0)
                    handCirclingBonus *= currentAngle % 180 == 0 || nextAngle % 180 == 0 ? 1.0 : nextAngle > currentAngle ? 1.0 :
                    nextColumn < noteColumn ? 1 + Math.Pow(distance, circlingBonus) / 4.0 * (Math.Pow(Math.Sin((currentAngle - angleChange) * Math.PI / 180), 2) / 4 + 1) : 1.0;

                if ((note.rightHand && note.handSwingDirection == Parity.Backhand || !note.rightHand && note.handSwingDirection == Parity.Forehand) && currentAngle < 0)
                    handCirclingBonus *= currentAngle % 180 == 0 || nextAngle % 180 == 0 ? 1.0 : nextAngle > currentAngle ? 1.0 :
                    nextColumn > noteColumn ? 1 + Math.Pow(distance, circlingBonus) / 4.0 * (Math.Pow(Math.Sin((currentAngle - angleChange) * Math.PI / 180), 2) / 4 + 1) : 1.0;
            }

            handCirclingBonus *= Math.Pow(45 / _AngleStrictness, 0.5);

            return handCirclingBonus;
        }

        public static void globalReset()
        {
            _WidthStrain = 0;
            _RightHandMovementStrain = 0;
            _LeftHandMovementStrain = 0;
            _RightHandLinearity = 0;
            _LeftHandLinearity = 0;
        }
    }
}