using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649

namespace Footsies {
	/// <summary>
	/// Main update for battle engine
	/// Update player/ai input, fighter actions, hitbox/hurtbox collision, round start/end
	/// </summary>
	public class BattleCore : MonoBehaviour {
		#region constants and statics

		private const uint kMaxRecordingInputFrame = 60 * 60 * 5;
		private const uint kMaxRoundWon = 3;
		private const float kIntroStateTime = 3f;
		private const float kKOStateTime = 2f;
		private const float kEndStateTime = 3f;
		private const float kEndStateSkippableTime = 1.5f;
		private static readonly int roundStart = Animator.StringToHash("RoundStart");
		private static readonly int roundEnd = Animator.StringToHash("RoundEnd");

		#endregion

		#region public members and accessors

		public float BattleAreaWidth = 10f;
		public float BattleAreaMaxHeight = 2f;
		public bool debugP1Attack;
		public bool debugP2Attack;
		public bool debugP1Guard;
		public bool debugP2Guard;
		public bool debugPlayLastRoundInput;
		public System.Action<Fighter, Vector2, DamageResult> damageHandler;

		public Fighter Fighter1 { get; private set; }
		public Fighter Fighter2 { get; private set; }
		public uint Fighter1RoundWon { get; private set; }
		public uint Fighter2RoundWon { get; private set; }
		public List<Fighter> Fighters { get; } = new List<Fighter>();

		#endregion

		#region private members

		[SerializeField] private GameObject roundUI;
		[SerializeField] private List<FighterData> fighterDataList = new List<FighterData>();

		private readonly InputData[] recordingP1Input = new InputData[kMaxRecordingInputFrame];
		private readonly InputData[] recordingP2Input = new InputData[kMaxRecordingInputFrame];
		private readonly InputData[] lastRoundP1Input = new InputData[kMaxRecordingInputFrame];
		private readonly InputData[] lastRoundP2Input = new InputData[kMaxRecordingInputFrame];

		private float timer;
		private float roundStartTime;
		private int frameCount;
		private uint currentRecordingInputIndex;
		private uint currentReplayingInputIndex;
		private uint lastRoundMaxRecordingInput;
		private bool isReplayingLastRoundInput;
		private RoundStateType roundState = RoundStateType.Stop;
		private Animator roundUIAnimator;
		private BattleAI battleAI;
		private bool isDebugPause;

		#endregion

		#region Monobehaviour

		private void Awake() {
			// Setup dictionary from ScriptableObject data
			fighterDataList.ForEach((data) => data.SetupDictionary());

			Fighter1 = new Fighter();
			Fighter2 = new Fighter();

			Fighters.Add(Fighter1);
			Fighters.Add(Fighter2);

			debugP2Attack = false;
			debugP2Guard = false;

			if (roundUI != null) {
				roundUIAnimator = roundUI.GetComponent<Animator>();
			}
		}

		private void FixedUpdate() {
			switch (roundState) {
				case RoundStateType.Stop:
					ChangeRoundState(RoundStateType.Intro);
					break;
				case RoundStateType.Intro:
					UpdateIntroState();

					timer -= Time.deltaTime;
					if (timer <= 0f) {
						ChangeRoundState(RoundStateType.Fight);
					}

					if (debugPlayLastRoundInput
					    && !isReplayingLastRoundInput) {
						StartPlayLastRoundInput();
					}

					break;
				case RoundStateType.Fight:
					if (CheckUpdateDebugPause()) {
						break;
					}

					frameCount++;

					UpdateFightState();

					Fighter deadFighter = Fighters.Find((f) => f.IsDead);
					if (deadFighter != null) {
						ChangeRoundState(RoundStateType.KO);
					}

					break;
				case RoundStateType.KO:
					UpdateKOState();
					timer -= Time.deltaTime;
					if (timer <= 0f) {
						ChangeRoundState(RoundStateType.End);
					}

					break;
				case RoundStateType.End:
					UpdateEndState();

					timer -= Time.deltaTime;
					if (timer <= 0f
					    || timer <= kEndStateSkippableTime && IsKOSkipButtonPressed()) {
						ChangeRoundState(RoundStateType.Stop);
					}

					break;
			}
		}

		#endregion

		#region state machine

		private void ChangeRoundState(RoundStateType state) {
			roundState = state;
			switch (roundState) {
				case RoundStateType.Stop:

					if (Fighter1RoundWon >= kMaxRoundWon
					    || Fighter2RoundWon >= kMaxRoundWon) {
						GameManager.Instance.LoadTitleScene();
					}

					break;
				case RoundStateType.Intro:

					Fighter1.SetupBattleStart(fighterDataList[0], new Vector2(-2f, 0f), true);
					Fighter2.SetupBattleStart(fighterDataList[0], new Vector2(2f, 0f), false);

					timer = kIntroStateTime;

					roundUIAnimator.SetTrigger(roundStart);

					if (GameManager.Instance.IsVsCPU) {
						battleAI = new BattleAI(this);
					}

					break;
				case RoundStateType.Fight:

					roundStartTime = Time.fixedTime;
					frameCount = -1;

					currentRecordingInputIndex = 0;

					break;
				case RoundStateType.KO:

					timer = kKOStateTime;

					CopyLastRoundInput();

					Fighter1.ClearInput();
					Fighter2.ClearInput();

					battleAI = null;

					roundUIAnimator.SetTrigger(roundEnd);

					break;
				case RoundStateType.End:

					timer = kEndStateTime;

					List<Fighter> deadFighter = Fighters.FindAll((f) => f.IsDead);
					if (deadFighter.Count == 1) {
						if (deadFighter[0] == Fighter1) {
							Fighter2RoundWon++;
							Fighter2.RequestWinAction();
						} else if (deadFighter[0] == Fighter2) {
							Fighter1RoundWon++;
							Fighter1.RequestWinAction();
						}
					}

					break;
			}
		}

		private void UpdateIntroState() {
			InputData p1Input = GetP1InputData();
			InputData p2Input = GetP2InputData();
			RecordInput(p1Input, p2Input);
			Fighter1.UpdateInput(p1Input);
			Fighter2.UpdateInput(p2Input);

			Fighters.ForEach((f) => f.IncrementActionFrame());

			Fighters.ForEach((f) => f.UpdateIntroAction());
			Fighters.ForEach((f) => f.UpdateMovement());
			Fighters.ForEach((f) => f.UpdateBoxes());

			UpdatePushCharacterVsCharacter();
			UpdatePushCharacterVsBackground();
		}

		private void UpdateFightState() {
			InputData p1Input = GetP1InputData();
			InputData p2Input = GetP2InputData();
			RecordInput(p1Input, p2Input);
			Fighter1.UpdateInput(p1Input);
			Fighter2.UpdateInput(p2Input);

			Fighters.ForEach((f) => f.IncrementActionFrame());

			Fighters.ForEach((f) => f.UpdateActionRequest());
			Fighters.ForEach((f) => f.UpdateMovement());
			Fighters.ForEach((f) => f.UpdateBoxes());

			UpdatePushCharacterVsCharacter();
			UpdatePushCharacterVsBackground();
			UpdateHitboxHurtboxCollision();
		}

		private void UpdateKOState() { }

		private void UpdateEndState() {
			Fighters.ForEach((f) => f.IncrementActionFrame());

			Fighters.ForEach((f) => f.UpdateActionRequest());
			Fighters.ForEach((f) => f.UpdateMovement());
			Fighters.ForEach((f) => f.UpdateBoxes());

			UpdatePushCharacterVsCharacter();
			UpdatePushCharacterVsBackground();
		}

		#endregion

		#region input
		
		private static bool IsKOSkipButtonPressed() {
			return InputManager.Instance.GetButton(InputManager.Command.P1Attack)
			       || InputManager.Instance.GetButton(InputManager.Command.P2Attack);
		}

		private InputData GetP1InputData() {
			if (isReplayingLastRoundInput) {
				return lastRoundP1Input[currentReplayingInputIndex];
			}

			float time = Time.fixedTime - roundStartTime;

			InputData p1Input = new InputData();
			p1Input.Input |= InputManager.Instance.GetButton(InputManager.Command.P1Left) ? (int) InputDefine.Left : 0;
			p1Input.Input |= InputManager.Instance.GetButton(InputManager.Command.P1Right)
				? (int) InputDefine.Right
				: 0;
			p1Input.Input |= InputManager.Instance.GetButton(InputManager.Command.P1Attack)
				? (int) InputDefine.Attack
				: 0;
			p1Input.Time = time;

			if (debugP1Attack) {
				p1Input.Input |= (int) InputDefine.Attack;
			}

			if (debugP1Guard) {
				p1Input.Input |= (int) InputDefine.Left;
			}

			return p1Input;
		}

		private InputData GetP2InputData() {
			if (isReplayingLastRoundInput) {
				return lastRoundP2Input[currentReplayingInputIndex];
			}

			float time = Time.fixedTime - roundStartTime;

			InputData p2Input = new InputData();

			if (battleAI != null) {
				p2Input.Input |= battleAI.GetNextAIInput();
			} else {
				p2Input.Input |= InputManager.Instance.GetButton(InputManager.Command.P2Left)
					? (int) InputDefine.Left
					: 0;
				p2Input.Input |= InputManager.Instance.GetButton(InputManager.Command.P2Right)
					? (int) InputDefine.Right
					: 0;
				p2Input.Input |= InputManager.Instance.GetButton(InputManager.Command.P2Attack)
					? (int) InputDefine.Attack
					: 0;
			}

			p2Input.Time = time;

			if (debugP2Attack) {
				p2Input.Input |= (int) InputDefine.Attack;
			}

			if (debugP2Guard) {
				p2Input.Input |= (int) InputDefine.Right;
			}

			return p2Input;
		}
		
		private void RecordInput(InputData p1Input, InputData p2Input) {
			if (currentRecordingInputIndex >= kMaxRecordingInputFrame) {
				return;
			}

			recordingP1Input[currentRecordingInputIndex] = p1Input.ShallowCopy();
			recordingP2Input[currentRecordingInputIndex] = p2Input.ShallowCopy();
			currentRecordingInputIndex++;

			if (isReplayingLastRoundInput) {
				if (currentReplayingInputIndex < lastRoundMaxRecordingInput) {
					currentReplayingInputIndex++;
				}
			}
		}

		private void CopyLastRoundInput() {
			for (int i = 0; i < currentRecordingInputIndex; i++) {
				lastRoundP1Input[i] = recordingP1Input[i].ShallowCopy();
				lastRoundP2Input[i] = recordingP2Input[i].ShallowCopy();
			}

			lastRoundMaxRecordingInput = currentRecordingInputIndex;

			isReplayingLastRoundInput = false;
			currentReplayingInputIndex = 0;
		}

		private void StartPlayLastRoundInput() {
			isReplayingLastRoundInput = true;
			currentReplayingInputIndex = 0;
		}

		#endregion

		#region updates

		private void UpdatePushCharacterVsCharacter() {
			Rect rect1 = Fighter1.Pushbox.Rect;
			Rect rect2 = Fighter2.Pushbox.Rect;

			if (!rect1.Overlaps(rect2)) { return; }

			if (Fighter1.Position.x < Fighter2.Position.x) {
				Fighter1.ApplyPositionChange((rect1.xMax - rect2.xMin) * -1 / 2, Fighter1.Position.y);
				Fighter2.ApplyPositionChange((rect1.xMax - rect2.xMin) * 1 / 2, Fighter2.Position.y);
			} else if (Fighter1.Position.x > Fighter2.Position.x) {
				Fighter1.ApplyPositionChange((rect2.xMax - rect1.xMin) * 1 / 2, Fighter1.Position.y);
				Fighter2.ApplyPositionChange((rect2.xMax - rect1.xMin) * -1 / 2, Fighter1.Position.y);
			}
		}

		private void UpdatePushCharacterVsBackground() {
			float stageMinX = BattleAreaWidth * -1 / 2;
			float stageMaxX = BattleAreaWidth / 2;

			foreach (Fighter fighter in Fighters) {
				if (fighter.Pushbox.XMin < stageMinX) {
					fighter.ApplyPositionChange(stageMinX - fighter.Pushbox.XMin, fighter.Position.y);
				} else if (fighter.Pushbox.XMax > stageMaxX) {
					fighter.ApplyPositionChange(stageMaxX - fighter.Pushbox.XMax, fighter.Position.y);
				}
			}
		}

		private void UpdateHitboxHurtboxCollision() {
			foreach (Fighter attacker in Fighters) {
				Vector2 damagePos = Vector2.zero;
				bool isHit = false;
				bool isProximity = false;
				int hitAttackID = 0;

				foreach (Fighter damaged in Fighters) {
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

						damageHandler(damaged, damagePos, damageResult);
					} else if (isProximity) {
						damaged.NotifyInProximityGuardRange();
					}
				}
			}
		}

		private bool CheckUpdateDebugPause() {
			if (Input.GetKeyDown(KeyCode.F1)) {
				isDebugPause = !isDebugPause;
			}

			if (isDebugPause) {
				// press f2 during debug pause to 
				if (Input.GetKeyDown(KeyCode.F2)) {
					return false;
				} else {
					return true;
				}
			}

			return false;
		}

		#endregion

		public int GetFrameAdvantage(bool getP1) {
			int p1FrameLeft = Fighter1.CurrentActionFrameCount - Fighter1.CurrentActionFrame;
			if (Fighter1.IsAlwaysCancellable) {
				p1FrameLeft = 0;
			}

			int p2FrameLeft = Fighter2.CurrentActionFrameCount - Fighter2.CurrentActionFrame;
			if (Fighter2.IsAlwaysCancellable) {
				p2FrameLeft = 0;
			}

			return getP1
				? p2FrameLeft - p1FrameLeft
				: p1FrameLeft - p2FrameLeft;
		}
	}
}