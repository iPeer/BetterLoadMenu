using BetterLoadMenu.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterLoadMenu.Cache
{
    public class ConstructCacheEntry
    {

        public ConstructCacheEntry(string filePath/*, EditorFacility facility*/, string vesselName, string vesselDescription, int stages, int parts, float cost, float weight)
        {
            this.FilePath = filePath;
            this.Name = vesselName;
            this.Description = vesselDescription;
            this.Stages = stages;
            this.Parts = parts;
            this.Cost = cost;
            this.Weight = weight;
            //this.Facility = facility // TODO: Set this later when we can toggle it
        }
#pragma warning disable 0618
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
#pragma warning restore 0618

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
                ConfigNode root = new ConfigNode();
                root.AddNode(c);
                return root;
            }
        }

        public void saveCache()
        {
            string path = Path.Combine(Utils.getCacheSaveDirectory(), string.Format("{0}.vessel", KSPUtil.SanitizeFilename(VesselName)));
            if (!Directory.Exists(Utils.getCacheSaveDirectory()))
            {
                Directory.CreateDirectory(Utils.getCacheSaveDirectory());
            }
            ConfigNode.Save(path);
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

    }
}
