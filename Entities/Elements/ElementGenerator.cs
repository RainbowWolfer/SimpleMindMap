using MindMap.Entities.Elements.TextShapes;
using MindMap.Entities.Identifications;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Elements {
	public static class ElementGenerator {
		public const long ID_Rectangle = 1;
		public const long ID_Ellipse = 2;
		public const long ID_Polygon = 3;
		public const long ID_Image = 4;

		public static Element GetElement(MindMapPage? page, long type_id, Identity? identity) {
			return type_id switch {
				ID_Rectangle => new MyRectangle(page, identity),
				ID_Ellipse => new MyEllipse(page, identity),
				ID_Polygon => new MyPolygon(page, identity),
				ID_Image => new MyImage(page, identity),
				_ => throw new Exception($"ID({type_id}) Not Found"),
			};
		}

		public static string GetTypeDefaultName(long typeID) {
			return typeID switch {
				ID_Rectangle => "Rectangle",
				ID_Ellipse => "Ellipse",
				ID_Polygon => "Polygon",
				ID_Image => "Image",
				_ => $"Type {typeID} not found",
			};
		}
	}
}
