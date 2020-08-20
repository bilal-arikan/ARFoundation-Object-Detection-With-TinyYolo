using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    private Texture2D boxOutlineTexture;
    private GUIStyle labelStyle;
    private IList<BoundingBox> boxOutlines;

    public Text ResultsText;
    public RawImage CameraScreen;
    public AspectRatioFitter CamFitter;
    public Button SearchButton;
    public Text SearchingText;
    public float ShowHigherThan = 5f;

    private float ratio;
    private float cameraScale = 1f;
    private string lastDetectedLabel;
    private bool firstUpdate = true;
    private float shiftX = 0f;
    private float shiftY = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        boxOutlineTexture = new Texture2D(1, 1);
        boxOutlineTexture.SetPixel(0, 0, Color.red);
        boxOutlineTexture.Apply();

        labelStyle = new GUIStyle();
        labelStyle.fontSize = 50;
        labelStyle.normal.textColor = Color.red;

        CalculateShift(NNModelController.IMAGE_SIZE);

    }
    public void OnGUI()
    {
        if (this.boxOutlines != null && this.boxOutlines.Any())
        {
            foreach (var outline in this.boxOutlines)
            {
                DrawBoxOutline(outline, 1, shiftX, shiftY);
            }
        }
    }
    private void Update()
    {

    }
    public void ShowResult(List<BoundingBox> results)
    {
        this.boxOutlines = results;

    }
    public void ShowResult(List<KeyValuePair<string, float>> results)
    {
        ResultsText.text = String.Empty;
        lastDetectedLabel = results.FirstOrDefault().Key;

        var highers = results.Where(p => p.Value > ShowHigherThan);
        if (highers.Any())
        {
            foreach (var result in highers)
            {
                ResultsText.text += String.Format("{0:0.000}%", result.Value) + "\t: " + result.Key + "\n";
            }
        }
    }

    private void DrawBoxOutline(BoundingBox outline, float scaleFactor, float shiftX, float shiftY)
    {
        var x = outline.Dimensions.X * scaleFactor + shiftX;
        var width = outline.Dimensions.Width * scaleFactor;
        var y = outline.Dimensions.Y * scaleFactor + shiftY;
        var height = outline.Dimensions.Height * scaleFactor;

        DrawRectangle(new Rect(x, y, width, height), 4, Color.red);
        DrawLabel(new Rect(x + 10, y + 10, 200, 20), $"{outline.Label}: {(int)(outline.Confidence * 100)}%");
    }


    public void DrawRectangle(Rect area, int frameWidth, Color color)
    {
        Rect lineArea = area;
        lineArea.height = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Top line

        lineArea.y = area.yMax - frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Bottom line

        lineArea = area;
        lineArea.width = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Left line

        lineArea.x = area.xMax - frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Right line
    }


    private void DrawLabel(Rect position, string text)
    {
        GUI.Label(position, text, labelStyle);
    }
    private void CalculateShift(int inputSize)
    {
        int smallest;

        if (Screen.width < Screen.height)
        {
            smallest = Screen.width;
            this.shiftY = (Screen.height - smallest) / 2f;
        }
        else
        {
            smallest = Screen.height;
            this.shiftX = (Screen.width - smallest) / 2f;
        }

        this.cameraScale = smallest / (float)inputSize;
    }
}
