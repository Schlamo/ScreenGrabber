using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance = null;
    int paragraphs = 0;
    Canvas canvas;

    private void Awake()
    {
        if (CanvasManager.instance == null)
            instance = this;
    }

    void Start ()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
	}

    public void AddParagraph(Paragraph p)
    {
        GameObject obj = new GameObject("Paragraph");
        obj.transform.parent = canvas.transform;
        Text txt = obj.AddComponent<Text>();
        txt.text = p.Text;

        if (p.Bold)
            Boldalize(txt);

        if (p.Italic)
            Italicalize(txt);

        if (p.Italic && p.Bold)
            Boldalicalize(txt);

        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        txt.font = ArialFont;
        txt.material = ArialFont.material;

        obj.transform.localScale = new Vector3(1, 1, 1);
        obj.transform.position = new Vector3(0, 0, 0);
        txt.transform.position = new Vector3(0, 0, 0);
        obj.transform.Translate(new Vector3(0, paragraphs*-3, 0));
        txt.color = p.Color;
        paragraphs++;
    }

    void Update ()
    {

    }

    void Italicalize(Text txt)
    {
        txt.fontStyle = FontStyle.Italic;
    }

    void Boldalize(Text txt)
    {
        txt.fontStyle = FontStyle.Bold;
    }

    void Boldalicalize(Text txt)
    {
        txt.fontStyle = FontStyle.BoldAndItalic;
    }
}
