﻿using MindMap.Entities;
using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
using MindMap.Entities.Interactions;
using MindMap.Entities.Locals;
using MindMap.Entities.Presets;
using MindMap.Entities.Tags;
using MindMap.Pages.Interfaces;
using MindMap.SubWindows.Dialogs;
using MindMap.UserControls.MindMapPageControls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Pages {
	public partial class MindMapPage: Page, IPage {//Editor Page
		public readonly ConnectionsManager connectionsManager;
		public readonly EditHistory editHistory;
		public readonly ImagesAssets imagesAssets;
		public readonly MultiSelectionFrame multiSelectionFrame;
		public bool holdShift;
		private string? savePath = null;
		private string fileName = "(Not Saved)";
		private bool hasChanged = false;

		public string FileName {
			get => fileName;
			set {
				fileName = value;
				FileNameText.Text = FileName + (HasChanged ? "*" : "");
			}
		}

		public MindMapPage() {
			InitializeComponent();
			multiSelectionFrame = new MultiSelectionFrame(this, (s, e) => {
				BackgroundRectangle_MouseDown(s, e);
			}, (s, e) => {
				BackgroundRectangle_MouseMove(s, e);
			}, (s, e) => {
				BackgroundRectangle_MouseUp(s, e);
			});
			connectionsManager = new ConnectionsManager(this);
			connectionsManager.OnConnectionPathChanged += args => {
				RefreshMapElementsListView();
			};
			editHistory = new EditHistory(this);
			editHistory.OnHistoryChanged += EditHistory_OnHistoryChanged;
			editHistory.OnUndo += EditHistory_OnUndo;
			editHistory.OnRedo += EditHistory_OnRedo;

			imagesAssets = new ImagesAssets();

			MainCanvas.MouseMove += MainCanvas_MouseMove;
			MainCanvas.MouseUp += MainCanvas_MouseUp;

			LoadElementsPreset();

			HideElementProperties();

			MainCanvas_TranslateTransform.X = 40;
			MainCanvas_TranslateTransform.Y = 40;

			FileNameText.Text = fileName;
			CreatedDateText.Text = $"Created Date ({DateTime.Now})";

			SavingPanel.Visibility = Visibility.Collapsed;
			LoadingPanel.Visibility = Visibility.Collapsed;

			OnClose();
			MainWindow.Instance?.KeyManager.Register(
				() => holdShift = true,
				() => holdShift = false,
			false, Key.LeftShift);
			//MainWindow.Instance?.KeyManager.Register(
			//	() => Save(),
			//	() => { },
			//true, Key.LeftCtrl, Key.S);
			MainWindow.Instance?.KeyManager.Register(
				() => Undo(),
				() => { },
			false, Key.LeftCtrl, Key.Z);
			MainWindow.Instance?.KeyManager.Register(
				() => Redo(),
				() => { },
			false, Key.LeftCtrl, Key.Y);
		}

		public void OnClose() {
			MainWindow.Instance?.KeyManager.Remove(Key.LeftShift);
			MainWindow.Instance?.KeyManager.Remove(Key.LeftCtrl, Key.S);
			MainWindow.Instance?.KeyManager.Remove(Key.LeftCtrl, Key.Z);
			MainWindow.Instance?.KeyManager.Remove(Key.LeftCtrl, Key.Y);
		}

		private void LoadElementsPreset() {
			if(AppSettings.Current == null) {
				return;
			}
			List<ElementPresetsGroup> presets = AppSettings.Current.ElementPresetsGroups;
			List<ElementPresetsGroupView> deleted = new();
			foreach(var item in ElementsPresetsPanel.Children) {
				if(item is ElementPresetsGroupView view) {
					deleted.Add(view);
				}
			}

			foreach(var item in deleted) {
				ElementsPresetsPanel.Children.Remove(item);
			}

			foreach(ElementPresetsGroup preset in presets.Reverse<ElementPresetsGroup>()) {
				var group = new ElementPresetsGroupView(this, preset);
				ElementsPresetsPanel.Children.Insert(0, group);
			}
		}

		public void RemovePresetGroup(ElementPresetsGroup group) {
			foreach(var item in ElementsPresetsPanel.Children) {
				if(item is ElementPresetsGroupView view && view.Group == group) {
					ElementsPresetsPanel.Children.Remove(view);
					break;
				}
			}
		}

		public void CallAddToPresetDialog(Element element) {
			if(MainWindow.Instance == null || AppSettings.Current == null) {
				return;
			}
			if(AppSettings.Current.ElementPresetsGroups.Where(i => !i.Unchangable).Count() == 0) {
				MessageBox.Show(MainWindow.Instance, "You don't have any presets group except default. \nPlease create a new one by clicking 'Add New Group' on the left.", "Problem", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}
			var dialog = new ElementPresetsGroupSelectorView(MainWindow.Instance, element.ElementTypeName);
			dialog.ShowDialog();
			if(dialog.DialogResult == true && dialog.CurrentSelected != null && dialog.NameValid) {
				AddToPresetGroup(element, dialog.CurrentSelected, dialog.NewNameText);
			}
		}

		public async void AddToPresetGroup(Element element, ElementPresetsGroup group, string newName) {
			var preset = new ElementPreset(
				newName,
				element.TypeID,
				JsonConvert.SerializeObject(element.Properties, Formatting.Indented),
				element.GetSize());
			group.Presets.Add(preset);
			UpdatePresetGroup(group);
			await Local.SaveAppSettings();
		}

		public async void RemoveFromPresetGroup(ElementPreset preset, string? groupName = null) {
			if(AppSettings.Current == null) {
				return;
			}
			ElementPresetsGroup? found = null;
			if(string.IsNullOrWhiteSpace(groupName)) {
				found = AppSettings.Current.ElementPresetsGroups.Find(p => p.Name == groupName);
				if(found == null) {
					return;
				}
				found.Presets.Remove(preset);
			} else {//not tested in this section
				foreach(ElementPresetsGroup item in AppSettings.Current.ElementPresetsGroups) {
					foreach(ElementPreset p in item.Presets) {
						if(p == preset) {
							found = item;
							break;
						}
					}
					if(found != null) {
						found.Presets.Remove(preset);
					}
				}
			}
			if(found != null) {
				UpdatePresetGroup(found);
				await Local.SaveAppSettings();
			}
		}

		public async void MovePresetToTop(ElementPreset preset, string groupName) {
			if(AppSettings.Current == null || string.IsNullOrEmpty(groupName)) {
				return;
			}
			ElementPresetsGroup? found = AppSettings.Current.ElementPresetsGroups.Find(p => p.Name == groupName);
			if(found == null || found.Presets.IndexOf(preset) <= 0) {//already at first or index cannot be found
				return;
			}
			found.Presets.Remove(preset);
			found.Presets.Insert(0, preset);

			UpdatePresetGroup(found);
			await Local.SaveAppSettings();
		}

		public async void MovePresetToLeft(ElementPreset preset, string groupName) {
			if(AppSettings.Current == null || string.IsNullOrEmpty(groupName)) {
				return;
			}
			ElementPresetsGroup? found = AppSettings.Current.ElementPresetsGroups.Find(p => p.Name == groupName);
			if(found == null) {
				return;
			}
			int index = found.Presets.IndexOf(preset);
			if(index <= 0) {//same as above
				return;
			}
			found.Presets.Remove(preset);
			found.Presets.Insert(index - 1, preset);
			UpdatePresetGroup(found);
			await Local.SaveAppSettings();
		}

		public async void MovePresetToRight(ElementPreset preset, string groupName) {
			if(AppSettings.Current == null || string.IsNullOrEmpty(groupName)) {
				return;
			}
			ElementPresetsGroup? found = AppSettings.Current.ElementPresetsGroups.Find(p => p.Name == groupName);
			if(found == null) {
				return;
			}
			int index = found.Presets.IndexOf(preset);
			if(index == -1 || index == found.Presets.Count - 2) {
				return;
			}
			found.Presets.Remove(preset);
			found.Presets.Insert(index + 1, preset);
			UpdatePresetGroup(found);
			await Local.SaveAppSettings();
		}

		private void UpdatePresetGroup(ElementPresetsGroup group) {
			foreach(var item in ElementsPresetsPanel.Children) {
				if(item is ElementPresetsGroupView view && view.Group == group) {
					view.Group = group;
					break;
				}
			}
		}

		private void EditHistory_OnUndo(EditHistory.IChange obj) {
			MainWindow.Instance?.SetUndoMenuItemActive(editHistory.GetPreviousHistories().Count > 0);
			MainWindow.Instance?.SetRedoMenuItemActive(true);
			UpdateHistoryListView();
			if(ResizeFrame.Current?.elements.Count == 1) {
				ShowElementProperties(ResizeFrame.Current.elements[0]);
			}
		}

		private void EditHistory_OnRedo(EditHistory.IChange obj) {
			MainWindow.Instance?.SetRedoMenuItemActive(editHistory.GetFuturesHistories().Count > 0);
			MainWindow.Instance?.SetUndoMenuItemActive(true);
			UpdateHistoryListView();
			if(ResizeFrame.Current?.elements.Count == 1) {
				ShowElementProperties(ResizeFrame.Current.elements[0]);
			}
		}

		private void EditHistory_OnHistoryChanged(List<EditHistory.IChange> history) {
			HasChanged = true;
			MainWindow.Instance?.SetUndoMenuItemActive(true);
			UpdateHistoryListView(history);
		}

		public void UpdateHistoryListView(List<EditHistory.IChange>? history = null) {
			EditHistoryListView.Items.Clear();
			foreach(EditHistory.IChange item in history ?? editHistory.GetPreviousHistories()) {
				Grid grid = new() {
					Tag = item,
				};
				grid.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(1, GridUnitType.Auto),
				});
				grid.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(1, GridUnitType.Star),
				});
				grid.Children.Add(item.GetIcon().Generate());
				TextBlock text = new() {
					Text = item.ToString(),
					Margin = new Thickness(10, 0, 0, 0),
					TextWrapping = TextWrapping.Wrap,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Center,
					Width = 210,
				};
				Grid.SetColumn(text, 1);
				grid.Children.Add(text);
				ToolTipService.SetToolTip(grid, item.Date);
				EditHistoryListView.Items.Add(grid);
			}
		}

		private void EditHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is Grid grid && grid.Tag is EditHistory.IChange change) {
				UpdateEditHistoryDetail(change.GetPreviousDetail());
			} else {
				UpdateEditHistoryDetail(null);
			}
		}

		private void UpdateEditHistoryDetail(string? content) {
			if(string.IsNullOrWhiteSpace(content)) {
				DetailText.Visibility = Visibility.Collapsed;
				DetailText.Text = "";
			} else {
				DetailText.Visibility = Visibility.Visible;
				DetailText.Text = content;
			}
		}

		//private Task? OprationHintTextTask = null;
		public async void SetSetOprationHintText(string text, int delay = 5000) {
			//if(OprationHintTextTask != null) {
			//	OprationHintTextTask.Dispose();
			//}
			//OprationHintTextTask = Task.Run(async () => await SetOprationHintTextAsync(text, delay));
			await SetOprationHintTextAsync(text, delay);
		}

		private async Task SetOprationHintTextAsync(string text, int delay) {
			OprationHintText.Visibility = Visibility.Visible;
			OprationHintText.Text = text;
			await Task.Delay(delay);
			OprationHintText.Visibility = Visibility.Collapsed;
		}

		public async void Save(bool saveAs = false) {
			SavingPanel.Visibility = Visibility.Visible;
			savePath = (await Local.Save(
				elements.Values.ToList(),
				connectionsManager,
				editHistory,
				imagesAssets,
				saveAs ? null : savePath
			)) ?? savePath;
			if(!string.IsNullOrWhiteSpace(savePath)) {
				FileName = savePath[(savePath.LastIndexOf('\\') + 1)..];
				HasChanged = false;
				MainWindow.SetTitle($"Mind Map - {FileName}");
				//set created time
				SetSetOprationHintText("Saved Successfully");

				await AddRecentOpenFile(FileName, savePath);
			}
			SavingPanel.Visibility = Visibility.Collapsed;
		}

		public async Task Load(MapInfo mapInfo, FileInfo fileInfo) {
			LoadingPanel.Visibility = Visibility.Visible;
			FileName = fileInfo.FileName;
			savePath = fileInfo.FilePath;
			MainWindow.SetTitle($"{App.Name} - {FileName}");
			CreatedDateText.Text = $"Created Date ({fileInfo.CreatedDate})";
			await AddRecentOpenFile(FileName, savePath);

			imagesAssets.SetAssets(mapInfo.imagesAssets);

			foreach(ElementInfo ele in mapInfo.elements) {
				Element element = AddElement(ele.type_id, ele.identity, ele.position, ele.size, ele.connectionControls, false);
				element.SetProperty(ele.propertyJson);
				await Task.Delay(1);
			}

			foreach(ConnectionInfo item in mapInfo.connections) {
				Element? from = elements.Select(e => e.Value).ToList().Find(i => i.Identity == item.from_element);
				ConnectionControl? fromDot = from?.GetConnectionControlByID(item.from_dot.ID);
				Element? to = elements.Select(e => e.Value).ToList().Find(i => i.Identity == item.to_element);
				ConnectionControl? toDot = to?.GetConnectionControlByID(item.to_dot.ID);
				if(from == null || to == null || fromDot == null || toDot == null) {
					continue;
				}
				ConnectionPath? connection = connectionsManager.AddConnection(fromDot, toDot, item.identity, false);
				connection?.SetProperty(item.propertyJson);
				await Task.Delay(1);
			}

			editHistory.SetHistory(mapInfo.history);
			UpdateHistoryListView();
			LoadingPanel.Visibility = Visibility.Collapsed;
			SetSetOprationHintText("Loaded Successfully");
		}

		private async Task AddRecentOpenFile(string name, string path) {
			if(AppSettings.Current == null) {
				return;
			}
			foreach(var item in AppSettings.Current.RecentOpenFilesList) {
				if(item.name == name || item.path == path) {
					return;
				}
			}
			AppSettings.Current.RecentOpenFilesList.Add(new(name, path, DateTime.Now));
			await Local.SaveAppSettings();
		}

		public void Redo() => editHistory.Redo();

		public void Undo() => editHistory.Undo();

		public bool HasChanged {
			get => hasChanged;
			set {
				hasChanged = value;
				FileNameText.Text = FileName + (HasChanged ? "*" : "");
			}
		}

		public void ClearResizePanel() {
			if(previous != null && elements.ContainsKey(previous)) {
				elements[previous].SetConnectionsFrameVisible(true);
				elements[previous].UpdateConnectionsFrame();
			}
			ResizeFrame.Current?.ClearResizeFrame();
			ResizeFrame.Current = null;
		}

		public void Deselect(bool includePath = true) {
			//if(ResizeFrame.Current != null) {
			//	ResizeFrame.Current.elements.ForEach(e => e.Deselect());
			//}
			foreach(Element item in elements.Values) {
				item.Deselect();
			}
			if(includePath) {
				connectionsManager.CurrentSelection?.Deselect();
			}
			HideElementProperties();
		}

		private ConnectionPath? previewLine;

		public void UpdatePreviewLine(ConnectionControl from, Vector2 to, ConnectionControl? target) {
			if(previewLine == null) {
				previewLine = new ConnectionPath(this, from, to);
			}
			previewLine.Update(to, target);
		}

		public void ClearPreviewLine() {
			if(previewLine == null) {
				return;
			}
			MainCanvas.Children.Remove(previewLine.Path);
			previewLine = null;
		}

		private readonly Dictionary<Vector2, FrameworkElement> backgroundPool = new();
		//private const int SIZE = 4;
		//private const int GAP = 20;
		private void UpdateBackgroundDot() {
			double shapSize = AppSettings.Current?.BackgroundShapeSize ?? 4;
			double gap = AppSettings.Current?.BackgroundShapeGap ?? 20;
			var offset = -new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y).ToInt() / gap;
			var size = new Vector2(MainCanvas.ActualWidth, MainCanvas.ActualHeight).ToInt() / gap;
			for(int i = offset.X_Int; i < size.X + offset.X; i++) {
				for(int j = offset.Y_Int; j < size.Y + offset.Y; j++) {
					if(backgroundPool.ContainsKey(new Vector2(i, j))) {
						continue;
					}
					FrameworkElement framework;
					if(AppSettings.Current == null || AppSettings.Current.BackgroundStyle == BackgroundStyle.Dot) {
						framework = new Ellipse() {
							Width = shapSize,
							Height = shapSize,
							Fill = new SolidColorBrush(Colors.Gray),
						};
					} else if(AppSettings.Current.BackgroundStyle == BackgroundStyle.Rect) {
						framework = new Rectangle() {
							Width = shapSize,
							Height = shapSize,
							Fill = new SolidColorBrush(Colors.Gray),
						};
					} else if(AppSettings.Current.BackgroundStyle == BackgroundStyle.Heart) {
						framework = new TextBlock() {
							Width = shapSize,
							Height = shapSize,
							Foreground = new SolidColorBrush(Colors.Gray),
							FontFamily = new FontFamily("Segoe MDL2 Assets"),
							HorizontalAlignment = HorizontalAlignment.Center,
							VerticalAlignment = VerticalAlignment.Center,
							Text = "\uE0A5",
							FontSize = shapSize,
						};
					} else {
						return;
					}
					framework.MouseDown += BackgroundRectangle_MouseDown;
					Canvas.SetLeft(framework, i * gap - shapSize / 2);
					Canvas.SetTop(framework, j * gap - shapSize / 2);
					backgroundPool.Add(new Vector2(i, j), framework);
					BackgroundCanvas.Children.Add(framework);
				}
			}
		}

		private void ClearBackground() {
			backgroundPool.Clear();
			BackgroundCanvas.Children.Clear();
		}

		public void NotifyBackgroundStyleChanged() {
			ClearBackground();
			UpdateBackgroundDot();
		}

		private Dictionary<int, FrameworkElement> horizontalRuleFrameworks = new();
		private Dictionary<int, FrameworkElement> verticalRuleFrameworks = new();
		private const int RULE_ADJUST_OFFSET = -15;//match value of rule canvas
		private void UpdateCoordiantionRules() {
			Vector2 coord = new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y);
			Size size = MainCanvas.RenderSize;
			int ruleMinGap = (int)(AppSettings.Current?.CanvasRuleGap ?? 100);
			List<int> required_x = new();
			for(int x = -(int)coord.X / ruleMinGap * ruleMinGap
				; x < (double)(size.Width - coord.X)
				; x += ruleMinGap) {
				required_x.Add(x);
				FrameworkElement framework;
				if(horizontalRuleFrameworks.ContainsKey(x)) {
					framework = horizontalRuleFrameworks[x];
				} else {
					framework = new TextBlock() {
						Text = $"{x}",
					};
					horizontalRuleFrameworks.Add(x, framework);
					TopCoordRule.Children.Add(framework);
				}
				Canvas.SetTop(framework, 0);
				Canvas.SetLeft(framework, coord.X + x + RULE_ADJUST_OFFSET);
			}
			foreach(var item in horizontalRuleFrameworks) {
				item.Value.Visibility = required_x.Contains(item.Key) ? Visibility.Visible : Visibility.Collapsed;
			}

			List<int> required_y = new();
			for(int y = -(int)coord.Y / ruleMinGap * ruleMinGap
				; y < (double)(size.Height - coord.Y)
				; y += ruleMinGap) {
				required_y.Add(y);
				FrameworkElement framework;
				if(verticalRuleFrameworks.ContainsKey(y)) {
					framework = verticalRuleFrameworks[y];
				} else {
					framework = new TextBlock() {
						Text = $"{y}",
						LayoutTransform = new RotateTransform(-90),
					};
					verticalRuleFrameworks.Add(y, framework);
					LeftCoordRule.Children.Add(framework);
				}
				Canvas.SetLeft(framework, 0);
				Canvas.SetTop(framework, coord.Y + y + RULE_ADJUST_OFFSET);
			}
			foreach(var item in verticalRuleFrameworks) {
				item.Value.Visibility = required_y.Contains(item.Key) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void ClearCoordinationRules() {
			horizontalRuleFrameworks.Clear();
			verticalRuleFrameworks.Clear();
			TopCoordRule.Children.Clear();
			LeftCoordRule.Children.Clear();
		}

		public void EnableRule(bool enable = true) {
			TopCoordRule.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
			LeftCoordRule.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
			TopLeftRuleIcon.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
		}

		public void NotifyCoordinationRuleStyleChanged() {
			ClearCoordinationRules();
			if(AppSettings.Current?.EnableCanvasRule ?? true) {
				EnableRule(true);
				UpdateCoordiantionRules();
			} else {
				EnableRule(false);
			}
		}

		private Vector2 GetCenterViewPoint() {
			Vector2 coord = new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y);
			Size size = MainCanvas.RenderSize;
			return (-coord + new Vector2(size.Width, size.Height) / 2);
		}

		public readonly Dictionary<FrameworkElement, Element> elements = new();

		//public void ShowElementProperties() {
		//	Element? found = elements.FirstOrDefault(p => p.Key == current).Value;
		//	if(found == null) {
		//		return;
		//	}
		//	ShowElementProperties(found);
		//}

		public void ShowElementProperties(Element element, bool showPropertiesTabItem = false) {
			ElementPropertiesPanel.Children.Clear();

			ElementPropertiesPanel.Children.Add(element.CreateElementProperties());
			foreach(Panel item in Element.CreatePropertiesList(element, editHistory)) {
				ElementPropertiesPanel.Children.Add(item);
			}

			if(showPropertiesTabItem) {
				PropertiesTabItem.IsSelected = true;
			}
		}

		public void ShowConnectionPathProperties(ConnectionPath path) {
			Deselect(false);
			ClearResizePanel();
			ElementPropertiesPanel.Children.Clear();
			ElementPropertiesPanel.Children.Add(path.CreatePropertiesPanel());
		}

		public void HideElementProperties() {
			ElementPropertiesPanel.Children.Clear();
			ElementPropertiesPanel.Children.Add(new TextBlock() {
				Text = "(No Selection)",
				HorizontalAlignment = HorizontalAlignment.Center,
				FontSize = 14,
				Margin = new Thickness(0, 10, 0, 0),
			});
		}

		public List<ConnectionControl> GetAllConnectionDots(Element self) {
			List<ConnectionControl> result = new();
			foreach(Element item in elements.Values) {
				if(item == self) {
					continue;
				}
				item.GetAllConnectionDots().ForEach(result.Add);
			}
			return result;
		}

		public void UpdateCount() {
			ConnectionsCountText.Text = $" : {connectionsManager.Count}";
			ElementsCountText.Text = $" : {elements.Count}";
		}

		public Element? FindElementByIdentity(Identity identity, bool matchName = false) {
			return elements.Select(e => e.Value).ToList().Find(e => {
				if(matchName) {
					return e.Identity.RefEquals(identity);
				} else {
					return e.Identity == identity;
				}
			});
		}

		public ConnectionPath? FindConnectionPathByIdentity(Identity identity, bool matchName = false) {
			return connectionsManager.FindConnectionPathByIdentity(identity, matchName);
		}

		public Element AddElement(long type_id, Identity? identity = null, Vector2 position = default, Vector2 size = default, ControlsInfo? initialControls = null, bool submitEditHistory = true) {
			Element element = ElementGenerator.GetElement(this, type_id, identity);
			element.SetFramework();
			element.SetPosition(position);
			element.SetSize(size == default ? element.DefaultSize : size);
			element.CreateConnectionsFrame(initialControls);
			element.CreateFlyoutMenu();
			element.Target.MouseDown += (s, e) => Element_MouseDown(s, e, element);
			elements.Add(element.Target, element);
			if(element is IUpdate update) {
				update.Update();
			}
			UpdateCount();
			RefreshMapElementsListView();
			if(submitEditHistory) {
				editHistory.SubmitByElementCreated(element);
			}
			return element;
		}

		public void RemoveElement(Element element, bool submitEditHistory = true) {
			if(!elements.ContainsKey(element.Target) || !MainCanvas.Children.Contains(element.Target)) {
				throw new Exception($"Remove Element ({element.Identity.Name}) Failed");
			}
			List<ConnectionPath> related = element.GetRelatedPaths();
			ControlsInfo connections = element.ConnectionsFrame?.GetControlsInfo() ?? new();
			Deselect();
			ClearResizePanel();
			elements.Remove(element.Target);
			MainCanvas.Children.Remove(element.Target);
			element.ConnectionsFrame?.ClearConnections();
			UpdateCount();
			RefreshMapElementsListView();
			if(submitEditHistory) {
				editHistory.SubmitByElementDeleted(element, related, connections);
			}
		}

		public void RemoveElement(Identity identity, bool submitEditHistory = true) {
			Element? element = elements.Select(e => e.Value).ToList().Find(i => i.Identity == identity);
			if(element == null) {
				throw new Exception($"Cannot Find Element ({identity})");
			}
			RemoveElement(element, submitEditHistory);
		}

		public void PutElementOnTop(Element element) {
			//FrameworkElement? target = null;

			int lastIndex = MainCanvas.Children.Count;
			for(int i = MainCanvas.Children.Count - 1; i >= 0; i--) {
				if(MainCanvas.Children[i] is FrameworkElement e && e.Tag is ResizeFrameworkTag) {
					lastIndex = i;
				}
			}

			for(int i = 0; i < MainCanvas.Children.Count; i++) {
				if(MainCanvas.Children[i] is FrameworkElement e &&
					e.Tag is ElementFrameworkTag tag &&
					tag.Target == element
				) {
					var targets = element.GetRelatedFrameworks();
					foreach(FrameworkElement item in targets) {
						MainCanvas.Children.Remove(item);
					}
					targets.Reverse();
					foreach(FrameworkElement item in targets) {
						MainCanvas.Children.Insert(lastIndex - targets.Count, item);
					}
					break;
				}
			}

			for(int i = 0; i < MainCanvas.Children.Count; i++) {
				if(MainCanvas.Children[i] is FrameworkElement e &&
					e.Tag is ElementFrameworkTag tag &&
					tag.Target == element
				) {
					List<FrameworkElement> frameworks = new();
					foreach(ConnectionPath item in element.GetRelatedPaths()) {
						frameworks.AddRange(item.GetRelatedFrameworks());
					}
					foreach(FrameworkElement fe in frameworks) {
						MainCanvas.Children.Remove(fe);
					}
					frameworks.Reverse();
					foreach(FrameworkElement fe in frameworks) {
						MainCanvas.Children.Insert(lastIndex - frameworks.Count, fe);
					}
					break;
				}
			}

		}

		public void AddElementByClick(long id, Vector2 size, string propertyJson, bool isInDefaultGroup = false) {
			Element element = AddElement(id, null, default, size, null, false);
			element.SetProperty(propertyJson);
			if(isInDefaultGroup && element is TextRelated text) {
				text.SetText(f => AppSettings.Current?.ElementDefaultText ?? f);
			}
			Reposition(element);
			editHistory.SubmitByElementCreated(element);
		}

		private void Reposition(Element element) {
			Vector2 defaultPosition;
			if(AppSettings.Current == null) {
				defaultPosition = Vector2.Zero;
			} else if(AppSettings.Current.EnableElementDefaultPositionCentered) {
				defaultPosition = GetCenterViewPoint() - element.GetSize() / 2;
			} else {
				defaultPosition = AppSettings.Current.ElementDefaultPosition;
			}
			const int GAP = 20;
			int count = 0;
			foreach(Element item in elements.Select(e => e.Value).Where(e => e != element)) {
				Vector2 pos = item.GetPosition() - Vector2.One * GAP * count - defaultPosition;
				if(pos.Magnitude < 5) {
					count++;
				}
			}
			element.SetPosition(Vector2.One * count * GAP + defaultPosition);
			element.UpdateConnectionsFrame();
		}

		private Vector2 clickStartPos;
		private List<ElementReframeworkInfo> current = new();
		private bool MultiSelectionClicking => holdShift;
		private bool hasMoved;
		private MouseType mouseType;
		private void Element_MouseDown(object sender, MouseButtonEventArgs e, Element element) {
			FrameworkElement framework = (FrameworkElement)sender;
			if(elements.ContainsKey(framework)) {
				if(elements[framework].IsLocked) {
					return;
				}
			}
			current.Clear();
			clickStartPos = e.GetPosition(MainCanvas);
			if(ResizeFrame.Current != null) {
				if(ResizeFrame.Current.elements.Count != 0) {
					foreach(Element item in ResizeFrame.Current.elements) {
						current.Add(new ElementReframeworkInfo(
							target: item,
							elementStartPos: item.GetPosition(),
							offset: clickStartPos - item.GetPosition(),
							isPrimary: false
						));
					}
				}
			}
			if(elements.ContainsKey(framework)) {
				Element clicked = elements[framework];
				if(!current.Select(c => c.Target).Contains(clicked)) {
					if(!MultiSelectionClicking) {
						current.Clear();
						Deselect();
						ClearResizePanel();
					}
					current.Add(new ElementReframeworkInfo(
						target: clicked,
						elementStartPos: clicked.GetPosition(),
						offset: clickStartPos - clicked.GetPosition(),
						isPrimary: true
					));
				} else {
					ElementReframeworkInfo? found = current.Find(c => c.Target == clicked);
					if(found != null) {
						found.IsPrimary = true;
					}
				}
			}

			Mouse.Capture(sender as UIElement);
			hasMoved = false;
			PutElementOnTop(element);
			if(e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Left;
			} else if(e.MouseDevice.RightButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Right;
			} else if(e.MouseDevice.MiddleButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Middle;
			}

		}

		private void MainCanvas_MouseMove(object sender, MouseEventArgs e) {
			FrameworkElement framework = (FrameworkElement)sender;
			if(elements.ContainsKey(framework)) {
				if(elements[framework].IsLocked) {
					return;
				}
			}
			if(e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
				hasMoved = true;
				Vector2 mouse_position = e.GetPosition(MainCanvas);

				foreach(ElementReframeworkInfo item in current) {
					item.Target.SetPosition(mouse_position - item.Offset);
					item.Target.UpdateConnectionsFrame();
				}
				ResizeFrame.Current?.UpdateResizeFrame();
			}
			if(multiSelectionDrag && !HasMouseMoved(e, _multiSelectionStartPos, true)) {
				multiSelectionFrame.Update(_multiSelectionStartPos, e.GetPosition(MainCanvas));
			}
		}

		private FrameworkElement? previous;
		private int clickCount = 0;
		private int lastClickTimeStamp;
		private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			FrameworkElement framework = (FrameworkElement)sender;
			if(elements.ContainsKey(framework)) {
				Element element = elements[framework];
				if(element.IsLocked) {
					ShowElementProperties(element);
					return;
				}
			}
			if(current.Count != 0) {
				Element? element = current.Find(c => c.IsPrimary)?.Target;
				if(element == null) {
					throw new Exception("it should not be null. check elements assignments");
				}
				if(clickStartPos != e.GetPosition(MainCanvas) && hasMoved) {
					editHistory.SubmitByElementFrameworkChanged(
						current.Select(i => new EditHistory.ElementFrameworkChange.FrameworkChangeItem() {
							Identity = i.Target.Identity,
							FromSize = i.Target.GetSize(),
							FromPosition = i.ElementStartPos,
							ToSize = i.Target.GetSize(),
							ToPosition = i.Target.GetPosition(),
						}).ToList(),
						EditHistory.FrameChangeType.Move
					);
				} else if(MultiSelectionClicking) {
					if(mouseType == MouseType.Left) {
						if(ResizeFrame.Current == null) {
							ResizeFrame.Create(this, element);
							element.SetConnectionsFrameVisible(false);
						} else {
							ResizeFrame.Current.AddElement(element);
						}
					}
				} else {
					FrameworkElement? currentFramework = current.Find(c => c.IsPrimary)?.Framework;
					switch(mouseType) {
						case MouseType.Left:
							ResizeFrame.Create(this, element);
							element.SetConnectionsFrameVisible(false);
							//Debug.WriteLine("left mouse");
							clickCount = previous == currentFramework && e.Timestamp - lastClickTimeStamp <= 500 ? clickCount + 1 : 0;
							lastClickTimeStamp = e.Timestamp;
							//Debug.WriteLine(clickCount);
							if(clickCount == 1) {
								//Debug.WriteLine("This is double click");
								element.DoubleClick();
								ShowElementProperties(element, true);
							} else {
								element.LeftClick(e);//care
								ShowElementProperties(element);
							}
							break;
						case MouseType.Middle:
							//Debug.WriteLine("middle mouse");
							element.MiddleClick();
							break;
						case MouseType.Right:
							//Debug.WriteLine("flyout menu");
							element.RightClick();
							break;
						default:
							throw new Exception($"Mouse Type Error {mouseType}");
					}
					previous = currentFramework;
				}
			}
			current.Clear();
			Mouse.Capture(null);
			backgroundMovementDrag = false;
			if(e.ChangedButton == MouseButton.Left && multiSelectionDrag) {
				multiSelectionDrag = false;
				//Debug.WriteLine("MOUSE UP IN CANVAS" + multiSelectionDrag);
				multiSelectionFrame.Disappear();
			}
		}

		private class ElementReframeworkInfo {
			public Element Target { get; set; }
			public FrameworkElement Framework => Target.Target;
			public Vector2 ElementStartPos { get; set; }
			public Vector2 Offset { get; set; }
			public bool IsPrimary { get; set; }

			public ElementReframeworkInfo(Element target, Vector2 elementStartPos, Vector2 offset, bool isPrimary = false) {
				Target = target;
				ElementStartPos = elementStartPos;
				Offset = offset;
				IsPrimary = isPrimary;
			}
		}

		private bool backgroundMovementDrag;
		private bool multiSelectionDrag;
		private Vector2 _multiSelectionStartPos;
		private Vector2 _dragStartPos;
		private Vector2 _translateStartPos;
		private bool HasMouseMoved(MouseEventArgs e, Vector2 startPos, bool canvasRoot) {
			return (e.GetPosition(canvasRoot ? MainCanvas : this) - _dragStartPos).Magnitude < 1;
		}

		private void BackgroundRectangle_MouseDown(object sender, MouseButtonEventArgs e) {
			_dragStartPos = e.GetPosition(this);
			_multiSelectionStartPos = e.GetPosition(MainCanvas);
			if(e.LeftButton == MouseButtonState.Pressed) {
				multiSelectionFrame.Appear();
				multiSelectionDrag = true;
			} else {
				_translateStartPos = new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y);
				backgroundMovementDrag = true;
			}
		}

		private void BackgroundRectangle_MouseMove(object sender, MouseEventArgs e) {
			if(e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released) {
				multiSelectionFrame.Disappear();
			}
			if(e.LeftButton == MouseButtonState.Pressed && multiSelectionDrag && !HasMouseMoved(e, _multiSelectionStartPos, true)) {
				//Debug.WriteLine("MOVE" + multiSelectionDrag);
				multiSelectionFrame.Update(_multiSelectionStartPos, e.GetPosition(MainCanvas));
			} else {
				if(!backgroundMovementDrag) {
					return;
				}
				Vector2 delta = e.GetPosition(this) - _dragStartPos;
				MainCanvas_TranslateTransform.X = delta.X + _translateStartPos.X;
				MainCanvas_TranslateTransform.Y = delta.Y + _translateStartPos.Y;

				UpdateBackgroundDot();
			}
			UpdateCoordiantionRules();
		}

		private void BackgroundRectangle_MouseUp(object sender, MouseButtonEventArgs e) {
			backgroundMovementDrag = false;
			ClearResizePanel();
			Deselect();
			current.Clear();
			Mouse.Capture(null);
			multiSelectionFrame.Disappear();
			if(e.ChangedButton == MouseButton.Left && multiSelectionDrag && !HasMouseMoved(e, _dragStartPos, true)) {
				multiSelectionDrag = false;
				List<Element> selected = multiSelectionFrame.GetSelected(elements.Select(e => e.Value).Where(e => !e.IsLocked));
				if(selected.Count == 0) {
					ClearResizePanel();
				} else {
					ResizeFrame.Create(this, selected.ToArray());
				}
			}
		}

		private void MainCanvas_Loaded(object sender, RoutedEventArgs e) {
			//UpdateBackgroundDot();
		}

		private long lastClickTick = 0;
		private void TabItemHeader_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			if(DateTime.Now.Ticks - lastClickTick < 2500000) {
				if(RightTabControl.Width > 100) {
					RightTabControl.Width = 20;
				} else {
					RightTabControl.Width = 280;
				}
				RightTabControl.UpdateLayout();
				//UpdateBackgroundDot();
			}
			lastClickTick = DateTime.Now.Ticks;
		}

		private void MapElementsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}

		private void RefreshMapElementsListView() {
			//foreach(object item in MainCanvas.Children) {
			//	MapElementsListView.Items.Add(item.ToString());
			//}
			List<ElementListViewItem> items = new();
			foreach(Element item in elements.Values) {
				items.Add(new ElementListViewItem(item));
			}
			MapElementsListView.ItemsSource = items;
		}

		public void SetTestPoints(bool clear, params Vector2[] points) {
			if(clear) {
				TestCanvas.Children.Clear();
			}
			foreach(Vector2 point in points) {
				Ellipse ellipse = new() {
					Fill = Brushes.Salmon,
					Height = 10,
					Width = 10,
				};
				Canvas.SetLeft(ellipse, point.X - 5);
				Canvas.SetTop(ellipse, point.Y - 5);
				TestCanvas.Children.Add(ellipse);
			}
		}

		private enum MouseType {
			Left, Middle, Right
		}

		private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
			UpdateBackgroundDot();
			UpdateCoordiantionRules();
		}

		private void MapElementsListViewItemGrid_MouseDown(object sender, MouseButtonEventArgs e) {
			if(sender is not Grid grid || grid.Tag is not Element element) {
				return;
			}
			if(e.ChangedButton == MouseButton.Left) {
				ClearResizePanel();
				Deselect();
				PutElementOnTop(element);
				if(e.ClickCount >= 2) {
					ResizeFrame.Create(this, element);
					ShowElementProperties(element);
				}
			}

		}

		private void MapElementsMenuItemSelect_Click(object sender, RoutedEventArgs e) {
			if(sender is not MenuItem item || item.Tag is not Element element) {
				return;
			}
			PutElementOnTop(element);
			ResizeFrame.Create(this, element);
			ShowElementProperties(element);
		}

		private async void AddNewGroupButton_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null) {
				return;
			}
			const int MAX_WHILE_LIMIT = 100;//make sure no duplicate name
			string groupName;
			int count = AppSettings.Current.ElementPresetsGroups.Count + 1;
			int triedCount = 0;
			do {
				groupName = $"New Group {count++}";
				triedCount++;
			} while(AppSettings.Current.ElementPresetsGroups.Select(g => g.Name).Contains(groupName) && triedCount < MAX_WHILE_LIMIT);
			if(AppSettings.Current.ElementPresetsGroups.Select(g => g.Name).Contains(groupName)) {
				groupName = $"New Group {new string(DateTime.Now.Ticks.ToString().TakeLast(5).ToArray())}";
			}
			var group = new ElementPresetsGroup(groupName);
			AppSettings.Current.ElementPresetsGroups.Add(group);
			int index = ElementsPresetsPanel.Children.IndexOf(AddNewGroupButton);
			var groupView = new ElementPresetsGroupView(this, group);
			ElementsPresetsPanel.Children.Insert(index, groupView);

			await Local.SaveAppSettings();
		}
	}

	public class ElementListViewItem {
		public Element Item { get; set; }
		public string Icon => Item.Icon.icon;
		public string FontFamily => Item.Icon.fontFamily;
		public string Title => Item.Identity.Name;
		public ElementListViewItem(Element item) {
			Item = item;
		}
	}
}