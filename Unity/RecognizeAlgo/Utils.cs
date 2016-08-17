using OpenCVForUnity;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace MagicCircuit
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

        // @Override
        public void drawBoundingBox(Mat image, List<Point> bb, Scalar color)
        {
            for (var i = 0; i < bb.Count - 1; i++)
            {
                Imgproc.line(image, bb[i], bb[i + 1], color, 10);
            }
            Imgproc.line(image, bb[bb.Count - 1], bb[0], color, 10);

            Point center = new Point((bb[0].x + bb[2].x) / 2, (bb[0].y + bb[2].y) / 2);
            Point right = new Point((bb[3].x + bb[2].x) / 2, (bb[3].y + bb[2].y) / 2);

            Imgproc.line(image, center, right, color, 10);
        }

        // @Override
        public void drawBoundingBox(Mat image, List<Point> homo, OpenCVForUnity.Rect rect, Scalar color)
        {
            /*float radius = (float)rect.width / 2;
            float theta = Mathf.Atan2((float)(homo[2].y + homo[3].y), (float)(homo[2].x + homo[3].x));
            float _x = radius * Mathf.Cos(theta);
            float _y = radius * Mathf.Sin(theta);*/
            Point center = new Point((rect.tl().x + rect.br().x) / 2, (rect.tl().y + rect.br().y) / 2);

            double _x = (homo[3].x - homo[0].x) / 2;
            double _y = (homo[3].y - homo[0].y) / 2;

            Point right = new Point((center.x + _x), (center.y + _y));

            Imgproc.rectangle(image, rect.tl(), rect.br(), color, 10);
            Imgproc.line(image, center, right, color, 10);
        }

        public void drawPoint(Mat image, List<List<Point>> listLine, OpenCVForUnity.Rect rect)
        {
            System.Random rnd = new System.Random();

            Point center = new Point(rect.tl().x, rect.tl().y );
            
            for (var j = 0; j < listLine.Count; j++)
            {
                Scalar color = new Scalar(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                for (var i = 0; i < listLine[j].Count; i++)
                    Imgproc.circle(image, new Point(listLine[j][i].x + center.x, listLine[j][i].y + center.y), 5, color);
            }
            
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

    public enum ItemType
    {
        Battery,
        Switch,
        Bulb,
        CircuitLine                             //如果是线的话，则是点的集合
    }
    
    public class CircuitItem                    //图标管理类 id,名字，类型，坐标
    {
        public int ID { get; set; }
        public string name { get; set; }
        public ItemType type { get; set; }      //图标类型
        public List<Vector3> list { get; set; } //图标的坐标
        public double theta;                    //图标的朝向（单位：角度）
        public int showOrder;                   //显示顺序 从0开始（图标的显示顺序是灯泡）
    }
}
 