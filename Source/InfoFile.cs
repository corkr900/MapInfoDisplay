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
			string modname = GetModDisplayName(area);
			string lobbyname = GetLobbyDisplayName(areaData);
			string creator = GetCreator(areaData);

			bool includeMod = IncludeMod(area, mapname, modname);
			bool includeLobby = IncludeLobby(area, lobbyname);
			bool includeCreator = IncludeCreator(area, creator);

			string template = "corkr900_MapInfoDisplay_Map";
			if (includeMod) template += "_Mod";
			if (includeLobby) template += "_Lobby";
			if (includeCreator) template += "_Creator";

			string result = "";
			if (UseMultiline()) {
				mapname += "\n";
				if (!string.IsNullOrEmpty(modname)) modname += "\n";
				if (!string.IsNullOrEmpty(lobbyname)) lobbyname += "\n";
				result = string.Format(Dialog.Get(template), mapname, modname, lobbyname, creator);
				result = result.Replace("\n ", "\n");
			}
			else result = string.Format(Dialog.Get(template), mapname, modname, lobbyname, creator);
			return result;
		}

		private static bool IncludeLobby(AreaKey area, string lobbyname) {
			return (MapInfoDisplayModule.Settings?.IncludeMod ?? true)
				&& false;  // TODO - check whether there is a lobby with translation
		}

		public static bool UseMultiline() {
			return MapInfoDisplayModule.Settings?.UseMultiline ?? false;
		}

		public static bool IncludeMod(AreaKey area, string mapname, string modname) {
			return (MapInfoDisplayModule.Settings?.IncludeMod ?? true)
				&& area.LevelSet != VANILLA_LEVEL_SET
				&& mapname.ToUpperInvariant() != modname.ToUpperInvariant();
		}

		public static bool IncludeCreator(AreaKey area, string creator) {
			return (MapInfoDisplayModule.Settings?.IncludeCreator ?? true)
				&& !string.IsNullOrEmpty(creator)
				&& area.LevelSet != VANILLA_LEVEL_SET;
	}

		private static string GetModDisplayName(AreaKey area) {
			// TODO handle randomizer
			// TODO
			return Dialog.CleanLevelSet(area.LevelSet);
		}

		private static string GetLobbyDisplayName(AreaData area) {
			// TODO
			return "PLACEHOLDER - Lobby";
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
			string key = areaData.Name.DialogKeyify() + "_author";
			string check = Dialog.Get(key);
			if (check == $"[{key}]") return "";
			string[] trimIf = Dialog.Get("corkr900_MapInfoDisplay_TrimFromCreator").Split('^');
			bool useBy = !trimIf.Any(check.StartsWith);
			return useBy
				? string.Format(Dialog.Get("corkr900_MapInfoDisplay_ByCreator"), check)
				: check;
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
				+ "}\n\n"
				+ "/* You can add your own CSS stlyes here! */\n";
		}

	}
}
