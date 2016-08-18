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

    public RecognizeAlgo recognizeAlgo;

    public List<CircuitItem> listItem;


    // Functions :
    // Use this for initialization
    void Start() {

        // Initialize webcam
        StartCoroutine(init());

        // Intialize RecogniazeAlgo
        recognizeAlgo = new RecognizeAlgo(light_tex,
                                          battery_tex,
                                          switch_tex,
                                          line_tex);

        listItem = new List<CircuitItem>();
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

        listItem = new List<CircuitItem>();

        if (!initDone)
            return;

        if (webCamTexture.didUpdateThisFrame)
        {
            Utils.webCamTextureToMat(webCamTexture, frameImg);

            // Image Processing Codes
            Mat resultImg = recognizeAlgo.process(frameImg, ref listItem);

            Debug.Log("listItem.Count : " + listItem.Count);


            Utils.matToTexture2D(resultImg, texture);
        }
    }
}