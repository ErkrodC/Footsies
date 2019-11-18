using UnityEngine;
using XInputDotNetPure;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Footsies {
	public class InputManager : Singleton<InputManager> {
		public enum Command {
			P1Left,
			P1Right,
			P1Attack,
			P2Left,
			P2Right,
			P2Attack,
			Cancel,
		}

		public enum PadMenuInputState {
			Up,
			Down,
			Confirm,
		}

		public class GamePadHelper {
			public bool IsSet;
			public PlayerIndex PlayerIndex;
			public GamePadState State;
		}

		public GamePadHelper[] gamePads = new GamePadHelper[2];

		private int previousMenuInput;
		private int currentMenuInput;

		private float stickThreshold = 0.01f;

		private void Awake() {
			DontDestroyOnLoad(gameObject);

			for (int i = 0; i < gamePads.Length; i++) {
				gamePads[i] = new GamePadHelper();
			}
		}

		private void Update() {
			UpdateGamePad();

			if (IsPadConnected(0)) {
				if (EventSystem.current != null) {
					GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
					if (selectedObject != null) {
						if (IsMenuInputDown(PadMenuInputState.Confirm)) {
							UIEventAction eventAction = selectedObject.GetComponent<UIEventAction>();
							if (eventAction != null) {
								eventAction.InvokeAction();
							}
						} else if (IsMenuInputDown(PadMenuInputState.Up)
						           || IsMenuInputDown(PadMenuInputState.Down)) {
							Selectable selectable = selectedObject.GetComponent<Selectable>();
							if (selectable != null) {
								Selectable changedSelectable = IsMenuInputDown(PadMenuInputState.Up)
									? selectable.FindSelectableOnUp()
									: selectable.FindSelectableOnDown();
								if (changedSelectable != null) {
									changedSelectable.Select();
								}
							}
						}
					}
				}
			}
		}

		public bool GetButton(Command command) {
			if (IsPadConnected(0)) {
				if (command == Command.P1Left
				    && IsXInputLeft(gamePads[0].State)) {
					return true;
				} else if (command == Command.P1Right
				           && IsXInputRight(gamePads[0].State)) {
					return true;
				} else if (command == Command.P1Attack
				           && gamePads[0].State.Buttons.A == ButtonState.Pressed) {
					return true;
				}
			}

			if (IsPadConnected(1)) {
				if (command == Command.P2Left
				    && IsXInputLeft(gamePads[1].State)) {
					return true;
				} else if (command == Command.P2Right
				           && IsXInputRight(gamePads[1].State)) {
					return true;
				} else if (command == Command.P2Attack
				           && gamePads[1].State.Buttons.A == ButtonState.Pressed) {
					return true;
				}
			}

			return Input.GetButton(GetInputName(command));
		}

		public bool GetButtonDown(Command command) {
			return Input.GetButtonDown(GetInputName(command));
		}

		public bool GetButtonUp(Command command) {
			return Input.GetButtonUp(GetInputName(command));
		}

		private bool IsPadConnected(int padNumber) {
			if (padNumber >= gamePads.Length) {
				return false;
			}

			if (!gamePads[padNumber].IsSet || !gamePads[padNumber].State.IsConnected) {
				return false;
			}

			return true;
		}

		private void UpdateGamePad() {
			for (int i = 0; i < gamePads.Length; i++) {
				if (!IsPadConnected(i)) {
					for (int j = 0; j < 4; j++) {
						PlayerIndex testPlayerIndex = (PlayerIndex) j;
						if (IsPlayerIndexInUsed(testPlayerIndex)) {
							continue;
						}

						GamePadState testState = GamePad.GetState(testPlayerIndex);
						if (testState.IsConnected) {
							Globals.Logger.Log($"Set pad {testPlayerIndex} to player {i + 1}");
							gamePads[i].PlayerIndex = testPlayerIndex;
							gamePads[i].IsSet = true;
							gamePads[i].State = GamePad.GetState(testPlayerIndex);
							break;
						}
					}
				}
			}

			previousMenuInput = ComputeInput(gamePads[0]);

			for (int i = 0; i < gamePads.Length; i++) {
				gamePads[i].State = GamePad.GetState(gamePads[i].PlayerIndex);
			}

			currentMenuInput = ComputeInput(gamePads[0]);
		}

		private int ComputeInput(GamePadHelper pad) {
			if (!pad.IsSet || !pad.State.IsConnected) {
				return 0;
			}

			GamePadState state = pad.State;

			int i = 0;
			if (IsXInputUp(state)) {
				i |= 1 << (int) PadMenuInputState.Up;
			}

			if (IsXInputDown(state)) {
				i |= 1 << (int) PadMenuInputState.Down;
			}

			if (state.Buttons.A == ButtonState.Pressed) {
				i |= 1 << (int) PadMenuInputState.Confirm;
			}

			return i;
		}

		private bool IsMenuInputDown(PadMenuInputState checkInput) {
			int checkInputNo = 1 << (int) checkInput;
			return (previousMenuInput & checkInputNo) == 0 && (currentMenuInput & checkInputNo) > 0;
		}

		private bool IsPlayerIndexInUsed(PlayerIndex index) {
			for (int i = 0; i < gamePads.Length; i++) {
				if (IsPadConnected(i)
				    && gamePads[i].PlayerIndex == index) {
					return true;
				}
			}

			return false;
		}

		private string GetInputName(Command command) {
			switch (command) {
				case Command.P1Left: return "P1_Left";
				case Command.P1Right: return "P1_Right";
				case Command.P1Attack: return "P1_Attack";
				case Command.P2Left: return "P2_Left";
				case Command.P2Right: return "P2_Right";
				case Command.P2Attack: return "P2_Attack";
				case Command.Cancel: return "Cancel";
			}

			return "";
		}

		private bool IsXInputUp(GamePadState state) {
			if (state.DPad.Up == ButtonState.Pressed) {
				return true;
			}

			if (state.ThumbSticks.Left.Y > stickThreshold) {
				return true;
			}

			return false;
		}

		private bool IsXInputDown(GamePadState state) {
			if (state.DPad.Down == ButtonState.Pressed) {
				return true;
			}

			if (state.ThumbSticks.Left.Y < -stickThreshold) {
				return true;
			}

			return false;
		}

		private bool IsXInputLeft(GamePadState state) {
			if (state.DPad.Left == ButtonState.Pressed) {
				return true;
			}

			if (state.ThumbSticks.Left.X < -stickThreshold) {
				return true;
			}

			return false;
		}

		private bool IsXInputRight(GamePadState state) {
			if (state.DPad.Right == ButtonState.Pressed) {
				return true;
			}

			if (state.ThumbSticks.Left.X > stickThreshold) {
				return true;
			}

			return false;
		}
	}
}