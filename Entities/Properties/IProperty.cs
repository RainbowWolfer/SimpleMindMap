using MindMap.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

//Debug.WriteLine("Start");
//_BaseProperty p = new();
//p.color.control = new ColorPicker();
//p.color.ControlBack += c => {
//	if(c is ColorPicker cp) {
//		cp.SelectedColor = p.color.ActualValue;//??
//	}
//};

namespace MindMap.Entities.Properties {
	public interface IProperty: ICloneable {
		public IProperty Translate(string json);
	}

	public class _BaseProperty: _Property {
		public readonly Value<string> text = new("Hello World");
		public readonly Value<Color> color = new(Colors.Red);


	}

	public abstract class _Property {
		public void SubmitChanges() {

		}
		public _Property? previous;
	}

	public class Value<T> {
		public T PreviewValue { get; private set; }
		public T ActualValue { get; private set; }
		public Control? control;//should it be sealed to a class?
		public Action<Control>? ControlBack;
		public Value(T value) {
			this.PreviewValue = value;
			this.ActualValue = value;
		}
		public void UpdatePreview(T value) {
			PreviewValue = value;
			ControlCallBack();
		}
		public void SubmitValue(T value) {
			ActualValue = value;
			ControlCallBack();
		}
		private void ControlCallBack() {
			if(control != null) {
				ControlBack?.Invoke(control);
			}
		}

		public override bool Equals(object? obj) {
			return obj is Value<T> v && v.ActualValue != null && v.ActualValue.Equals(ActualValue);
		}

		public override int GetHashCode() => base.GetHashCode();
	}
}
