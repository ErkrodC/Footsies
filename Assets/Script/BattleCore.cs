using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649

namespace Footsies {
	/// <summary>
	/// Main update for battle engine
	/// Update player/ai input, fighter actions, hitbox/hurtbox collision, round start/end
	/// </summary>
	public class BattleCore : MonoBehaviour {
		private const uint kMaxRecordingInputFrame = 60 * 60 * 5;
		
		#region public members and accessors

		public event Action<Fighter, Vector2, DamageResult> DamageOccurred;
		public float BattleAreaWidth = 10f;
		public float BattleAreaMaxHeight = 2f;
		[SerializeField] private GameObject roundUI;
		public List<FighterData> FighterDataList = new List<FighterData>(); // TODO rename
		public bool debugP1Attack;
		public bool debugP2Attack;
		public bool debugP1Guard;
		public bool debugP2Guard;
		public bool debugPlayLastRoundInput;
		public Fighter Fighter1 { get; private set; }
		public Fighter Fighter2 { get; private set; }
		public List<Fighter> Fighters { get; } = new List<Fighter>();

		public Animator RoundUIAnimator { get; private set; }
		public uint CurrentRecordingInputIndex { get; set; }
		public float RoundStartTime { get; set; }

		#endregion

		#region private members

		public InputData[] RecordingP1Input { get; } = new InputData[kMaxRecordingInputFrame];
		public InputData[] RecordingP2Input { get; } = new InputData[kMaxRecordingInputFrame];
		public InputData[] LastRoundP1Input { get; } = new InputData[kMaxRecordingInputFrame];
		public InputData[] LastRoundP2Input { get; }= new InputData[kMaxRecordingInputFrame];
		public float Timer { get; set; }
		private StateMachine stateMachine;
		public BattleAI BattleAI { get; set; }
		public bool IsReplayingLastRoundInput { get; set; }
		public uint CurrentReplayingInputIndex { get; set; }
		public uint LastRoundMaxRecordingInput { get; set; }
		public uint Fighter1RoundWon { get; set; }
		public uint Fighter2RoundWon { get; set; }

		#endregion

		#region Monobehaviour

		private void Awake() {
			if (GameManager.Instance.IsVsCPU) { // TODO check this
				BattleAI = new BattleAI(this);
			}
			
			// Setup dictionary from ScriptableObject data
			foreach (FighterData data in FighterDataList) {
				data.SetupDictionary();
			}

			Fighter1 = new Fighter();
			Fighter2 = new Fighter();

			Fighters.Add(Fighter1);
			Fighters.Add(Fighter2);

			debugP2Attack = false;
			debugP2Guard = false;

			if (roundUI != null) {
				RoundUIAnimator = roundUI.GetComponent<Animator>();
			}
			
			stateMachine = new StateMachine();
			stateMachine.AddState(new StopState(this));
			stateMachine.AddState(new IntroState(this));
			stateMachine.AddState(new FightState(this));
			stateMachine.AddState(new KOState(this));
			stateMachine.AddState(new EndState(this));
			
			stateMachine.SetState<StopState>();
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

		public InputData GetP1InputData() {
			if (IsReplayingLastRoundInput) {
				return LastRoundP1Input[CurrentReplayingInputIndex];
			}

			float time = Time.fixedTime - RoundStartTime;

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

		public InputData GetP2InputData() {
			if (IsReplayingLastRoundInput) {
				return LastRoundP2Input[CurrentReplayingInputIndex];
			}

			float time = Time.fixedTime - RoundStartTime;

			InputData p2Input = new InputData();

			if (BattleAI != null) {
				p2Input.Input |= BattleAI.GetNextAIInput();
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

		public void RecordInput(InputData p1Input, InputData p2Input) {
			if (CurrentRecordingInputIndex >= kMaxRecordingInputFrame) {
				return;
			}

			RecordingP1Input[CurrentRecordingInputIndex] = p1Input.ShallowCopy();
			RecordingP2Input[CurrentRecordingInputIndex] = p2Input.ShallowCopy();
			CurrentRecordingInputIndex++;

			if (IsReplayingLastRoundInput) {
				if (CurrentReplayingInputIndex < LastRoundMaxRecordingInput) {
					CurrentReplayingInputIndex++;
				}
			}
		}

		public void UpdatePushCharacterVsCharacter() {
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

		public void UpdatePushCharacterVsBackground() {
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
	}
}