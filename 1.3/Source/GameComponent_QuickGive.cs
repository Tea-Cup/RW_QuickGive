using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Foxy.QuickGive {
	public class GameComponent_QuickGive : GameComponent {
		public bool windowOpen = false;
		public Vector2? windowPos = null;
		public bool favsOpen = false;
		public bool fullStack = false;
		public bool minimized = false;
		public HashSet<string> favorites = new HashSet<string>();

		public GameComponent_QuickGive(Game game) : base() { }

		public override void FinalizeInit() {
			base.FinalizeInit();
			if(windowOpen) Find.WindowStack.Add(new Dialog_QuickGive());
		}

		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref windowOpen, "open", false);
			Scribe_Values.Look(ref windowPos, "position", null);
			Scribe_Values.Look(ref favsOpen, "favsOpen", false);
			Scribe_Values.Look(ref fullStack, "fullStack", false);
			Scribe_Values.Look(ref minimized, "minimized", false);
			Scribe_Collections.Look(ref favorites, "favorites");
		}
	}
}
