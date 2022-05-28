
//using System.IO;
//using System.Text;
//using System.Xml.Linq;

//using UnityEditor;
//using System.Text.RegularExpressions;
//using System.Collections.Generic;
//using UnityEngine;

//public class ProjectGenAssetImporter : AssetPostprocessor
//{

//    // necessary for XLinq to save the xml project file in utf8
//    class Utf8StringWriter : StringWriter
//    {
//        // -----------------------------------------------------------
//        public override Encoding Encoding
//        {
//            get { return Encoding.UTF8; }
//        }
//    }

//    static string OnGeneratedCSProject(string path, string content)
//    {
//        // process only file beginning SBC - my custom packages
//        if (!ProcessFile(path))
//        {
//            return content;
//        }
//        //Debug.LogError("process: " + path);

//        // parse the document and make some changes
//        XDocument document = XDocument.Parse(content);
//        AdjustDocument(document);

//        // save the changes using the Utf8StringWriter
//        Utf8StringWriter str = new Utf8StringWriter();
//        document.Save(str);

//        return str.ToString();
//    }

//    // -----------------------------------------------------------
//    static bool ProcessFile(string name)
//    {
//        //Regex regex = new Regex(@"[\/\\]SBC\..*\.csproj$");
//        Regex regex = new Regex(@"[\/\\]Saro\..*\.csproj$");
//        Match match = regex.Match(name);

//        return match.Success;
//            //|| name.Contains("XAsset.csproj")
//            //|| name.Contains("XAsset.Editor.csproj");
//    }

//    // -----------------------------------------------------------
//    static void AdjustDocument(XDocument document)
//    {
//        // get namespace of document
//        XNamespace ns = document.Root.Name.Namespace;

//        // get all Compile elements
//        IEnumerable<XElement> compileElements = document.Root.Descendants(ns + "Compile");

//        // regex to find which part of Include attribute of Compile element to use for Link element value
//        // check for Editor or Runtime (recommended folders: https://docs.unity3d.com/Manual/cus-layout.html)

//        Regex regex = new Regex(@"\\(Runtime|Editor)\\.*\.cs$");

//        // add child Link element to each Compile element
//        foreach (XElement el in compileElements)
//        {
//            string fileName = el.Attribute("Include").Value;

//            // skip assets folder
//            if (fileName.StartsWith("Assets", System.StringComparison.Ordinal)) continue;

//            Match match = regex.Match(fileName);

//            if (match.Success)
//            {
//                // substr from 1 to exclude initial slash character
//                XElement link = new XElement(ns + "Link")
//                {
//                    Value = match.Value.Substring(1)
//                };

//                el.Add(link);
//            }
//        }
//    }
//}