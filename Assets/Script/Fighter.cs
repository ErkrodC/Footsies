using System.Collections.Generic;
using UnityEngine;

namespace Footsies {
	public class Fighter {

		#region constants

		private const int kMaxSpriteShakeFrame = 6;
		private const int kInputRecordFrame = 180;

		#endregion

		#region public members and accessors
		
		public bool IsFaceRight;
		public Vector2 Position;
		public Pushbox Pushbox;
		public List<Hitbox> Hitboxes = new List<Hitbox>();
		public List<Hurtbox> Hurtboxes = new List<Hurtbox>();

		public bool IsDead => vitalHealth <= 0;
		public int GuardHealth { get; private set; }
		public int CurrentActionID { get; private set; }
		public int CurrentActionFrame { get; private set; }
		public int CurrentActionFrameCount => fighterData.Actions[CurrentActionID].frameCount;
		public bool IsAlwaysCancellable => fighterData.Actions[CurrentActionID].alwaysCancelable;
		public int CurrentHitStunFrame { get; private set; }
		public int SpriteShakePosition { get; private set; }

		#endregion

		#region private members

		private readonly int[] input = new int[kInputRecordFrame];
		private readonly int[] inputDown = new int[kInputRecordFrame];
		private readonly int[] inputUp = new int[kInputRecordFrame];

		private int vitalHealth;
		private int bufferActionID = -1;
		private int reserveDamageActionID = -1;
		private float velocityX;
		private bool isInputBackward;
		private bool hasWon;
		private bool isReserveProximityGuard;
		private FighterData fighterData;

		private int CurrentActionHitCount { get; set; }
		private bool IsActionEnd => CurrentActionFrame >= fighterData.Actions[CurrentActionID].frameCount;
		private bool IsInHitStun => CurrentHitStunFrame > 0;

		#endregion

		/// <summary>
		/// Setup fighter at the start of battle
		/// </summary>
		/// <param name="fighterData"></param>
		/// <param name="startPosition"></param>
		/// <param name="isPlayerOne"></param>
		public void SetupBattleStart(FighterData fighterData, Vector2 startPosition, bool isPlayerOne) {
			this.fighterData = fighterData;
			Position = startPosition;
			IsFaceRight = isPlayerOne;

			vitalHealth = 1;
			GuardHealth = fighterData.startGuardHealth;
			hasWon = false;

			velocityX = 0;

			ClearInput();

			SetCurrentAction((int) CommonActionID.Stand);
		}

		/// <summary>
		/// Update action frame
		/// </summary>
		public void IncrementActionFrame() {
			// Decrease sprite shake count and swap +/- (used by BattleGUI for fighter sprite position)
			if (Mathf.Abs(SpriteShakePosition) > 0) {
				SpriteShakePosition *= -1;
				SpriteShakePosition += SpriteShakePosition > 0 ? -1 : 1;
			}

			// If fighter is in hit stun then the action frame stay the same
			if (CurrentHitStunFrame > 0) {
				CurrentHitStunFrame--;
				return;
			}

			CurrentActionFrame++;

			// For loop motion (winning pose etc.) set action frame back to loop start frame
			if (IsActionEnd) {
				if (fighterData.Actions[CurrentActionID].isLoop) {
					CurrentActionFrame = fighterData.Actions[CurrentActionID].loopFromFrame;
				}
			}
		}

		/// <summary>
		/// UpdateInput
		/// </summary>
		/// <param name="inputData"></param>
		public void UpdateInput(InputData inputData) {
			// Shift input history by 1 frame
			for (int i = input.Length - 1; i >= 1; i--) {
				input[i] = input[i - 1];
				inputDown[i] = inputDown[i - 1];
				inputUp[i] = inputUp[i - 1];
			}

			// Insert new input data
			input[0] = inputData.Input;
			inputDown[0] = (input[0] ^ input[1]) & input[0];
			inputUp[0] = (input[0] ^ input[1]) & ~input[0];
			//Globals.Logger.Log(System.Convert.ToString(input[0], 2) + " " + System.Convert.ToString(inputDown[0], 2) + " " + System.Convert.ToString(inputUp[0], 2));
		}

		/// <summary>
		/// Action request for intro state ()
		/// </summary>
		public void UpdateIntroAction() {
			RequestAction((int) CommonActionID.Stand);
		}

		/// <summary>
		/// Update action request
		/// </summary>
		public void UpdateActionRequest() {
			// If won then just request win animation
			if (hasWon) {
				RequestAction((int) CommonActionID.Win);
				return;
			}

			// If there is any reserve damage action, set that to current action
			// Use for playing damage motion after hit stun ended (only use this for guard break currently)
			if (reserveDamageActionID != -1
			    && CurrentHitStunFrame <= 0) {
				SetCurrentAction(reserveDamageActionID);
				reserveDamageActionID = -1;
				return;
			}

			// If there is any buffer action, set that to current action
			// Use for canceling normal to special attack
			if (bufferActionID != -1
			    && CanCancelAttack()
			    && CurrentHitStunFrame <= 0) {
				SetCurrentAction(bufferActionID);
				bufferActionID = -1;
				return;
			}

			bool isForward = IsForwardInput(input[0]);
			bool isBackward = IsBackwardInput(input[0]);
			bool isAttack = IsAttackInput(inputDown[0]);
			if (CheckSpecialAttackInput()) {
				if (isBackward || isForward) {
					RequestAction((int) CommonActionID.BSpecial);
				} else {
					RequestAction((int) CommonActionID.NSpecial);
				}
			} else if (isAttack) {
				if ((CurrentActionID == (int) CommonActionID.NAttack || CurrentActionID == (int) CommonActionID.BAttack)
				    && !IsActionEnd) {
					RequestAction((int) CommonActionID.NSpecial);
				} else {
					if (isBackward || isForward) {
						RequestAction((int) CommonActionID.BAttack);
					} else {
						RequestAction((int) CommonActionID.NAttack);
					}
				}
			}

			if (CheckForwardDashInput()) {
				RequestAction((int) CommonActionID.DashForward);
			} else if (CheckBackwardDashInput()) {
				RequestAction((int) CommonActionID.DashBackward);
			}


			// for proximity guard check
			isInputBackward = isBackward;

			if (isForward && isBackward) {
				RequestAction((int) CommonActionID.Stand);
			} else if (isForward) {
				RequestAction((int) CommonActionID.Forward);
			} else if (isBackward) {
				if (isReserveProximityGuard) {
					RequestAction((int) CommonActionID.GuardProximity);
				} else {
					RequestAction((int) CommonActionID.Backward);
				}
			} else {
				RequestAction((int) CommonActionID.Stand);
			}

			isReserveProximityGuard = false;
		}

		/// <summary>
		/// Update character position
		/// </summary>
		public void UpdateMovement() {
			if (IsInHitStun) {
				return;
			}

			// Position changes from walking forward and backward
			int sign = IsFaceRight ? 1 : -1;
			switch (CurrentActionID) {
				case (int) CommonActionID.Forward:
					Position.x += fighterData.forwardMoveSpeed * sign * Time.deltaTime;
					return;
				case (int) CommonActionID.Backward:
					Position.x -= fighterData.backwardMoveSpeed * sign * Time.deltaTime;
					return;
			}

			// Position changes from action data
			MovementData movementData = fighterData.Actions[CurrentActionID].GetMovementData(CurrentActionFrame);
			if (movementData != null) {
				velocityX = movementData.velocity_x;
				if (!Mathf.Approximately(velocityX, 0)) {
					Position.x += velocityX * sign * Time.deltaTime;
				}
			}
		}

		public void UpdateBoxes() {
			ApplyCurrentActionData();
		}

		/// <summary>
		/// Apply position changed to all variable
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void ApplyPositionChange(float x, float y) {
			Position.x += x;
			Position.y += y;

			foreach (Hitbox hitbox in Hitboxes) {
				hitbox.Rect.x += x;
				hitbox.Rect.y += y;
			}

			foreach (Hurtbox hurtbox in Hurtboxes) {
				hurtbox.Rect.x += x;
				hurtbox.Rect.y += y;
			}

			Pushbox.Rect.x += x;
			Pushbox.Rect.y += y;
		}

		public void NotifyAttackHit(Fighter damagedFighter, Vector2 damagePos) {
			CurrentActionHitCount++;
		}

		public DamageResult NotifyDamaged(AttackData attackData, Vector2 damagePos) {
			bool isGuardBreak = false;
			if (attackData.guardHealthDamage > 0) {
				GuardHealth -= attackData.guardHealthDamage;
				if (GuardHealth < 0) {
					isGuardBreak = true;
					GuardHealth = 0;
				}
			}

			if (CurrentActionID == (int) CommonActionID.Backward
			    || fighterData.Actions[CurrentActionID].Type == ActionType.Guard
			) // if in blocking motion, automatically block next attack
			{
				if (isGuardBreak) {
					SetCurrentAction(attackData.guardActionID);
					reserveDamageActionID = (int) CommonActionID.GuardBreak;
					SoundManager.Instance.PlayFighterSE(
						fighterData.Actions[reserveDamageActionID].audioClip,
						IsFaceRight,
						Position.x
					);
					return DamageResult.GuardBreak;
				} else {
					SetCurrentAction(attackData.guardActionID);
					return DamageResult.Guard;
				}
			} else {
				if (attackData.vitalHealthDamage > 0) {
					vitalHealth -= attackData.vitalHealthDamage;
					if (vitalHealth <= 0) {
						vitalHealth = 0;
					}
				}

				SetCurrentAction(attackData.damageActionID);
				return DamageResult.Damage;
			}
		}

		public void NotifyInProximityGuardRange() {
			if (isInputBackward) {
				isReserveProximityGuard = true;
			}
		}

		public bool CanAttackHit(int attackID) {
			if (!fighterData.AttackData.ContainsKey(attackID)) {
				Globals.Logger.LogWarning($"Attack hit but AttackID={attackID} is not registered");
				return true;
			}

			if (CurrentActionHitCount >= fighterData.AttackData[attackID].numberOfHit) {
				return false;
			}

			return true;
		}

		public AttackData GetAttackData(int attackID) {
			if (!fighterData.AttackData.ContainsKey(attackID)) {
				Globals.Logger.LogWarning($"Attack hit but AttackID={attackID} is not registered");
				return null;
			}

			return fighterData.AttackData[attackID];
		}

		public void SetHitStun(int hitStunFrame) {
			CurrentHitStunFrame = hitStunFrame;
		}

		public void SetSpriteShakeFrame(int spriteShakeFrame) {
			if (spriteShakeFrame > kMaxSpriteShakeFrame) {
				spriteShakeFrame = kMaxSpriteShakeFrame;
			}

			SpriteShakePosition = spriteShakeFrame * (IsFaceRight ? -1 : 1);
		}

		public int GetHitStunFrame(DamageResult damageResult, int attackID) {
			if (damageResult == DamageResult.Guard) {
				return fighterData.AttackData[attackID].guardStunFrame;
			} else if (damageResult == DamageResult.GuardBreak) {
				return fighterData.AttackData[attackID].guardBreakStunFrame;
			}

			return fighterData.AttackData[attackID].hitStunFrame;
		}

		public int GetGuardStunFrame(int attackID) {
			return fighterData.AttackData[attackID].guardStunFrame;
		}

		public void RequestWinAction() {
			hasWon = true;
		}

		/// <summary>
		/// Request action, if condition is met then set the requested action to current action
		/// </summary>
		/// <param name="actionID"></param>
		/// <param name="startFrame"></param>
		/// <returns></returns>
		public bool RequestAction(int actionID, int startFrame = 0) {
			if (IsActionEnd) {
				SetCurrentAction(actionID, startFrame);
				return true;
			}

			if (CurrentActionID == actionID) {
				return false;
			}

			if (fighterData.Actions[CurrentActionID].alwaysCancelable) {
				SetCurrentAction(actionID, startFrame);
				return true;
			} else {
				foreach (CancelData cancelData in fighterData.Actions[CurrentActionID].GetCancelData(CurrentActionFrame)
				) {
					if (cancelData.actionID.Contains(actionID)) {
						if (cancelData.execute) {
							bufferActionID = actionID;
							return true;
						} else if (cancelData.buffer) {
							bufferActionID = actionID;
						}
					}
				}
			}

			return false;
		}

		public Sprite GetCurrentMotionSprite() {
			MotionFrameData motionData = fighterData.Actions[CurrentActionID].GetMotionData(CurrentActionFrame);
			return motionData == null ? null : fighterData.MotionData[motionData.motionID].sprite;
		}

		public void ClearInput() {
			for (int i = 0; i < input.Length; i++) {
				input[i] = 0;
				inputDown[i] = 0;
				inputUp[i] = 0;
			}
		}

		private bool CanCancelAttack() {
			if (fighterData.canCancelOnWhiff) {
				return true;
			} else if (CurrentActionHitCount > 0) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Set current action
		/// </summary>
		/// <param name="actionID"></param>
		/// <param name="startFrame"></param>
		private void SetCurrentAction(int actionID, int startFrame = 0) {
			CurrentActionID = actionID;
			CurrentActionFrame = startFrame;

			CurrentActionHitCount = 0;
			bufferActionID = -1;
			reserveDamageActionID = -1;
			SpriteShakePosition = 0;

			if (fighterData.Actions[CurrentActionID].audioClip) {
				if (CurrentActionID == (int) CommonActionID.GuardBreak) {
					return;
				}

				SoundManager.Instance.PlayFighterSE(
					fighterData.Actions[CurrentActionID].audioClip,
					IsFaceRight,
					Position.x
				);
			}
		}

		/// <summary>
		/// Special attack input check (hold and release)
		/// </summary>
		/// <returns></returns>
		private bool CheckSpecialAttackInput() {
			if (!IsAttackInput(inputUp[0])) {
				return false;
			}

			for (int i = 1; i < fighterData.specialAttackHoldFrame; i++) {
				if (!IsAttackInput(input[i])) {
					return false;
				}
			}

			return true;
		}

		private bool CheckForwardDashInput() {
			if (!IsForwardInput(inputDown[0])) {
				return false;
			}

			for (int i = 1; i < fighterData.dashAllowFrame; i++) {
				if (IsBackwardInput(input[i])) {
					return false;
				}

				if (IsForwardInput(input[i])) {
					for (int j = i + 1; j < i + fighterData.dashAllowFrame; j++) {
						if (!IsForwardInput(input[j]) && !IsBackwardInput(input[j])) {
							return true;
						}
					}

					return false;
				}
			}

			return false;
		}

		private bool CheckBackwardDashInput() {
			if (!IsBackwardInput(inputDown[0])) {
				return false;
			}

			for (int i = 1; i < fighterData.dashAllowFrame; i++) {
				if (IsForwardInput(input[i])) {
					return false;
				}

				if (IsBackwardInput(input[i])) {
					for (int j = i + 1; j < i + fighterData.dashAllowFrame; j++) {
						if (!IsForwardInput(input[j]) && !IsBackwardInput(input[j])) {
							return true;
						}
					}

					return false;
				}
			}

			return false;
		}

		private static bool IsAttackInput(int input) {
			return (input & (int) InputDefine.Attack) > 0;
		}

		private bool IsForwardInput(int input) {
			return IsFaceRight
				? (input & (int) InputDefine.Right) > 0
				: (input & (int) InputDefine.Left) > 0;
		}

		private bool IsBackwardInput(int input) {
			return IsFaceRight
				? (input & (int) InputDefine.Left) > 0
				: (input & (int) InputDefine.Right) > 0;
		}

		/// <summary>
		/// Copy data from current action and convert relative box position with fighter position
		/// </summary>
		private void ApplyCurrentActionData() {
			Hitboxes.Clear();
			Hurtboxes.Clear();

			foreach (HitboxData hitbox in fighterData.Actions[CurrentActionID].GetHitboxData(CurrentActionFrame)) {
				Hitbox box = new Hitbox {
					Rect = TransformToFightRect(hitbox.rect, Position, IsFaceRight),
					Proximity = hitbox.proximity,
					AttackID = hitbox.attackID
				};
				
				Hitboxes.Add(box);
			}

			foreach (HurtboxData hurtbox in fighterData.Actions[CurrentActionID].GetHurtboxData(CurrentActionFrame)) {
				Hurtbox box = new Hurtbox();
				Rect rect = hurtbox.useBaseRect ? fighterData.baseHurtBoxRect : hurtbox.rect;
				box.Rect = TransformToFightRect(rect, Position, IsFaceRight);
				Hurtboxes.Add(box);
			}

			PushboxData pushBoxData = fighterData.Actions[CurrentActionID].GetPushboxData(CurrentActionFrame);
			if (pushBoxData != null) {
				Pushbox = new Pushbox();
				Rect pushRect = pushBoxData.useBaseRect ? fighterData.basePushBoxRect : pushBoxData.rect;
				Pushbox.Rect = TransformToFightRect(pushRect, Position, IsFaceRight);
			}
		}

		/// <summary>
		/// Convert relative box position with current fighter position
		/// </summary>
		/// <param name="dataRect"></param>
		/// <param name="basePosition"></param>
		/// <param name="isFaceRight"></param>
		/// <returns></returns>
		private static Rect TransformToFightRect(Rect dataRect, Vector2 basePosition, bool isFaceRight) {
			int sign = isFaceRight ? 1 : -1;

			Rect fightPosRect = new Rect {
				x = basePosition.x + dataRect.x * sign,
				y = basePosition.y + dataRect.y,
				width = dataRect.width,
				height = dataRect.height
			};

			return fightPosRect;
		}
	}
}