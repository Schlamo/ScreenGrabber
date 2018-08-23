using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System;

public class NotesManager : MonoBehaviour
{

    #region Private Members
    private Dictionary<int, string> notes = new Dictionary<int, string>();
    #endregion

    #region Public Members
    public string presentationName = "Presentation";
    public string sourceDirectory = "Presentation";
    public string targetDirectory = "UnzippedPresentation";
    #endregion

    #region MonoBehavior Functions
    private void Start () {
        this.targetDirectory = "/" + targetDirectory + "/";

        if(UnzipPresentation(Application.dataPath + "/" + sourceDirectory + "/" + presentationName + ".pptx"))
        {
            ReadNotes(Application.dataPath + targetDirectory);
            LogNotes();
        }
        else
        {
            Debug.LogError("Unzipping Presentation failed.");
        }
    }

    private void OnApplicationQuit()
    {
        EmptyDirectory(Application.dataPath + targetDirectory);
    }
    #endregion

    #region Private Methods
    private bool AddNote(List<string> parts, int slide)
    {
        if (parts.Count == 0)
            return false;

        string note = "";

        foreach(string part in parts)
        {
            note += part;
        }
        notes.Add(slide, note);

        return true;
    }

    private void ReadNotes(string directory)
    {
        for(int i = 0; i < DirCount(new DirectoryInfo(directory + "ppt/slides/")); i++)
        {
            int slideNum = i + 1;
            bool insideFld = false;
            string file = "notesSlide" + (i+1) + ".xml";
            try
            {
                XmlReader reader = XmlReader.Create(directory + "ppt/notesSlides/" + file, new XmlReaderSettings());
                List<string> parts = new List<string>();

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if(reader.Name.Equals("a:fld"))
                            {
                                insideFld = true;
                            }
                            else if (reader.Name == "a:t")
                            {
                                if (!insideFld)
                                    parts.Add(reader.ReadInnerXml());
                                else
                                    slideNum = Int32.Parse(reader.ReadInnerXml());
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (reader.Name == "a:t")
                            {
                                parts.Add(" ");
                            }
                            else if(reader.Name == "a:fld")
                            {
                                insideFld = false;
                            }
                            break;
                    }
                }

                AddNote(parts, slideNum);
                reader.Close();
            }
            catch{}
        }
    }

    private static int DirCount(DirectoryInfo dir)
    {
        int i = 0;
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension.Contains("xml"))
                i++;
        }
        return i;
    }

    private bool UnzipPresentation(string path)
    {
        if (!Directory.Exists(Application.dataPath + targetDirectory))
            Directory.CreateDirectory(Application.dataPath + targetDirectory);

        try
        {
            ZipUtil.Unzip(path, Application.dataPath + targetDirectory);
        }
        catch 
        {
            Debug.LogError("Directory is empty, broken or doesn't exist. Moron!");
            return false;
        }

        return true;
    }

    private void EmptyDirectory(string path)
    {
        DirectoryInfo directory = new DirectoryInfo(path);
        DirectoryInfo[] childDirectories = directory.GetDirectories();
        FileInfo[] files = directory.GetFiles();

        foreach(FileInfo file in files)
        {
            File.Delete(path + file.Name);
        }
        foreach(DirectoryInfo dir in childDirectories)
        {
            EmptyDirectory(path + dir.Name + "/");
        }
        Directory.Delete(path);
    }
    #endregion

    #region Public Methods
    public void LogNotes()
    {
        foreach (KeyValuePair<int, string> note in notes)
        {
            Debug.Log("Slide " + note.Key + ":" + note.Value);
        }
    }

    public string GetNotesForSlide(int slide)
    {
        string note;
        this.notes.TryGetValue(slide, out note);

        return note;
    }
    #endregion
}
