using BetterLoadMenu.Cache;
using BetterLoadMenu.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterLoadMenu.Editor
{
    public class GUIManager
    {

        public static GUIManager Instance { get; private set; }

        private const string VEHICLE_TITLE = "<size=12><color=orange><b>{0}</b></color></size>";
        private const string VEHICLE_DATA = "<size=10><color=white>{0}</color></size>";
        private const string LOAD_BUTTON = "<b><color=#bada55>Load</color></b>";
        private const string CANCEL_BUTTON = "<b><color=white>Cancel</color></b>";
        private const string DELETE_BUTTON = "<b><color=red>Delete</color></b>";
        private const string MERGE_BUTTON = "<b><color=orange>Merge</color></b>";
        private const string GENERATE_THUMBNAIL = "<b><color=white>Generate Thumbnail</color></b>";
        private Rect _winPos = new Rect();
        private Vector2 _scrollPos = Vector2.zero;
        private const int WINDOW_ID = 238837562;

        private int selectedCraft = -1;
        public int sortMode = 0;

        private int shownEntries, facilityEntries, totalEntries;

        private string searchText = String.Empty;
        private EditorFacility selectedEditor = EditorFacility.VAB;

        private bool _guiVisible = false;
        public bool guiVisible
        {
            get
            {
                return _guiVisible;
            }
            set
            {
                _guiVisible = value;
                if (value)
                {
                    selectedEditor = EditorDriver.editorFacility;
                }
                //Logger.Log("GUIManager - GUI is now {0}", (value ? "visible" : "hidden"));
            }
        }

        GUIStyle _label, _button, _scroll, _toggle, _window, _textField;

        public GUIManager()
        {

            Logger.Log("GUIManager - Starting up");

            Instance = this;

            /*_scrollBar = HighLogic.Skin.verticalScrollbar;
            _scrollBar.fixedWidth = 0f;*/

            GUISkin skin = (GUISkin)GUISkin.Instantiate(HighLogic.Skin);

            _window = new GUIStyle(skin.window);

            _label = new GUIStyle(skin.label);
            _label.richText = true;
            _label.normal.textColor = Color.white;
            _label.fontSize = 5;
            _label.stretchWidth = false;

            _button = new GUIStyle(skin.button);
            _button.richText = true;
            //_button.alignment = TextAnchor.MiddleLeft;

            _toggle = new GUIStyle(skin.button);
            _toggle.richText = true;
            _toggle.alignment = TextAnchor.MiddleLeft;

            _scroll = new GUIStyle(skin.textArea); // Trust me

            _textField = new GUIStyle(skin.textField);

        }

        public void OnGUI() // Yes, I'm using OnGUI. I do not understand the new UI system at all.   
        {
            if (guiVisible)
                _winPos = GUILayout.Window(WINDOW_ID, _winPos, OnWindow, "BETTER LOAD MENU", _window, GUILayout.Width(400f), GUILayout.Height(600f));
        }

        void OnWindow(int id)
        {


            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            if (GUILayout.Toggle(this.selectedEditor == EditorFacility.VAB, "VAB", _button)) { switchSelectedEditor(EditorFacility.VAB); }
            if (GUILayout.Toggle(this.selectedEditor == EditorFacility.SPH, "SPH", _button)) { switchSelectedEditor(EditorFacility.SPH); }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Search:");
            this.searchText = GUILayout.TextField(this.searchText, _textField, GUILayout.MinWidth(320f), GUILayout.MaxWidth(320f));
            
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Sort by");
            if (GUILayout.Toggle(sortMode == 0, "Name", _button)) { updateSortMode(0); }
            if (GUILayout.Toggle(sortMode == 1, "Price", _button)) { updateSortMode(1); }
            if (GUILayout.Toggle(sortMode == 2, "Parts", _button)) { updateSortMode(2); }
            if (GUILayout.Toggle(sortMode == 3, "Weight", _button)) { updateSortMode(3); }
            if (GUILayout.Toggle(sortMode == 4, "Stages", _button)) { updateSortMode(4); }

            GUILayout.EndHorizontal();

            GUILayout.Label(String.Format("<size=10><color=orange>{0} total entries, {2} entries for this facility, {1} shown</color></size>", this.totalEntries, this.shownEntries, this.facilityEntries), _label);

            //_scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, _scroll, new GUIStyle(), new GUIStyle(), GUILayout.MaxWidth(400f));
            //_scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, _scrollBar, _scrollBar, _scroll, GUILayout.MaxWidth(400f));
            GUI.skin = HighLogic.Skin;
            _scrollPos = GUILayout.BeginScrollView(_scrollPos/*, GUILayout.MaxWidth(400f)*/);
            GUI.skin = null;

            renderEntries();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(CANCEL_BUTTON, _button))
            {
                hideGUI();
            }

            if (selectedCraft != -1)
            {
                ConstructCacheEntry craft = getCraftList()[selectedCraft];

                if (EditorLogic.RootPart != null)
                {
                    if (GUILayout.Button(MERGE_BUTTON, _button))
                    {
                        // I have no idea how to do this
                        ShipConstruct sc = ShipConstruction.LoadShip(craft.FilePath);
                        EditorLogic.fetch.SpawnConstruct(sc);
                        hideGUI();
                    }
                }
                if (GUILayout.Button(DELETE_BUTTON, _button))
                {
                    hideGUI();
                    DialogGUIButton[] options = new DialogGUIButton[] 
                    {
                        new DialogGUIButton("<b><color=red>Yes, delete it</color></b>", () => deleteCraft(craft)),
                        new DialogGUIButton("No, I changed my mind!", () => cancelDelete())
                    };
                    MultiOptionDialog mod = new MultiOptionDialog("Are you sure you want to delete this vessel? <b>THIS CANNOT BE UNDONE</b>!", "Confirm Delete", HighLogic.UISkin, options);
                    PopupDialog.SpawnPopupDialog(mod, false, HighLogic.UISkin, true);
                }
                if (GUILayout.Button(LOAD_BUTTON, _button))
                {
                    EditorLogic.LoadShipFromFile(craft.FilePath);
                    /*craft.updateThumbnail();*/
                    hideGUI();
                }
                /*if (!craft.HasThumbnail)
                {
                    if (GUILayout.Button(GENERATE_THUMBNAIL, _button))
                    {
                        craft.generateThumbnail();
                    }
                }*/
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();

        }

        public void switchSelectedEditor(EditorFacility e)
        {
            if (e != this.selectedEditor)
                this.selectedEditor = e;
        }

        public void deleteCraft(ConstructCacheEntry cce)
        {
            Editor.Instance.cacheManager.deleteCraft(cce);
            resetControls();
            this._guiVisible = true;
        }

        public void cancelDelete()
        {
            this._guiVisible = true;
        }

        public void resetControls()
        {
            this.selectedCraft = -1;
            _scrollPos = Vector2.zero;
        }

        public void updateSortMode(int m)
        {
            if (m != this.sortMode)
            {
                resetControls();
                this.sortMode = m;
            }
        }

        public void hideGUI()
        {
            if (this._guiVisible) // Fix stackoverflow when hiding GUI (thanks 1.1)
            {
                this.guiVisible = false;
                this.selectedCraft = -1;
                Editor.Instance._button.SetFalse();
                _scrollPos = Vector2.zero;
            }
        }

        void renderEntries()
        {
            List<ConstructCacheEntry> entries = getCraftList();
            if (entries.Count == 0)
            {
                GUILayout.Label("There are no saved vessels to display", _label);
                return;
            }

            int currentCraft = 0;

            foreach (ConstructCacheEntry cce in entries) 
            {

                GUILayout.BeginHorizontal();

                GUIContent gc = new GUIContent();
                gc.image = cce.ThumbnailTex;
                string vesselInfo = String.Format("Cost: {0:n0} / Parts: {1:n0} / Stages: {2:n0} / Weight: {3:n2} t", cce.Cost, cce.Parts, cce.Stages, cce.Weight);
                gc.text = String.Format(VEHICLE_TITLE, cce.Name) + "\n" + String.Format(VEHICLE_DATA, vesselInfo);

                //GUILayout.Button(gc, _toggle, GUILayout.MaxHeight(50f));
                if (GUILayout.Toggle(currentCraft == selectedCraft, gc, _toggle, GUILayout.MaxHeight(50f), GUILayout.MaxWidth(this.shownEntries < 9 ? 360f : 345f)))
                {
                    selectedCraft = currentCraft;
                }

                GUILayout.EndHorizontal();
                currentCraft++;
            }
        }

        public List<ConstructCacheEntry> getCraftList()
        {
            List<ConstructCacheEntry> list = new List<ConstructCacheEntry>(Editor.Instance.cacheManager.getFullCache());

            this.totalEntries = list.Count;

            list.RemoveAll(a => a.Facility != this.selectedEditor); // Remove all entries from the list that don't match the current editor

            this.facilityEntries = list.Count;

            // Apply search filtering
            if (!string.IsNullOrEmpty(this.searchText.Trim()))
            {
                list.RemoveAll(a => !a.VesselName.ToLower().Contains(this.searchText.ToLower()));
            }

            this.shownEntries = list.Count;

            // Apply ordering

            /* Sorting orders: 0 = name, 1 = price, 2 = parts, 3= weight */
            /* Note: Using .Order here produces a blocking error: AssemblyLoader: Exception loading 'BetterLoadMenu': System.Reflection.ReflectionTypeLoadException: The classes in the module cannot be loaded. */
            if (sortMode == 1) // price
                list.Sort((x, y) => x.Cost.CompareTo(y.Cost));
            else if (sortMode == 2) // parts
                list.Sort((x, y) => x.Parts.CompareTo(y.Parts));
            else if (sortMode == 3) // weight
                list.Sort((x, y) => x.Weight.CompareTo(y.Weight));
            else if (sortMode == 4) // stages
                list.Sort((x, y) => x.Stages.CompareTo(y.Stages));
            else // name
                list.Sort((x, y) => x.VesselName.CompareTo(y.VesselName));

            return list;

        }

    }
}
