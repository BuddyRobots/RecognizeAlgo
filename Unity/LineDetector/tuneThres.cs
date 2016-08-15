using UnityEngine;
using OpenCVForUnity;
using MagicCircuit;
using System.Collections;

public class tuneThres : MonoBehaviour
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
    private const int tex_width  = 640;//1120;//640;
    private const int tex_height = 480;

    // parameters for color threshold
    public Texture2D component;

    [Range(500.0f, 10000.0f)]
    public int area = 2000;
    [Range(0.0f, 180.0f)]
    public int h_min = 0, h_max = 180;
    [Range(0.0f, 255.0f)]
    public int s_min = 0, s_max = 255;
    [Range(0.0f, 255.0f)]
    public int v_min = 0, v_max = 255;

    private ColorThreshold colorThres;


    // Use this for initialization
    void Start() {

        // Initialize webcam
        StartCoroutine(init());

        colorThres = new ColorThreshold(component.name, ref area,
            ref h_min, ref h_max, ref s_min, ref s_max, ref v_min, ref v_max);
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

            // Image Processing Codes
            Mat resultImg = colorThres.tuneThreshold(frameImg, ref area, ref h_min, ref h_max, ref s_min, ref s_max, ref v_min, ref v_max);

            Utils.matToTexture2D(resultImg, texture);
        }
    }

    void OnApplicationQuit() {
        // Save color threshold
        colorThres.saveThres();
        Debug.Log("ColorThreshold saved");
    }

}