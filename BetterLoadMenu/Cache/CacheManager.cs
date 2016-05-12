using BetterLoadMenu.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace BetterLoadMenu.Cache
{

    /*[KSPAddon(KSPAddon.Startup.EditorAny, true)]*/
    public class CacheManager/* : MonoBehaviour */
    {

        private List<ConstructCacheEntry> ENTRY_CACHE = new List<ConstructCacheEntry>();
        private List<ConstructCacheData> CACHE_DATA = new List<ConstructCacheData>();

        private ScreenMessage lastScreenMessage;

        public bool hasInitialised = false;

        //public static CacheManager Instance { get; private set; }

        /*public void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }*/

        public void loadCache(bool force = false)
        {
            if (this.hasInitialised && !force) { Logger.LogWarning("Attempt to initialise cache manager after it has already been initialised!"); return; }
            Logger.LogDebug("Cache directory: {0}", Utils.getCacheSaveBaseDirectory());
            FileInfo[] vab = Utils.getCacheFilesForEditor(EditorFacility.VAB);
            FileInfo[] sph = Utils.getCacheFilesForEditor(EditorFacility.SPH);
            FileInfo[] files = vab.Concat(sph).ToArray();
            Logger.LogDebug("Cache files: {0}", files.Length);
            foreach (FileInfo f in files)
            {
                ConfigNode n = ConfigNode.Load(f.FullName);
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
            return ENTRY_CACHE;
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
            /*FileInfo[] vessel_list = Utils.loadVesselsForCurrentEditor(EditorDriver.editorFacility);
            foreach (FileInfo f in vessel_list)
            {
                Logger.Log(f.FullName);
                ConstructCache cc = new ConstructCache(f);
                Logger.Log("----- Name: {0}, Weight: {1}, Cost: {2}, Stages: {3}", cc.VesselName, cc.Weight, cc.Cost, cc.Stages);
                cc.CacheEntry.saveCache();
            }*/

            FileInfo[] vab = Utils.loadVesselsForCurrentEditor(EditorFacility.VAB);
            FileInfo[] sph = Utils.loadVesselsForCurrentEditor(EditorFacility.SPH);
            FileInfo[] all = vab.Concat(sph).ToArray();

            generateCacheFromArray(all);

        }

        public void generateCacheFromArray(FileInfo[] arr)
        {
            foreach (FileInfo f in arr)
            {
                Logger.LogDebug("Current file: {0}", f.FullName);
                ConstructCache cc = new ConstructCache(f);
                Logger.LogDebug("{0}", cc.CacheEntry.ConfigNode);
                long modified = f.LastWriteTimeUtc.Ticks;
                long size = f.Length;
                string hash = Utils.getHashForFile(f);

                Logger.Log("Cache data for '{0}': {1} // {2} // {3}", f.Name, modified, size, hash);

                ConstructCacheData ccd = new ConstructCacheData(f.Name, f.FullName, hash, modified, size);

                CACHE_DATA.Add(ccd);

                cc.CacheEntry.saveCache();
            }

            saveMainCacheData();

        }

        /// <summary>
        /// Compares the cache directory to the craft directory and updates, adds or removes cache data depending on the state of the folder.
        /// </summary>
        public void updateCache()
        {

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            loadMainCacheData();

            List<ConstructCacheData> cache = new List<ConstructCacheData>(CACHE_DATA);

            ConfigNode cfg = ConfigNode.Load(Path.Combine(Utils.getCacheSaveBaseDirectory(), "cacheData.cfg"));

            FileInfo[] vab = Utils.loadVesselsForCurrentEditor(EditorFacility.VAB);
            FileInfo[] sph = Utils.loadVesselsForCurrentEditor(EditorFacility.SPH);

            /*FileInfo[] cvab = Utils.getCacheFilesForEditor(EditorFacility.VAB);
            FileInfo[] csph = Utils.getCacheFilesForEditor(EditorFacility.SPH);*/

            List<FileInfo> updated = new List<FileInfo>();
            List<ConstructCacheData> removed = new List<ConstructCacheData>();
            List<FileInfo> added = new List<FileInfo>();

            if (lastScreenMessage != null) 
                ScreenMessages.RemoveMessage(lastScreenMessage);

            lastScreenMessage = new ScreenMessage("<color=red><b>BetterLoadMenu is updating its cache. This may take a few minutes if you have a lot of vessels or your first time loading since installing BLM.</b></color>", Mathf.Infinity, ScreenMessageStyle.UPPER_RIGHT);
            ScreenMessages.PostScreenMessage(lastScreenMessage);
            Logger.Log("Checking for new, missing or updated vessels...");

            FileInfo[] all = vab.Concat(sph).ToArray();

            Logger.Log("Combining 2 checklists: {0} + {1} = {2}", vab.Length, sph.Length, all.Length);

            int craft = 1;

            foreach (FileInfo f in all)
            {

                ScreenMessages.RemoveMessage(lastScreenMessage);
                lastScreenMessage = new ScreenMessage(String.Format("<color=red><b>BetterLoadMenu is updating its cache. This may take a few minutes if you have a lot of vessels or your first time loading since installing BLM. [{0}/{1}]</b></color>", craft++, all.Length), Mathf.Infinity, ScreenMessageStyle.UPPER_RIGHT);
                ScreenMessages.PostScreenMessage(lastScreenMessage);

                // Does the file have a cache?
                ConstructCacheData ccd = cache.FirstOrDefault(a => a.FilePath.Equals(f.FullName));
                if (ccd == null)
                {
                    added.Add(f);
                    continue;
                }
                else
                {
                    bool fileSizeDiffers = f.Length != ccd.FileSize;
                    bool modifiedDiffers = f.LastWriteTime.Ticks != ccd.Modified;

                    if (!modifiedDiffers && !fileSizeDiffers) // If neither differ, check the hash, just in case.
                    {
                        if (!ccd.Hash.Equals(Utils.getHashForFile(f))) // If the hashes DON'T match, add it to the list to be updated.
                        {
                            updated.Add(f);
                            cache.Remove(ccd);
                        }
                        else
                        {
                            cache.Remove(ccd);
                        }
                    }
                    else
                    {
                        updated.Add(f);
                        cache.Remove(ccd);
                    }

                    // Regardless of whether we have an update or not, remove it from the iterative list
                    //cache.Remove(ccd); // Not working?

                }

            }

            // If I did all the above code right, the only entries left in the iterative cache list *SHOULD* be deleted items. (Spoiler: I didn't get this right the first time)
            removed.AddRange(cache);

            Logger.Log("Cache check has completed: {0} entiries require adding, {1} require updating and {2} require removing", added.Count, updated.Count, removed.Count);

            if (removed.Count > 0)
            {
                Logger.Log("Removing {0} entries...", removed.Count);
                foreach (ConstructCacheData c in removed)
                {
                    ConstructCacheEntry cce = ENTRY_CACHE.FirstOrDefault(b => b.FilePath.Equals(c.FilePath));
                    if (cce == null)
                        continue;
                    Logger.LogWarning("AUTOMATIC CACHE DELETION IS DISABLED FOR SAFETY PURPOSES");
                    Logger.LogWarning("Would delete: {0}", cce.FilePath);
                    //deleteCraft(cce);
                }
            }

            if (updated.Count > 0)
            {
                Logger.Log("Updating {0} entries...", updated.Count);
                generateCacheFromArray(updated.ToArray());
            }

            if (added.Count > 0)
            {
                Logger.Log("Adding {0} entries...", added.Count);
                generateCacheFromArray(added.ToArray());
            }

            sw.Stop();
            Logger.Log("Cache update was completed in {0:n2} seconds.", sw.Elapsed.TotalSeconds);
            ScreenMessages.RemoveMessage(lastScreenMessage);
            lastScreenMessage = null;
            ScreenMessages.PostScreenMessage("BetterLoadMenu has finished updating its cache", 5f, ScreenMessageStyle.UPPER_RIGHT);

        }

        public void saveMainCacheData()
        {
            Utils.createCacheDirectories();
            ConfigNode c = new ConfigNode();
            /*c.AddNode("CACHE_DATA");
            ConfigNode cfg = c.GetNode("CACHE_DATA");*/
            foreach (ConstructCacheData d in CACHE_DATA)
            {
                cfg.AddNode(d.ConfigNode);
            }
            cfg.Save(Path.Combine(Utils.getCacheSaveBaseDirectory(), "cacheData.cfg"));
        }

        public void loadMainCacheData()
        {
            Logger.Log("Loading main cache data...");
            CACHE_DATA.Clear();
            string configPath = Path.Combine(Utils.getCacheSaveBaseDirectory(), "cacheData.cfg");
            if (!File.Exists(configPath))
            {
                Logger.LogWarning("No vessel cache is present for this save!");
                return;
            }
            ConfigNode n = ConfigNode.Load(configPath);
            //ConfigNode n = cfg.GetNode("CACHE_DATA");
            int x = 1;
            foreach (ConfigNode c in n.GetNodes())
            {
                Logger.Log("{0}: {1}", x++, c.name);
                long modified = Convert.ToInt64(c.GetValue("Modified"));
                long filesize = Convert.ToInt64(c.GetValue("FileSize"));

                string hash = c.GetValue("FileHash");
                string path = c.GetValue("FilePath");
                string name = c.GetValue("FileName");

                ConstructCacheData ccd = new ConstructCacheData(name, path, hash, modified, filesize);

                CACHE_DATA.Add(ccd);
            }
            Logger.Log("Loaded {0} entries into main cache", CACHE_DATA.Count);
        }

        [Obsolete("Use deleteCraft(ConstructCacheEntry) instead", true)]
        public void deleteCraft(int id)
        {
            ConstructCacheEntry cce = ENTRY_CACHE[id];
            removeCacheEntry(cce);
            File.Delete(cce.FilePath);
            string path = Path.Combine(Utils.getCacheSaveDirectory(), string.Format("{1}_{0}.vessel", KSPUtil.SanitizeFilename(cce.VesselName), Utils.getFacilityNameFromSavePath(cce.FilePath)));
            File.Delete(path);
        }

        public void deleteCraft(ConstructCacheEntry cce)
        {
            if (File.Exists(cce.FilePath))
                File.Delete(cce.FilePath);
            removeCacheEntry(cce);
            string path = Path.Combine(Utils.getCacheSaveDirectory(cce.Facility), string.Format("{0}.vessel", KSPUtil.SanitizeFilename(cce.VesselName)));
            File.Delete(path);
        }

    }
}
