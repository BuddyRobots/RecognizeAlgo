using OpenCVForUnity;
using System.Collections.Generic;
using UnityEngine;

namespace CardDetection
{
    public class myUtils
    {
        public void drawBoundingBox(Mat image, List<Point> bb)
        {
            for (var i = 0; i < bb.Count - 1; i++)
            {
                Imgproc.line(image, bb[i], bb[i + 1], new Scalar(0, 0, 255), 10);
            }
            Imgproc.line(image, bb[bb.Count - 1], bb[0], new Scalar(0, 0, 255), 10);

            Point center = new Point((bb[0].x + bb[2].x) / 2, (bb[0].y + bb[2].y) / 2);
            Point right = new Point((bb[3].x + bb[2].x) / 2, (bb[3].y + bb[2].y) / 2);

            Imgproc.line(image, center, right, new Scalar(0, 255, 0), 10);
        }

        public void drawStatistics(Mat image, Stats stats)
        {
            const int font = Core.FONT_HERSHEY_PLAIN;
            string str1, str2, str3;

            str1 = "Matches: ";
            str1 += stats.matches;
            str2 = "Inliers: ";
            str2 += stats.inliers;
            str3 = "Inlier ratio: ";
            str3 += stats.ratio;

            Imgproc.putText(image, str1, new Point(0, image.rows() - 90), font, 2, new Scalar(255, 255, 255), 3);
            Imgproc.putText(image, str2, new Point(0, image.rows() - 60), font, 2, new Scalar(255, 255, 255), 3);
            Imgproc.putText(image, str3, new Point(0, image.rows() - 30), font, 2, new Scalar(255, 255, 255), 3);
        }

        public MatOfPoint2f kp2Point(List<KeyPoint> keypoints)
        {
            double[] points = new double[keypoints.Count * 2];
            for(var i = 0; i < keypoints.Count; i++)
            {
                points[2 * i] = keypoints[i].pt.x;
                points[2 * i + 1] = keypoints[i].pt.y;
            }

            Mat tmp = new Mat(keypoints.Count, 1, CvType.CV_32FC2);
            tmp.put(0, 0, points);

            return new MatOfPoint2f(tmp);
        }

        public List<Point> perTrans(List<Point> src, Mat h)
        {
            List<Point> res = new List<Point>();
            double[,] H = new double[3, 3];

            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                {
                    H[i, j] = h.get(i, j)[0];
                }

            for (var i = 0; i < src.Count; i++)
            {
                double x = (H[0, 0] * src[i].x + H[0, 1] * src[i].y + H[0, 2]) / (H[2, 0] * src[i].x + H[2, 1] * src[i].y + H[2, 2]);
                double y = (H[1, 0] * src[i].x + H[1, 1] * src[i].y + H[1, 2]) / (H[2, 0] * src[i].x + H[2, 1] * src[i].y + H[2, 2]);
                res.Add(new Point(x, y));
            }
            return res;
        }
    }    
}