using UnityEngine;
using System.Collections;

namespace CardDetection
{
    public class Stats
    {
        public int matches;
        public int inliers;
        public double ratio;
        public int keypoints;

        public Stats()
        {
            matches = 0;
            inliers = 0;
            ratio = 0;
            keypoints = 0;
        }

        public static Stats operator +(Stats a, Stats b)
        {
            Stats result = new Stats();
            result.matches = a.matches + b.matches;
            result.inliers = a.inliers + b.inliers;
            result.ratio = a.ratio + b.ratio;
            result.keypoints = a.keypoints + b.keypoints;
            return result;
        }

        public static Stats operator /(Stats a, Stats b)
        {
            Stats result = new Stats();
            result.matches = a.matches / b.matches;
            result.inliers = a.inliers / b.inliers;
            result.ratio = a.ratio / b.ratio;
            result.keypoints = a.keypoints / b.keypoints;
            return result;
        }
    }
}

