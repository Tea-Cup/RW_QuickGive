using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Foxy.QuickGive {
	[StaticConstructorOnStartup]
	public static class Static {
		private static Game game = null;
		private static GameComponent_QuickGive comp = null;
		private static GameComponent_QuickGive Component {
			get {
				if (game != Current.Game) {
					game = null;
					comp = null;
				}
				if (game == null) game = Current.Game;
				if (comp == null) comp = game?.GetComponent<GameComponent_QuickGive>();
				return comp;
			}
		}

		public static bool WindowInitiallyOpen {
			get => Component?.windowOpen ?? false;
			set {
				if(Component != null) Component.windowOpen = value;
			}
		}
		public static Vector2? WindowInitialPosition {
			get => Component?.windowPos;
			set {
				if (Component != null) Component.windowPos = value;
			}
		}
		public static bool FavoritesInitiallyOpen {
			get => Component?.favsOpen ?? false;
			set {
				if (Component != null) Component.favsOpen = value;
			}
		}
		public static bool WindowInitiallyMinimized {
			get => Component?.minimized ?? false;
			set {
				if (Component != null) Component.minimized = value;
			}
		}
		public static bool MenuInitiallyStack {
			get => Component?.fullStack ?? false;
			set {
				if (Component != null) Component.fullStack = value;
			}
		}

		public static bool IsFavorite(string defName) {
			return Component?.favorites?.Contains(defName) ?? false;
		}
		public static void SetFavorite(string defName, bool favorite) {
			if(favorite) Component?.favorites?.Add(defName);
			else Component?.favorites?.Remove(defName);
		}

		static Static() {
			Logger.Log("Applying patches...");
			Harmony h = new Harmony("Foxy.QuickGive");
			h.PatchAll();
			Logger.Log("Patches applied.");
		}
	}
}
