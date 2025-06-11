using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace Foxy.QuickGive {
	internal class Dialog_QuickGive : Window {
		private const float GAP = 2f;
		private const float ITEM_HEIGHT = 22f;
		private const float PADDING = 5f;
		private const float PADDING_DOUBLE = PADDING + PADDING;
		private const float LEFT_WIDTH = 300f;
		private const float RIGHT_WIDTH = 300f;
		private const float WINDOW_HEIGHT = 400f;
		private const float RIGHT_HANDLE_WIDTH = 10f;
		private const float HEADER_HEIGHT = 18f;
		private const float HEADER_GAP = 4f;
		private const float HEADER_OFFSET = HEADER_HEIGHT + HEADER_GAP;

		private Vector2 listScrollPosition = Vector2.zero;
		private readonly List<GiveItemInfo> filteredThings = new List<GiveItemInfo>();
		private float listHeight = 0f;

		private Vector2 favsScrollPosition = Vector2.zero;
		private readonly Dictionary<string, GiveItemInfo> favoriteItems = new Dictionary<string, GiveItemInfo>();
		private float favsHeight = 0f;
		private readonly HashSet<GiveItemInfo> waitingFavorites = new HashSet<GiveItemInfo>();

		private string oldFilter = "";
		private bool fullStackFlag = Static.MenuInitiallyStack;
		private bool minimized = Static.WindowInitiallyMinimized;
		private bool rightOpen = Static.FavoritesInitiallyOpen;

		private readonly List<GiveItemInfo> allThings = new List<GiveItemInfo>();
		protected override float Margin => 0f;

		public Dialog_QuickGive() {
			forcePause = false;
			doCloseX = false;
			onlyOneOfTypeAllowed = true;
			onlyDrawInDevMode = true;
			draggable = true;
			preventCameraMotion = false;
			layer = WindowLayer.GameUI;
			doWindowBackground = false;
			closeOnCancel = false;
			closeOnAccept = false;
			closeOnClickedOutside = false;

			var allDefs = DefDatabase<ThingDef>.AllDefs;

			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(IsValidThing)) {
				GiveItemInfo gii = new GiveItemInfo(def) { favorite = Static.IsFavorite(def.defName) };
				if (gii.favorite) favoriteItems.Add(def.defName, gii);
				allThings.Add(gii);
			}
			UpdateFilter("");
			Static.WindowInitiallyOpen = true;
		}

		private static bool IsValidThing(ThingDef def) {
			if (def == null) return false;
			if (def.thingClass == null) return false;
			try {
				return DebugThingPlaceHelper.IsDebugSpawnable(def);
			} catch (Exception e) {
				Logger.Warn(
					$"{e.GetType().Name} in IsDebugSpawnable on def {def.defName} from {def.modContentPack?.Name}. " +
					"Please leave a comment in QuickGive mod page or a discussion with this information.");
				Logger.Warn(e);
				return false;
			}
		}

		protected override void SetInitialSizeAndPosition() {
			base.SetInitialSizeAndPosition();
			Vector2? pos = Static.WindowInitialPosition;
			if (pos.HasValue) {
				windowRect.position = pos.Value;
			}
			UpdateSize();
		}

		public override void PostClose() {
			base.PostClose();
			Static.WindowInitiallyOpen = false;
		}

		public override void DoWindowContents(Rect inRect) {
			if (Event.current.type == EventType.Layout) return;
			Static.WindowInitialPosition = windowRect.position;
			GameFont oldFont = Text.Font;
			Text.Font = GameFont.Tiny;
			Rect outLeft = new Rect(0, 0, LEFT_WIDTH + PADDING_DOUBLE, WINDOW_HEIGHT + PADDING_DOUBLE);

			if (minimized) {
				outLeft.height = HEADER_HEIGHT + PADDING_DOUBLE;
			}

			Rect inLeft = outLeft.ContractedBy(PADDING);
			Widgets.DrawWindowBackground(outLeft);
			Widgets.BeginGroup(inLeft);
			RenderLeftSection(inLeft.AtZero());
			Widgets.EndGroup();

			if (!minimized) {
				Rect outRight = new Rect(outLeft.width, 0, RIGHT_WIDTH + PADDING_DOUBLE, outLeft.height);

				if (!rightOpen) {
					outRight.width = RIGHT_HANDLE_WIDTH;
				}

				Widgets.DrawWindowBackground(outRight);

				Rect handleRect = outRight.RightPartPixels(RIGHT_HANDLE_WIDTH);
				if (Widgets.ButtonText(handleRect, rightOpen ? "<" : ">")) {
					SetRightOpen(!rightOpen);
				}

				if (rightOpen) {
					Rect inRight = outRight.ContractedBy(PADDING);
					inRight.width -= RIGHT_HANDLE_WIDTH;
					Widgets.BeginGroup(inRight);
					RenderRightSection(inRight.AtZero());
					Widgets.EndGroup();
				}
			}

			foreach (GiveItemInfo gii in waitingFavorites) {
				UpdateFavorite(gii);
			}
			waitingFavorites.Clear();
			Text.Font = oldFont;
		}

		private void RenderLeftSection(Rect inRect) {
			Rect rectClose = new Rect(inRect.width - HEADER_HEIGHT, 0, HEADER_HEIGHT, HEADER_HEIGHT);
			if (Widgets.ButtonImage(rectClose, TexButton.CloseXSmall)) Close();

			Rect rectMin = new Rect(rectClose.x - HEADER_HEIGHT - HEADER_GAP, 0, HEADER_HEIGHT, HEADER_HEIGHT);
			if (Widgets.ButtonImage(rectMin, minimized ? Resources.texRestore : Resources.texMinimize)) {
				SetMinimized(!minimized);
			}

			Rect rectHeader = new Rect(0, 0, rectMin.x - HEADER_GAP, HEADER_HEIGHT);
			Widgets.Label(rectHeader, Resources.strWindowHeader);

			if (minimized) return;

			Rect rectFilter = new Rect(0, HEADER_OFFSET, inRect.width, 30f);
			string newFilter = Widgets.TextField(rectFilter, oldFilter);

			if (newFilter != oldFilter) {
				UpdateFilter(newFilter);
			}
			oldFilter = newFilter;

			Rect rectStack = new Rect(0, rectFilter.yMax + GAP, inRect.width, 30f);
			bool oldFlag = fullStackFlag;
			Widgets.CheckboxLabeled(rectStack, Resources.strStackFlag, ref fullStackFlag);
			Static.MenuInitiallyStack = fullStackFlag;
			if (oldFlag != fullStackFlag && DebugTools.curTool is MyDebugTool myTool) {
				OnItemClick(myTool.item);
			}

			float listY = rectStack.yMax + GAP + 1;
			Rect rectList = new Rect(0, listY, inRect.width, inRect.height - listY);

			float scrollPadding = listHeight > rectList.height ? 16f : 0f;
			Rect rectListView = new Rect(0, 0, rectList.width - scrollPadding, listHeight);
			Widgets.BeginScrollView(rectList, ref listScrollPosition, rectListView);
			Rect viewport = new Rect(0, listScrollPosition.y, rectList.width, rectList.height);

			Rect rectItem = new Rect(0, 0, rectListView.width, ITEM_HEIGHT);
			foreach (GiveItemInfo gii in filteredThings) {
				RenderItem(gii, ref rectItem, false, viewport.Overlaps(rectItem));
			}
			Widgets.EndScrollView();
		}

		private void RenderRightSection(Rect inRect) {
			Rect headerRect = new Rect(0, 0, inRect.width, Resources.strFavoritesHeader.GetHeightCached());
			Widgets.Label(headerRect, Resources.strFavoritesHeader);

			float listY = headerRect.yMax + GAP;
			Rect rectList = new Rect(0, listY, inRect.width, inRect.height - listY);

			float scrollPadding = favsHeight > rectList.height ? 16f : 0f;
			Rect rectListView = new Rect(0, 0, rectList.width - scrollPadding, favsHeight);
			Widgets.BeginScrollView(rectList, ref favsScrollPosition, rectListView);
			Rect viewport = new Rect(0, favsScrollPosition.y, rectList.width, rectList.height);

			Rect rectItem = new Rect(0, 0, rectListView.width, ITEM_HEIGHT);
			foreach (GiveItemInfo gii in favoriteItems.Values) {
				bool doRender = viewport.Overlaps(rectItem);
				RenderItem(gii, ref rectItem, true, doRender);
			}

			Widgets.EndScrollView();
		}

		private void RenderItem(GiveItemInfo gii, ref Rect inRect, bool rightItem, bool doRender) {
			if (doRender) {
				Rect rectFav, rectItem;
				if (rightItem) {
					rectFav = inRect.RightPartPixels(ITEM_HEIGHT);
					rectItem = inRect.LeftPartPixels(inRect.width - ITEM_HEIGHT);
				} else {
					rectFav = inRect.LeftPartPixels(ITEM_HEIGHT);
					rectItem = inRect.RightPartPixels(inRect.width - ITEM_HEIGHT);
				}

				if (gii.tooltip != null) TooltipHandler.TipRegion(rectItem, new TipSignal(gii.tooltip));
				if (Widgets.ButtonText(rectItem, gii.label)) OnItemClick(gii);

				Texture2D tex = gii.favorite ? Resources.texStarOn : Resources.texStarOff;
				if (Widgets.ButtonImage(rectFav.ContractedBy(GAP), tex)) waitingFavorites.Add(gii);
			}

			inRect.y += ITEM_HEIGHT + GAP;
		}

		private void SetMinimized(bool value) {
			minimized = value;
			Static.WindowInitiallyMinimized = minimized;
			UpdateSize();
		}

		private void SetRightOpen(bool value) {
			rightOpen = value;
			Static.FavoritesInitiallyOpen = rightOpen;
			UpdateSize();
		}

		private void UpdateSize() {
			float width = LEFT_WIDTH;
			float height = 18f;

			if (!minimized) {
				height = WINDOW_HEIGHT;
				width += PADDING + PADDING;
				if (rightOpen) width += RIGHT_WIDTH;
				else width += RIGHT_HANDLE_WIDTH;
			}

			windowRect.width = PADDING + width + PADDING;
			windowRect.height = PADDING + height + PADDING;
		}

		private void UpdateFilter(string filter) {
			listHeight = GAP;
			filteredThings.Clear();
			foreach (GiveItemInfo gii in allThings) {
				if (!gii.MatchesFilter(filter)) continue;
				listHeight += ITEM_HEIGHT + GAP;
				filteredThings.Add(gii);
			}
		}

		private void OnItemClick(GiveItemInfo gii) {
			bool fullStack = fullStackFlag;
			DebugTools.curTool = new MyDebugTool(gii, fullStack ? gii.stackLabel : gii.label, () => {
				DebugThingPlaceHelper.DebugSpawn(gii.def, UI.MouseCell(), fullStack ? -1 : 1, false, null, false);
			});
		}
		private void UpdateFavorite(GiveItemInfo gii) {
			gii.favorite = !gii.favorite;
			Static.SetFavorite(gii.def.defName, gii.favorite);
			if (gii.favorite) {
				if (favoriteItems.Count == 0) SetRightOpen(true);
				favoriteItems.Add(gii.def.defName, gii);
				favsHeight += ITEM_HEIGHT + GAP;
			} else {
				favoriteItems.Remove(gii.def.defName);
				favsHeight -= ITEM_HEIGHT + GAP;
			}
		}

		private class GiveItemInfo {
			public ThingDef def;
			public string label;
			public string tooltip;
			public string stackLabel;
			public bool favorite;
			public bool hasLabel;

			public GiveItemInfo(ThingDef def) {
				this.def = def;
				hasLabel = def.LabelCap != null;
				label = hasLabel ? def.LabelCap.ToStringSafe() : def.defName;
				tooltip = hasLabel ? def.defName : null;
				stackLabel = def.stackLimit > 1 ? $"{label} x{def.stackLimit}" : label;
			}

			public bool MatchesFilter(string filter) {
				if (filter.Length == 0) return true;
				if (def.defName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) return true;
				if (hasLabel && label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) return true;
				return false;
			}
		}
		private class MyDebugTool : DebugTool {
			public readonly GiveItemInfo item;
			public MyDebugTool(GiveItemInfo item, string label, Action clickAction) : base(label, clickAction) {
				this.item = item;
			}
		}
	}
}
