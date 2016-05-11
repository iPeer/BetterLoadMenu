using BetterLoadMenu.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterLoadMenu.Cache
{
    public class CacheManager
    {

        private List<ConstructCacheEntry> ENTRY_CACHE = new List<ConstructCacheEntry>();

        public bool hasInitialised = false;


        public void loadCache(bool force = false)
        {
            if (this.hasInitialised && !force) { Logger.LogWarning("Attempt to initialise cache manager after it has already been initialised!"); return; }
            //if (cacheUpdateNeededLight()) { Logger.Log("Vessel folder contents have changed, regenerating cache..."); generateCache(); } // TODO: Reenable
            Logger.LogDebug("Cache directory: {0}", Utils.getCacheSaveDirectory());
            string[] files = Directory.GetFiles(Utils.getCacheSaveDirectory());
            Logger.LogDebug("Cache files: {0}", files.Length);
            foreach (string f in files)
            {
                ConfigNode n = ConfigNode.Load(f);
                ConfigNode c = n.GetNode("CACHE_DATA");

                /*
                c.AddValue("VESSEL_PARTS", Parts);
                c.AddValue("VESSEL_WEIGHT", Weight);
                c.AddValue("VESSEL_COST", Cost);
                c.AddValue("VESSEL_NAME", VesselName);
                c.AddValue("VESSEL_DESCRIPTION", Description);
                c.AddValue("VESSEL_PATH", FilePath);
                c.AddValue("VESSEL_FACILITY", Facility);
                c.AddValue("VESSEL_THUMBNAIL", Thumbnail);
                 */

                string name = c.GetValue("VESSEL_NAME");
                string description = c.GetValue("VESSEL_DESCRIPTION");
                string thumbnail = c.GetValue("VESSEL_THUMBNAIL");
                string path = c.GetValue("VESSEL_PATH");
                //EditorFacility facility = c.GetValue("VESSEL_FACILITY").Equals("SPH") ? EditorFacility.SPH : EditorFacility.VAB;

                float cost = Convert.ToSingle(c.GetValue("VESSEL_COST"));
                float weight = Convert.ToSingle(c.GetValue("VESSEL_WEIGHT"));
                int parts = Convert.ToInt32(c.GetValue("VESSEL_PARTS"));
                int stages = Convert.ToInt32(c.GetValue("VESSEL_STAGES"));

                ConstructCacheEntry cce = new ConstructCacheEntry(path, name, description, stages, parts, cost, weight, thumbnail);

                addCacheEntry(cce);

            }

            Logger.Log("CacheManager - Loaded {0} vessel cache(s)", ENTRY_CACHE.Count);

            this.hasInitialised = true;

        }

        public int addCacheEntry(ConstructCacheEntry cce)
        {

            if (ENTRY_CACHE.Contains(cce))
                Logger.LogWarning("Attempt to add cache entry that is already cached!");
            else
                ENTRY_CACHE.Add(cce);

            return ENTRY_CACHE.Count;

        }

        public List<ConstructCacheEntry> getFullCache()
        {
            return new List<ConstructCacheEntry>(ENTRY_CACHE.AsReadOnly());
        }

        public int removeCacheEntry(ConstructCacheEntry cce)
        {
            ENTRY_CACHE.Remove(cce);
            return ENTRY_CACHE.Count;
        }

        public ConstructCacheEntry getFirstEntryForVesselName(string name)
        {
            return ENTRY_CACHE.First(a => a.Name.Equals(name));
        }

        public void generateCache()
        {
            FileInfo[] vessel_list = Utils.loadVesselsForCurrentEditor(EditorDriver.editorFacility);
            foreach (FileInfo f in vessel_list)
            {
                Logger.Log(f.FullName);
                ConstructCache cc = new ConstructCache(f);
                Logger.Log("----- Name: {0}, Weight: {1}, Cost: {2}, Stages: {3}", cc.VesselName, cc.Weight, cc.Cost, cc.Stages);
                cc.CacheEntry.saveCache();
            }
        }

        public bool cacheUpdateNeededLight() // TODO
        {
            return true;
        }

    }
}
