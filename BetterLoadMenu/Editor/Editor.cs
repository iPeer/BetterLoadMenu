using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
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

        public ApplicationLauncherButton _button;
        private EditorType _editor = EditorType.VAB;

        public CacheManager cacheManager;
        public GUIManager guiManager;
        public static Editor Instance { get; private set; }

        private bool buttonAdded = false;
        private bool eventsRegistered = false;

        void Start()
        {
            Logger.Log("Editor Start()");

            Instance = this;
            DontDestroyOnLoad(this);

        }

        private void onEditorStarted()
        {
            Logger.Log("Editor started.");
            if (EditorDriver.editorFacility == EditorFacility.SPH)
                _editor = EditorType.SPH;
            cacheManager.updateCache();
        }

        void Awake()
        {

            Logger.Log("Editor Awake()");
            if (!eventsRegistered)
            {
                eventsRegistered = true;
                Logger.Log("Registering for events");
                GameEvents.onGUIApplicationLauncherReady.Add(() => manageToolbar(false));
                GameEvents.onEditorStarted.Add(onEditorStarted);

                Logger.Log("Adding button hooks");
                EditorLogic.fetch.saveBtn.onClick.AddListener(() => updateSavedVesselCache());
                EditorLogic.fetch.launchBtn.onClick.AddListener(() => updateSavedVesselCache(SaveButtonSource.LAUNCH));
            }
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
            // TODO: Settings

        }

        void OnGUI() {
            guiManager.OnGUI();
        }

        /*private void OnDestroy()
        {
            Logger.Log("I'm being destroyed!");
            manageToolbar(true);
        }*/

        private void manageToolbar(bool remove = false)
        {

            Logger.Log("{0} toobar button", (remove ? "Removing" : "Adding"));

            if (remove)
            {

                ApplicationLauncher.Instance.RemoveModApplication(_button);

            }
            else {
                if (buttonAdded) { return; }
                buttonAdded = true;
                _button = ApplicationLauncher.Instance.AddModApplication(
                    this.loadButtonClick,
                    this.guiManager.hideGUI,
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

            Logger.Log("Craft name: {0}", vesselName);

            string savePath = Utils.getCurrentEditorShipPath(EditorDriver.editorFacility);
            string craftFilename = String.Format("{0}.craft", KSPUtil.SanitizeFilename(vesselName));
            string fullSavePath = Path.Combine(savePath, craftFilename);
            //string fullSavePath = String.Format("{0}{2}{1}", savePath, craftFilename, Path.DirectorySeparatorChar);

            Logger.Log("Craft file location: {0}", fullSavePath);

            string vesselDescription = EditorLogic.fetch.shipDescriptionField.text;

            int stages = Staging.StageCount;
            int parts = EditorLogic.fetch.ship.parts.Count;
            float _, __;
            float weight = EditorLogic.fetch.ship.GetShipMass(out _, out __);
            float cost = EditorLogic.fetch.ship.GetShipCosts(out _, out __);
            string vesselThumbnail = Utils.getThumbnailPathForVesselName(vesselName, EditorDriver.editorFacility);

            Logger.Log("Thumbnail: {0}", vesselThumbnail);

            ConstructCacheEntry cce = new ConstructCacheEntry(fullSavePath, vesselName, vesselDescription, stages, parts, cost, weight, vesselThumbnail);
            Logger.LogDebug("{0}", cce.ConfigNode);
            cce.saveCache();
            cacheManager.addCacheEntry(cce);

        }

        private void loadButtonClick()
        {
            if (!cacheManager.hasInitialised)
            {
                cacheManager.loadCache();
            }
            guiManager.guiVisible = true;
        }

    }
}
