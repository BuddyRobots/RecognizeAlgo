using UnityEngine;
using OpenCVForUnity;
using MagicCircuit;
using System.Collections;
using System.Collections.Generic;

public class getImage : MonoBehaviour
{
    // parameters for video capture
    [HideInInspector]
    public Texture2D texture;

    private WebCamTexture webCamTexture;
    private WebCamDevice webCamDevice;
    private Mat frameImg;

    private bool initDone = false;
    private const int cam_width  = 640;
    private const int cam_height = 480;
    private const int tex_width  = 640;//640;//1280;
    private const int tex_height = 480;//480;//720;

    // textures
    public Texture2D light_tex;
    public Texture2D battery_tex;
    public Texture2D switch_tex;
    public Texture2D line_tex;

    // parameters for card detection
    private CardDetector light_detector;
    private CardDetector battery_detector;
    private CardDetector switch_detector;

    // parameters for line detection
    private LineDetector line_detector;

    private myUtils util;


    // Functions :
    // Use this for initialization
    void Start() {

        // Initialize webcam
        StartCoroutine(init());

        // Initialize detectors
        light_detector = new CardDetector(light_tex);
        battery_detector = new CardDetector(battery_tex);
        switch_detector = new CardDetector(switch_tex);

        line_detector = new LineDetector(line_tex);

        util = new myUtils();
    }

    private IEnumerator init()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            initDone = false;
            frameImg.Dispose();
        }

        webCamDevice = WebCamTexture.devices[0];
        webCamTexture = new WebCamTexture(webCamDevice.name, cam_width, cam_height);
        webCamTexture.Play();

        while (true)
        {
            if (webCamTexture.didUpdateThisFrame)
            {
                frameImg = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC3);

                texture = new Texture2D(tex_width, tex_height, TextureFormat.RGBA32, false);
                gameObject.GetComponent<Renderer>().material.mainTexture = texture;

                initDone = true;
                break;
            }
            else
            {
                yield return 0;
            }
        }
    }

    // Update is called once per frame
    void Update() {

        if (!initDone)
            return;

        if (webCamTexture.didUpdateThisFrame)
        {
            Utils.webCamTextureToMat(webCamTexture, frameImg);
            Mat resultImg = frameImg.clone();

            // Image Processing Codes
            List<List<Point>> bb = new List<List<Point>>();
            List<OpenCVForUnity.Rect> rect = new List<OpenCVForUnity.Rect>();

            /// Detect Cards
            // Detect lights
            light_detector.detectCard(frameImg, ref bb, ref rect);
            for (var i = 0; i < bb.Count; i++)
            {
                if (bb[i].Count < 4) break;
                util.drawBoundingBox(resultImg, bb[i], rect[i], new Scalar(0, 255, 0));
            }
            light_detector.removeCard(ref frameImg, rect);
            bb.Clear();
            rect.Clear();

            // Detect batteries
            battery_detector.detectCard(frameImg, ref bb, ref rect);
            for (var i = 0; i < bb.Count; i++)
            {
                if (bb[i].Count < 4) break;
                util.drawBoundingBox(resultImg, bb[i], rect[i], new Scalar(0, 0, 255));
            }
            battery_detector.removeCard(ref frameImg, rect);
            bb.Clear();
            rect.Clear();

            // Detect switches
            switch_detector.detectCard(frameImg, ref bb, ref rect);
            for (var i = 0; i < bb.Count; i++)
            {
                if (bb[i].Count < 4) break;
                util.drawBoundingBox(resultImg, bb[i], rect[i], new Scalar(255, 255, 0));
            }
            switch_detector.removeCard(ref frameImg, rect);
            bb.Clear();
            rect.Clear();

            /// Detect Lines
            List<List<List<Point>>> listLine = new List<List<List<Point>>>();
            line_detector.detectLine(frameImg, ref listLine, ref rect);
            for (var i = 0; i < listLine.Count; i++)
            {
                util.drawPoint(resultImg, listLine[i], rect[i]);
            }
            rect.Clear();

            Utils.matToTexture2D(resultImg, texture);
        }
    }
}