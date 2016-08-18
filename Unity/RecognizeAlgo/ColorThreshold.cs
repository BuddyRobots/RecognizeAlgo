using UnityEngine;
using OpenCVForUnity;
using System.Collections.Generic;

namespace MagicCircuit
{
    public class ColorThreshold
    {
        private int h_min = 0, h_max = 180;
        private int s_min = 0, s_max = 255;
        private int v_min = 0, v_max = 255;

        private int area = 2000;
        private string component;

        public ColorThreshold(string _component)
        {
            component = _component;
            // Read in hsv thresholds
            if (PlayerPrefs.HasKey(component + "_h_min"))
                loadThres();
            else
                saveThres();
        }

        //@Override constructor for tunning hsv threshold
        public ColorThreshold(string _component, ref int _area,
            ref int _h_min, ref int _h_max,
            ref int _s_min, ref int _s_max,
            ref int _v_min, ref int _v_max)
        {
            component = _component;
            // Read in hsv thresholds
            if (PlayerPrefs.HasKey(component + "_h_min"))
                loadThres();
            else
                saveThres();

            _h_min = h_min; _h_max = h_max;
            _s_min = s_min; _s_max = s_max;
            _v_min = v_min; _v_max = v_max;
            _area = area;
        }

        public void getCards(Mat frame, ref List<Mat> l_roi, ref List<OpenCVForUnity.Rect> rect)
        {
            Mat hsvImg = new Mat();
            Mat resultImg = new Mat();

            if (l_roi.Count != 0)
                l_roi.Clear();
            if (rect.Count != 0)
                rect.Clear();

            // Color Thresholding
            Imgproc.cvtColor(frame, hsvImg, Imgproc.COLOR_RGB2HSV);
            Core.inRange(hsvImg, new Scalar(h_min, s_min, v_min), new Scalar(h_max, s_max, v_max), resultImg);
            Imgproc.morphologyEx(resultImg, resultImg, Imgproc.MORPH_OPEN, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(3, 3)));
            Imgproc.morphologyEx(resultImg, resultImg, Imgproc.MORPH_CLOSE, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(8, 8)));

            // Find Contours
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(resultImg, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE, new Point(0, 0));

            // Extract components using contour area
            for (int i = 0; i < contours.Count; i++)
            {
                if (Imgproc.contourArea(contours[i]) > area)
                {
                    OpenCVForUnity.Rect re = Imgproc.boundingRect(contours[i]);

                    // Extract only the correspoding component from frame using roi
                    // The size of roi is a variable
                    Mat roi = new Mat(frame, re);
                    l_roi.Add(roi);
                    rect.Add(re);
                }                   
            }
            return;
        }

        public void getLines(Mat frame, ref List<Mat> l_roi, ref List<OpenCVForUnity.Rect> rect)
        {
            Mat hsvImg = new Mat();
            Mat resultImg = new Mat();
            Mat lineImg = new Mat();

            if (l_roi.Count != 0)
                l_roi.Clear();
            if (rect.Count != 0)
                rect.Clear();

            // Color Thresholding
            Imgproc.cvtColor(frame, hsvImg, Imgproc.COLOR_RGB2HSV);
            Core.inRange(hsvImg, new Scalar(h_min, s_min, v_min), new Scalar(h_max, s_max, v_max), resultImg);
            Imgproc.morphologyEx(resultImg, resultImg, Imgproc.MORPH_OPEN, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(3, 3)));
            Imgproc.morphologyEx(resultImg, resultImg, Imgproc.MORPH_CLOSE, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(8, 8)));
            lineImg = resultImg.clone();

            // Find Contours
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(resultImg, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE, new Point(0, 0));

            // Extract components using contour area
            for (int i = 0; i < contours.Count; i++)
            {
                if (Imgproc.contourArea(contours[i]) > area)
                {
                    OpenCVForUnity.Rect re = Imgproc.boundingRect(contours[i]);

                    // Extract only the correspoding component from frame using roi
                    // The size of roi is a variable
                    Mat roi = new Mat(lineImg, re);
                    l_roi.Add(roi);
                    rect.Add(re);
                }
            }
            return;
        }

        public Mat tuneThreshold(Mat frame, ref int _area,
           ref int _h_min, ref int _h_max,
           ref int _s_min, ref int _s_max,
           ref int _v_min, ref int _v_max)
        {
            Mat hsvImg = new Mat();
            Mat resultImg = new Mat();

            // Color Thresholding
            Imgproc.cvtColor(frame, hsvImg, Imgproc.COLOR_RGB2HSV);
            Core.inRange(hsvImg, new Scalar(_h_min, _s_min, _v_min), new Scalar(_h_max, _s_max, _v_max), resultImg);
            Imgproc.morphologyEx(resultImg, resultImg, Imgproc.MORPH_OPEN, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(3, 3)));
            Imgproc.morphologyEx(resultImg, resultImg, Imgproc.MORPH_CLOSE, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(8, 8)));

            // Find Contours
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(resultImg, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE, new Point(0, 0));

            // Extract components using contour area
            List<Mat> l_roi = new List<Mat>();

            for (int i = 0; i < contours.Count; i++)
            {
                if (Imgproc.contourArea(contours[i]) > _area)
                {
                    Imgproc.drawContours(frame, contours, i, new Scalar(255, 0, 0), 2, 8, hierarchy, 0, new Point());
                    OpenCVForUnity.Rect rect = Imgproc.boundingRect(contours[i]);
                    Imgproc.rectangle(frame, rect.tl(), rect.br(), new Scalar(0, 0, 255), 3);
                }
            }

            h_min = _h_min; h_max = _h_max;
            s_min = _s_min; s_max = _s_max;
            v_min = _v_min; v_max = _v_max;
            area = _area;

            return frame;
        }

        public void saveThres()
        {
            PlayerPrefs.SetInt(component + "_h_min", h_min);
            PlayerPrefs.SetInt(component + "_h_max", h_max);
            PlayerPrefs.SetInt(component + "_s_min", s_min);
            PlayerPrefs.SetInt(component + "_s_max", s_max);
            PlayerPrefs.SetInt(component + "_v_min", v_min);
            PlayerPrefs.SetInt(component + "_v_max", v_max);
            PlayerPrefs.SetInt(component + "_area", area);
        }

        void loadThres()
        {
            h_min = PlayerPrefs.GetInt(component + "_h_min");
            h_max = PlayerPrefs.GetInt(component + "_h_max");
            s_min = PlayerPrefs.GetInt(component + "_s_min");
            s_max = PlayerPrefs.GetInt(component + "_s_max");
            v_min = PlayerPrefs.GetInt(component + "_v_min");
            v_max = PlayerPrefs.GetInt(component + "_v_max");
            area = PlayerPrefs.GetInt(component + "_area");
        }        
    }
}