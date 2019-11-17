using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649

namespace Footsies {
	/// <summary>
	/// Fighter data. Contain status and motion, attack and action data
	/// </summary>
	[CreateAssetMenu]
	public class FighterData : ScriptableObject {
		public int startGuardHealth = 3;

		public float forwardMoveSpeed = 2.2f;
		public float backwardMoveSpeed = 1.8f;

		public int dashAllowFrame = 10;

		public int specialAttackHoldFrame = 60;

		public bool canCancelOnWhiff;

		[SerializeField] public Rect baseHurtBoxRect;

		[SerializeField] public Rect basePushBoxRect;

		[SerializeField] private ActionDataContainer actionDataContainer;

		[SerializeField] private AttackDataContainer attackDataContainer;

		[SerializeField] private MotionDataContainer motionDataContainer;

		public Dictionary<int, ActionData> Actions => actions;

		private Dictionary<int, ActionData> actions = new Dictionary<int, ActionData>();

		public Dictionary<int, AttackData> AttackData => attackData;

		private Dictionary<int, AttackData> attackData = new Dictionary<int, AttackData>();

		public Dictionary<int, MotionData> MotionData => motionData;

		private Dictionary<int, MotionData> motionData = new Dictionary<int, MotionData>();

		public void SetupDictionary() {
			if (actionDataContainer == null) {
				Debug.LogError("ActionDataContainer is not set");
				return;
			} else if (attackDataContainer == null) {
				Debug.LogError("ActionDataContainer is not set");
				return;
			}

			actions = new Dictionary<int, ActionData>();
			foreach (ActionData action in actionDataContainer.actions) {
				actions.Add(action.actionID, action);
			}

			attackData = new Dictionary<int, AttackData>();
			foreach (AttackData data in attackDataContainer.attackDataList) {
				attackData.Add(data.attackID, data);
			}

			motionData = new Dictionary<int, MotionData>();
			foreach (MotionData data in motionDataContainer.motionDataList) {
				motionData.Add(data.motionID, data);
			}
		}
	}
}