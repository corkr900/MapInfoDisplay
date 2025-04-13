using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.MapInfoDisplay
{
	[ModImportName("CollabUtils2.LobbyHelper")]
	public static class CollabUtils2_LobbyHelper {
		public static Func<string, bool> IsCollabLevelSet;
		public static Func<string, bool> IsCollabMap;
		public static Func<string, bool> IsCollabLobby;
		public static Func<string, bool> IsCollabGym;
		public static Func<string, bool> IsHeartSide;
		public static Func<string, string> GetLobbyForMap;
		public static Func<string, string> GetLobbyLevelSet;
		public static Func<string, string> GetLobbyForLevelSet;
		public static Func<string, string> GetLobbyForGym;
		public static Func<string, string> GetCollabNameForSID;
	}

	public static class Collab {
		public static bool IsCollabLevelSet(string levelSet) => CollabUtils2_LobbyHelper.IsCollabLevelSet?.Invoke(levelSet) ?? false;
		public static bool IsCollabMap(string sid) => CollabUtils2_LobbyHelper.IsCollabMap?.Invoke(sid) ?? false;
		public static bool IsCollabLobby(string sid) => CollabUtils2_LobbyHelper.IsCollabLobby?.Invoke(sid) ?? false;
		public static bool IsCollabGym(string sid) => CollabUtils2_LobbyHelper.IsCollabGym?.Invoke(sid) ?? false;
		public static bool IsHeartSide(string sid) => CollabUtils2_LobbyHelper.IsHeartSide?.Invoke(sid) ?? false;
		public static string GetLobbyForMap(string sid) => CollabUtils2_LobbyHelper.GetLobbyForMap?.Invoke(sid) ?? "";
		public static string GetLobbyLevelSet(string sid) => CollabUtils2_LobbyHelper.GetLobbyLevelSet?.Invoke(sid) ?? "";
		public static string GetLobbyForLevelSet(string levelSet) => CollabUtils2_LobbyHelper.GetLobbyForLevelSet?.Invoke(levelSet) ?? "";
		public static string GetLobbyForGym(string gymSID) => CollabUtils2_LobbyHelper.GetLobbyForGym?.Invoke(gymSID) ?? "";
		public static string GetCollabNameForSID(string sid) => CollabUtils2_LobbyHelper.GetCollabNameForSID?.Invoke(sid) ?? "";
	}
}
