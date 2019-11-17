using UnityEngine;


namespace Footsies {
	public class BoxBase {
		public Rect Rect;

		public float XMin => Rect.x - Rect.width / 2;
		public float XMax => Rect.x + Rect.width / 2;
		public float YMin => Rect.y;
		public float YMax => Rect.y + Rect.height;

		public bool Overlaps(BoxBase otherBox) {
			bool c1 = otherBox.XMax >= XMin;
			bool c2 = otherBox.XMin <= XMax;
			bool c3 = otherBox.YMax >= YMin;
			bool c4 = otherBox.YMin <= YMax;

			return c1 && c2 && c3 && c4;
		}
	}
}