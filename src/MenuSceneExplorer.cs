using ImGuiNET;
using ImPlotNET;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MenuSceneDebugTool
{
	internal class SceneImgui
	{
		public enum SoundFlag
		{
			Slider,
			Select,
			Confirm,
			Switch,
			Tick
		}

		public static Dictionary<SoundFlag, SoundID> soundLibrary = new(){
			{ SoundFlag.Slider, SoundID.MENU_Scroll_Tick },
			{ SoundFlag.Select, SoundID.MENU_Next_Slugcat },
			{ SoundFlag.Confirm, SoundID.MENU_Button_Successfully_Assigned },
			{ SoundFlag.Switch, SoundID.MENU_MultipleChoice_Clicked },
			{ SoundFlag.Tick, SoundID.MENU_Checkbox_Check },
		};

		internal static bool AddSound(bool v, bool checkForClick, SoundFlag sound)
		{
			if (v && SceneExplorer.lastMenu != null && soundLibrary.ContainsKey(sound) && (!checkForClick || (checkForClick && ImGui.IsItemClicked()) || (checkForClick && (ImGui.IsItemEdited() || ImGui.IsItemToggledOpen() || ImGui.IsItemDeactivated()))))
			{
				SceneExplorer.lastMenu.PlaySound(soundLibrary[sound]);
			}
			return v;
		}

		internal static void HelpMarker(string v)
		{
			ImGui.SameLine();
			ImGui.TextDisabled("(?)");
			if (ImGui.BeginItemTooltip())
			{
				ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
				ImGui.TextUnformatted(Translate(v));
				ImGui.PopTextWrapPos();
				ImGui.EndTooltip();
			}
		}
		internal static void ChangeSelectedScene(SlideShow slideshow)
		{
			if (slideshow == null || SceneExplorer.loadedScenes == null || SceneExplorer.loadedScenes.Count < SceneExplorer.selectedMenuScene) { return; }

			slideshow.current = SceneExplorer.selectedMenuScene;
			if (slideshow.current >= 0 && slideshow.scene != null)
			{
				slideshow.inSceneTime = 0f;
				slideshow.crossfadeInd = 0;

				slideshow.scene.RemoveSprites();
				slideshow.pages[0].subObjects.Remove(slideshow.scene);
				slideshow.preloadedScenes[slideshow.current].Hide();

				if (slideshow.current < slideshow.playList.Count)
				{
					slideshow.scene = slideshow.preloadedScenes[slideshow.current];
					slideshow.pages[0].subObjects.Add(slideshow.scene);
					slideshow.preloadedScenes[slideshow.current].Show();
					slideshow.time = slideshow.playList[slideshow.current].startAt;
					SceneExplorer.SetMusicTime(slideshow.manager.musicPlayer?.song?.subTracks?[0]?.source, slideshow.time);
					
				}
			}
		}

		internal static void ChangeSelectedScene(SlugcatSelectMenu select)
		{
			if (select == null) { return; }
			int index = 0;
			for (int i = 0; i < select.slugcatPages.Count; i++)
			{
				if (select.slugcatPages[i] != null && CheckMenuSceneAgainstLoaded(select.slugcatPages[i].slugcatImage))
				{
					index = i;
					break;
				}
			}

		int[] searches = [
				index - select.slugcatPageIndex,
				select.slugcatPages.Count + (index - select.slugcatPageIndex),
				(select.slugcatPages.Count + (select.slugcatPageIndex - index)) * -1
				];
			int lowestSearch = select.slugcatPages.Count;
			foreach (int i in searches)
			{
				if (Math.Abs(i) <= Math.Abs(lowestSearch))
					lowestSearch = i;
			}

			select.quedSideInput = lowestSearch;
		}

		internal static bool CheckMenuSceneAgainstLoaded(MenuScene image)
		{
			return SceneExplorer.loadedScenes != null && SceneExplorer.loadedScenes.Count > SceneExplorer.selectedMenuScene && SceneExplorer.loadedScenes[SceneExplorer.selectedMenuScene] == image;
		}

		public static bool SliderFloat(string label, ref float v, float v_min, float v_max) => AddSound(ImGui.SliderFloat(label, ref v, v_min, v_max), false, SoundFlag.Slider);
		public static bool DragFloatRange2(string label, ref float v_min, ref float v_max, float speed, float b_min, float b_max, string minFormat, string maxFormat) => AddSound(ImGui.DragFloatRange2(label, ref v_min, ref v_max, speed, b_min, b_max, minFormat, maxFormat), false, SoundFlag.Slider);

		public static bool Button(string label) => AddSound(ImGui.Button(label), false, SoundFlag.Confirm);

		public static bool CollapsingHeader(string label) => AddSound(ImGui.CollapsingHeader(label), true, SoundFlag.Tick);
		public static bool CollapsingHeader(string label, ImGuiTreeNodeFlags flags) => AddSound(ImGui.CollapsingHeader(label, flags), true, SoundFlag.Tick);

		public unsafe static bool BeginTable(string label, int columns, System.Numerics.Vector2 size)
		{
			return ImGui.BeginTable(label, columns, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit, size);
		}
		public unsafe static bool BeginTable(string label, int columns, int defaultCount, float? width, int? maxItems = 4)
		{
			if (width == null) { width = ImGui.GetContentRegionAvail().X; }
			if (maxItems != null) { defaultCount = Math.Min(defaultCount + 1, (int)maxItems); }
			return ImGui.BeginTable(label, columns, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit, new((float)width, maxItems == null ? -1f : ImGui.GetFrameHeightWithSpacing() * defaultCount));
		}

		public static bool Selectable(string label, bool v) => AddSound(ImGui.Selectable(label, v), false, SoundFlag.Select);
		public static bool Selectable(string label, ref bool v) => AddSound(ImGui.Selectable(label, ref v), false, SoundFlag.Select);
		public static bool Selectable(string label, bool v, ImGuiSelectableFlags flags) => AddSound(ImGui.Selectable(label, v, flags), false, SoundFlag.Select);
		public static bool Selectable(string label, ref bool v, ImGuiSelectableFlags flags) => AddSound(ImGui.Selectable(label, ref v, flags), false, SoundFlag.Select);

		public static bool TreeNodeEx(string label, ImGuiTreeNodeFlags flags) => AddSound(ImGui.TreeNodeEx(label, flags), true, SoundFlag.Select);

		public static bool MenuItem(string label) => AddSound(ImGui.MenuItem(label), false, SoundFlag.Select);
		public static bool BeginMenu(string label) => AddSound(ImGui.BeginMenu(label), true, SoundFlag.Select);

		public static bool BeginPopupModal(string id, bool fullSize)
		{
			ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new(0.5f, 0.5f));
			if (fullSize)
			{
				ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize * 0.5f);
			}
			return ImGui.BeginPopupModal(Translate(id), fullSize ? ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove : ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize);
		}

		public static void Text(string s) => ImGui.Text(Translate(s));
		public static void TextDisabled(string s) => ImGui.TextDisabled(Translate(s));
		public static void SeparatorText(string s) => ImGui.SeparatorText(Translate(s));
		public static string Translate(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				Assets.AddPossibleStringToTranslation(s);
			}
			return SceneExplorer.lastMenu?.Translate(s);
		}

		public static bool DragPoint(int id, ref double x, ref double y, System.Numerics.Vector4 col) => AddSound(ImPlot.DragPoint(id, ref x, ref y, col), true, SoundFlag.Confirm);
		public static bool DragLineX(int id, ref double x, System.Numerics.Vector4 col) => AddSound(ImPlot.DragLineX(id, ref x, col), true, SoundFlag.Tick);

		public static bool TimePoint(int id, ref double x, ref double y) => ImPlot.DragPoint(id, ref x, ref y, new(0, 1, 0, 1), ImPlot.GetStyle().MarkerSize, ImPlotDragToolFlags.NoCursors);
	}

	internal class Modals
	{
		public static readonly string slideShowPreview = "Quick Slideshow Selector";
		internal static readonly string deleteLayerString = "Confirm Deletion";
		internal static string sceneFolderChange = "Confirm Scene Folder Change";

		public static int? selectedSlideshow = null;

		internal unsafe static void RenderSlideshowViewer()
		{
			if (SceneImgui.BeginPopupModal(slideShowPreview, true))
			{
				float bottomPadding = ImGui.GetContentRegionAvail().Y - (ImGui.GetTextLineHeightWithSpacing() * 1.5f);
				ImGui.BeginGroup();
				if (ImGui.BeginChild("SlideshowPreview", new(ImGui.GetContentRegionAvail().X / 2.5f, bottomPadding), ImGuiChildFlags.Borders))
				{
					// TODO: Add images and shit here IG
					ImGui.EndChild();
				}

				ImGui.SameLine();

				if (SceneImgui.BeginTable("Avaliable Slideshows", 1, new(-1f, bottomPadding)))
				{
					ImGui.TableSetupColumn(SceneImgui.Translate("Slideshow ID"));
					ImGui.TableSetupScrollFreeze(0, 1);
					ImGui.TableHeadersRow();

					for (int i = 0; i < Assets.avaliableSlideShows.Count; i++)
					{
						ImGui.TableNextRow();
						ImGui.TableSetColumnIndex(0);

						if (SceneImgui.Selectable(Assets.avaliableSlideShows[i].value, selectedSlideshow != null && selectedSlideshow == i)) { selectedSlideshow = i; }
					}

					ImGui.EndTable();
				}
				ImGui.EndGroup();

				if (selectedSlideshow == null) { ImGui.BeginDisabled(); }
				if (SceneImgui.Button(SceneImgui.Translate("Play Selected Scene"))) { SceneExplorer.lastMenu.manager.nextSlideshow = Assets.avaliableSlideShows[(int)selectedSlideshow]; SceneExplorer.lastMenu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow); ImGui.CloseCurrentPopup(); }
				if (selectedSlideshow == null) { ImGui.EndDisabled(); }
				ImGui.EndPopup();
			}
		}

		internal unsafe static void LayerDeletionConfirmation()
		{
			if (SceneImgui.BeginPopupModal(deleteLayerString, false))
			{
				ImGui.TextColored(*ImGui.GetStyleColorVec4(ImGuiCol.PlotHistogram), SceneImgui.Translate("WARNING"));
				ImGui.TextWrapped(SceneImgui.Translate("layer_deletion_warning"));

				if (SceneImgui.Button(SceneImgui.Translate("Yes, delete layers")))
				{
					LayerViewer.confirmedDeletion = true;
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				if (SceneImgui.Button(SceneImgui.Translate("No, take me back")))
				{
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				ImGui.Checkbox(SceneImgui.Translate("Always Ask"), ref LayerViewer.askBeforeDeletion);
				ImGui.EndPopup();
			}
		}

		internal unsafe static void ChangeSceneFolder()
		{
			if (SceneImgui.BeginPopupModal(sceneFolderChange, false))
			{
				ImGui.TextColored(*ImGui.GetStyleColorVec4(ImGuiCol.PlotHistogram), SceneImgui.Translate("WARNING"));
				ImGui.TextWrapped(SceneImgui.Translate("scene_folder_warning"));

				if (SceneImgui.Button(SceneImgui.Translate("Yes, change folder")))
				{
					SceneExplorer.shouldChangeFolder = true;
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				if (SceneImgui.Button(SceneImgui.Translate("No, take me back")))
				{
					SceneExplorer.queuedFolderChange = "";
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
		}
	}

	internal class SceneExplorer
	{

		internal static int selectedMenuScene;
		internal static List<MenuScene> loadedScenes = [];
		internal static Menu.Menu lastMenu;
		internal static bool gatheredSlideshows;

		internal static void Draw()
		{
			if (loadedScenes != null)
			{
				if (ImGui.Begin("Scenes", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoTitleBar))
				{
					bool slideShowModal = false;
					if (ImGui.BeginMenuBar())
					{
						if (SceneImgui.BeginMenu(SceneImgui.Translate("Quick Menu")))
						{
							if (SceneImgui.MenuItem(SceneImgui.Translate("Load Slideshow"))) { slideShowModal = true; }

							ImGui.EndMenu();
						}

						ImGui.EndMenuBar();
					}

					if (slideShowModal && !ImGui.IsPopupOpen(Modals.slideShowPreview))
					{
						ImGui.OpenPopup(Modals.slideShowPreview);
					}
					Modals.RenderSlideshowViewer();

					SlideShowEditor();

					SceneSelector();

					SceneEditor();

					LayerViewer.LayerEditor();

					ImGui.End();
				}
			}
		}

		public struct ScrollingAudioData
		{
			public int MaxSize;
			public int Offset;
			public SimpleNativeArray<Vector2> Data;
			public ScrollingAudioData(int maxSize = 2000)
			{
				MaxSize = maxSize;
				Offset = 0;
				Data = new SimpleNativeArray<Vector2>(maxSize);
			}
			public ScrollingAudioData()
			{
				MaxSize = 2000;
				Offset = 0;
				Data = new SimpleNativeArray<Vector2>(2000);
			}

			public void AddPoint(float x, float y)
			{
				if (Data.Length < MaxSize)
				{
					Data.Add(new Vector2(x, y));
					return;
				}
				Data._items[Offset] = new Vector2(x, y);
				Offset = (Offset + 1) % MaxSize;
			}

			public void Erase()
			{
				if (Data.Length > 0)
				{
					Data.Clear();
					Offset = 0;
				}
			}

		}

		public struct SimpleNativeArray<T> where T : unmanaged
		{
			public T[] _items;
			public int _size;
			public int _capacity;
			private static readonly T[] _emptyArray = new T[0];

			public SimpleNativeArray(int capacity)
			{
				_capacity = capacity;
				_items = new T[_capacity];
				_size = 0;
				// Globals.mls.LogInfo($"SimpleNativeArray.ctor. Capacity={_capacity}, _items.Length={_items.Length}");
			}

			public void Add(T item)
			{
				if (_size == _items.Length)
				{
					EnsureCapacity(_size + 1);
				}

				_items[_size++] = item;
			}

			private void EnsureCapacity(int min)
			{
				if (_items.Length < min)
				{
					int num = _items.Length == 0 ? 4 : _items.Length * 2;
					if ((uint)num > 2146435071u)
					{
						num = 2146435071;
					}

					if (num < min)
					{
						num = min;
					}

					Capacity = num;
				}
			}

			public void Clear()
			{
				if (_size > 0)
				{
					Array.Clear(_items, 0, _size);
					_size = 0;
				}
			}

			public int Capacity
			{
				get
				{
					return _items.Length;
				}
				set
				{
					if (value < _size)
					{
						throw new ArgumentOutOfRangeException($"Too small capacity: {value}");
					}

					if (value == _items.Length)
					{
						return;
					}

					if (value > 0)
					{
						T[] array = new T[value];
						if (_size > 0)
						{
							Array.Copy(_items, 0, array, 0, _size);
						}

						_items = array;
					}
					else
					{
						_items = _emptyArray;
					}
				}
			}

			public int Length
			{
				get
				{
					if (_items == null)
						return 0;
					return _items.Length;
				}
			}
		}

		internal unsafe static void SlideShowEditor()
		{
			if (lastMenu != null && lastMenu is SlideShow slideShow)
			{
				if (SceneImgui.CollapsingHeader(SceneImgui.Translate("Slideshow Properties"), ImGuiTreeNodeFlags.DefaultOpen))
				{
					if (ImGui.BeginCombo(SceneImgui.Translate("Song"), slideShow.waitForMusic))
					{
						List<string> songs = Assets.loadedSongNames;
						songs.Sort();
						for (int i = 0; i < songs.Count; i++)
						{
							if ((SceneImgui.Selectable(songs[i], songs[i].ToLowerInvariant() == slideShow.waitForMusic.ToLowerInvariant()) || ImGui.IsItemHovered()) && slideShow.waitForMusic != songs[i])
							{
								UnityEngine.Debug.Log($"Changed music to: {songs[i]}");
								//musicData.Erase();
								slideShow.waitForMusic = songs[i];
								slideShow.manager.musicPlayer.FadeOutAllSongs(0f);
								slideShow.manager.musicPlayer.MenuRequestsSong(slideShow.waitForMusic, 1f, 0f);
							}
						}

						ImGui.EndCombo();
					}

					if (ImPlot.BeginPlot(SceneImgui.Translate("Timeline"), new(-1, 140f)))
					{
						double time = slideShow.time;
						ImPlot.SetupAxes("##Play Time", "##Evil Axis", ImPlotAxisFlags.None, ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoGridLines);
						ImPlot.SetupAxisLimits(ImAxis.X1, time - 5, time + 5, musicPaused ? ImPlotCond.Once : ImPlotCond.Always);
						ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 1, ImPlotCond.Always);

						System.Numerics.Vector4 prevPrevCol = new(0.3f, 0.3f, 0.3f, 1f);
						System.Numerics.Vector4 prevCol = new(1f, 0f, 0f, 1f);
						System.Numerics.Vector4 currentCol = new(1f, 1f, 0f, 1f);
						System.Numerics.Vector4 nextCol = new(0f, 1f, 1f, 1f);

						int index = 0;
						for (int i = 0; i < slideShow.playList.Count; i++)
						{
							SlideShow.Scene scene = slideShow.playList[i];
							double startTime = scene.startAt;
							double endTime = i == slideShow.playList.Count - 1 ? scene.fadeOutStartAt + 1f : slideShow.playList[i + 1].startAt;
							double fadeOutStartAt = scene.fadeOutStartAt;

							System.Numerics.Vector4 thisCol = i < slideShow.current - 1 ? prevPrevCol : i < slideShow.current ? prevCol : i > slideShow.current ? nextCol : currentCol;
							SceneImgui.DragLineX(index, ref startTime, thisCol);
							index++;
							SceneImgui.DragLineX(index, ref endTime, thisCol);
							index++;
							if (scene.sceneID != MenuScene.SceneID.Empty)
							{
								SceneImgui.DragLineX(index, ref fadeOutStartAt, thisCol);
								index++;
							}

							double yMin = 0;
							double yMax = 1;
							ImPlot.DragRect(i, ref startTime, ref yMin, ref endTime, ref yMax, thisCol, ImPlotDragToolFlags.NoInputs);

							ImPlot.TagX(Mathf.Lerp(scene.startAt, (float)endTime, 0.5f), thisCol, scene.sceneID.value);

							SceneImgui.DragLineX(index, ref time, new(1, 1, 1, 1));
							index++;
						}

						ImPlot.EndPlot();
					}

					if (slideShow.current > 0)
					{
						SlideShow.Scene playScene = slideShow.playList?[slideShow.current];
						SlideShow.Scene nextScene = null;
						if (slideShow.current < slideShow.playList.Count - 1)
						{
							nextScene = slideShow.playList[slideShow.current + 1];
						}

						if (playScene != null)
						{
							AudioSource source = slideShow.manager.musicPlayer?.song?.subTracks?[0]?.source;

							bool buttonPressed = false;
							ImGui.BeginGroup();
							if (SceneImgui.Button(SceneImgui.Translate("Rewind"))) { slideShow.Rewind(); buttonPressed = true; }
							ImGui.SameLine();
							if (SceneImgui.Button("<<")) { slideShow.time = playScene.startAt; slideShow.inSceneTime = 0; buttonPressed = true; }
							ImGui.SameLine();
							if (SceneImgui.Button("<")) { slideShow.time = Mathf.Max(slideShow.time - 0.1f, playScene.startAt); slideShow.inSceneTime = Mathf.Max(slideShow.inSceneTime - 0.1f, 0f); buttonPressed = true; }
							ImGui.SameLine();
							if (SceneImgui.Button(SceneImgui.Translate(shouldStall ? "Play" : "Pause"))) { shouldStall = !shouldStall; StopStartMusic(source); }
							ImGui.SameLine();
							if (SceneImgui.Button(">")) { slideShow.time = Mathf.Min(slideShow.time + 0.1f, nextScene == null ? playScene.fadeOutStartAt + 1f : nextScene.startAt); slideShow.inSceneTime = Mathf.Min(slideShow.inSceneTime + 0.1f, 1f); buttonPressed = true; }
							ImGui.SameLine();
							if (SceneImgui.Button(">>")) { slideShow.time = playScene.fadeOutStartAt; slideShow.inSceneTime = playScene.startAt / playScene.fadeOutStartAt; buttonPressed = true; }
							ImGui.SameLine();
							if (SceneImgui.Button(SceneImgui.Translate("Skip"))) { slideShow.time = nextScene == null ? playScene.fadeOutStartAt + 1f : nextScene.startAt; buttonPressed = true; }
							ImGui.EndGroup();

							if (buttonPressed) { SetMusicTime(source, slideShow.time); if (musicPaused) { pausedAt = slideShow.time; } }

							ImGui.ProgressBar((slideShow.time - playScene.startAt) / ((nextScene == null ? playScene.fadeOutStartAt + 1f : nextScene.startAt) - playScene.startAt), new(), "");
						}
					}
				}
			}
		}

		private static void StopStartMusic(AudioSource source)
		{
			if (source != null)
			{
				if (musicPaused)
				{
					musicPaused = false;
					source.mute = false;
					SetMusicTime(source, pausedAt);
				}
				else
				{
					musicPaused = true;
					source.mute = true;
					pausedAt = source.time;
				}
			}
		}

		internal static void SetMusicTime(AudioSource source, float time)
		{
			if (source != null)
			{
				source.time = time;
			}
		}

		internal unsafe static void SceneSelector()
		{
			if (loadedScenes == null) return;

			if (SceneImgui.CollapsingHeader(lastMenu is SlideShow ? "SlideShowMenuScenes" : "InteractiveMenuScenes", ImGuiTreeNodeFlags.DefaultOpen))
			{
				if (loadedScenes != null && loadedScenes.Count > selectedMenuScene && SceneImgui.BeginTable("##MenuScenesList", 2, loadedScenes.Count, null))
				{
					ImGui.TableSetupColumn("MenuScene");
					ImGui.TableSetupColumn(SceneImgui.Translate("Images"));
					ImGui.TableSetupScrollFreeze(0, 1);
					ImGui.TableHeadersRow(); 
					
					selectedMenuScene = lastMenu is SlideShow show ? show.current : lastMenu is SlugcatSelectMenu select ? select.slugcatPageIndex : 0;

					int lastCount = loadedScenes.Count;
					for (int i = 0; i < loadedScenes.Count; i++)
					{
						MenuScene scene = loadedScenes[i];
						if (scene != null && lastCount == loadedScenes.Count && scene.depthIllustrations != null && scene.flatIllustrations != null)
						{
							ImGui.TableNextRow();
							ImGui.TableSetColumnIndex(0);
							bool isSelected = loadedScenes != null && loadedScenes.Count > selectedMenuScene && loadedScenes[selectedMenuScene] != null && loadedScenes[selectedMenuScene] == scene;
							if (SceneImgui.Selectable($"{scene.sceneID.value}##{i}", isSelected))
							{
								if (loadedScenes.Count > i)
								{
									selectedMenuScene = i;
									
									if (lastMenu is SlideShow show2)
									{
										SceneImgui.ChangeSelectedScene(show2);
									}
									else if (lastMenu is SlugcatSelectMenu select2)
									{
										SceneImgui.ChangeSelectedScene(select2);
									}
								}
							}
							ImGui.TableSetColumnIndex(1);
							ImGui.Text($"{scene.depthIllustrations.Count + scene.flatIllustrations.Count}");
						}
					}

					ImGui.EndTable();
				}

				// TODO: add ability to add/remove scenes (for slideshows) here
			}
		}

		private static void ResetSceneVariables()
		{
			selectedDepths = null;
			wantToAddDepth = false;
			newDepth = 0f;
			selectedCrossGroup = null;
			selectedCrossFade = null;
		}

		public static bool[] selectedDepths;
		public static bool wantToAddDepth = false;
		public static float newDepth = 0f;
		public static int? selectedCrossGroup;
		public static MenuDepthIllustration selectedCrossFade;
		private static MenuScene lastScene;
		private static Vector2 cameraOffset;
		private static bool setOffsetPos;
		private static Vector2 oldCameraOffset;
		private static Vector2 defaultOffset;
		public static SimpleNativeArray<Vector2> depthPointers = new(50);
		public static SimpleNativeArray<Vector2> cameraPositionPointers = new(50);
		private static bool[] selectedDot;
		private static bool[] clickedDots;
		public static bool shouldStall;
		private static Vector3 midPointPos;
		internal static string queuedFolderChange;
		internal static bool shouldChangeFolder;
		private static bool musicPaused;
		private static float pausedAt;

		internal unsafe static void SceneEditor()
		{
			if (loadedScenes == null) return;

			if (loadedScenes != null && loadedScenes.Count > selectedMenuScene && loadedScenes[selectedMenuScene] != null)
			{
				MenuScene scene = loadedScenes[selectedMenuScene];

				bool openSceneChangeModal = false;
				if (SceneImgui.CollapsingHeader(SceneImgui.Translate("MenuScene Properties"), ImGuiTreeNodeFlags.DefaultOpen))
				{
					if (ImGui.BeginCombo(SceneImgui.Translate("Scene Folder"), string.IsNullOrEmpty(scene.sceneFolder) ? "NONE" : scene.sceneFolder))
					{
						string[] folders = Assets.storedSceneFoldersAndFiles.Keys.ToArray();

						for (int i = 0; i < folders.Length; i++)
						{
							if (SceneImgui.Selectable(folders[i], scene.sceneFolder == folders[i])) { openSceneChangeModal = true; queuedFolderChange = folders[i]; }
						}

						ImGui.EndCombo();
					}
					SceneImgui.DragFloatRange2(SceneImgui.Translate("Scene Blur"), ref scene.blurMin, ref scene.blurMax, 0.05f, 0f, 1f, "Min %.1f", "Max %.1f");

					if (scene is InteractiveMenuScene interactive && interactive.depthIllustrations?.Count > 0)
					{
						InteractiveElements(interactive);
					}
					else if (scene is SlideShowMenuScene slideScene && lastMenu is SlideShow show)
					{
						SlideShowElements(slideScene, show);
					}
				}

				if (openSceneChangeModal)
				{
					ImGui.OpenPopup(Modals.sceneFolderChange);
				}

				Modals.ChangeSceneFolder();

				if (shouldChangeFolder && !string.IsNullOrEmpty(queuedFolderChange))
				{
					shouldChangeFolder = false;
					scene.sceneFolder = queuedFolderChange;
					queuedFolderChange = "";

					foreach (var image in scene.depthIllustrations)
					{
						image.RemoveSprites();
					}
					scene.depthIllustrations.Clear();
					foreach (var image in scene.flatIllustrations)
					{
						image.RemoveSprites();
					}
					scene.flatIllustrations.Clear();

					// Load new images here
					Assets.ForceLoadImagesForCache(scene);
				}
			}
		}

		private unsafe static void InteractiveElements(InteractiveMenuScene interactive)
		{
			SceneImgui.SeparatorText("Interactive Elements");
			if (selectedDepths == null || (interactive.idleDepths.Count > 0 && selectedDepths.Length != interactive.idleDepths.Count) || (interactive.idleDepths.Count == 0 && selectedDepths.Length != interactive.depthIllustrations.Count))
			{
				selectedDepths = new bool[interactive.idleDepths.Count > 0 ? interactive.idleDepths.Count : interactive.depthIllustrations.Count];
			}

			bool hasIdleDepths = interactive.idleDepths.Count > 0;
			if (ImGui.BeginListBox("##IdleDepths", new(ImGui.GetItemRectSize().X / 2f, 100)))
			{
				List<float> depths = interactive.idleDepths.Count > 0 ? interactive.idleDepths : interactive.depthIllustrations.Select(x => x.depth).ToList();

				for (int dp = 0; dp < depths.Count; dp++)
				{
					if (depths[dp] == Hooks.activeIdleDepth)
					{
						if (SceneImgui.Selectable($"{depths[dp]}<##{dp}", selectedDepths[dp], ImGuiSelectableFlags.Highlight)) { selectedDepths[dp] = !selectedDepths[dp]; }
					}
					else
					{
						if (SceneImgui.Selectable($"{depths[dp]}##{dp}", selectedDepths[dp], ImGuiSelectableFlags.None)) { selectedDepths[dp] = !selectedDepths[dp]; }
					}
				}

				ImGui.EndListBox();
			}
			ImGui.SameLine();
			ImGui.BeginGroup();
			SceneImgui.Text("Idle Depths");
			ImGui.SameLine();
			SceneImgui.HelpMarker("Idle_depth_explanation");

			bool oneSelected = selectedDepths.Any(x => x);
			if (!wantToAddDepth && SceneImgui.Button("+"))
			{
				wantToAddDepth = true;
			}
			if (hasIdleDepths)
			{
				ImGui.SameLine();
				if (oneSelected && SceneImgui.Button("-"))
				{
					int[] deleteDepths = interactive.idleDepths.Where(x => selectedDepths[interactive.idleDepths.IndexOf(x)]).Select(x => interactive.idleDepths.IndexOf(x)).ToArray();
					for (int e = 0; e < deleteDepths.Length; e++)
					{
						interactive.idleDepths.RemoveAt(e);
					}
				}
			}

			if (oneSelected && SceneImgui.Button(SceneImgui.Translate("Duplicate")))
			{
				float[] toDup = hasIdleDepths ? interactive.idleDepths.ToArray() : interactive.depthIllustrations.Select(x => x.depth).ToArray();
				int[] duplicateDepths = toDup.Where(x => selectedDepths[Array.IndexOf(toDup, x)]).Select(x => Array.IndexOf(toDup, x)).ToArray();
				for (int d = 0; d < duplicateDepths.Length; d++)
				{
					interactive.idleDepths.Add(toDup[duplicateDepths[d]]);
				}
			}

			ImGui.EndGroup();

			if (wantToAddDepth)
			{
				ImGui.BeginGroup();
				SceneImgui.SliderFloat(SceneImgui.Translate("Depth"), ref newDepth, 0f, 15f);

				ImGui.SameLine();
				if (SceneImgui.Button("+")) { wantToAddDepth = false; interactive.idleDepths.Add(newDepth); }
				ImGui.SameLine();
				if (SceneImgui.Button("-")) { wantToAddDepth = false; }
				ImGui.EndGroup();
			}
		}

		private unsafe static void SlideShowElements(SlideShowMenuScene slideScene, SlideShow show)
		{
			SceneImgui.SeparatorText("Slideshow Elements");

			if (slideScene != lastScene)
			{
				lastScene = slideScene;
				cameraOffset = new(-(show.manager.rainWorld.screenSize.x * 0.5f), -(show.manager.rainWorld.screenSize.y * 0.5f));
				defaultOffset = cameraOffset;
				clickedDots = new bool[slideScene.cameraMovementPoints.Count];
			}

			if (ImPlot.BeginSubplots(SceneImgui.Translate("Camera Keyframes"), 1, 2, new(ImGui.GetContentRegionAvail().X, 300f)))
			{
				if (selectedDot != null && selectedDot.Any(x => x))
				{
					int index = Array.IndexOf(selectedDot, selectedDot.Where(x => x).First());
					float time = (float)(index * 1) / (float)(slideScene.cameraMovementPoints.Count - 1);
					show.inSceneTime = time;
					slideScene.camPos = slideScene.cameraMovementPoints[index] / 300f;
					slideScene.lastCamPos = slideScene.camPos;
				}
				else if (shouldStall)
				{
					show.inSceneTime = Mathf.Lerp(0f, 1f, Futile.mousePosition.x / show.manager.rainWorld.screenSize.x);
				}

				if (ImPlot.BeginPlot("##SlideshowCameraDepth"))
				{
					ImPlot.SetupAxes(SceneImgui.Translate("Display Time"), SceneImgui.Translate("Camera Depth"), ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.AutoFit);
					ImPlot.SetupAxesLimits(0, 1, 0, 2, ImPlotCond.Always);

					depthPointers.Clear();
					double[] timePoint = new double[slideScene.cameraMovementPoints.Count];
					for (int i = 0; i < slideScene.cameraMovementPoints.Count; i++)
					{
						timePoint[i] = (double)((float)(i * 1) / (float)(slideScene.cameraMovementPoints.Count - 1));
						double depthPoint = Math.Max(slideScene.cameraMovementPoints[i].z, 0f);
						depthPointers.Add(new((float)timePoint[i], (float)depthPoint));
						if (SceneImgui.DragPoint(i, ref timePoint[i], ref depthPoint, clickedDots[i] ? new(1, 0, 0, 1) : *ImGui.GetStyleColorVec4(ImGuiCol.PlotHistogram))) { if (ImGui.IsItemFocused()) { clickedDots[i] = !clickedDots[i]; } shouldStall = true; slideScene.focus = (float)depthPoint; slideScene.cameraMovementPoints[i] = new(slideScene.cameraMovementPoints[i].x, slideScene.cameraMovementPoints[i].y, Mathf.Min((float)depthPoint, 2f)); }
					}

					for (int i = 0; i < timePoint.Length - 1; i++)
					{
						if (show.inSceneTime < timePoint[i + 1] && show.inSceneTime > timePoint[i])
						{
							float lerp = (show.inSceneTime - (float)timePoint[i]) / ((float)timePoint[i + 1] - (float)timePoint[i]);
							midPointPos = Vector3.Lerp(slideScene.cameraMovementPoints[i], slideScene.cameraMovementPoints[i + 1], lerp);
							if (slideScene.cameraMovementPoints[i].z < 0f || slideScene.cameraMovementPoints[i + 1].z < 0f)
							{
								midPointPos.z = Mathf.Lerp(Mathf.Max(0f, slideScene.cameraMovementPoints[i].z), Mathf.Max(0f, slideScene.cameraMovementPoints[i + 1].z), lerp);
							}
							break;
						}
					}


					fixed (float* xPtr = &depthPointers._items[0].x)
					fixed (float* yPtr = &depthPointers._items[0].y)
					{
						ref float xv = ref *xPtr;
						ref float yv = ref *yPtr;
						ImPlot.SetNextLineStyle(*ImGui.GetStyleColorVec4(ImGuiCol.PlotHistogram));
						ImPlot.PlotLine("##DepthLines", ref xv, ref yv, depthPointers._size, 0, 0, 2 * 4);
					}

					double midX = show.inSceneTime;
					double midY = Mathf.Max(midPointPos.z, 0f);
					SceneImgui.TimePoint(slideScene.cameraMovementPoints.Count + 1, ref midX, ref midY);

					ImPlot.EndPlot();
				}

				if (ImPlot.BeginPlot("##SlideshowCameraPosition"))
				{
					double rectX1 = cameraOffset.x;
					double rectY1 = cameraOffset.y;
					double rectX2 = show.manager.rainWorld.screenSize.x + cameraOffset.x;
					double rectY2 = show.manager.rainWorld.screenSize.y + cameraOffset.y;

					ImPlot.SetupAxes(SceneImgui.Translate("Keyframe Positions"), "##Y");
					ImPlot.SetupAxesLimits(rectX1, rectX2, rectY1, rectY2, ImPlotCond.Once);

					if (ImPlot.DragRect(0, ref rectX1, ref rectY1, ref rectX2, ref rectY2, new(1, 0, 0, 1)))
					{
						if (!setOffsetPos)
						{
							setOffsetPos = true;
							oldCameraOffset = cameraOffset;
						}
						cameraOffset = new((float)rectX1, (float)rectY1);

						Vector2 offset = (oldCameraOffset - cameraOffset) * 0.2f;
						for (int i = 0; i < slideScene.cameraMovementPoints.Count; i++)
						{
							slideScene.cameraMovementPoints[i] = new(slideScene.cameraMovementPoints[i].x + offset.x, slideScene.cameraMovementPoints[i].y + offset.y, slideScene.cameraMovementPoints[i].z);
						}
					}
					else
					{
						setOffsetPos = false;
					}

					cameraPositionPointers.Clear();
					selectedDot = new bool[slideScene.cameraMovementPoints.Count];
					for (int i = 0; i < slideScene.cameraMovementPoints.Count; i++)
					{
						cameraPositionPointers.Add(slideScene.cameraMovementPoints[i]);
						double x = slideScene.cameraMovementPoints[i].x;
						double y = slideScene.cameraMovementPoints[i].y;
						if (SceneImgui.DragPoint(i, ref x, ref y, clickedDots[i] ? new(1, 0, 0, 1) : *ImGui.GetStyleColorVec4(ImGuiCol.PlotHistogram))) { if (ImGui.IsItemClicked()) { clickedDots[i] = !clickedDots[i]; } selectedDot[i] = true; shouldStall = true; slideScene.cameraMovementPoints[i] = new((float)x, (float)y, slideScene.cameraMovementPoints[i].z); }

					}

					fixed (float* xPtr = &cameraPositionPointers._items[0].x)
					fixed (float* yPtr = &cameraPositionPointers._items[0].y)
					{
						ref float xv = ref *xPtr;
						ref float yv = ref *yPtr;
						ImPlot.SetNextLineStyle(*ImGui.GetStyleColorVec4(ImGuiCol.PlotHistogram));
						ImPlot.PlotLine("##PositionLines", ref xv, ref yv, cameraPositionPointers._size, 0, 0, 2 * 4);
					}

					double midX = midPointPos.x;
					double midY = midPointPos.y;
					SceneImgui.TimePoint(slideScene.cameraMovementPoints.Count + 1, ref midX, ref midY);

					ImPlot.EndPlot();
				}
				ImPlot.EndSubplots();
			}

			if (SceneImgui.Button(SceneImgui.Translate("Add Keyframe")))
			{
				AddPoint(slideScene);
			}
			if (clickedDots.Any(x => x))
			{
				ImGui.SameLine();
				if (SceneImgui.Button(SceneImgui.Translate("Remove Keyframe(s)")))
				{
					for (int i = slideScene.cameraMovementPoints.Count - 1; i >= 0; i--)
					{
						if (clickedDots[i])
						{
							slideScene.cameraMovementPoints.RemoveAt(i);
						}
					}
				}
			}

			// TODO slideshow duration (may just handle this in slideshow properties?)

			/* Wes gonnta figure this out later: (observations)
			 * there are two main types of crossfades, a dict which holds the index of the image that should fade, and a seperate list within a scene which controls when and how many cross fades are in the scene
			 * so two main things are needed: a number of images proportional to the number of crossfades per group
			 * to make a group you would need enough images to cover the crossfade, but this might be able to be cheesed by inputting the same image multiple times?
			 * needs more testing :/
			 */
			//if (SceneImgui.BeginTable("CrossfadeList", 2, scene.crossFades.Count, null, maxItems: 6))
			//{
			//	ImGui.TableSetupColumn(SceneImgui.Translate("Crossfades"));
			//	ImGui.TableSetupColumn(SceneImgui.Translate("Frames"));
			//	ImGui.TableSetupScrollFreeze(0, 1);
			//	ImGui.TableHeadersRow();

			//	List<int> crossfadeIndexes = scene.crossFades.Keys.ToList();
			//	for (int i = 0; i < crossfadeIndexes.Count; i++)
			//	{
			//		ImGui.TableNextRow();
			//		ImGui.TableSetColumnIndex(0);

			//		if (SceneImgui.TreeNodeEx($"{crossfadeIndexes[i]}", selectedCrossGroup != null && selectedCrossGroup == i ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None))
			//		{
			//			selectedCrossGroup = i;

			//			List<MenuDepthIllustration> crossImages = scene.crossFades[crossfadeIndexes[i]];
			//			for (int j = 0; j < crossImages.Count; j++)
			//			{
			//				if (SceneImgui.Selectable($"{crossImages[j].fileName}##{i}{j}", selectedCrossFade == crossImages[j])) { selectedCrossFade = crossImages[j]; }
			//			}

			//			ImGui.TreePop();
			//		}

			//		ImGui.TableSetColumnIndex(1);
			//		ImGui.Text($"{scene.crossFades[crossfadeIndexes[i]].Count}");
			//	}

			//	ImGui.EndTable();

			//	ImGui.BeginGroup();
			//	if (LayerViewer.selectedLayers != null && LayerViewer.selectedLayers.Any(x => x) && SceneImgui.Button(SceneImgui.Translate("Add Crossfade"))) { /*TODO: ADD SUPPORT FOR SELECTED LAYER, USE CUSTOM ADDITION BECAUSE ADDCROSSFADE ONLY ADDS INT FOR THE LAST ADDED SCENE*/ }
			//	else { SceneImgui.TextDisabled("Choose a layer or existing crossfade"); SceneImgui.HelpMarker("slideshow_crossfade_explanation"); }
			//	if (selectedCrossGroup != null)
			//	{
			//		if (SceneImgui.Button(SceneImgui.Translate("Remove Crossfade"))) { scene.crossFades.Remove(crossfadeIndexes[(int)selectedCrossGroup]); }
			//	}
			//	ImGui.EndGroup();

			//	ImGui.BeginGroup();
			//	if (selectedCrossGroup != null)
			//	{
			//		if (LayerViewer.selectedLayers.Any(x => x) && SceneImgui.Button(SceneImgui.Translate("Add Image To Group")) && scene.crossFades.ContainsKey((int)selectedCrossGroup))
			//		{
			//			MenuIllustration 
			//		}

			//		if (selectedCrossFade != null && SceneImgui.Button(SceneImgui.Translate("Remove Image From Group")))
			//		{
			//			for (int i = 0; i < crossfadeIndexes.Count; i++)
			//			{
			//				if (scene.crossFades.ContainsKey(crossfadeIndexes[i]) && scene.crossFades[crossfadeIndexes[i]] != null && scene.crossFades[crossfadeIndexes[i]].Contains(selectedCrossFade))
			//				{
			//					scene.crossFades[crossfadeIndexes[i]].Remove(selectedCrossFade);
			//					selectedCrossFade = null;
			//				}
			//			}
			//		}
			//	}
			//	ImGui.EndGroup();
			//}
		}

		private static void AddPoint(SlideShowMenuScene slideScene)
		{
			slideScene.cameraMovementPoints.Add(slideScene.cameraMovementPoints[slideScene.cameraMovementPoints.Count - 1] + new Vector3(20f, 20f, 0f));
			clickedDots = new bool[slideScene.cameraMovementPoints.Count];
		}
	}

	internal class LayerViewer
	{
		public static bool[] selectedLayers = [];
		private static int lastSelected;
		internal static LayerTab currentTab;
		private static Vector2 setDragOffset;
		private static float setDepthOffset;
		private static bool grabbedShaders;

		public enum LayerTab
		{
			Depth,
			Flat
		}

		internal struct ShaderInfo
		{
			internal FShader shader;
			internal bool needsDepth;

			internal ShaderInfo(FShader shader, bool needsDepth)
			{
				this.shader = shader;
				this.needsDepth = needsDepth;
			}
		}
		internal static Dictionary<MenuDepthIllustration.MenuShader, ShaderInfo> shaderIndex = [];
		internal static FShader[] originalShaders;
		private static bool originalNotSet;
		private static bool needsRefresh;
		private static MenuScene lastScene;
		internal static bool askBeforeDeletion = true;
		internal static bool confirmedDeletion;
		private static int clickedSelected;
		private static int lastSelectableSelected;

		internal static void LayerEditor()
		{
			if (SceneExplorer.loadedScenes == null) return;

			if (SceneExplorer.loadedScenes != null && SceneExplorer.loadedScenes.Count > 0 && SceneExplorer.loadedScenes.Count > SceneExplorer.selectedMenuScene)
			{
				MenuScene scene = SceneExplorer.loadedScenes[SceneExplorer.selectedMenuScene];

				if (scene == null) { return; }

				if (lastScene != scene)
				{
					lastScene = scene;

					Assets.GrabMissingImages(ref scene);

					Assets.RefreshCache(scene);
				}
				MenuIllustration[] images = currentTab == LayerTab.Depth ? scene.depthIllustrations.ToArray() : scene.flatIllustrations.ToArray();

				if (selectedLayers.Length != images.Length)
				{
					selectedLayers = new bool[images.Length];
				}

				if (ImGui.BeginTabBar("##LayerTabList"))
				{
					if (ImGui.BeginTabItem(SceneImgui.Translate("Depth Images"))) { currentTab = LayerTab.Depth; SetUpLayerSelectables(images); ImGui.EndTabItem(); }

					if (ImGui.BeginTabItem(SceneImgui.Translate("Flat Images"))) { currentTab = LayerTab.Flat; SetUpLayerSelectables(images); ImGui.EndTabItem(); }

					ImGui.EndTabBar();
				}


				if (SceneImgui.Button(SceneImgui.Translate("Add Layer")))
				{
					AddLayer(scene, images);
				}
				
				bool openLayerDeletionWarning = false;
				if (selectedLayers.Any(x => x))
				{
					ImGui.SameLine();
					if (SceneImgui.Button(SceneImgui.Translate("Remove Layer")))
					{
						if (!askBeforeDeletion)
						{
							confirmedDeletion = true;
						}
						else
						{
							openLayerDeletionWarning = true;
						}
					}

					if (selectedLayers.Any(x => !x))
					{
						CalculateLayerMovement(scene, images);
					}
					
				}

				if (openLayerDeletionWarning)
				{
					ImGui.OpenPopup(Modals.deleteLayerString);
				}

				if (confirmedDeletion)
				{
					confirmedDeletion = false;
					int[] imagesToRemove = images.Where(x => selectedLayers[images.IndexOf(x)]).Select(x => images.IndexOf(x)).ToArray();

					for (int i = imagesToRemove.Length - 1; i >= 0; i--)
					{
						int index = imagesToRemove[i];
						if (currentTab == LayerTab.Depth)
						{
							scene.depthIllustrations.RemoveAt(index);
						}
						else
						{
							scene.flatIllustrations.RemoveAt(index);
						}
						images[index].RemoveSprites();
						if (scene.subObjects.Contains(images[index]))
						{
							scene.subObjects.Remove(images[index]);
						}
					}

					selectedLayers = new bool[images.Length];
				}

				Modals.LayerDeletionConfirmation();

				if (images.Length > 0)
				{
					bool anyDepth = images.Any(x => x is MenuDepthIllustration);

					LayerPlots(images, anyDepth);

					if (selectedLayers.Any(x => x) && lastSelected != -1 && ((currentTab == LayerTab.Depth && scene.depthIllustrations.Count > lastSelected) || (currentTab == LayerTab.Flat && scene.flatIllustrations.Count > lastSelected)))
					{
						LayerProperties(scene, images, anyDepth);
					}
				}
			}
		}

		private static void CalculateLayerMovement(MenuScene scene, MenuIllustration[] images)
		{
			int?[] minMaxSelected = new int?[2];
			int?[] minMaxNonSelected = new int?[2];
			for (int i = 0; i < selectedLayers.Length; i++)
			{
				if (selectedLayers[i])
				{
					if (minMaxSelected[0] == null || i < minMaxSelected[0]) { minMaxSelected[0] = i; }
					if (minMaxSelected[1] == null || i > minMaxSelected[1]) { minMaxSelected[1] = i; }
				}
				else
				{
					if (minMaxNonSelected[0] == null || i < minMaxNonSelected[0]) { minMaxNonSelected[0] = i; }
					if (minMaxNonSelected[1] == null || i > minMaxNonSelected[1]) { minMaxNonSelected[1] = i; }
				}
			}

			bool lowerCanidate = minMaxSelected[0] != null && minMaxNonSelected[0] != null && minMaxNonSelected[0] < minMaxSelected[0];
			bool higherCanidate = minMaxSelected[1] != null && minMaxNonSelected[1] != null && minMaxNonSelected[1] > minMaxSelected[1];

			ImGui.SameLine();
			ImGui.BeginDisabled(!higherCanidate);
			if (SceneImgui.Button(SceneImgui.Translate("Move Up")))
			{
				int[] imagesToMove = images.Where(x => selectedLayers[images.IndexOf(x)]).Select(x => images.IndexOf(x)).ToArray();
				for (int i = imagesToMove.Length - 1; i >= 0; i--)
				{
					int index = imagesToMove[i];
					if (currentTab == LayerTab.Depth)
					{
						scene.depthIllustrations.SwapItems(index, index + 1);
					}
					else
					{
						scene.flatIllustrations.SwapItems(index, index + 1);
					}
					selectedLayers.SwapItems(index, index + 1);
				}
				Assets.RefreshImageLayering(scene);
			}
			ImGui.EndDisabled();

			ImGui.SameLine();
			ImGui.BeginDisabled(!lowerCanidate);
			if (SceneImgui.Button(SceneImgui.Translate("Move Down")))
			{
				int[] imagesToMove = images.Where(x => selectedLayers[images.IndexOf(x)]).Select(x => images.IndexOf(x)).ToArray();
				for (int i = imagesToMove.Length - 1; i >= 0; i--)
				{
					int index = imagesToMove[i];
					if (currentTab == LayerTab.Depth)
					{
						scene.depthIllustrations.SwapItems(index, index - 1);
					}
					else
					{
						scene.flatIllustrations.SwapItems(index, index - 1);
					}
					selectedLayers.SwapItems(index, index - 1);
				}
				Assets.RefreshImageLayering(scene);
			}
			ImGui.EndDisabled();
		}

		private static void AddLayer(MenuScene scene, MenuIllustration[] images)
		{
			string? firstFile = null;
			if (Assets.storedSceneFoldersAndFiles.ContainsKey(scene.sceneFolder.ToLowerInvariant()) && Assets.storedSceneFoldersAndFiles[scene.sceneFolder.ToLowerInvariant()].Count > 0)
			{
				List<string> files = Assets.storedSceneFoldersAndFiles[scene.sceneFolder.ToLowerInvariant()];
				if (images.Where(x => x.fileName.ToCharArray().Where(char.IsDigit).Count() > 0).Count() > 0 || files.Where(x => x.ToCharArray().Where(char.IsDigit).Count() > 0).Count() > 0)
				{
					int[] imageCanidates = images.Where(x => x.fileName.ToCharArray().Where(char.IsDigit).Count() > 0).Select(x => int.Parse(string.Join("", x.fileName.Where(char.IsDigit)))).ToArray();
					int[] fileCanidates = files.Where(x => x.ToCharArray().Where(char.IsDigit).Count() > 0).Select(x => int.Parse(string.Join("", x.Where(char.IsDigit)))).ToArray();
					Array.Sort(imageCanidates);
					Array.Sort(fileCanidates);

					if (imageCanidates.Length > 0 && fileCanidates.Length > 0 && fileCanidates.Any(x => !imageCanidates.Contains(x)))
					{
						Assets.ComputeSomeVeryCursedFileNumberNabbing(files, fileCanidates.Where(x => !imageCanidates.Contains(x)).Max(), ref firstFile);
					}
					else if (fileCanidates.Length > 0 && imageCanidates.Length == 0)
					{
						Assets.ComputeSomeVeryCursedFileNumberNabbing(files, fileCanidates.Max(), ref firstFile);
					}
					else
					{
						firstFile = files[0];
					}
				}
				else
				{
					firstFile = files[0];
				}
			}
			else
			{
				firstFile = images.Length > 0 ? images[images.Length - 1].fileName : null;
			}

			if (!string.IsNullOrEmpty(firstFile))
			{
				MenuIllustration? illus = null;
				if (currentTab == LayerTab.Depth)
				{
					illus = new MenuDepthIllustration(scene.menu, scene, scene.sceneFolder, firstFile, new(0f, 0f), 15f, MenuDepthIllustration.MenuShader.Basic)
					{
						lastPos = new(0f, 0f),
						setAlpha = 1f
					};
				}
				else
				{
					illus = new(scene.menu, scene, scene.sceneFolder, firstFile, new(0f, 0f), true, true)
					{
						setAlpha = 1f
					};
				}

				if (illus != null)
				{
					scene.AddIllustration(illus);
				}
			}

			Array.Resize(ref selectedLayers, selectedLayers.Length + 1);
			Assets.RefreshImageLayering(scene);
		}

		private static void LayerPlots(MenuIllustration[] images, bool anyDepth)
		{
			float rRatio = 0.5f;
			float cRatio = anyDepth ? 1.5f : 0.5f;
			MenuIllustration[] activeImages = images.Where(x => selectedLayers[images.IndexOf(x)]).ToArray();
			MenuIllustration[] idleImages = images.Where(x => !activeImages.Contains(x)).ToArray();

			if (ImPlot.BeginSubplots("##LayerPlots", 1, anyDepth ? 2 : 1, new(-1, 300f), ImPlotSubplotFlags.NoResize, ref rRatio, ref cRatio))
			{
				if (ImPlot.BeginPlot(SceneImgui.Translate("Layer Positions")))
				{
					ImPlot.SetupAxes("##X Pos", "##Y Pos", ImPlotAxisFlags.AutoFit);
					ImPlot.SetupAxesLimits(-SceneExplorer.lastMenu.manager.rainWorld.screenSize.x, SceneExplorer.lastMenu.manager.rainWorld.screenSize.x, -SceneExplorer.lastMenu.manager.rainWorld.screenSize.y, SceneExplorer.lastMenu.manager.rainWorld.screenSize.y, ImPlotCond.Always);

					for (int p = 0; p < 2; p++)
					{
						MenuIllustration[] illus = p == 0 ? idleImages : activeImages;
						for (int i = illus.Length - 1; i >= 0; i--)
						{
							System.Numerics.Vector4 col = p == 1 ? new(1, 0, 0, 1) : new(0.5f, 0.5f, 0.5f, 1);
							double xMin = illus[i].pos.x - (illus[i].sprite.width / 2f);
							double xMax = illus[i].pos.x + (illus[i].sprite.width / 2f);
							double yMin = illus[i].pos.y - (illus[i].sprite.height / 2f);
							double yMax = illus[i].pos.y + (illus[i].sprite.height / 2f);
							ImPlot.DragRect(i, ref xMin, ref yMin, ref xMax, ref yMax, col, ImPlotDragToolFlags.NoInputs | ImPlotDragToolFlags.NoCursors);

							double actualX = illus[i].pos.x;
							double actualY = illus[i].pos.y;

							if (p == 1 && SceneImgui.DragPoint(i, ref actualX, ref actualY, col))
							{
								setDragOffset = illus[i].pos;

								Vector2 drag = new((float)actualX, (float)actualY);
								MenuIllustration[] selectedImages = images.Where(x => selectedLayers[images.IndexOf(x)]).ToArray();
								for (int j = 0; j < selectedImages.Length; j++)
								{
									selectedImages[j].pos += drag - setDragOffset;
								}
							}
						}
					}
					ImPlot.EndPlot();
				}

				if (anyDepth && ImPlot.BeginPlot(SceneImgui.Translate("Layer Depths")))
				{
					ImPlot.SetupAxes("##DiscardAxis", "##Depth", ImPlotAxisFlags.AutoFit | ImPlotAxisFlags.NoTickLabels);
					ImPlot.SetupAxesLimits(0, 1, 0, 15, ImPlotCond.Always);

					for (int i = 0; i < images.Length; i++)
					{
						System.Numerics.Vector4 col = selectedLayers[i] ? new(1, 0, 0, 1) : new(0.5f, 0.5f, 0.5f, 1);
						if (images[i] is MenuDepthIllustration depth)
						{
							double imgDepth = depth.depth;
							if (ImPlot.DragLineY(i, ref imgDepth, col, 1f, selectedLayers[i] ? ImPlotDragToolFlags.None : ImPlotDragToolFlags.NoInputs))
							{
								setDepthOffset = depth.depth;
								float drag = (float)imgDepth;

								MenuDepthIllustration[] selectedImages = images.Where(x => x is MenuDepthIllustration && selectedLayers[images.IndexOf(x)]).Select(x => x as MenuDepthIllustration).ToArray();
								for (int j = 0; j < selectedImages.Length; j++)
								{
									selectedImages[j].depth += drag - setDepthOffset;
								}
							}

							ImPlot.TagY(imgDepth, col, $"{i}");
						}
					}

					ImPlot.EndPlot();
				}
				ImPlot.EndSubplots();
			}

			if (ImPlot.BeginPlot(SceneImgui.Translate("Layer Alphas"), new(-1, 150f)))
			{
				ImPlot.SetupAxes(SceneImgui.Translate("Time"), SceneImgui.Translate("Opacity"), ImPlotAxisFlags.NoTickLabels);
				ImPlot.SetupAxesLimits(0, 1, 0, 1, ImPlotCond.Always);

				// Set up alpha visualizers here

				ImPlot.EndPlot();
			}
		}

		private unsafe static void SetUpLayerSelectables(MenuIllustration[] images)
		{
			int columns = images.Any(x => x is MenuDepthIllustration) ? 6 : 4;
			if (SceneImgui.BeginTable("##LayerTable", columns, images.Length, null))
			{
				ImGui.TableSetupColumn("#");
				ImGui.TableSetupColumn(SceneImgui.Translate("Name"));
				ImGui.TableSetupColumn(SceneImgui.Translate("Position"));
				ImGui.TableSetupColumn(SceneImgui.Translate("Alpha"));
				if (columns > 4)
				{
					ImGui.TableSetupColumn(SceneImgui.Translate("Depth"));
					ImGui.TableSetupColumn(SceneImgui.Translate("Shader"));
				}
				ImGui.TableSetupScrollFreeze(0, 1);
				ImGui.TableHeadersRow();

				for (int i = images.Length - 1; i >= 0; i--)
				{
					MenuIllustration image = images[i];

					ImGui.TableNextRow();
					ImGui.TableSetColumnIndex(0);
					ImGui.Text($"{i}");

					ImGui.TableSetColumnIndex(1);
					if (SceneImgui.Selectable($"{image.fileName}##{i}", ref selectedLayers[i]))
					{
						if (lastSelectableSelected != i) { clickedSelected = 0; lastSelectableSelected = i; }
						else
						{
							clickedSelected++;
						}

						if (clickedSelected >= 2)
						{
							clickedSelected = 0;
							selectedLayers = new bool[images.Length];
							selectedLayers[i] = true;
						}

						if (lastSelected == i && !selectedLayers[i]) { clickedSelected = 0; lastSelected = Array.IndexOf(selectedLayers, true); }
						else if (selectedLayers[i]) { lastSelected = i; }
					}

					ImGui.TableSetColumnIndex(2);
					ImGui.Text($"{image.pos}");

					ImGui.TableSetColumnIndex(3);
					ImGui.Text($"{(image.setAlpha == null ? image.alpha : image.setAlpha)}");

					if (columns > 4 && image is MenuDepthIllustration depth)
					{
						ImGui.TableSetColumnIndex(4);
						ImGui.Text($"{depth.depth}");

						ImGui.TableSetColumnIndex(5);
						ImGui.Text($"{depth.shader.value}");
					}
				}

				if (!selectedLayers.Any(x => x)) { lastSelected = -1; lastSelectableSelected = -1; }

				ImGui.EndTable();
			}
		}

		private static void LayerProperties(MenuScene scene, MenuIllustration[] images, bool anyDepth)
		{
			if (!grabbedShaders)
			{
				shaderIndex.Clear();
				MenuDepthIllustration.MenuShader[] shaders = MenuDepthIllustration.MenuShader.values.entries.Select(x => ExtEnumBase.Parse(typeof(MenuDepthIllustration.MenuShader), x, true)).Select(x => x as MenuDepthIllustration.MenuShader).ToArray();
				for (int j = 0; j < shaders.Length; j++)
				{
					MenuDepthIllustration discard = new(SceneExplorer.lastMenu, SceneExplorer.lastMenu.scene.owner, "", "empty", new(), 1f, shaders[j]);
					if (!shaderIndex.ContainsKey(shaders[j]))
					{
						shaderIndex.Add(shaders[j], new(discard.sprite.shader, discard.sprite.scaleY == 0.5f));
					}
				}
				grabbedShaders = true;
			}

			MenuIllustration lastImage = currentTab == LayerTab.Depth ? scene.depthIllustrations[lastSelected] : scene.flatIllustrations[lastSelected];

			if (lastImage != null && ImGui.BeginCombo(SceneImgui.Translate("File Name"), lastImage.fileName))
			{
				string[] filesInFolder = Assets.cachedElements.Keys.ToArray();
				for (int i = 0; i < filesInFolder.Length; i++)
				{
					if (SceneImgui.Selectable(filesInFolder[i], filesInFolder[i].ToLowerInvariant() == lastImage.fileName.ToLowerInvariant()))
					{
						lastImage.fileName = filesInFolder[i];
						lastImage.sprite.SetElementByName(Assets.cachedElements[filesInFolder[i]]);
					}
				}
				ImGui.EndCombo();
			}

			if (anyDepth && grabbedShaders && lastImage is MenuDepthIllustration depth2 && ImGui.BeginCombo(SceneImgui.Translate("Shader"), depth2.shader.value))
			{
				if (!originalNotSet)
				{
					originalNotSet = true;

					originalShaders = images.Select(x => x.sprite.shader).ToArray();
				}

				List<MenuDepthIllustration.MenuShader> selectables = shaderIndex.Keys.ToList();
				selectables.Sort((x, y) => x.value.CompareTo(y.value));
				if (ImGui.BeginTable("##ShaderInformation", 2))
				{
					for (int j = 0; j < selectables.Count; j++)
					{
						ImGui.TableNextRow();

						ImGui.TableSetColumnIndex(0);
						MenuDepthIllustration[] selectedImages = images.Where(x => selectedLayers[images.IndexOf(x)] && x is MenuDepthIllustration).Select(x => x as MenuDepthIllustration).ToArray();
						if (SceneImgui.Selectable(selectables[j].value, depth2.shader == selectables[j]))
						{
							for (int i = 0; i < selectedImages.Length; i++)
							{
								selectedImages[i].size = new Vector2((float)selectedImages[i].texture.width, (float)selectedImages[i].texture.height / 2f);
								selectedImages[i].sprite.scaleY = 0.5f;
								if (shaderIndex.ContainsKey(selectables[j]))
								{
									selectedImages[i].sprite.shader = shaderIndex[selectables[j]].shader;
									selectedImages[i].shader = selectables[j];
									if (!shaderIndex[selectables[j]].needsDepth)
									{
										selectedImages[i].size.y = selectedImages[i].texture.height;
										selectedImages[i].sprite.scaleY = 1f;
										selectedImages[i].sprite.color = Color.white;
									}
								}
							}
						}

						// Add hover previews :D

						ImGui.TableSetColumnIndex(1);
						if (shaderIndex.ContainsKey(selectables[j]))
						{
							SceneImgui.Text(shaderIndex[selectables[j]].needsDepth ? "Requires Depth Map" : "");
						}
						else
						{
							grabbedShaders = false;
						}
					}

					ImGui.EndTable();
				}
				ImGui.EndCombo();
			}
			SceneImgui.HelpMarker("shader_explanation");
		}
	}
}
