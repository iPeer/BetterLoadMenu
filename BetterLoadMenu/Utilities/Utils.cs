using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string getCacheSaveDirectory()
        {
            return String.Format("{0}saves/{1}/Ships/BLM_CACHE/", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
        }

        public static string getSavePathForVesselName(string VesselName, EditorFacility facility)
        {
            string start = getCurrentEditorShipPath(facility);
            return Path.Combine(start, string.Format("{0}.craft", KSPUtil.SanitizeFilename(VesselName)));
        }
    }
}
