using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Foxy.QuickGive {
	public static class BackComp {
		private static readonly Dictionary<string, Vector2> labelWidthCache = new Dictionary<string, Vector2>();

		public static Vector2 GetSizeCached(this string s) {
			if(labelWidthCache.Count > 2000 || (Time.frameCount % 40000 == 0 && labelWidthCache.Count > 100)) {
				labelWidthCache.Clear();
			}
			s = s.StripTags();
			if(!labelWidthCache.TryGetValue(s, out var value)) {
				value = Text.CalcSize(s);
				labelWidthCache.Add(s, value);
			}
			return value;
		}
		public static float GetHeightCached(this string s) {
			return s.GetSizeCached().y;
		}
	}
}
