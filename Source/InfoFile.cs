using Celeste.Mod.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.MapInfoDisplay {
	internal class InfoFile {

		private static readonly string TOP_FOLDER = "ModFiles";
		private static readonly string MOD_FOLDER = "corkr900_MapInfoDisplay";

		private static readonly string TXT_FILE_NAME = "MapInfo.txt";
		private static readonly string HTML_FILE_NAME = "MapInfo.html";
		private static readonly string CSS_FILE_NAME = "style.css";

		private static readonly string VANILLA_LEVEL_SET = "Celeste";

		public static void Write(Session session) {
			string text = GetText(session);
			File.WriteAllText(GetTxtPath(), text);
			File.WriteAllText(GetHtmlPath(), MakeHtmlDocument(text));
			string cssPath = Path.Combine(GetDir(), CSS_FILE_NAME);
			if (!File.Exists(cssPath)) File.WriteAllText(cssPath, GetDefaultCss());
		}

		private static string GetDir() {
			string dirpath = Path.Combine(Everest.PathGame, TOP_FOLDER, MOD_FOLDER);
			if (!Directory.Exists(dirpath)) {
				Directory.CreateDirectory(dirpath);
			}
			return dirpath;
		}

		private static string GetTxtPath() {
			return Path.Combine(GetDir(), TXT_FILE_NAME);
		}

		private static string GetHtmlPath() {
			return Path.Combine(GetDir(), HTML_FILE_NAME);
		}

		private static string GetText(Session session) {
			if (session == null) return string.Empty;
			AreaKey area = session.Area;
			AreaData areaData = AreaData.Get(area);

			string mapname = GetMapDisplayName(area, areaData);
			string modname = GetModDisplayName(areaData);
			string lobbyname = GetLobbyDisplayName(areaData);
			string creator = GetCreator(areaData);

			bool includeMod = IncludeMod(area, mapname, modname);
			bool includeLobby = IncludeLobby(area, modname, mapname, lobbyname);
			bool includeCreator = IncludeCreator(area, creator);

			string template = "corkr900_MapInfoDisplay_Map";
			if (includeMod) template += "_Mod";
			if (includeLobby) template += "_Lobby";
			if (includeCreator) template += "_Creator";

			string result = string.Format(Dialog.Get(template), mapname, modname, lobbyname, creator)
				.Replace("<n>", UseMultiline() ? "\n" : " ");
			return result;
		}

		private static bool IncludeLobby(AreaKey area, string modname, string mapname, string lobbyname) {
			bool mainCheck = (MapInfoDisplayModule.Settings?.IncludeMod ?? true)
				&& !string.IsNullOrEmpty(lobbyname)
				&& lobbyname.ToUpperInvariant() != mapname.ToUpperInvariant()
				&& lobbyname.ToUpperInvariant() != modname.ToUpperInvariant();
			if (!mainCheck) return false;
			// Don't show lobby name in single-lobby collabs.
			// To check, get the levels registered immediately before and immediately after the lobby.
			// If neither of them have the same level set as the lobby, it's a single-lobby collab
			string lobbySID = Collab.GetLobbyForMap(area.SID);
			AreaData lobbyData = AreaData.Get(lobbySID);
			if (lobbyData == null) return false;
			AreaData previous = AreaData.Get(lobbyData.ID - 1);
			AreaData next = AreaData.Get(lobbyData.ID + 1);
			bool previousIsPrologue =
				previous == null ? false
				: (previous.SID?.Split("/")?.Last()?.StartsWith("0-") ?? false);
			bool isSingleLobbyCollab =
				(previous?.LevelSet != lobbyData.LevelSet || previousIsPrologue)
				&& next?.LevelSet != lobbyData.LevelSet;
			return !isSingleLobbyCollab;
		}

		public static bool UseMultiline() {
			return MapInfoDisplayModule.Settings?.UseMultiline ?? false;
		}

		public static bool IncludeMod(AreaKey area, string mapname, string modname) {
			return (MapInfoDisplayModule.Settings?.IncludeMod ?? true)
				&& !string.IsNullOrEmpty(modname)
				&& area.LevelSet != VANILLA_LEVEL_SET
				&& mapname.ToUpperInvariant() != modname.ToUpperInvariant();
		}

		public static bool IncludeCreator(AreaKey area, string creator) {
			return (MapInfoDisplayModule.Settings?.IncludeCreator ?? true)
				&& !string.IsNullOrEmpty(creator)
				&& area.LevelSet != VANILLA_LEVEL_SET;
	}

		private static string GetModDisplayName(AreaData area) {
			// TODO handle randomizer
			// Try to get the level set name
			if (TryGetDialog(out string setName, area.LevelSet)) return setName;
			// Safeguard against infinite recursion (probably impossible, but be safe)
			if (Collab.IsCollabLobby(area.SID)) return Collab.GetCollabNameForSID(area.SID);
			// Try to get the collab level's lobby's mod name - i.e. the lobby's level set name
			string lobbySID = Collab.IsCollabGym(area.SID)
				? Collab.GetLobbyForGym(area.SID)
				: Collab.GetLobbyForMap(area.SID);
			if (string.IsNullOrEmpty(lobbySID)) return "";
			if (lobbySID == area.SID) return Collab.GetCollabNameForSID(area.SID);
			AreaData lobbyData = AreaData.Get(lobbySID);
			if (lobbyData == null) return "";
			return GetModDisplayName(lobbyData);
		}

		private static string GetLobbyDisplayName(AreaData area) {
			string lobbySID = Collab.GetLobbyForMap(area.SID);
			if (string.IsNullOrEmpty(lobbySID)) return "";
			AreaData lobbyData = AreaData.Get(lobbySID);
			return lobbyData == null ? ""
				: TryGetDialog(out string lobbyDispName, lobbyData.Name) ? lobbyDispName
				: "";
		}

		private static string GetMapDisplayName(AreaKey area, AreaData data) {
			// TODO handle randomizer
			return (area.Mode != AreaMode.Normal)
				? string.Format("{0} {1}", Dialog.Get(data.Name), GetTranslatedSide(area.Mode))
				: Dialog.Get(data.Name);
		}

		private static string GetTranslatedSide(AreaMode? mode) {
			switch (mode) {
				case AreaMode.Normal:
					return "";
				case AreaMode.BSide:
					return Dialog.Get("OVERWORLD_REMIX");
				case AreaMode.CSide:
					return Dialog.Get("OVERWORLD_REMIX2");
				default:
					return mode.ToString();
			}
		}

		private static string GetCreator(AreaData areaData) {
			if (!TryGetDialog(out string check, areaData.Name, suffix: "_author")) return "";
			string[] trimIf = Dialog.Get("corkr900_MapInfoDisplay_TrimFromCreator").Split('^');
			bool useBy = !trimIf.Any(check.StartsWith);
			return useBy
				? string.Format(Dialog.Get("corkr900_MapInfoDisplay_ByCreator"), check)
				: check;
		}

		private static bool TryGetDialog(out string result, string unformattedKey, string prefix = "", string suffix = "") {
			string key = prefix + unformattedKey.DialogKeyify() + suffix;
			string check = Dialog.Get(key);
			if (check == $"[{key}]") {
				result = "";
				return false;
			}
			result = check;
			return true;
		}

		private static string MakeHtmlDocument(string text) {
			text = HttpUtility.HtmlEncode(text);
			text = text.Replace("\n", "<br/>");

			return $"<!DOCTYPE html>\n"
				+ $"<html lang=\"en\">\n"
				+ $"<head>\n"
				+ $"  <meta charset=\"UTF-8\">\n"
				+ $"  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n"
				+ $"  <meta http-equiv=\"refresh\" content=\"2\">\n"
				+ $"  <title>Map Info</title>\n"

				+ $"</head>\n"
				+ $"<body>\n"
				+ $"  <div>{text}</div>\n"
				+ $"</body>\n"
				+ $"</html>\n";
		}

		private static string GetDefaultCss() {
			return "body {\n"
				+ "  font-family: sans-serif;\n"
				+ "  background-color: rgba(0, 0, 0, 0);\n"
				+ "  margin: 0px auto;\n"
				+ "  overflow: hidden;\n"
				+ "  font-size: 2em;\n"
				+ "}\n\n"
				+ "/* You can add your own CSS stlyes here! */\n";
		}

	}
}
