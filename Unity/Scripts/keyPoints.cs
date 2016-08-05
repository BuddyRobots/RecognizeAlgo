using UnityEngine;
using System.Collections;
using OpenCVForUnity;
using CardDetection;
using System.Collections.Generic;

public class keyPoints : MonoBehaviour {

    public Texture2D texture;

    private WebCamTexture webCamTexture;
    private WebCamDevice webCamDevice;
    private Mat frameImg;
    private Mat templImg;
    private FeatureTracker akazeTracker;

    private bool initDone = false;
    private const int width = 640;
    private const int height = 480;
    private const int tex_width = 640;//1120;//640;
    private const int tex_height = 480;

    // Use this for initialization
    void Start () {

        // Initialize webcam
        StartCoroutine(init());

        // Load template
        Texture2D template = Resources.Load("medium_light") as Texture2D;
        templImg = new Mat(template.height, template.width, CvType.CV_8UC3);
        Utils.texture2DToMat(template, templImg);

        // Creat template bounding box
        List<Point> bb = new List<Point>();
        bb.Add(new Point(0, 0));
        bb.Add(new Point(0, templImg.rows()));
        bb.Add(new Point(templImg.cols(), templImg.rows()));
        bb.Add(new Point(templImg.cols(), 0));

        // Initialize AKAZE Tracker
        akazeTracker = new FeatureTracker();
        akazeTracker.setTempl(templImg, bb);
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
        webCamTexture = new WebCamTexture(webCamDevice.name, width, height);
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
    void Update () {
        if (!initDone)
            return;

        if (webCamTexture.didUpdateThisFrame)
        {
            OpenCVForUnity.Utils.webCamTextureToMat(webCamTexture, frameImg);

            Mat resultImg = akazeTracker.process(frameImg);

            OpenCVForUnity.Utils.matToTexture2D(resultImg, texture);
        }
    }
}
