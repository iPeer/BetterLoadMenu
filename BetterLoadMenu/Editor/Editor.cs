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

    [KSPAddon(KSPAddon.Startup.EditorAny, true)]
    public class Editor : MonoBehaviour 
    {

        private ApplicationLauncherButton _button;
        private EditorType _editor = EditorType.VAB;

        public CacheManager cacheManager;
        public GUIManager guiManager;
        public static Editor Instance { get; private set; }

        void Awake()
        {

            Instance = this;

            Logger.Log("Editor Awake()");
            // TODO: Settings

            Logger.Log("Registering for events");
            GameEvents.onGUIApplicationLauncherReady.Add(() => manageToolbar(false));

            Logger.Log("Adding button hooks");
            EditorLogic.fetch.saveBtn.onClick.AddListener(() => updateSavedVesselCache());
            EditorLogic.fetch.launchBtn.onClick.AddListener(() => updateSavedVesselCache(SaveButtonSource.LAUNCH));

            if (EditorDriver.editorFacility == EditorFacility.SPH)
                _editor = EditorType.SPH;

            if (cacheManager == null)
            {
                Logger.Log("Setting up cache manager");
                cacheManager = new CacheManager();
            }

            if (guiManager == null)
            {
                Logger.Log("Setting up GUI Manager");
                guiManager = new GUIManager();
            }

        }

        void OnGUI() {
            guiManager.OnGUI();
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

            string vesselName = "Auto-Saved Ship";
            if (sbs == SaveButtonSource.SAVE)
                vesselName = EditorLogic.fetch.shipNameField.text;
            string savePath = Utils.getCurrentEditorShipPath(EditorDriver.editorFacility);
            string fullSavePath = Path.Combine(savePath, String.Format("{0}.craft", KSPUtil.SanitizeFilename(vesselName)));

            string vesselDescription = EditorLogic.fetch.shipDescriptionField.text;

            int stages = Staging.StageCount;
            int parts = EditorLogic.fetch.ship.parts.Count;
            float _, __;
            float weight = EditorLogic.fetch.ship.GetShipMass(out _, out __);
            float cost = EditorLogic.fetch.ship.GetShipCosts(out _, out __);
            string vesselThumbnail = Utils.getThumbnailPathForVesselName(vesselName, EditorDriver.editorFacility);

            ConstructCacheEntry cce = new ConstructCacheEntry(fullSavePath, vesselName, vesselDescription, stages, parts, cost, weight, vesselThumbnail);
            Logger.LogDebug("{0}", cce.ConfigNode);
            cce.saveCache();
            // TODO: Add to cache manager

        }

        private void loadButtonClick()
        {
            if (!cacheManager.hasInitialised)
            {
                cacheManager.loadCache();
            }
            GUIManager.Instance.guiVisible = true;
        }

        private void cancelVesselLoading()
        {
            GUIManager.Instance.guiVisible = false;
        }

    }
}
