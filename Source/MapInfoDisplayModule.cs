using FMOD.Studio;
using MonoMod.ModInterop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Celeste.TextMenuExt;

namespace Celeste.Mod.MapInfoDisplay;

public class MapInfoDisplayModule : EverestModule {
    public static MapInfoDisplayModule Instance { get; private set; }

    public override Type SettingsType => typeof(MapInfoDisplayModuleSettings);
    public static MapInfoDisplayModuleSettings Settings => (MapInfoDisplayModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(MapInfoDisplayModuleSession);
    public static MapInfoDisplayModuleSession Session => (MapInfoDisplayModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(MapInfoDisplayModuleSaveData);
    public static MapInfoDisplayModuleSaveData SaveData => (MapInfoDisplayModuleSaveData) Instance._SaveData;

    public MapInfoDisplayModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(MapInfoDisplayModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(MapInfoDisplayModule), LogLevel.Info);
#endif
    }

    public override void Load() {
		On.Celeste.LevelLoader.StartLevel += OnLevelLoaderStart;
		Everest.Events.Level.OnExit += onLevelExit;
		typeof(CollabUtils2_LobbyHelper).ModInterop();
	}

    public override void Unload() {
		On.Celeste.LevelLoader.StartLevel -= OnLevelLoaderStart;
		Everest.Events.Level.OnExit -= onLevelExit;
	}

	public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
		base.CreateModMenuSection(menu, inGame, snapshot);

		ButtonExt btn = new ButtonExt(Dialog.Clean("corkr900_MapInfoDisplay_OpenInstructions"));
		btn.Pressed(() => {
			// open the instructions file
			string url = "https://github.com/corkr900/MapInfoDisplay/wiki/Map-Info-Display";
			if (!OpenUrl(url)) {
				SetSubtext(btn, menu, "Head2Head_SettingsManager_CantOpenURLs");
			}
			else {
				SetSubtext(btn, menu, "Head2Head_SettingsManager_OpenedURL");
			}
		});
		menu.Add(btn);
	}

	internal static bool OpenUrl(string url) {
		try {
			Process.Start(url);
			return true;
		}
		catch {
			// This is a hack because of https://github.com/dotnet/corefx/issues/10361
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo() { FileName = url, UseShellExecute = true });
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				Process.Start("xdg-open", url);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				Process.Start("open", url);
			}
			else {
				return false;
			}
			return true;
		}
	}

	private static void SetSubtext(ButtonExt btn, TextMenu menu, string newSubtext, params string[] fmtArgs) {
		if (!menu.Items.Contains(btn)) return;
		int idx = menu.IndexOf(btn);
		if (menu.Items.Count >= menu.IndexOf(btn) + 2 && menu.Items[idx + 1] is EaseInSubHeaderExt oldDescription) {
			menu.Remove(oldDescription);
		}
		btn.AddDescription(menu, string.Format(GetDialogWithLineBreaks(newSubtext), fmtArgs));
		if (menu.IndexOf(btn) == menu.Selection) {
			btn.OnEnter();
		}
	}

	private static string GetDialogWithLineBreaks(string key) {
		return (Dialog.Has(key) ? Dialog.Get(key) : key ?? "").Replace("{n}", "\n").Replace("{break}", "\n");
	}


	private void OnLevelLoaderStart(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
		orig(self);
        InfoFile.Write(self.session);
	}

	private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
		if (mode != LevelExit.Mode.Restart) {
			InfoFile.Write(null);
		}
	}

}