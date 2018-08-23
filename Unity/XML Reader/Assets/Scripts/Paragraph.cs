using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paragraph
{
    private string text;
    private int xOff;
    private int yOff;
    private int size;
    private bool bold;
    private Color color;
    private bool italic;
    private bool underlined;

    public Paragraph ()
    {
        color = new Color(0, 0, 0);
    }

    public string Text
    {
        get
        {
            return text;
        }

        set
        {
            text = value;
        }
    }

    public int XOff
    {
        get
        {
            return xOff;
        }

        set
        {
            xOff = value;
        }
    }

    public int YOff
    {
        get
        {
            return yOff;
        }

        set
        {
            yOff = value;
        }
    }

    public bool Bold
    {
        get
        {
            return bold;
        }

        set
        {
            bold = value;
        }
    }

    public bool Italic
    {
        get
        {
            return italic;
        }

        set
        {
            italic = value;
        }
    }

    public bool Underlined
    {
        get
        {
            return underlined;
        }

        set
        {
            underlined = value;
        }
    }

    public int Size
    {
        get
        {
            return size;
        }

        set
        {
            size = value;
        }
    }

    public Color Color
    {
        get
        {
            return color;
        }

        set
        {
            color = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    override public string ToString()
    {
        string str = text;

        if(bold && italic)
        {
            str += "(BOLD | ITALIC)";
        }
        else if (bold && !italic)
        {
            str += "(BOLD)";
        }
        else if (!bold && italic)
        {
            str += "(ITALIC)";
        }
    
        return str;
    }
}
