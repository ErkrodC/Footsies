using System;
using UnityEngine;

namespace Footsies {
	public class FightState : IState {
		public event Action<Fighter, Vector2, DamageResult> DamageOccurred;
		
		public StateMachine StateMachine { get; set; }
		
		private int frameCount;
		private bool isDebugPause;
		private readonly BattleCore battleCore;

		public FightState(BattleCore battleCore) {
			this.battleCore = battleCore;
		}
		
		public void OnEnter() {
			battleCore.RoundStartTime = Time.fixedTime;
			frameCount = -1;

			battleCore.CurrentRecordingInputIndex = 0;
		}

		public void Tick() {
			if (CheckUpdateDebugPause()) {
				return;
			}

			frameCount++;

			UpdateFightState();

			Fighter deadFighter = battleCore.Fighters.Find(f => f.IsDead);
			if (deadFighter != null) {
				StateMachine.SetState<KOState>();
			}
		}

		public void OnExit() {
			throw new System.NotImplementedException();
		}
		
		private bool CheckUpdateDebugPause() {
			if (Input.GetKeyDown(KeyCode.F1)) {
				isDebugPause = !isDebugPause;
			}

			if (isDebugPause) {
				// press f2 during debug pause to 
				return !Input.GetKeyDown(KeyCode.F2);
			}

			return false;
		}
		
		private void UpdateFightState() {
			InputData p1Input = battleCore.GetP1InputData();
			InputData p2Input = battleCore.GetP2InputData();
			battleCore.RecordInput(p1Input, p2Input);
			battleCore.Fighter1.UpdateInput(p1Input);
			battleCore.Fighter2.UpdateInput(p2Input);

			foreach (Fighter fighter in battleCore.Fighters) {
				fighter.IncrementActionFrame();
				fighter.UpdateActionRequest();
				fighter.UpdateMovement();
				fighter.UpdateBoxes();
			}

			battleCore.UpdatePushCharacterVsCharacter();
			battleCore.UpdatePushCharacterVsBackground();
			UpdateHitboxHurtboxCollision();
		}
		
		private void UpdateHitboxHurtboxCollision() {
			foreach (Fighter attacker in battleCore.Fighters) {
				Vector2 damagePos = Vector2.zero;
				bool isHit = false;
				bool isProximity = false;
				int hitAttackID = 0;

				foreach (Fighter damaged in battleCore.Fighters) {
					if (attacker == damaged) {
						continue;
					}

					foreach (Hitbox hitbox in attacker.Hitboxes) {
						// continue if attack already hit
						if (!attacker.CanAttackHit(hitbox.AttackID)) {
							continue;
						}

						foreach (Hurtbox hurtbox in damaged.Hurtboxes) {
							if (hitbox.Overlaps(hurtbox)) {
								if (hitbox.Proximity) {
									isProximity = true;
								} else {
									isHit = true;
									hitAttackID = hitbox.AttackID;
									float x1 = Mathf.Min(hitbox.XMax, hurtbox.XMax);
									float x2 = Mathf.Max(hitbox.XMin, hurtbox.XMin);
									float y1 = Mathf.Min(hitbox.YMax, hurtbox.YMax);
									float y2 = Mathf.Max(hitbox.YMin, hurtbox.YMin);
									damagePos.x = (x1 + x2) / 2;
									damagePos.y = (y1 + y2) / 2;
									break;
								}
							}
						}

						if (isHit) {
							break;
						}
					}

					if (isHit) {
						attacker.NotifyAttackHit(damaged, damagePos);
						DamageResult damageResult =
							damaged.NotifyDamaged(attacker.GetAttackData(hitAttackID), damagePos);

						int hitStunFrame = attacker.GetHitStunFrame(damageResult, hitAttackID);
						attacker.SetHitStun(hitStunFrame);
						damaged.SetHitStun(hitStunFrame);
						damaged.SetSpriteShakeFrame(hitStunFrame / 3);

						DamageOccurred?.Invoke(damaged, damagePos, damageResult);
					} else if (isProximity) {
						damaged.NotifyInProximityGuardRange();
					}
				}
			}
		}
	}
}