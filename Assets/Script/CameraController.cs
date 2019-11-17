using UnityEngine;

namespace Footsies {
	public class CameraController : MonoBehaviour {
		private void Awake() {
			DontDestroyOnLoad(gameObject);
		}
	}
}