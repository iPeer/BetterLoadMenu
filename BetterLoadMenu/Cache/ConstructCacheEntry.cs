using BetterLoadMenu.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterLoadMenu.Cache
{
    public class ConstructCacheEntry
    {

        public ConstructCacheEntry(string filePath/*, EditorFacility facility*/, string vesselName, string vesselDescription, int stages, int parts, float cost, float weight, string thumbPath)
        {
            this.FilePath = filePath;
            this.Name = vesselName;
            this.Description = vesselDescription;
            this.Stages = stages;
            this.Parts = parts;
            this.Cost = cost;
            this.Weight = weight;
            this.Facility = Utils.getFacilityFromSavePath(filePath);
            this.Thumbnail = thumbPath;
            this.ThumbnailTex = ShipConstruction.GetThumbnail(thumbPath);
            //Logger.Log("Thumbnail path: {0}", System.IO.Path.Combine(KSPUtil.ApplicationRootPath, string.Format("{0}.png", thumbPath)));
            this.HasThumbnail = File.Exists(System.IO.Path.Combine(KSPUtil.ApplicationRootPath, string.Format("{0}.png", thumbPath)));
        }

#pragma warning disable 0618, 0612
        [Obsolete]
        public ConstructCacheEntry(ConstructCache c)
        {
            this.Name = c.Construct.shipName;
            this.Description = c.Construct.shipDescription;
            this.Facility = c.Construct.shipFacility;

            this.Cost = c.Cost;
            this.Weight = c.Weight;
            this.Stages = c.Stages;
            this.Parts = c.PartCount;
        }
#pragma warning restore 0618, 0612

        public EditorFacility Facility { get; private set; }

        public ConfigNode ConfigNode
        {
            get
            {
                ConfigNode c = new ConfigNode("CACHE_DATA");
                c.AddValue("VESSEL_PARTS", Parts);
                c.AddValue("VESSEL_WEIGHT", Weight);
                c.AddValue("VESSEL_COST", Cost);
                c.AddValue("VESSEL_NAME", VesselName);
                c.AddValue("VESSEL_DESCRIPTION", Description);
                c.AddValue("VESSEL_PATH", FilePath);
                c.AddValue("VESSEL_FACILITY", Facility);
                c.AddValue("VESSEL_THUMBNAIL", Thumbnail);
                c.AddValue("VESSEL_STAGES", Stages);
                ConfigNode root = new ConfigNode();
                root.AddNode(c);
                return root;
            }
        }

        public void saveCache()
        {
            string facility = Utils.getFacilityNameFromSavePath(FilePath);
            string path = Path.Combine(Utils.getCacheSaveDirectory(this.Facility), string.Format("{0}.vessel", KSPUtil.SanitizeFilename(VesselName)));
            Utils.createCacheDirectories();
            try
            {
                ConfigNode.Save(path);
            }
            catch (Exception e) 
            { 
                Logger.LogError("Couldn't save cache data for '{0}': {1}", VesselName, e.ToString());
                Logger.LogError("Stack trace: {0}", e.StackTrace);
            }
        }

        public string Name { get; private set; }
        public string VesselName { get { return Name; } private set { Name = value; } }
        public string Description { get; private set; }
        string _path;
        public string FilePath
        {
            get
            {
                return _path ?? Utils.getSavePathForVesselName(VesselName, Facility);
            }
            private set
            {
                _path = value;
            }
        }

        public float Cost { get; private set; }
        public float Weight { get; private set; }

        public int Stages { get; private set; }
        public int Parts { get; private set; }

        public string Thumbnail { get; private set; }
        public Texture2D ThumbnailTex { get; private set; }

        private bool _hasThumbnail = false;
        public bool HasThumbnail
        {
            get
            {
                return this._hasThumbnail;
            }
            private set
            {
                this._hasThumbnail = value;
            }
        }

        public void generateThumbnail()
        {
            ShipConstruct sc = /*ShipConstruction.LoadShip(this.FilePath);*/new ShipConstruct();
            ConfigNode cn = ConfigNode.Load(this.FilePath);
            sc.LoadShip(cn);
            //Logger.Log("Base folder: {0}", Utils.getKSPBaseFolder());
            ShipConstruction.CaptureThumbnail(sc, Utils.getKSPBaseFolder(), string.Format("{0}.png", this.Thumbnail));
            sc.LoadShip(new ConfigNode()); // Clear editor
            this.HasThumbnail = true;
            this.ThumbnailTex = ShipConstruction.GetThumbnail(this.Thumbnail);
        }


        public void updateThumbnail()
        {
            this.HasThumbnail = true;
            this.ThumbnailTex = ShipConstruction.GetThumbnail(this.Thumbnail);
        }
    }
}
