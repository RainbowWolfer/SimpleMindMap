using MindMap.Entities.Elements;

namespace MindMap.Entities.Tags {
	public class ElementFrameworkTag {
		public Element Target { get; set; }

		public ElementFrameworkTag(Element element) {
			Target = element;
		}
	}
}
