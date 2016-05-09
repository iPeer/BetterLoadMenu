using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// ------- 

using BetterLoadMenu.Utilities;
using System.IO;
using KSP.UI.Screens;
using BetterLoadMenu.Cache;

namespace BetterLoadMenu.Editor
{

    public enum SaveButtonSource
    {
        SAVE,
        LAUNCH
    }

    public enum EditorType
    {
        VAB, 
        SPH
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Editor : MonoBehaviour 
    {

        private ApplicationLauncherButton _button;
        private EditorType _editor = EditorType.VAB;

        public static Editor Instance { get; private set; }

        void Start()
        {

            Instance = this;

            Logger.Log("Editor Start()");
            // TODO: Settings

            Logger.Log("Registering for events");
            GameEvents.onGUIApplicationLauncherReady.Add(() => manageToolbar(false));

            Logger.Log("Adding button hooks");
            EditorLogic.fetch.saveBtn.onClick.AddListener(() => updateSavedVesselCache());
            EditorLogic.fetch.launchBtn.onClick.AddListener(() => updateSavedVesselCache(SaveButtonSource.LAUNCH));

            if (EditorDriver.editorFacility == EditorFacility.SPH)
                _editor = EditorType.SPH;


        }

        private void OnDestroy()
        {
            Logger.Log("I'm being destroyed!");
            manageToolbar(true);
        }

        private void manageToolbar(bool remove = false)
        {

            Logger.Log("{0} toobar button", (remove ? "Removing" : "Adding"));

            if (remove)
            {

                ApplicationLauncher.Instance.RemoveModApplication(_button);

            }
            else {
                _button = ApplicationLauncher.Instance.AddModApplication(
                    loadButtonClick,
                    cancelVesselLoading,
                    null, 
                    null, 
                    null, 
                    null,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                    EditorLogic.fetch.loadBtn.image.sprite.texture
                );
            }
        }

        private void updateSavedVesselCache(SaveButtonSource sbs = SaveButtonSource.SAVE)
        {

        }

        private void loadButtonClick()
        {
            FileInfo[] vessel_list = Utils.loadVesselsForCurrentEditor(_editor);
            Logger.Log("------");
            foreach (FileInfo f in vessel_list)
            {
                Logger.Log(f.FullName);
                ConstructCache cc = new ConstructCache(f);
                Logger.Log("----- Name: {0}, Weight: {1}, Cost: {2}, Stages: {3}", cc.VesselName, cc.Weight, cc.Cost, cc.Stages);
                cc.CacheEntry.saveCache();
            }
        }

        private void cancelVesselLoading()
        {

        }

    }
}
