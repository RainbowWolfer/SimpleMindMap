using MindMap.Entities.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MindMap.Entities.Elements.Interfaces {
	public interface IAnnotation: ITextContainer {
		TextBlock AnnotationTextBlock { get; set; }
		Direction Direction { get; set; }
	}
}
