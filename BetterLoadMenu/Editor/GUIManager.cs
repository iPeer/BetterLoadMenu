using BetterLoadMenu.Cache;
using BetterLoadMenu.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterLoadMenu.Editor
{
    public class GUIManager
    {

        public static GUIManager Instance { get; private set; }

        private const string VEHICLE_TITLE = "<size=12><color=orange><b>{0}</b></color></size>";
        private const string VEHICLE_DATA = "<size=8><color=white>{0}</color></size>";
        private Rect _winPos = new Rect();
        private Vector2 _scrollPos = Vector2.zero;
        private const int WINDOW_ID = 238837562;

        private int selectedCraft = -1;

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
                Logger.Log("GUIManager - GUI is now {0}", (value ? "visible" : "hidden"));
            }
        }

        GUIStyle _label, _button, _scroll, _toggle;

        public GUIManager()
        {

            Logger.Log("GUIManager - Starting up");

            Instance = this;

            _label = new GUIStyle(HighLogic.Skin.label);
            _label.richText = true;
            _label.normal.textColor = Color.white;
            _label.fontSize = 5;

            _button = new GUIStyle(HighLogic.Skin.button);
            _button.richText = true;
            //_button.alignment = TextAnchor.MiddleLeft;

            _toggle = new GUIStyle(HighLogic.Skin.button);
            _toggle.richText = true;
            _toggle.alignment = TextAnchor.MiddleLeft;

            _scroll = new GUIStyle(HighLogic.Skin.scrollView);

        }

        public void OnGUI() // Yes, I'm using OnGUI. I do not understand the new UI system at all.   
        {
            if (guiVisible)
                _winPos = GUILayout.Window(WINDOW_ID, _winPos, OnWindow, "BETTER LOAD MENU", GUILayout.Width(400f), GUILayout.Height(600f));
        }

        void OnWindow(int id)
        {


            GUILayout.BeginVertical();
            GUILayout.Label("Stuff and things");
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, _scroll, GUILayout.MaxWidth(400f));
            renderEntries();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.DragWindow();

        }


        void renderEntries()
        {
            List<ConstructCacheEntry> entries = Editor.Instance.cacheManager.getFullCache();

            int currentCraft = 0;

            foreach (ConstructCacheEntry cce in entries) 
            {

                GUILayout.BeginHorizontal();

                GUIContent gc = new GUIContent();
                gc.image = cce.ThumbnailTex;
                string vesselInfo = String.Format("Cost: {0:n0} / Parts: {1:n0} / Stages: {2:n0} / Weight: {3:n2} t", cce.Cost, cce.Parts, cce.Stages, cce.Weight);
                gc.text = String.Format(VEHICLE_TITLE, cce.Name) + "\n" + String.Format(VEHICLE_DATA, vesselInfo);

                //GUILayout.Button(gc, _toggle, GUILayout.MaxHeight(50f));
                if (GUILayout.Toggle(currentCraft == selectedCraft, gc, _toggle, GUILayout.MaxHeight(50f), GUILayout.MaxWidth(340f)))
                {
                    selectedCraft = currentCraft;
                }

                GUILayout.EndHorizontal();
                currentCraft++;
            }
        }

    }
}
