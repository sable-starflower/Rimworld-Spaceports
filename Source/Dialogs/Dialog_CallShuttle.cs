using System;
using UnityEngine;
using Verse;

namespace Spaceports.Dialogs
{
	public class Dialog_CallShuttle : Window
	{

		private Action confirmAction;
		private bool EnoughSilver;
		private bool PadAvailable;
		public override Vector2 InitialSize => new Vector2(300f, 230f);
		public Dialog_CallShuttle(Action confirmAction, bool EnoughSilver, bool PadAvailable)
		{
			this.confirmAction = confirmAction;
			this.EnoughSilver = EnoughSilver;
			this.PadAvailable = PadAvailable;
			forcePause = true;
			closeOnClickedOutside = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.Label("Spaceports_Uplink".Translate().Colorize(Color.gray));
			listingStandard.GapLine();
            listingStandard.Label("Spaceports_CallTaxiInfo".Translate() + GetBlockedReasons());
            if (listingStandard.ButtonText("Spaceports_CallTaxiCancel".Translate()))
            {
				Close();
            }
            if (EnoughSilver == false || PadAvailable == false)
            {
				listingStandard.ButtonText("Spaceports_CallTaxiConfirm".Translate().Colorize(Color.red));
			}
			else if(EnoughSilver == true && PadAvailable == true)
            {
				if (listingStandard.ButtonText("Spaceports_CallTaxiConfirm".Translate()))
				{
					Close();
					confirmAction();
				}
			}

			listingStandard.End();
		}

		private string GetBlockedReasons() 
		{
			string blockedReasons = "";
			if (!EnoughSilver)
			{
				blockedReasons += "Spaceports_CannotProceedSilver".Translate().Colorize(Color.red);
			}
			if (!PadAvailable)
			{
				blockedReasons += "Spaceports_CannotProceedNoPads".Translate().Colorize(Color.red);
			}
			return blockedReasons;
		}
	}
}
