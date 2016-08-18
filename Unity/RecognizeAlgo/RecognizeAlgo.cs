using UnityEngine;
using OpenCVForUnity;
using MagicCircuit;
using System.Collections;
using System.Collections.Generic;

public class RecognizeAlgo {

    // parameters for card detection
    private CardDetector light_detector;
    private CardDetector battery_detector;
    private CardDetector switch_detector;

    // parameters for line detection
    private LineDetector line_detector;

    private myUtils util;

    public RecognizeAlgo(Texture2D light_tex,
                         Texture2D battery_tex,
                         Texture2D switch_tex,
                         Texture2D line_tex)
    {
        // Initialize detectors
        light_detector = new CardDetector(light_tex);
        battery_detector = new CardDetector(battery_tex);
        switch_detector = new CardDetector(switch_tex);

        line_detector = new LineDetector(line_tex);

        util = new myUtils();

        // Set default hsv thresholds
        if (!PlayerPrefs.HasKey("battery_h_min"))
            saveDefault();
    }

    public Mat process(Mat frameImg, ref List<CircuitItem> listItem)
    {
        Mat resultImg = frameImg.clone();

        int ID = 0;
        CircuitItem tmpItem;

        List<List<Point>> bb = new List<List<Point>>();
        List<OpenCVForUnity.Rect> rect = new List<OpenCVForUnity.Rect>();

        /// Detect Cards =============================================================
        // Detect batteries
        battery_detector.detectCard(frameImg, ref bb, ref rect);

        for (var i = 0; i < bb.Count; i++)
        {
            if (bb[i].Count < 4) break;
            util.drawBoundingBox(resultImg, bb[i], rect[i], new Scalar(0, 0, 255));

            // Add to CircuitItem
            tmpItem = new CircuitItem(ID, "Battery", ItemType.Battery, ID++);
            tmpItem.extractCard(bb[i], rect[i]);
            listItem.Add(tmpItem);
        }

        battery_detector.removeCard(ref frameImg, rect);
        bb.Clear();
        rect.Clear();

        // Detect lights
        light_detector.detectCard(frameImg, ref bb, ref rect);

        for (var i = 0; i < bb.Count; i++)
        {
            if (bb[i].Count < 4) break;
            util.drawBoundingBox(resultImg, bb[i], rect[i], new Scalar(0, 255, 0));

            // Add to CircuitItem
            tmpItem = new CircuitItem(ID, "Bulb", ItemType.Bulb, ID++);
            tmpItem.extractCard(bb[i], rect[i]);
            listItem.Add(tmpItem);
        }

        light_detector.removeCard(ref frameImg, rect);
        bb.Clear();
        rect.Clear();

        // Detect switches
        switch_detector.detectCard(frameImg, ref bb, ref rect);

        for (var i = 0; i < bb.Count; i++)
        {
            if (bb[i].Count < 4) break;
            util.drawBoundingBox(resultImg, bb[i], rect[i], new Scalar(255, 255, 0));

            // Add to CircuitItem
            tmpItem = new CircuitItem(ID, "Switch", ItemType.Switch, ID++);
            tmpItem.extractCard(bb[i], rect[i]);
            listItem.Add(tmpItem);
        }

        switch_detector.removeCard(ref frameImg, rect);
        bb.Clear();
        rect.Clear();

        /// Detect Lines =============================================================
        List<List<List<Point>>> listLine = new List<List<List<Point>>>();
        line_detector.detectLine(frameImg, ref listLine, ref rect);

        for (var i = 0; i < listLine.Count; i++)
        {
            util.drawPoint(resultImg, listLine[i], rect[i]);
        }

        // Add to CircuitItem
        for(var i = 0; i < listLine.Count; i++)
            for(var j = 0; j < listLine[i].Count; j++)
            {
                tmpItem = new CircuitItem(ID, "CircuitLine", ItemType.CircuitLine, ID++);
                tmpItem.extractLine(listLine[i][j], rect[i]);
                listItem.Add(tmpItem);
            }

        rect.Clear();

        return resultImg;
    }

    void saveDefault()
    {
        PlayerPrefs.SetInt("battery_h_min", 84);
        PlayerPrefs.SetInt("battery_h_max", 150);
        PlayerPrefs.SetInt("battery_s_min", 84);
        PlayerPrefs.SetInt("battery_s_max", 255);
        PlayerPrefs.SetInt("battery_v_min", 54);
        PlayerPrefs.SetInt("battery_v_max", 255);
        PlayerPrefs.SetInt("battery_area", 10000);

        PlayerPrefs.SetInt("light_h_min", 34);
        PlayerPrefs.SetInt("light_h_max", 82);
        PlayerPrefs.SetInt("light_s_min", 68);
        PlayerPrefs.SetInt("light_s_max", 255);
        PlayerPrefs.SetInt("light_v_min", 7);
        PlayerPrefs.SetInt("light_v_max", 227);
        PlayerPrefs.SetInt("light_area", 10000);

        PlayerPrefs.SetInt("switch_h_min", 0);
        PlayerPrefs.SetInt("switch_h_max", 57);
        PlayerPrefs.SetInt("switch_s_min", 38);
        PlayerPrefs.SetInt("switch_s_max", 255);
        PlayerPrefs.SetInt("switch_v_min", 34);
        PlayerPrefs.SetInt("switch_v_max", 194);
        PlayerPrefs.SetInt("switch_area", 10000);

        PlayerPrefs.SetInt("line_h_min", 0);
        PlayerPrefs.SetInt("line_h_max", 180);
        PlayerPrefs.SetInt("line_s_min", 0);
        PlayerPrefs.SetInt("line_s_max", 255);
        PlayerPrefs.SetInt("line_v_min", 0);
        PlayerPrefs.SetInt("line_v_max", 100);
        PlayerPrefs.SetInt("line_area", 500);
    }
}
