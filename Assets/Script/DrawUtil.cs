﻿using UnityEngine;

namespace Footsies {
	public static class DrawUtil {
		//****************************************************************************************************
		//  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color, width : float) : void
		//  
		//  Draws a GUI line on the screen.
		//  
		//  DrawLine makes up for the severe lack of 2D line rendering in the Unity runtime GUI system.
		//  This function works by drawing a 1x1 texture filled with a color, which is then scaled
		//   and rotated by altering the GUI matrix.  The matrix is restored afterwards.
		//****************************************************************************************************
		private static Texture2D lineTex;
		private static Texture2D rectText;

		public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width) {
			// Generate a single pixel texture if it doesn't exist
			if (!lineTex) {
				lineTex = new Texture2D(1, 1);
			}

			Matrix4x4 saveMatrix = GUI.matrix;
			Color saveColor = GUI.color;
			GUI.color = color;

			Vector2 delta = pointB - pointA;
			GUIUtility.ScaleAroundPivot(new Vector2(delta.magnitude, width), Vector2.zero);
			GUIUtility.RotateAroundPivot(Vector2.Angle(delta, Vector2.right) * Mathf.Sign(delta.y), Vector2.zero);
			GUI.matrix = Matrix4x4.TRS(pointA, Quaternion.identity, Vector3.one) * GUI.matrix;

			GUI.DrawTexture(new Rect(Vector2.zero, Vector2.one), lineTex);

			GUI.matrix = saveMatrix;
			GUI.color = saveColor;
		}

		public static void DrawRect(Rect rect, Color color) {
			// Generate a single pixel texture if it doesn't exist
			if (!rectText) {
				rectText = new Texture2D(1, 1);
			}

			// Store current GUI color, so we can switch it back later,
			// and set the GUI color to the color parameter
			Color savedColor = GUI.color;
			GUI.color = color;

			// Finally, draw the actual line.
			// We're really only drawing a 1x1 texture from pointA.
			// The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
			//  render with the proper width, length, and angle.
			GUI.DrawTexture(rect, rectText);

			// We're done.  Restore GUI color to whatever they were before.
			GUI.color = savedColor;
		}
	}
}