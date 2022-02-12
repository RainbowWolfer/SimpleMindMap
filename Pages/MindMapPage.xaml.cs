using MindMap.Entities;
using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
using MindMap.Entities.Locals;
using MindMap.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MindMap.Pages {
	public partial class MindMapPage: Page {//Editor Page
		public readonly ConnectionsManager connectionsManager;
		public readonly EditHistory editHistory;
		public bool holdShift;
		private string _path = "";
		private string fileName = "(Not Saved)";
		private bool elementsChanged = false;

		public string FileName {
			get => fileName;
			set {
				fileName = value;
				FileNameText.Text = FileName + (ElementsChanged ? "*" : "");
			}
		}

		public MindMapPage() {
			InitializeComponent();
			connectionsManager = new ConnectionsManager(this);
			editHistory = new EditHistory(this);
			editHistory.OnHistoryChanged += EditHistory_OnHistoryChanged;
			editHistory.OnUndo += EditHistory_OnUndo;
			editHistory.OnRedo += EditHistory_OnRedo;

			MainCanvas.MouseMove += MainCanvas_MouseMove;
			MainCanvas.MouseUp += MainCanvas_MouseUp;

			SizeChanged += (s, e) => UpdateBackgroundDot();

			HideElementProperties();

			FileNameText.Text = fileName;
			CreatedDateText.Text = $"Created Date ({DateTime.Now})";

			SavingPanel.Visibility = Visibility.Collapsed;
			LoadingPanel.Visibility = Visibility.Collapsed;

			MainWindow.Instance?.KeyManager.Register(
				() => holdShift = true,
				() => holdShift = false,
			false, Key.LeftShift);
			MainWindow.Instance?.KeyManager.Register(
				() => Save(),
				() => { },
			true, Key.LeftCtrl, Key.S);
			MainWindow.Instance?.KeyManager.Register(
				() => Undo(),
				() => { },
			false, Key.LeftCtrl, Key.Z);
			MainWindow.Instance?.KeyManager.Register(
				() => Redo(),
				() => { },
			false, Key.LeftCtrl, Key.Y);
		}

		private void EditHistory_OnUndo(EditHistory.IChange obj) {
			UpdateHistoryListView();
		}

		private void EditHistory_OnRedo(EditHistory.IChange obj) {
			UpdateHistoryListView();
		}

		private void EditHistory_OnHistoryChanged(List<EditHistory.IChange> history) {
			UpdateHistoryListView(history);
		}

		private void UpdateHistoryListView(List<EditHistory.IChange>? history = null) {
			EditHistoryListView.Items.Clear();
			foreach(var item in history ?? editHistory.GetHistory()) {
				Grid grid = new() {
					Tag = item,
				};
				grid.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(1, GridUnitType.Auto),
				});
				grid.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(1, GridUnitType.Star),
				});
				grid.Children.Add(item.Icon.Generate());
				TextBlock text = new() {
					Text = item.ToString(),
					Margin = new Thickness(10, 0, 0, 0),
					TextWrapping = TextWrapping.Wrap,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Width = 230,
				};
				Grid.SetColumn(text, 1);
				grid.Children.Add(text);
				ToolTipService.SetToolTip(grid, item.Date);
				EditHistoryListView.Items.Add(grid);
			}
		}

		private void EditHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is Grid grid && grid.Tag is EditHistory.IChange change) {
				UpdateEditHistoryDetail(change.GetDetail());
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

		public async void Save() {
			SavingPanel.Visibility = Visibility.Visible;
			_path = await Local.Save(elements.Values.ToList(), connectionsManager, _path);
			FileName = _path[(_path.LastIndexOf('\\') + 1)..];
			ElementsChanged = false;
			//set window title
			//set created time
			SavingPanel.Visibility = Visibility.Collapsed;
			SetSetOprationHintText("Saved Successfully");
		}

		public async void Load(Local.MapInfo mapInfo, FileInfo fileInfo) {
			LoadingPanel.Visibility = Visibility.Visible;
			FileName = fileInfo.FileName;
			_path = fileInfo.FilePath;
			CreatedDateText.Text = $"Created Date ({fileInfo.CreatedDate})";

			foreach(Local.ElementInfo ele in mapInfo.elements) {
				AddToElementsDictionary(ele.type_id switch {
					Element.ID_Rectangle => new MyRectangle(this, ele.element_id, ele.propertyJson),
					Element.ID_Ellipse => new MyEllipse(this, ele.element_id, ele.propertyJson),
					Element.ID_Polygon => new MyPolygon(this, ele.element_id, ele.propertyJson),
					_ => throw new Exception($"ID{ele.type_id} Not Found"),
				}, ele.position, ele.size);
				await Task.Delay(1);
			}
			foreach(Local.ConnectionInfo item in mapInfo.connections) {
				Element? from = elements.Select(e => e.Value).ToList().Find(i => i.ID == item.from_parent_id);
				ConnectionControl? fromDot = from?.GetConnectionControlByID(item.from_dot_id);
				Element? to = elements.Select(e => e.Value).ToList().Find(i => i.ID == item.to_parent_id);
				ConnectionControl? toDot = to?.GetConnectionControlByID(item.to_dot_id);
				if(from == null || to == null || fromDot == null || toDot == null) {
					continue;
				}
				connectionsManager.Add(fromDot, toDot, item.propertyJson);
				await Task.Delay(1);
			}
			LoadingPanel.Visibility = Visibility.Collapsed;
			SetSetOprationHintText("Loaded Successfully");
		}

		public void Redo() => editHistory.Redo();

		public void Undo() => editHistory.Undo();

		private void DebugButton_Click(object sender, RoutedEventArgs e) {
			connectionsManager.DebugConnections();
		}

		private static ResizeFrame? Selection => ResizeFrame.Current;

		public bool ElementsChanged {
			get => elementsChanged;
			set {
				elementsChanged = value;
				FileNameText.Text = FileName + (ElementsChanged ? "*" : "");
			}
		}

		public void ClearResizePanel() {
			Selection?.ClearResizeFrame(MainCanvas);
			if(previous != null && elements.ContainsKey(previous)) {
				elements[previous].SetConnectionsFrameVisible(true);
				elements[previous].UpdateConnectionsFrame();
			}
		}

		public void Deselect(bool includePath = true) {
			if(Selection != null && elements.ContainsKey(Selection.target)) {
				elements[Selection.target].Deselect();
			}
			if(includePath) {
				connectionsManager.CurrentSelection?.Deselect();
			}
			HideElementProperties();
		}

		private ConnectionPath? previewLine;

		public void UpdatePreviewLine(ConnectionControl from, Vector2 to) {
			if(previewLine == null) {
				previewLine = new ConnectionPath(this, MainCanvas, from, to);
			}
			previewLine.Update(to);
		}

		public void ClearPreviewLine() {
			if(previewLine == null) {
				return;
			}
			MainCanvas.Children.Remove(previewLine.Path);
			previewLine = null;
		}

		private readonly Dictionary<Vector2, Shape> backgroundPool = new();
		private const int SIZE = 4;
		private const int GAP = 20;
		private void UpdateBackgroundDot() {
			var offset = -new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y).ToInt() / GAP;
			var size = new Vector2(MainCanvas.ActualWidth, MainCanvas.ActualHeight).ToInt() / GAP;
			for(int i = offset.X_Int; i < size.X + offset.X; i++) {
				for(int j = offset.Y_Int; j < size.Y + offset.Y; j++) {
					if(backgroundPool.ContainsKey(new Vector2(i, j))) {
						continue;
					}
					Ellipse t = new() {
						Width = SIZE,
						Height = SIZE,
						Fill = new SolidColorBrush(Colors.Gray),
					};
					Canvas.SetLeft(t, i * GAP);
					Canvas.SetTop(t, j * GAP);
					backgroundPool.Add(new Vector2(i, j), t);
					BackgroundCanvas.Children.Add(t);
				}
			}
		}

		public readonly Dictionary<FrameworkElement, Element> elements = new();

		public void ShowElementProperties(Element element) {
			ElementPropertiesPanel.Children.Clear();
			foreach(Panel item in element.CreatePropertiesList()) {
				ElementPropertiesPanel.Children.Add(item);
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

		public void AddElementFromHistory(Element target) {
			AddToElementsDictionary(value: target,
				position: target.GetPosition(),
				size: target.GetSize(),
				property: target.Properties,
				submitEditHistory: false
			);
		}

		private void AddToElementsDictionary(Element value, Vector2 position, Vector2 size, IProperty? property = null, bool submitEditHistory = true) {
			value.SetPosition(position);
			value.SetSize(size == default ? value.DefaultSize : size);
			value.SetFramework();
			if(property != null) {
				value.SetProperty(property);
			}
			value.CreateConnectionsFrame();
			value.CreateFlyoutMenu();
			value.Target.MouseDown += Element_MouseDown;
			elements.Add(value.Target, value);
			if(value is IUpdate update) {
				update.Update();
			}
			if(submitEditHistory) {
				editHistory.SubmitByElementCreated(value);
			}
		}

		private void AddElement(Type type) {
			if(type == typeof(MyRectangle)) {
				AddToElementsDictionary(new MyRectangle(this), Vector2.Zero, default);
			} else if(type == typeof(MyEllipse)) {
				AddToElementsDictionary(new MyEllipse(this), Vector2.Zero, default);
			} else if(type == typeof(MyPolygon)) {
				AddToElementsDictionary(new MyPolygon(this), Vector2.Zero, default);
			}
			UpdateCount();
		}

		public void RemoveElement(Element element) {
			if(!elements.ContainsKey(element.Target) || !MainCanvas.Children.Contains(element.Target)) {
				return;
			}
			Deselect();
			ClearResizePanel();
			elements.Remove(element.Target);
			MainCanvas.Children.Remove(element.Target);
			UpdateCount();
		}

		private void Element_MouseDown(object sender, MouseButtonEventArgs e) {
			FrameworkElement? target = sender as FrameworkElement;
			current = target;
			if(current != Selection?.target) {
				ClearResizePanel();
				Deselect();
			}
			startPos = e.GetPosition(MainCanvas);
			offset = startPos - new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target));
			elementStartPos = new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target));
			Mouse.Capture(sender as UIElement);
			hasMoved = false;
			if(e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Left;
			} else if(e.MouseDevice.RightButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Right;
			} else if(e.MouseDevice.MiddleButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Middle;
			}

		}

		private void AddRectableButton_Click(object sender, RoutedEventArgs e) {
			AddElement(typeof(MyRectangle));
		}

		private void AddEllipseButton_Click(object sender, RoutedEventArgs e) {
			AddElement(typeof(MyEllipse));
		}

		private void AddPolygonButton_Click(object sender, RoutedEventArgs e) {
			AddElement(typeof(MyPolygon));
		}

		private Vector2 offset;
		private Vector2 startPos;//check for click
		private Vector2 elementStartPos;
		private FrameworkElement? current;
		private bool hasMoved;
		private MouseType mouseType;
		private void MainCanvas_MouseMove(object sender, MouseEventArgs e) {
			if(current == null || e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
				return;
			}
			hasMoved = true;
			Vector2 mouse_position = e.GetPosition(MainCanvas);

			Canvas.SetLeft(current, mouse_position.X - offset.X);
			Canvas.SetTop(current, mouse_position.Y - offset.Y);

			Selection?.UpdateResizeFrame();

			if(current != null && elements.ContainsKey(current)) {
				elements[current].UpdateConnectionsFrame();
			}
		}

		private FrameworkElement? previous;
		private int clickCount = 0;
		private int lastClickTimeStamp;
		private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			if(current != null) {
				Element? element = elements.ContainsKey(current) ? elements[current] : null;
				if(element == null) {
					throw new Exception("it should not be null. check elements assignments");
				}
				if(startPos != e.GetPosition(MainCanvas) && hasMoved) {
					//Debug.WriteLine($"{elementStartPos.ToString(2)} - {element.GetPosition().ToString(2)}");
					editHistory.SubmitByElementPositionChanged(element, elementStartPos, element.GetPosition());
				} else {
					switch(mouseType) {
						case MouseType.Left:
							ResizeFrame.Create(this, current, element);
							element.SetConnectionsFrameVisible(false);
							Debug.WriteLine("left mouse");
							clickCount = previous == current && e.Timestamp - lastClickTimeStamp <= 500 ? clickCount + 1 : 0;
							lastClickTimeStamp = e.Timestamp;
							Debug.WriteLine(clickCount);
							if(clickCount == 1) {
								Debug.WriteLine("This is double click");
								element.DoubleClick();
							} else {
								element.LeftClick();//care
								ShowElementProperties(element);
							}
							break;
						case MouseType.Middle:
							Debug.WriteLine("middle mouse");
							element.MiddleClick();
							break;
						case MouseType.Right:
							Debug.WriteLine("flyout menu");
							element.RightClick();
							break;
						default:
							throw new Exception($"Mouse Type Error {mouseType}");
					}
					previous = current;
				}
			}
			current = null;
			Mouse.Capture(null);
			_drag = false;
		}

		private bool _drag;
		private Vector2 _dragStartPos;
		private Vector2 _translatStartPos;
		private void BackgroundRectangle_MouseDown(object sender, MouseButtonEventArgs e) {
			_dragStartPos = e.GetPosition(this);
			_translatStartPos = new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y);
			_drag = true;
		}

		private void BackgroundRectangle_MouseMove(object sender, MouseEventArgs e) {
			if(!_drag) {
				return;
			}
			Vector2 delta = e.GetPosition(this) - _dragStartPos;
			MainCanvas_TranslateTransform.X = delta.X + _translatStartPos.X;
			MainCanvas_TranslateTransform.Y = delta.Y + _translatStartPos.Y;

			UpdateBackgroundDot();
		}

		private void BackgroundRectangle_MouseUp(object sender, MouseButtonEventArgs e) {
			_drag = false;
			ClearResizePanel();
			Deselect();
			current = null;
			Mouse.Capture(null);
		}

		private void MainCanvas_Loaded(object sender, RoutedEventArgs e) {
			UpdateBackgroundDot();
		}

		//not working as intended
		private void TabItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if(RightTabControl.Width > 100) {
				RightTabControl.Width = 20;
			} else {
				RightTabControl.Width = 280;
			}
			RightTabControl.UpdateLayout();
			UpdateBackgroundDot();
		}

		private enum MouseType {
			Left, Middle, Right
		}
	}
}