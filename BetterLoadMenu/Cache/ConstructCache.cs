using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BetterLoadMenu.Editor;
using BetterLoadMenu.Utilities;

namespace BetterLoadMenu.Cache
{
    public class ConstructCache
    {

        public ConstructCache(FileInfo fi, bool stock = false)
        {
            CraftEntry = new LoadCraftDialog.CraftEntry(fi, stock);
            CacheEntry = new ConstructCacheEntry(fi.FullName, CraftEntry.template.shipName, CraftEntry.template.shipDescription, Stages, PartCount, Cost, Weight, CraftEntry.thumbURL);
        }

#pragma warning disable 0618, 0612
        [Obsolete("Use ConstructCache(FileInfo[, bool]) instead", true)]
        public ConstructCache(ShipConstruct c)
        {
            this.Construct = c;
            CacheEntry = new ConstructCacheEntry(this);
        }


        [Obsolete("Use ConstructCache(FileInfo[, bool]) instead", true)]
        public ConstructCache(Vessel v)
        {
            ShipConstruct c = new ShipConstruct();
            ConfigNode vessel = new ConfigNode();
            v.protoVessel.Save(vessel);
            c.LoadShip(vessel);
            Construct = c;
            CacheEntry = new ConstructCacheEntry(this);
        }
#pragma warning restore 0618, 0612

        public LoadCraftDialog.CraftEntry CraftEntry { get; private set; }

        public string VesselName
        {
            get
            {
                return CraftEntry.template.shipName;
            }
        }

        public int Stages
        {
            get
            {
                return CraftEntry == null ? 0 : CraftEntry.stageCount;
            }
        }
        [Obsolete]
        public ShipConstruct Construct { get; private set; }
        public float Cost
        {
            get
            {
                return CraftEntry.template.totalCost;
            }
        }
        public int PartCount
        {
            get
            {
                return CraftEntry.partCount;
            }
        }

        public float Weight
        {
            get
            {
                return CraftEntry.template.totalMass;
            }
        }

        public ConstructCacheEntry CacheEntry { get; private set; }

    }
}
