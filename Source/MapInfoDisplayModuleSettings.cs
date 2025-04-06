using Monocle;
using Celeste.Mod;

namespace Celeste.Mod.MapInfoDisplay
{
	public class MapInfoDisplayModuleSettings : EverestModuleSettings {
		[SettingIgnore]
		private bool _useMultiline { get; set; } = false;

		[SettingIgnore]
		private bool _includeMod { get; set; } = true;

		[SettingIgnore]
		private bool _includeLobby { get; set; } = true;

		[SettingIgnore]
		private bool _includeCreator { get; set; } = true;

		[SettingName("corkr900_MapInfoDisplay_Setting_UseMultiline")]
		public bool UseMultiline {
			get => _useMultiline;
			set {
				if (_useMultiline != value) {
					_useMultiline = value;
					Refresh();
				}
			}
		}

		[SettingName("corkr900_MapInfoDisplay_Setting_IncludeMod")]
		public bool IncludeMod {
			get => _includeMod;
			set {
				if (_includeMod != value) {
					_includeMod = value;
					Refresh();
				}
			}
		}

		[SettingIgnore]  // TODO handle collab lobbies better
		[SettingName("corkr900_MapInfoDisplay_Setting_IncludeLobby")]
		public bool IncludeLobby {
			get => _includeLobby;
			set {
				if (_includeLobby != value) {
					_includeLobby = value;
					Refresh();
				}
			}
		}

		[SettingName("corkr900_MapInfoDisplay_Setting_IncludeCreator")]
		public bool IncludeCreator {
			get => _includeCreator;
			set {
				if (_includeCreator != value) {
					_includeCreator = value;
					Refresh();
				}
			}
		}

		private void Refresh() {
			Session session = (Monocle.Engine.Scene as Level)?.Session;
			InfoFile.Write(session);
		}
	}
}