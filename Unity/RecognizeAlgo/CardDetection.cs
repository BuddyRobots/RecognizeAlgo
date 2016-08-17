using UnityEngine;
using System.Collections.Generic;
using OpenCVForUnity;

namespace MagicCircuit
{
    public class CardDetector
    {
        private ColorThreshold colorThreshold;
        private FeatureTracker featureTracker;

        public CardDetector(Texture2D _component)
        {
            colorThreshold = new ColorThreshold(_component.name);

            // Initialize feature tracker
            featureTracker = new FeatureTracker();
            Mat component = new Mat(new Size(_component.width, _component.height), CvType.CV_8UC3);
            Utils.texture2DToMat(_component, component);

            // Creat template bounding box
            List<Point> bb = new List<Point>();
            bb.Add(new Point(0, 0));
            bb.Add(new Point(0, component.rows()));
            bb.Add(new Point(component.cols(), component.rows()));
            bb.Add(new Point(component.cols(), 0));

            featureTracker.setTempl(component, bb);
        }

        public void detectCard(Mat frame, ref List<List<Point>> l_bb, ref List<OpenCVForUnity.Rect> rect)
        {
            List<Mat> l_roi = new List<Mat>();

            colorThreshold.getCards(frame, ref l_roi, ref rect);

            for(var i = 0; i < l_roi.Count; i++)
            {
                List<Point> bb = featureTracker.process(l_roi[i]);
                
                l_bb.Add(bb);
            }
            return;
        }

        public void removeCard(ref Mat img, List<OpenCVForUnity.Rect> rect)
        {
            for(var i = 0; i < rect.Count; i++)
            {
                Mat white = new Mat(rect[i].size(), img.type(), new Scalar(255, 255, 255));

                Mat imgSubmat = img.submat(rect[i]);
                white.copyTo(imgSubmat);
            }
        }
    }
}