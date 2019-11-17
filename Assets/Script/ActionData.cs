using System.Collections.Generic;
using UnityEngine;

namespace Footsies {
	public abstract class FrameDataBase {
		public Vector2Int StartEndFrame;
	}

	[System.Serializable]
	public class MotionFrameData : FrameDataBase {
		public int motionID;
	}

	[System.Serializable]
	public class StatusData : FrameDataBase {
		public bool counterHit;
	}


	[System.Serializable]
	public class HitboxData : FrameDataBase {
		public Rect rect;
		public int attackID;
		public bool proximity;
	}

	[System.Serializable]
	public class HurtboxData : FrameDataBase {
		public Rect rect;
		public bool useBaseRect;
	}

	[System.Serializable]
	public class PushboxData : FrameDataBase {
		public Rect rect;
		public bool useBaseRect;
	}

	[System.Serializable]
	public class MovementData : FrameDataBase {
		public float velocity_x;
	}

	[System.Serializable]
	public class CancelData : FrameDataBase {
		public bool buffer;
		public bool execute;
		public List<int> actionID = new List<int>();
	}

	public enum ActionType {
		Movement,
		Attack,
		Damage,
		Guard,
	}

	[CreateAssetMenu]
	public class ActionData : ScriptableObject {
		public int actionID;
		public string actionName;
		public ActionType Type;
		public int frameCount;
		public bool isLoop;
		public int loopFromFrame;
		public MotionFrameData[] motions;
		public StatusData[] status;
		public HitboxData[] hitboxes;
		public HurtboxData[] hurtboxes;
		public PushboxData[] pushboxes;
		public MovementData[] movements;
		public CancelData[] cancels;
		public bool alwaysCancelable;
		public AudioClip audioClip;

		public MotionFrameData GetMotionData(int frame) {
			foreach (MotionFrameData data in motions) {
				if (frame >= data.StartEndFrame.x && frame <= data.StartEndFrame.y) {
					return data;
				}
			}

			return null;
		}

		public StatusData GetStatusData(int frame) {
			foreach (StatusData data in status) {
				if (frame >= data.StartEndFrame.x && frame <= data.StartEndFrame.y) {
					return data;
				}
			}

			return null;
		}

		public List<HitboxData> GetHitboxData(int frame) {
			List<HitboxData> hb = new List<HitboxData>();

			foreach (HitboxData data in hitboxes) {
				if (frame >= data.StartEndFrame.x && frame <= data.StartEndFrame.y) {
					hb.Add(data);
				}
			}

			return hb;
		}

		public List<HurtboxData> GetHurtboxData(int frame) {
			List<HurtboxData> hb = new List<HurtboxData>();

			foreach (HurtboxData data in hurtboxes) {
				if (frame >= data.StartEndFrame.x && frame <= data.StartEndFrame.y) {
					hb.Add(data);
				}
			}

			return hb;
		}

		public PushboxData GetPushboxData(int frame) {
			foreach (PushboxData data in pushboxes) {
				if (frame >= data.StartEndFrame.x && frame <= data.StartEndFrame.y) {
					return data;
				}
			}

			return null;
		}

		public MovementData GetMovementData(int frame) {
			foreach (MovementData data in movements) {
				if (frame >= data.StartEndFrame.x && frame <= data.StartEndFrame.y) {
					return data;
				}
			}

			return null;
		}

		public List<CancelData> GetCancelData(int frame) {
			List<CancelData> cd = new List<CancelData>();

			foreach (CancelData data in cancels) {
				if (frame >= data.StartEndFrame.x && frame <= data.StartEndFrame.y) {
					cd.Add(data);
				}
			}

			return cd;
		}
	}
}