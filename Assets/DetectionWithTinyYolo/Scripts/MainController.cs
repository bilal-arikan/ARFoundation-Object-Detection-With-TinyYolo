using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TFClassify;
using UnityEngine.XR.ARFoundation;
using System;

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    public bool AutoDetecting;
    [SerializeField] private Texture2D exampleTexture;
    private ARSessionOrigin _arSessionOrigin;
    private ARCameraBackground _arCameraBackground;
    private ARCameraManager _arCameraManager;
    private ARSession _arSession;
    [NonSerialized] public Texture2D _lastReceived = null;
    private float _lastUnloadTime;
    private long _timeStamp => new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
    public bool IsProcessing { get; private set; }


    private bool camAvailable = false;
    private bool isWorking = false;
    private Quaternion baseRotation;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        _arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        _arCameraBackground = FindObjectOfType<ARCameraBackground>();
        _arCameraManager = FindObjectOfType<ARCameraManager>();
        _arSession = FindObjectOfType<ARSession>();

        _arCameraManager.frameReceived += (e) =>
        {
            if (e.textures.Count > 0)
            {
                _lastReceived = e.textures[0];
            }
        };
    }


    private void Update()
    {
        if (Time.unscaledTime - _lastUnloadTime > 4)
        {
            Resources.UnloadUnusedAssets();
            _lastUnloadTime = Time.unscaledTime;
        }
        if (this.IsProcessing)
        {
            return;
        }

        if (_lastReceived == null)
        {
            return;
        }

        if (AutoDetecting)
        {
            if (exampleTexture)
                TFDetect(exampleTexture, null);
            else
                TFDetect(exampleTexture, null);
        }
    }
    public void TFDetect(Texture2D texture, Action<List<BoundingBox>> callback)
    {
        if (this.IsProcessing)
        {
            Debug.Log("_isProcessing true");
            callback?.Invoke(null);
            return;
        }
        if (!texture)
        {
            Debug.Log("texture null");
            callback?.Invoke(null);
            return;
        }

        this.IsProcessing = true;
        long startTime = _timeStamp;

        StartCoroutine(ProcessImage(texture, NNModelController.IMAGE_SIZE, result =>
        {
            // if (rawImage)
            //     rawImage.texture = texture;
            NNModelController.Instance.Detect(result, boxes =>
            {
                this.IsProcessing = false;
                long finishTime = _timeStamp;
                // if (boxes.Count > 0)
                // {
                //     Debug.Log(string.Join("\n", boxes.Select(r => r.Label + " " + r.Confidence + " " + r.Rect.center)));
                //     UnityEngine.Debug.Log(boxes.First().Label + " " + boxes.First().Confidence);
                // }
                var data = boxes.ToList();
                // LastDetectionData = data;
                callback?.Invoke(data);
                Resources.UnloadUnusedAssets();
            });
        }));
    }

    private IEnumerator ProcessImage(Texture texture, int inputSize, System.Action<Color32[]> callback)
    {
        if (texture is WebCamTexture)
            yield return StartCoroutine(TextureTools.CropSquare(texture as WebCamTexture,
                TextureTools.RectOptions.Center, snap =>
                {
                    var scaled = TextureTools.scaled(snap, inputSize, inputSize);
                    var rotated = TextureTools.RotateImageMatrix(scaled.GetPixels32(), scaled.width, scaled.height, 90);
                    callback(rotated);
                }));
        else if (texture is Texture2D)
            yield return StartCoroutine(TextureTools.CropSquare(texture as Texture2D,
                TextureTools.RectOptions.Center, snap =>
                {
                    var scaled = TextureTools.scaled(snap, inputSize, inputSize);
                    var rotated = TextureTools.RotateImageMatrix(scaled.GetPixels32(), scaled.width, scaled.height, 90);
                    callback(rotated);
                }));
    }

}
