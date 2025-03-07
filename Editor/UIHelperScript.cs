using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class UIHelperScript : EditorWindow
{
    [SerializeField] Sprite sprite;

    [SerializeField] Transform canvas;

    [Tooltip("You can copy this data from figma contains - width, height, top, left")]
    [SerializeField] string UIData;

    [SerializeField] string Top, Left;
    [SerializeField] int SizeMultiply = 1;

    [SerializeField] UIObjectType type;

    [SerializeField] int InputTextSize = 30;

    [MenuItem("Window/UIHelper")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(UIHelperScript));
    }

    // top: 82px; left: 274px;

    private void OnGUI()
    {
        canvas = EditorGUILayout.ObjectField("Canvas/Parent", canvas, typeof(Transform), true) as Transform;

        sprite = EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), true) as Sprite;

        UIData = EditorGUILayout.TextField("UI Data", UIData);

        GUI.Label(new Rect(3, 105, position.width, 20), "Size Multiplier");

        GUILayout.Space(15);
        SizeMultiply = EditorGUILayout.IntSlider(SizeMultiply, 1, 7);

        GUILayout.Space(20);

        type = (UIObjectType)EditorGUILayout.EnumPopup("UI Type", type);

        if (type == UIObjectType.InputField)
            InputTextSize = EditorGUILayout.IntField("Input text size", InputTextSize);

        GUILayout.Space(20);

        // Top = EditorGUILayout.TextField("Top", Top);
        // Left = EditorGUILayout.TextField("Left", Left);

        if (GUILayout.Button("Create UI"))
        {
            List<string> str = UIData.Split().ToList();
            for (int i = 0; i < str.Count - 1; i++)
            {
                if (str[i] == "top:")
                    Top = str[i + 1];

                if (str[i] == "left:")
                    Left = str[i + 1];
            }

            if (canvas == null)
            {
                try
                {
                    canvas = FindFirstObjectByType<Canvas>().transform;
                }
                catch
                {
                    Debug.LogError("Canvas or Parent are null");
                }
            }
            GameObject go = new GameObject(sprite.name);

            go.transform.parent = canvas;

            if (type == UIObjectType.Image)
                CreateImage(go);
            else if (type == UIObjectType.Button)
                CreateButton(go);
            else if (type == UIObjectType.InputField)
                CreateInputField(go);
            //go.AddComponent<TMP_InputField>();

            RectTransform rectTransform = go.GetComponent<RectTransform>();

            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);

            rectTransform.anchoredPosition = new Vector3(convertToInt(Left) * SizeMultiply, -convertToInt(Top) * SizeMultiply, 0);
            //rectTransform.anc

            Top = Left = "";
        }
    }

    long convertToInt(string s)
    {
        var f = Regex.Replace(s, "[^0-9.]", "");
        try
        {
            return Convert.ToInt64(f.ToString());
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    ///         Create Image Component
    /// </summary>
    void CreateImage(GameObject go)
    {
        Image image = go.AddComponent<Image>();

        image.sprite = sprite;
        image.SetNativeSize();
    }

    /// <summary>
    ///         Create Button Component
    /// </summary>
    void CreateButton(GameObject go)
    {
        CreateImage(go);
        go.AddComponent<Button>();
    }

    /// <summary>
    ///         Create InputField Component
    /// </summary>
    void CreateInputField(GameObject go)
    {
        go.AddComponent<RectTransform>();
        go.AddComponent<TMP_InputField>();
        go.AddComponent<Image>();
        var textField = go;
        //new GameObject("Input", typeof(CanvasRenderer), typeof(RectTransform), typeof(Image), typeof(TMP_InputField));

        // Set input field image
        textField.GetComponent<Image>().sprite = sprite;
        textField.GetComponent<Image>().SetNativeSize();

        Vector2 size = textField.GetComponent<RectTransform>().sizeDelta;

        textField.transform.SetParent(canvas.transform);
        //textField.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 30);
        textField.GetComponent<RectTransform>().localPosition = Vector3.zero;
        textField.GetComponent<TMP_InputField>().text = "";
        textField.GetComponent<TMP_InputField>().targetGraphic = textField.GetComponent<Image>();

        var placeHolder = new GameObject("Placeholder", typeof(CanvasRenderer), typeof(TextMeshProUGUI),
            typeof(LayoutElement));

        var text = new GameObject("Text", typeof(CanvasRenderer), typeof(TextMeshProUGUI));

        var textArea = new GameObject("Text Area", typeof(RectMask2D));

        textField.GetComponent<TMP_InputField>().textViewport = textArea.GetComponent<RectTransform>();
        textArea.transform.SetParent(textField.transform);
        textArea.GetComponent<RectTransform>().localPosition = Vector3.zero;
        textArea.GetComponent<RectTransform>().sizeDelta = size;

        placeHolder.transform.SetParent(textArea.transform);
        text.transform.SetParent(textArea.transform);
        text.GetComponent<TextMeshProUGUI>().verticalAlignment = VerticalAlignmentOptions.Middle;
        text.GetComponent<RectTransform>().localPosition = Vector3.zero;

        text.GetComponent<TextMeshProUGUI>().margin = new Vector4(35, 0, 35, 0);

        placeHolder.GetComponent<RectTransform>().localPosition = Vector3.zero;
        placeHolder.GetComponent<TextMeshProUGUI>().text = "Enter text...";
        placeHolder.GetComponent<TextMeshProUGUI>().verticalAlignment = VerticalAlignmentOptions.Middle;

        placeHolder.GetComponent<TextMeshProUGUI>().margin = new Vector4(35, 0, 35, 0);

        // Set Input fields text & placeholder
        textField.GetComponent<TMP_InputField>().textComponent = text.GetComponent<TextMeshProUGUI>();
        textField.GetComponent<TMP_InputField>().placeholder = placeHolder.GetComponent<TextMeshProUGUI>();

        // rect.localPosition = new Vector3(0, 0, 0);
        // rect.sizeDelta = new Vector2(200, 25);

        text.GetComponent<RectTransform>().sizeDelta = size;
        placeHolder.GetComponent<RectTransform>().sizeDelta = size;

        textField.GetComponent<TMP_InputField>().Select();
        textField.GetComponent<TMP_InputField>().ActivateInputField();
    }
}

public enum UIObjectType
{
    Image,
    Button,
    InputField
}