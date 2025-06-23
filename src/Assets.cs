using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MenuSceneDebugTool
{
	internal class Assets
	{
		internal static Dictionary<string, List<string>> storedSceneFoldersAndFiles = [];
		internal static Queue<string[]> queuedScenes = [];
		internal static Queue<string[]> queuedSongs = [];
		internal static List<string> loadedSongNames = [];

		public enum Search
		{
			SceneFiles,
			Songs
		
		}

		internal static List<SlideShow.SlideShowID> avaliableSlideShows = [];
		internal static List<string> stringsToTranslate = [];
		private static string stringFilePath;
		internal static Dictionary<string, string> cachedElements = [];
		internal static float? slideShowOffset;

		internal static void GrabMissingImages(ref MenuScene scene)
		{
			Plugin.TestingState = true;
			MenuScene? discard = null;
			Plugin.FlatTest = !scene.flatMode;
			if (scene is InteractiveMenuScene)
			{
				discard = new InteractiveMenuScene(scene.menu, scene.owner, scene.sceneID);
			}
			else if (scene is SlideShowMenuScene)
			{
				discard = new SlideShowMenuScene(scene.menu, scene.owner, scene.sceneID);
			}

			if (discard != null)
			{
				if (discard.flatIllustrations.Count > 0 && !scene.flatMode)
				{
					foreach (var image in discard.flatIllustrations)
					{
						scene.AddIllustration(image);
					}
				}
				if (discard.depthIllustrations.Count > 0 && scene.flatMode)
				{
					foreach (var image in discard.depthIllustrations)
					{
						scene.AddIllustration(image);
					}
				}
			}
			discard?.RemoveSprites();
			Plugin.TestingState = false;
		}

		internal static void ComputeSomeVeryCursedFileNumberNabbing(List<string> files, int maxInt, ref string firstFile)
		{
			firstFile = files[files.IndexOf(files.Where(x => x.ToCharArray().Where(char.IsDigit).Count() > 0 && int.Parse(string.Join("", x.Where(char.IsDigit))) == maxInt).First())];
		}

		public static void GatherAllSlideShowInfo()
		{
			avaliableSlideShows = SlideShow.SlideShowID.GetNames(typeof(SlideShow.SlideShowID)).Select(x => SlideShow.SlideShowID.Parse(typeof(SlideShow.SlideShowID), x, true) as SlideShow.SlideShowID).ToList();
			avaliableSlideShows.Sort((x, y) => x.value.CompareTo(y.value));
		}

		public static void GatherAllFiles(Search searchkey)
		{
			queuedScenes.Clear();
			queuedSongs.Clear();
			
			CheckModDirectories("", searchkey);

			if (searchkey == Search.SceneFiles)
			{
				CheckModDirectories("slugbase" + Path.DirectorySeparatorChar, searchkey);
				storedSceneFoldersAndFiles.Clear();
			}
			if (searchkey == Search.Songs)
			{
				CheckModDirectories("./Assets/Resources/", searchkey);
				loadedSongNames.Clear();
			}
			foreach (var files in searchkey == Search.SceneFiles ? queuedScenes : queuedSongs)
			{
				foreach (var file in files)
				{
					if (!string.IsNullOrEmpty(file) && File.Exists(file))
					{
						string path = StripFolderPath(file, searchkey).ToLowerInvariant();

						if (searchkey == Search.SceneFiles)
						{
							if (!storedSceneFoldersAndFiles.ContainsKey(path))
							{
								storedSceneFoldersAndFiles.Add(path, []);
							}
							storedSceneFoldersAndFiles[path].Add(Path.GetFileNameWithoutExtension(file).ToLowerInvariant());
						}
						if (searchkey == Search.Songs && !Path.GetExtension(file).ToLowerInvariant().Contains("meta"))
						{
							loadedSongNames.Add(Path.GetFileNameWithoutExtension(file).ToLowerInvariant());
						}
					}
				}
			}
		}

		private static void CheckModDirectories(string path, Search searchkey)
		{
			path = path.Replace('/', Path.DirectorySeparatorChar);
			for (int i = ModManager.ActiveMods.Count - 1; i >= 0; i--)
			{
				if (ModManager.ActiveMods[i].hasTargetedVersionFolder)
				{
					CheckForFolders(Path.Combine(ModManager.ActiveMods[i].TargetedPath.Replace('/', Path.DirectorySeparatorChar), path.ToLowerInvariant()), searchkey);
				}
				if (ModManager.ActiveMods[i].hasNewestFolder)
				{
					CheckForFolders(Path.Combine(ModManager.ActiveMods[i].NewestPath.Replace('/', Path.DirectorySeparatorChar), path.ToLowerInvariant()), searchkey);
				}
				CheckForFolders(Path.Combine(ModManager.ActiveMods[i].path.Replace('/', Path.DirectorySeparatorChar), path.ToLowerInvariant()), searchkey);
			}
			CheckForFolders(Path.Combine(Custom.RootFolderDirectory().Replace('/', Path.DirectorySeparatorChar), path.ToLowerInvariant()), searchkey);
		}

		private static void CheckForFolders(string path, Search searchkey)
		{
			if (Directory.Exists(path))
			{
				string[] search = Directory.GetDirectories(path);

				foreach (string folder in search)
				{
					if (searchkey == Search.SceneFiles)
					{
						if (folder.ToLowerInvariant().Contains("scenes") || folder.ToLowerInvariant().Contains("slideshows"))
						{
							queuedScenes.Enqueue(Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories));
						}
					}
					if (searchkey == Search.Songs)
					{
						if (folder.ToLowerInvariant().Contains("music"))
						{
							CheckForFolders(folder, searchkey);
						}
						if (folder.ToLowerInvariant().Contains("songs"))
						{
							queuedSongs.Enqueue(Directory.GetFiles(folder));
						}
					}
				}
			}
		}

		private static string StripFolderPath(string folder, Search searchkey)
		{
			folder = Path.GetDirectoryName(folder);
			string[] paths = folder.Split(Path.DirectorySeparatorChar);
			for (int i = 0; i < paths.Length; i++)
			{
				if (searchkey == Search.SceneFiles && (paths[i].ToLowerInvariant().Contains("scenes") || paths[i].ToLowerInvariant().Contains("slideshow")))
				{
					return folder.Substring(folder.IndexOf(paths[i]));
				}
			}

			return folder;
		}

		internal static bool FindStringsTxtFile()
		{
			string likelyCanditate = ModManager.ActiveMods.Where(x => x.id == Plugin.MOD_ID).FirstOrDefault().path;

			if (!string.IsNullOrEmpty(likelyCanditate) && Directory.Exists(likelyCanditate))
			{
				Plugin.Logger.LogInfo("Found mod path, searching for strings...");

				likelyCanditate += "/text/text_eng/strings.txt";
				likelyCanditate.Replace('/', Path.DirectorySeparatorChar);

				if (File.Exists(likelyCanditate))
				{
					Plugin.Logger.LogInfo("Found string file!");
					stringFilePath = likelyCanditate;
					return true;
				}
			}
			return false;
		}

		internal static void AddPossibleStringToTranslation(string s)
		{
			if (!Plugin.modTestingState) { return; }
			if (!stringsToTranslate.Contains(s))
			{
				stringsToTranslate.Add(s);

				if (Plugin.hasStringFile)
				{
					string[] stringKeys = File.ReadAllLines(stringFilePath).Where(x => !string.IsNullOrWhiteSpace(x) && x.Contains('|')).Select(x => x.Split('|').FirstOrDefault()).ToArray();
					using (FileStream fileWrite = new(stringFilePath, FileMode.Append))
					using (StreamWriter writer = new(fileWrite))
					{
						for (int i = 0; i < stringsToTranslate.Count; i++)
						{
							// Add a linereader and shit
							if (!stringKeys.Contains(stringsToTranslate[i]))
							{
								if (stringsToTranslate[i].Contains('_'))
								{
									writer.WriteLine($"{stringsToTranslate[i]}|WIP");
								}
								else
								{
									writer.WriteLine($"{stringsToTranslate[i]}|{stringsToTranslate[i]}");
								}
							}
						}

						writer.Close();
						fileWrite.Close();
					}
				}
			}
		}

		internal static void ForceLoadImagesForCache(MenuScene scene)
		{
			if (storedSceneFoldersAndFiles.ContainsKey(scene.sceneFolder.ToLowerInvariant()))
			{
				foreach (var image in Assets.storedSceneFoldersAndFiles[scene.sceneFolder.ToLowerInvariant()])
				{
					MenuIllustration illus = new(scene.menu, scene.owner, scene.sceneFolder, image, new(), false, true);
					illus.RemoveSprites();
				}
			}

			RefreshCache(scene);
		}

		internal static void RefreshCache(MenuScene scene)
		{
			Assets.cachedElements.Clear();
			if (!string.IsNullOrEmpty(scene.sceneFolder) && Assets.storedSceneFoldersAndFiles.ContainsKey(scene.sceneFolder.ToLowerInvariant()))
			{
				List<string> files = Assets.storedSceneFoldersAndFiles[scene.sceneFolder.ToLowerInvariant()];
				string[] cachedKeys = Futile.atlasManager._allElementsByName.Keys.ToArray();
				for (int i = 0; i < cachedKeys.Length; i++)
				{
					for (int j = 0; j < files.Count; j++)
					{
						if (files[j].ToLowerInvariant() == cachedKeys[i].ToLowerInvariant())
						{
							Assets.cachedElements.Add(files[j], cachedKeys[i]);
							break;
						}
					}
				}
			}
		}

		internal static void RefreshImageLayering(MenuScene scene)
		{
			for (int i = 0; i < scene.depthIllustrations.Count; i++)
			{
				MenuDepthIllustration illus = scene.depthIllustrations[i];
				if (illus != null && illus.sprite != null)
				{
					if (i != 0)
					{
						illus.sprite.MoveInFrontOfOtherNode(scene.depthIllustrations[i - 1].sprite);
					}

					if (scene.depthIllustrations.Count == 1)
					{
						illus.sprite.MoveToBack();
					}
				}
			}
			for (int i = 0; i < scene.flatIllustrations.Count; i++)
			{
				MenuIllustration illus = scene.flatIllustrations[i];
				if (illus != null && illus.sprite != null)
				{
					if (i != 0)
					{
						illus.sprite.MoveInFrontOfOtherNode(scene.flatIllustrations[i - 1].sprite);
					}

					if (scene.flatIllustrations.Count == 1)
					{
						illus.sprite.MoveToBack();
					}
				}
			}
		}
	}
}
