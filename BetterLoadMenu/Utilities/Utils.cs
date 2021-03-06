﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BetterLoadMenu.Utilities
{
    public class Utils
    {

        public static string getCurrentEditorShipPath(Editor.EditorType editor)
        {
            return String.Format("{0}saves/{1}/Ships/{2}/", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder, (editor == Editor.EditorType.SPH ? "SPH" : "VAB"));
        }

        public static string getCurrentEditorShipPath(EditorFacility editor)
        {
            return getCurrentEditorShipPath(editor == EditorFacility.SPH ? Editor.EditorType.SPH : Editor.EditorType.VAB);
        }

        public static FileInfo[] loadVesselsForCurrentEditor(Editor.EditorType _editor)
        {
            string savePath = Utils.getCurrentEditorShipPath(_editor);
            Logger.LogDebug("Vessel path: {0}", savePath);
            DirectoryInfo di = new DirectoryInfo(savePath);
            return di.GetFiles();
        }

        public static FileInfo[] loadVesselsForCurrentEditor(EditorFacility editorFacility)
        {
            return loadVesselsForCurrentEditor(editorFacility == EditorFacility.SPH ? Editor.EditorType.SPH : Editor.EditorType.VAB);
        }

        public static ShipConstruct loadVesselFromFile(string file)
        {

            if (File.Exists(file))
            {
                Logger.Log("{0}", EditorLogic.fetch.ship == null);
                ShipConstruct previous_ship = EditorLogic.fetch.ship ?? new ShipConstruct(); // Save snapshot of editor
                ConfigNode cfg = ConfigNode.Load(file);

                ShipConstruct con = new ShipConstruct();
                con.LoadShip(cfg);
                // Reset editor to what it was before
                EditorLogic.fetch.ship.Clear();
                foreach (Part p in previous_ship)
                    EditorLogic.fetch.ship.Add(p);
                return con;
            }
            else
            {
                throw new FileNotFoundException("Specified craft file doesn't exist: " + file);
            }

        }


        public static string getCacheSaveBaseDirectory()
        {
            return String.Format("{0}saves/{1}/Ships/BLM_CACHE/", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
        }

        public static string getCacheSaveDirectory(EditorFacility editorFacility = EditorFacility.VAB)
        {
            return String.Format("{0}/{1}/", getCacheSaveBaseDirectory(), editorFacility.ToString());
        }

        public static FileInfo[] getCacheFilesForEditor(EditorFacility facility)
        {
            string path = getCacheSaveDirectory(facility);
            return (new DirectoryInfo(path)).GetFiles();
        }

        public static string getSavePathForVesselName(string VesselName, EditorFacility facility)
        {
            string start = getCurrentEditorShipPath(facility);
            return Path.Combine(start, string.Format("{0}.craft", KSPUtil.SanitizeFilename(VesselName)));
        }

        public static string getFacilityNameFromSavePath(string path)
        {
            string[] segs = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string facility = segs[segs.Length - 2];
            return facility;
        }

        public static EditorFacility getFacilityFromSavePath(string path)
        {
            return (getFacilityNameFromSavePath(path).Equals("SPH") ? EditorFacility.SPH : EditorFacility.VAB);
        }

        public static string getThumbnailPathForVesselName(string vesselName, EditorFacility editorFacility)
        {
            string fileName = String.Format("{0}_{1}_{2}.png", HighLogic.SaveFolder, editorFacility, KSPUtil.SanitizeFilename(vesselName));
            //string fullPath = Path.Combine(KSPUtil.ApplicationRootPath, "thumbs", fileName); // -- [Exception]: MissingMethodException: Method not found: 'System.IO.Path.Combine'. \\ I don't even know (it only happens here)
            string fullPath = String.Format("thumbs{0}{1}", Path.DirectorySeparatorChar, fileName);
            return fullPath;
        }


        public static string getKSPBaseFolder()
        {
            string root = KSPUtil.ApplicationRootPath; // .../Kerbal Space Program/../KSP[_x64]_Data
            string realRoot = root.Split(new string[] { String.Format("..{0}", Path.DirectorySeparatorChar), String.Format("..{0}", Path.AltDirectorySeparatorChar) }, StringSplitOptions.None)[0];
            Logger.Log(root);
            string[] baseFolderData = /*realRoot*/root.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return baseFolderData[baseFolderData.Length - 4]+Path.DirectorySeparatorChar;
        }

        public static string getHashForFile(FileInfo f)
        {
            using (FileStream fs = File.OpenRead(f.FullName))
            {
                SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider(); // We're (quickly) checking if the file has changed, not storing passwords, so SHA1 will work just fine
                byte[] bytes = sha1.ComputeHash(fs);
                return BitConverter.ToString(bytes); // The dashes can stay.
            }
        }

        public static void createCacheDirectories()
        {
            if (!Directory.Exists(Utils.getCacheSaveDirectory()))
                Directory.CreateDirectory(Utils.getCacheSaveBaseDirectory());
            if (!Directory.Exists(Utils.getCacheSaveDirectory(EditorFacility.SPH)))
                Directory.CreateDirectory(Utils.getCacheSaveDirectory(EditorFacility.SPH));
            if (!Directory.Exists(Utils.getCacheSaveDirectory(EditorFacility.VAB)))
                Directory.CreateDirectory(Utils.getCacheSaveDirectory(EditorFacility.VAB));
        }

        public static string createNodeNameFromCraftFile(string fileName)
        {
            string[] replace = new string[] { "(", ")", " ", "{", "}" };
            foreach (string c in replace) 
                fileName = fileName.Replace(c, "_");
            return fileName;
            
        }
    }
}
