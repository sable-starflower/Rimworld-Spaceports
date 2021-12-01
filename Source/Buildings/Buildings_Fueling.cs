using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using SharpUtils;
using UnityEngine;

namespace Spaceports.Buildings
{
    public class Building_FuelProcessor : Building
    {
        private int TotalProduced = 0;
        private int ProductionCache = 0;
        private int ProductionMode = 0; //0 for wet, 1 for dry
        private int UnitsPerRareTick
        {
            get
            {
                if(ProductionMode == 0) { return 6; }
                else { return 3; }
            }

        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TotalProduced, "TotalProduced", 0);
            Scribe_Values.Look(ref ProductionCache, "ProductionCache", 0);
            Scribe_Values.Look(ref ProductionMode, "ProductionMode", 0);
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                ProductionMode = GetProductionMode();
            }

            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            str += "\n" + "Spaceports_TotalFuelProduced".Translate(TotalProduced);
            str += "\n" + "Spaceports_ProductionCache".Translate(ProductionCache);
            return str;
        }

        public override void Tick()
        {
            base.Tick();
            if(Find.TickManager.TicksGame % 500 == 0)
            {
                RareTick();
            }
        }

        public void RareTick()
        {
            CompRefuelable FuelComp = this.GetComp<CompRefuelable>();
            if(FuelComp != null)
            {
                if (FuelComp.HasFuel && this.GetComp<CompPowerTrader>().PowerOn && !GetLinkedTanks().NullOrEmpty() && CanAnyTankAcceptFuelNow())
                {
                    TotalProduced += UnitsPerRareTick;
                    TryDistributeFuel();
                }
                else if(GetLinkedTanks().NullOrEmpty() || !CanAnyTankAcceptFuelNow())
                {
                    if(FuelComp.HasFuel && this.GetComp<CompPowerTrader>().PowerOn)
                    {
                        TotalProduced += UnitsPerRareTick;
                        ProductionCache += UnitsPerRareTick;
                    }
                }
            }
        }

        private int GetProductionMode()
        {
            if (Verse.ModLister.HasActiveModWithName("Dubs Bad Hygiene")) ;
            return 0;
        }

        private List<Building_FuelTank> GetLinkedTanks()
        {
            List<Building_FuelTank> LinkedTanks = new List<Building_FuelTank>();
            foreach (Thing t in this.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading)
            {
                Building_FuelTank tank = t as Building_FuelTank;
                if (t != null)
                {
                    LinkedTanks.Add(tank);
                }
            }
            return LinkedTanks;
        }

        private bool CanAnyTankAcceptFuelNow()
        {
            bool result = false;
            foreach(Building_FuelTank tank in GetLinkedTanks())
            {
                if(tank.FuelLevel() < 1000)
                {
                    result = true;
                }
            }
            return result;
        }

        private void TryDistributeFuel()
        {
            int DropsRemaining = UnitsPerRareTick;
            DropsRemaining += ProductionCache;
            ProductionCache = 0;
            while(DropsRemaining > 0)
            {
                foreach (Building_FuelTank tank in GetLinkedTanks())
                {
                    if (DropsRemaining > 0 && tank.CanAcceptFuelNow(1))
                    {
                        tank.AddFuel(1);
                        DropsRemaining--;
                    }
                    else if(DropsRemaining <= 0)
                    {
                        break;
                    }
                }
                if (!CanAnyTankAcceptFuelNow())
                {
                    break; //Emergency breakout to prevent game lock
                }
            }
        }
    }

    public class PlaceWorker_FuelProcessor : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 item in CompPowerPlantWater.GroundCells(loc, rot))
            {
                if (!map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                {
                    return new AcceptanceReport("TerrainCannotSupport_TerrainAffordance".Translate(checkingDef, TerrainAffordanceDefOf.Heavy));
                }
            }
            return true;
        }

        public override IEnumerable<TerrainAffordanceDef> DisplayAffordances()
        {
            yield return TerrainAffordanceDefOf.Heavy;
            yield return TerrainAffordanceDefOf.MovingFluid;
            yield return SpaceportsDefOf.ShallowWater;
        }
    }

    public class Building_FuelTank : Building
    {
        private int FusionFuelLevel = 0;
        private const int FuelCap = 1000;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref FusionFuelLevel, "FusionFuelLevel", 0);
            base.ExposeData();
        }

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            str += "\n" + "Spaceports_FuelLevel".Translate(FusionFuelLevel);
            return str;
        }

        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame % 250 == 0)
            {
                RareTick();
            }
        }

        public void RareTick()
        {
            if (!this.GetComp<CompPowerTrader>().PowerOn && this.FusionFuelLevel >= 0)
            {
                this.FusionFuelLevel = Mathf.Clamp(this.FusionFuelLevel - 4, 0, 1000);
            }
        }

        public int FuelLevel()
        {
            return FusionFuelLevel;
        }

        public bool CanAcceptFuelNow(int amount = 0)
        {
            if(FusionFuelLevel + amount > FuelCap)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void AddFuel(int amount)
        {
            FusionFuelLevel += amount;
        }

        public bool CanDrainFuelNow(int amount = 0)
        {
            if (FusionFuelLevel - amount < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void DrainFuel(int amount)
        {
            FusionFuelLevel -= amount;
        }

    }

    public class Building_FuelDispenser : Building
    {
        private int TotalSales = 0;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TotalSales, "TotalSales", 0);
            base.ExposeData();
        }

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            str += "\n" + "Spaceports_TotalSales".Translate(TotalSales);
            str += "\n" + GetNetworkInfo();
            return str;
        }

        private string GetNetworkInfo()
        {
            string result = "";
            int NetworkFuelLevel = 0;
            List<Building_FuelTank> tanks = new List<Building_FuelTank>();

            foreach (Building_FuelTank tank in this.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FuelTank>())
            {
                if (tank.PowerComp.PowerNet == this.PowerComp.PowerNet)
                {
                    tanks.Add(tank);
                    NetworkFuelLevel += tank.FuelLevel();
                }
            }

            result += "Spaceports_NetworkInfo".Translate(tanks.Count, NetworkFuelLevel);
            return result;
        }

        private Thing GenSilver(int amount)
        {
            Thing product = ThingMaker.MakeThing(ThingDefOf.Silver);
            product.stackCount = amount;
            return product;
        }

        public bool TrySellFuel()
        {
            int FuelRequested = Rand.RangeInclusive(250, 750);
            List<Building_FuelTank> tanks = new List<Building_FuelTank>();

            foreach(Building_FuelTank tank in this.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FuelTank>())
            {
                if(tank.PowerComp.PowerNet == this.PowerComp.PowerNet)
                {
                    tanks.Add(tank);
                }
            }

            foreach(Building_FuelTank tank in tanks)
            {
                if (tank.CanDrainFuelNow(FuelRequested))
                {
                    tank.DrainFuel(FuelRequested);
                    this.TotalSales += (int)(FuelRequested * 0.25f);
                    Thing silver = GenSilver((int)(FuelRequested * 0.25f));
                    GenPlace.TryPlaceThing(silver, this.InteractionCell, this.Map, ThingPlaceMode.Near);
                    return true;
                }
            }

            List<Building_FuelTank> TankPool = new List<Building_FuelTank>();
            int PoolSize = 0;
            foreach(Building_FuelTank tank in tanks)
            {
                if(tank.FuelLevel() > 0)
                {
                    TankPool.Add(tank);
                    PoolSize += tank.FuelLevel();
                    if(PoolSize >= FuelRequested)
                    {
                        break;
                    }
                }
            }

            if(PoolSize >= FuelRequested)
            {
                this.TotalSales += (int)(FuelRequested * 0.25f);
                Thing silver = GenSilver((int)(FuelRequested * 0.25f));
                GenPlace.TryPlaceThing(silver, this.InteractionCell, this.Map, ThingPlaceMode.Near);
                foreach (Building_FuelTank tank in TankPool)
                {
                    if(FuelRequested >= tank.FuelLevel() && tank.CanDrainFuelNow(tank.FuelLevel()))
                    {
                        FuelRequested -= tank.FuelLevel();
                        tank.DrainFuel(tank.FuelLevel());
                    }
                    else if(FuelRequested < tank.FuelLevel() && tank.CanDrainFuelNow(tank.FuelLevel() - FuelRequested))
                    {
                        FuelRequested -= tank.FuelLevel() - FuelRequested;
                        tank.DrainFuel(tank.FuelLevel() - FuelRequested);
                    }
                    if(FuelRequested <= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
