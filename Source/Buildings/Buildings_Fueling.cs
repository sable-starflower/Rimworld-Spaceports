using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Spaceports.Buildings
{
    public class Building_FuelProcessor : Building
    {
        //Large building
        //Must be placed adjacent to at least nine continguous water tiles and fueled with chemfuel plus a good chunk of power
        //This in turn "fills" linked fuel tanks with units of "fusion fuel"
    }

    public class Building_FuelTank : Building
    {
        //Medium building
        //Basically inert on its own
        //Has an internal fuel counter that can be ticked up by a linked and operating fuel processor,
        //or siphoned out by fuel dispeners on the same powernet
        private int FusionFuelLevel = 0;
        private const int FuelCap = 1000;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref FusionFuelLevel, "FusionFuelLevel", 0);
            base.ExposeData();
        }

        public override string GetInspectString()
        {
            return base.GetInspectString();
        }

        public bool CanAcceptFuelNow(int amount = 0)
        {
            if(FusionFuelLevel + amount >= FuelCap)
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
            if (FusionFuelLevel - amount <= 0)
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
        //Small building
        //Draws fusion fuel units from tanks on the same powernet and "sells" it to landing shuttles
        //the amount taken (and therefore amount of silver made) varies randomly within locked bounds
        private int TotalSales = 0;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TotalSales, "TotalSales", 0);
            base.ExposeData();
        }

        public override string GetInspectString()
        {
            return base.GetInspectString();
        }

        public bool TrySellFuel()
        {
            int FuelRequested = Rand.RangeInclusive(100, 500);
            return true;
        }
    }
}
