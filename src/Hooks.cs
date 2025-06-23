using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Music;
using System.Collections.Generic;

namespace MenuSceneDebugTool
{
	internal class Hooks
	{
		internal static float activeIdleDepth = 0f;
		internal static void Apply()
		{
			IL.Music.MusicPiece.Update += MusicPiece_Update;

			IL.Menu.InteractiveMenuScene.Update += InteractiveMenuScene_Update;
			IL.Menu.MenuScene.ctor += AddFlatModeCheck;
			IL.Menu.SlideShow.RawUpdate += SlideShow_RawUpdate1;
			On.Menu.SlideShow.NextScene += SlideShow_NextScene;
			On.Menu.MenuScene.ctor += MenuScene_ctor;
		}

		private static void MusicPiece_Update(ILContext il)
		{
			ILCursor cursor = new(il);

			cursor.GotoNext(x => x.MatchStloc(3));
			cursor.MoveAfterLabels();
			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate((MusicPiece self) => {
				UnityEngine.Debug.Log($"Music started: {self.name}");
			});
		}

		private static void MenuScene_ctor(On.Menu.MenuScene.orig_ctor orig, MenuScene self, Menu.Menu menu, MenuObject owner, MenuScene.SceneID sceneID)
		{
			orig(self, menu, owner, sceneID);

			if (SceneExplorer.lastMenu == null || SceneExplorer.lastMenu != menu)
			{
				SceneExplorer.lastMenu = menu;
				if (!SceneExplorer.loadedScenes.Contains(self))
				{
					SceneExplorer.loadedScenes.Clear();
				}

				if (!SceneExplorer.gatheredSlideshows)
				{
					SceneExplorer.gatheredSlideshows = true;
					Assets.GatherAllSlideShowInfo();
				}
			}
			if (!Plugin.TestingState)
			{
				SceneExplorer.loadedScenes.Add(self);
			}
		}

		private static void SlideShow_NextScene(On.Menu.SlideShow.orig_NextScene orig, SlideShow self)
		{
			self.inSceneTime = 0f;
			self.crossfadeInd = 0;
			if (self.current >= 0 && self.scene != null)
			{
				self.scene.RemoveSprites();
				self.pages[0].subObjects.Remove(self.scene);
				self.preloadedScenes[self.current].Hide();
				self.pages[0].RemoveSprites();
			}
			self.current++;
			if (self.current < self.playList.Count)
			{
				self.scene = self.preloadedScenes[self.current];
				self.pages[0].subObjects.Add(self.scene);
				self.preloadedScenes[self.current].Show();

				self.time = self.playList[self.current].startAt;
			}
		}

		private static void SlideShow_RawUpdate1(ILContext il)
		{
			ILCursor cursor = new(il);

			bool suc = cursor.TryGotoNext(
				MoveType.After,
				x => x.MatchStfld<SlideShow>(nameof(SlideShow.stall)),
				x => x.MatchLdarg(0)
				);

			if (!suc)
			{
				Plugin.Logger.LogWarning("Slideshow RawUpdate IL failed RIP");
				return;
			}


			static void ExplorerStall(SlideShow self)
			{
				self.stall = SceneExplorer.shouldStall;
			}
			cursor.EmitDelegate(ExplorerStall);
			cursor.Emit(OpCodes.Ldarg_0);
		}

		private static void InteractiveMenuScene_Update(ILContext il)
		{
			ILCursor cursor = new(il);

			bool success = cursor.TryGotoNext(
				move => move.MatchCallvirt(out _),
				move => move.MatchCall(out _),
				move => move.MatchLdcR4(1.5f),
				move => move.MatchCall(out _),
				move => move.MatchLdcR4(0.5f)
				);

			if (!success)
			{
				Plugin.Logger.LogWarning("IDLE DEPTH IL HOOK SOMEHOW FUCKED ITSELF");
				return;
			}

			cursor.Index++;
			cursor.EmitDelegate((float random) =>
			{
				if (SceneExplorer.wantToAddDepth)
				{
					random = SceneExplorer.newDepth;
				}
				activeIdleDepth = random;
			});
			cursor.Emit(OpCodes.Dup);

			bool successAgain = cursor.TryGotoNext(
				move => move.MatchLdfld(out _),
				move => move.MatchCall(out _),
				move => move.MatchLdcR4(1.5f),
				move => move.MatchCall(out _),
				move => move.MatchLdcR4(0.5f)
				);

			if (!successAgain)
			{
				Plugin.Logger.LogWarning("IDLE DEPTH IL HOOK SOMEHOW FUCKED ITSELF #2");
				return;
			}

			cursor.Index++;
			cursor.EmitDelegate((float random) =>
			{
				if (SceneExplorer.wantToAddDepth)
				{
					random = SceneExplorer.newDepth;
				}
				activeIdleDepth = random;
			});
			cursor.Emit(OpCodes.Dup);
			Plugin.Logger.LogMessage("IDLE DEPTH IL SUCCESS");
		}

		private static void AddFlatModeCheck(ILContext il)
		{
			ILCursor cursor = new(il);

			bool success = cursor.TryGotoNext(
				MoveType.After,
				x => x.MatchStfld<MenuScene>(nameof(MenuScene.flatMode))
				);
			if (!success)
			{
				Plugin.Logger.LogError("IL hook in MenuScene ctor failed! WTF!!! HOW??? ITS LITERALLY ONE VARIABLE WHO IS FUCKING WITH self!");
			}

			cursor.Emit(OpCodes.Ldarg_0);
			static void MenuSceneEditorCheck(MenuScene self)
			{
				self.flatMode = (Plugin.TestingState && Plugin.FlatTest) || self.flatMode;
			};
			cursor.EmitDelegate(MenuSceneEditorCheck);
		}
	}

	internal static class Extensions
	{
		public static void SwapItems<T>(this List<T> list, int idxX, int idxY)
		{
			if (idxX != idxY)
			{
				(list[idxY], list[idxX]) = (list[idxX], list[idxY]);
			}
		}

		public static void SwapItems<T>(this T[] array, int idxX, int idxY)
		{
			if (idxX != idxY)
			{
				(array[idxY], array[idxX]) = (array[idxX], array[idxY]);
			}
		}

		public static void Rewind(this SlideShow self)
		{
			self.inSceneTime = 0;
			self.crossfadeInd = 0;

			if (self.current > 0 && self.scene != null)
			{
				self.scene.RemoveSprites();
				self.pages[0].subObjects.Remove(self.scene);
				self.preloadedScenes[self.current].Hide();
				self.pages[0].RemoveSprites();

				self.current--;

				self.scene = self.preloadedScenes[self.current];
				self.pages[0].subObjects.Add(self.scene);
				self.preloadedScenes[self.current].Show();

				self.time = self.playList[self.current].startAt;
			}
		}

		public static void AddCrossFade(this MenuScene self, MenuDepthIllustration newIllu, int index)
		{
			if (!self.crossFades.ContainsKey(index))
			{
				self.crossFades[index] = [];
			}
			self.crossFades[index].Add(newIllu);
			newIllu.setAlpha = new float?(0f);
		}
	}
}
