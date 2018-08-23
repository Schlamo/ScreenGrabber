using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel;

public class FileInput : MonoBehaviour {

    //List<Paragraph> paragraphs = new List<Paragraph>();
    Color currentColor;
    Color defaultColor = new Color(0,0,0);
    //bool colorChangePossible = false;
    void Start ()
    {
        currentColor = defaultColor;
        XmlReader reader = XmlReader.Create("Assets/slides.xml", new XmlReaderSettings());

        //XmlDocument doc = new XmlDocument();
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.EndElement:
                    switch(reader.Name)
                    {
                        case "a:r":
                            currentColor = defaultColor;
                            break;

                        case "a:endParaRPr":
                            currentColor = defaultColor;
                            break;
                    }
                    break;
                case XmlNodeType.Element:

                    switch (reader.Name)
                    {
                        case "a:r":
                            Paragraph paragraph = new Paragraph();

                            do
                            {
                                reader.Read();
                                switch (reader.Name)
                                {
                                    case "a:rPr":
                                        try
                                        {
                                            paragraph.Size = Int32.Parse(reader.GetAttribute("sz"));
                                        }
                                        catch {}

                                        paragraph.Bold = reader.GetAttribute("b") == "1" ? true : false;
                                        paragraph.Italic = reader.GetAttribute("i") == "1" ? true : false;
                                        paragraph.Underlined = reader.GetAttribute("u") == "sng" ? true : false;
                                        break;
                                    case "a:srgbClr":
                                        try
                                        {
                                            string hexColor = "000000";
                                            hexColor = reader.GetAttribute("val");

                                            currentColor.r = Int32.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                                            currentColor.g = Int32.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                                            currentColor.b = Int32.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);

                                        }
                                        catch (System.Exception e)
                                        {
                                            UnityEngine.Debug.LogError(e.StackTrace);
                                        }
                                        break;
                                }
                            } while(reader.Name != "a:t");

                            paragraph.Color = currentColor;
                            paragraph.Text = reader.ReadInnerXml();

                            CanvasManager.instance.AddParagraph(paragraph);

                            //Debug.LogError(currentColor);
                            UnityEngine.Debug.Log(paragraph.ToString());
                            break;
                    }
                    break;
            }
        }
	}

    bool PassKeys ()
    {
        Process[] powerpoint = Process.GetProcesses();
        foreach(Process p in powerpoint)
        {
            if(p.ProcessName.Equals("POWERPNT.EXE"))
            {
                UnityEngine.Debug.Log(p.ProcessName);
            }
        }

        
        return true;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Jump")) {
            //PassKeys();
        }
	}
}
