using BepInEx;
using BepInEx.Logging;
using System;
using System.Security.Permissions;
using RWIMGUI.API;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MenuSceneDebugTool;

[BepInDependency("rwimgui", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MOD_ID, "Menuscene Debug Tool", "2.0.0")]
sealed class Plugin : BaseUnityPlugin
{
	public const string MOD_ID = "magicajaphet.cutsceneedittingtools";
	public static new ManualLogSource Logger;
	bool IsInit;

	private bool postInit;
	internal static bool FlatTest;
	internal static bool TestingState;

	internal static bool hasStringFile;

	internal static readonly bool modTestingState = true;

	public void OnEnable()
	{
		Logger = base.Logger;
		On.RainWorld.OnModsInit += OnModsInit;
		On.RainWorld.PostModsInit += RainWorld_PostModsInit;
	}

	private unsafe void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
	{
		orig(self);

		if (IsInit) return;
		IsInit = true;

		try
		{
			Logger.LogInfo("Initializing IMGUI set up");
			ImGUIAPI.AddCustomMenuCallback(&RenderMenuSceneDebug);

			Logger.LogInfo("Gathering png files and music on startup");
			Assets.GatherAllFiles(Assets.Search.Songs);
			Assets.GatherAllFiles(Assets.Search.SceneFiles);

			Logger.LogInfo("Gathering strings automatically for strings.txt");
			hasStringFile = Assets.FindStringsTxtFile();
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogException(ex);
		}
	}
	private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
	{
		orig(self);

		if (postInit) { return; }
		postInit = true;

		try
		{
			Hooks.Apply();
		}
		catch (Exception e)
		{
			Logger.LogError(e);
		}
	}
	public unsafe static void RenderMenuSceneDebug(ref IntPtr IDXGISwapChain, ref uint SyncInterval, ref uint Flags)
	{
		SceneExplorer.Draw();
	}
}
