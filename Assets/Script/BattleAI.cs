using System.Collections.Generic;
using UnityEngine;

namespace Footsies {
	public class BattleAI {
		private class FightState {
			public float DistanceX;
			public bool IsOpponentDamage;
			public bool IsOpponentGuardBreak;
			public bool IsOpponentBlocking;
			public bool IsOpponentNormalAttack;
			public bool IsOpponentSpecialAttack;
		}

		private BattleCore battleCore;

		private Queue<int> moveQueue = new Queue<int>();
		private Queue<int> attackQueue = new Queue<int>();

		// previous fight state data
		private FightState[] fightStates = new FightState[MaxFightStateRecord];
		public static readonly uint MaxFightStateRecord = 10;
		private int fightStateReadIndex = 5;

		public BattleAI(BattleCore core) {
			battleCore = core;
		}

		public int GetNextAIInput() {
			int input = 0;

			UpdateFightState();
			FightState fightState = GetCurrentFightState();
			if (fightState != null) {
				//Globals.Logger.Log(fightState.distanceX);
				if (moveQueue.Count > 0) {
					input |= moveQueue.Dequeue();
				} else if (moveQueue.Count == 0) {
					SelectMovement(fightState);
				}

				if (attackQueue.Count > 0) {
					input |= attackQueue.Dequeue();
				} else if (attackQueue.Count == 0) {
					SelectAttack(fightState);
				}
			}

			return input;
		}

		private void SelectMovement(FightState fightState) {
			if (fightState.DistanceX > 4f) {
				int rand = Random.Range(0, 2);
				if (rand == 0) {
					AddFarApproach1();
				} else {
					AddFarApproach2();
				}
			} else if (fightState.DistanceX > 3f) {
				int rand = Random.Range(0, 7);
				if (rand <= 1) {
					AddMidApproach1();
				} else if (rand <= 3) {
					AddMidApproach2();
				} else if (rand == 4) {
					AddFarApproach1();
				} else if (rand == 5) {
					AddFarApproach2();
				} else {
					AddNeutralMovement();
				}
			} else if (fightState.DistanceX > 2.5f) {
				int rand = Random.Range(0, 5);
				if (rand == 0) {
					AddMidApproach1();
				} else if (rand == 1) {
					AddMidApproach2();
				} else if (rand == 2) {
					AddFallBack1();
				} else if (rand == 3) {
					AddFallBack2();
				} else {
					AddNeutralMovement();
				}
			} else if (fightState.DistanceX > 2f) {
				int rand = Random.Range(0, 4);
				if (rand == 0) {
					AddFallBack1();
				} else if (rand == 1) {
					AddFallBack2();
				} else {
					AddNeutralMovement();
				}
			} else {
				int rand = Random.Range(0, 3);
				if (rand == 0) {
					AddFallBack1();
				} else if (rand == 1) {
					AddFallBack2();
				} else {
					AddNeutralMovement();
				}
			}
		}

		private void SelectAttack(FightState fightState) {
			if (fightState.IsOpponentDamage
			    || fightState.IsOpponentGuardBreak
			    || fightState.IsOpponentSpecialAttack) {
				AddTwoHitImmediateAttack();
			} else if (fightState.DistanceX > 4f) {
				int rand = Random.Range(0, 4);
				if (rand <= 3) {
					AddNoAttack();
				} else {
					AddDelaySpecialAttack();
				}
			} else if (fightState.DistanceX > 3f) {
				if (fightState.IsOpponentNormalAttack) {
					AddTwoHitImmediateAttack();
					return;
				}

				int rand = Random.Range(0, 5);
				if (rand <= 1) {
					AddNoAttack();
				} else if (rand <= 3) {
					AddOneHitImmediateAttack();
				} else {
					AddDelaySpecialAttack();
				}
			} else if (fightState.DistanceX > 2.5f) {
				int rand = Random.Range(0, 3);
				if (rand == 0) {
					AddNoAttack();
				} else if (rand == 1) {
					AddOneHitImmediateAttack();
				} else {
					AddTwoHitImmediateAttack();
				}
			} else if (fightState.DistanceX > 2f) {
				int rand = Random.Range(0, 6);
				if (rand <= 1) {
					AddOneHitImmediateAttack();
				} else if (rand <= 3) {
					AddTwoHitImmediateAttack();
				} else if (rand == 4) {
					AddImmediateSpecialAttack();
				} else {
					AddDelaySpecialAttack();
				}
			} else {
				int rand = Random.Range(0, 3);
				if (rand == 0) {
					AddOneHitImmediateAttack();
				} else {
					AddTwoHitImmediateAttack();
				}
			}
		}

		private void AddNeutralMovement() {
			for (int i = 0; i < 30; i++) {
				moveQueue.Enqueue(0);
			}

			Globals.Logger.Log("AddNeutral");
		}

		private void AddFarApproach1() {
			AddForwardInputQueue(40);
			AddBackwardInputQueue(10);
			AddForwardInputQueue(30);
			AddBackwardInputQueue(10);

			Globals.Logger.Log("AddFarApproach1");
		}

		private void AddFarApproach2() {
			AddForwardDashInputQueue();
			AddBackwardInputQueue(25);
			AddForwardDashInputQueue();
			AddBackwardInputQueue(25);

			Globals.Logger.Log("AddFarApproach2");
		}

		private void AddMidApproach1() {
			AddForwardInputQueue(30);
			AddBackwardInputQueue(10);
			AddForwardInputQueue(20);
			AddBackwardInputQueue(10);

			Globals.Logger.Log("AddMidApproach1");
		}

		private void AddMidApproach2() {
			AddForwardDashInputQueue();
			AddBackwardInputQueue(30);

			Globals.Logger.Log("AddMidApproach2");
		}

		private void AddFallBack1() {
			AddBackwardInputQueue(60);

			Globals.Logger.Log("AddFallBack1");
		}

		private void AddFallBack2() {
			AddBackwardDashInputQueue();
			AddBackwardInputQueue(60);

			Globals.Logger.Log("AddFallBack2");
		}

		private void AddNoAttack() {
			for (int i = 0; i < 30; i++) {
				attackQueue.Enqueue(0);
			}

			Globals.Logger.Log("AddNoAttack");
		}

		private void AddOneHitImmediateAttack() {
			attackQueue.Enqueue(GetAttackInput());
			for (int i = 0; i < 18; i++) {
				attackQueue.Enqueue(0);
			}

			Globals.Logger.Log("AddOneHitImmediateAttack");
		}

		private void AddTwoHitImmediateAttack() {
			attackQueue.Enqueue(GetAttackInput());
			for (int i = 0; i < 3; i++) {
				attackQueue.Enqueue(0);
			}

			attackQueue.Enqueue(GetAttackInput());
			for (int i = 0; i < 18; i++) {
				attackQueue.Enqueue(0);
			}

			Globals.Logger.Log("AddTwoHitImmediateAttack");
		}

		private void AddImmediateSpecialAttack() {
			for (int i = 0; i < 60; i++) {
				attackQueue.Enqueue(GetAttackInput());
			}

			attackQueue.Enqueue(0);

			Globals.Logger.Log("AddImmediateSpecialAttack");
		}

		private void AddDelaySpecialAttack() {
			for (int i = 0; i < 120; i++) {
				attackQueue.Enqueue(GetAttackInput());
			}

			attackQueue.Enqueue(0);

			Globals.Logger.Log("AddDelaySpecialAttack");
		}

		private void AddForwardInputQueue(int frame) {
			for (int i = 0; i < frame; i++) {
				moveQueue.Enqueue(GetForwardInput());
			}
		}

		private void AddBackwardInputQueue(int frame) {
			for (int i = 0; i < frame; i++) {
				moveQueue.Enqueue(GetBackwardInput());
			}
		}

		private void AddForwardDashInputQueue() {
			moveQueue.Enqueue(GetForwardInput());
			moveQueue.Enqueue(0);
			moveQueue.Enqueue(GetForwardInput());
		}

		private void AddBackwardDashInputQueue() {
			moveQueue.Enqueue(GetForwardInput());
			moveQueue.Enqueue(0);
			moveQueue.Enqueue(GetForwardInput());
		}

		private void UpdateFightState() {
			FightState currentFightState = new FightState();
			
			currentFightState.DistanceX = GetDistanceX();
			
			currentFightState.IsOpponentDamage = battleCore.Fighter1.CurrentActionID == (int) CommonActionID.Damage;
			
			currentFightState.IsOpponentGuardBreak =
				battleCore.Fighter1.CurrentActionID == (int) CommonActionID.GuardBreak;
			
			currentFightState.IsOpponentBlocking =
				battleCore.Fighter1.CurrentActionID == (int) CommonActionID.GuardCrouch
				|| battleCore.Fighter1.CurrentActionID == (int) CommonActionID.GuardStand
				|| battleCore.Fighter1.CurrentActionID == (int) CommonActionID.GuardM;
			
			currentFightState.IsOpponentNormalAttack =
				battleCore.Fighter1.CurrentActionID == (int) CommonActionID.NAttack
				|| battleCore.Fighter1.CurrentActionID == (int) CommonActionID.BAttack;
			
			currentFightState.IsOpponentSpecialAttack =
				battleCore.Fighter1.CurrentActionID == (int) CommonActionID.NSpecial
				|| battleCore.Fighter1.CurrentActionID == (int) CommonActionID.BSpecial;

			for (int i = 1; i < fightStates.Length; i++) {
				fightStates[i] = fightStates[i - 1];
			}

			fightStates[0] = currentFightState;
		}

		private FightState GetCurrentFightState() {
			return fightStates[fightStateReadIndex];
		}

		private float GetDistanceX() {
			return Mathf.Abs(battleCore.Fighter2.Position.x - battleCore.Fighter1.Position.x);
		}

		private int GetAttackInput() {
			return (int) InputDefine.Attack;
		}

		private int GetForwardInput() {
			return (int) InputDefine.Left;
		}

		private int GetBackwardInput() {
			return (int) InputDefine.Right;
		}
	}
}