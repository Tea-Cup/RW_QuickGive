using UnityEngine;
using Verse;

namespace Foxy.QuickGive {
	[StaticConstructorOnStartup]
	internal static class Resources {
		public static readonly Texture2D texOpenDebugGiveMenu = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenDebugGiveMenu");
		public static readonly Texture2D texMinimize = ContentFinder<Texture2D>.Get("UI/Buttons/Minimize");
		public static readonly Texture2D texRestore = ContentFinder<Texture2D>.Get("UI/Buttons/Restore");
		public static readonly Texture2D texStarOff = ContentFinder<Texture2D>.Get("UI/Buttons/star_off");
		public static readonly Texture2D texStarOn = ContentFinder<Texture2D>.Get("UI/Buttons/star_on");

		public static readonly string strOpenMenu = Translate("Foxy.QuickGive.OpenMenu", "Open quick give menu.");
		public static readonly string strCloseMenu = Translate("Foxy.QuickGive.CloseMenu", "Close quick give menu.");
		public static readonly string strStackFlag = Translate("Foxy.QuickGive.StackFlag", "Spawn full stack");
		public static readonly string strWindowHeader = Translate("Foxy.QuickGive.WindowHeader", "Spawn Item");
		public static readonly string strFavoritesHeader = Translate("Foxy.QuickGive.FavoritesHeader", "Favorites");

		static Resources() {
			Logger.Log("Resources loaded.");
		}

		private static string Translate(string key, string def) {
			if (key.TryTranslate(out var str)) return str.ToStringSafe();
			return def;
		}
	}
}
