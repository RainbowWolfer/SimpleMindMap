using MindMap.Entities.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMap.Entities.Elements.Interfaces {
	public interface IAnnotation: ITextContainer, IUpdate {
		Grid AnnotationGrid { get; set; }
		TranslateTransform GridTransform { get; set; }
		TextBlock AnnotationTextBlock { get; set; }
		TextBox AnnotationTextBox { get; set; }
		Direction Direction { get; set; }
		void SubmitTextChange();
		void ShowTextBox();
		void ShowTextBlock();
	}
}
