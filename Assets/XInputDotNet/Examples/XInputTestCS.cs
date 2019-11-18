using UnityEngine;
using XInputDotNetPure; // Required in C#

public class XInputTestCS : MonoBehaviour {
	private bool playerIndexSet;
	private PlayerIndex playerIndex;
	private GamePadState state;
	private GamePadState prevState;
	private Renderer rendererComponent;

	private void Awake() {
		rendererComponent = GetComponent<Renderer>();
	}

	private void FixedUpdate() {
		// SetVibration should be sent in a slower rate.
		// Set vibration according to triggers
		GamePad.SetVibration(playerIndex, state.Triggers.Left, state.Triggers.Right);
	}

	// Update is called once per frame
	private void Update() {
		// Find a PlayerIndex, for a single player game
		// Will find the first controller that is connected ans use it
		if (!playerIndexSet || !prevState.IsConnected) {
			for (int i = 0; i < 4; ++i) {
				PlayerIndex testPlayerIndex = (PlayerIndex) i;
				GamePadState testState = GamePad.GetState(testPlayerIndex);
				if (testState.IsConnected) {
					Footsies.Globals.Logger.Log($"GamePad found {testPlayerIndex}");
					playerIndex = testPlayerIndex;
					playerIndexSet = true;
				}
			}
		}

		prevState = state;
		state = GamePad.GetState(playerIndex);

		switch (prevState.Buttons.A) {
			// Detect if a button was pressed this frame
			case ButtonState.Released when state.Buttons.A == ButtonState.Pressed:
				rendererComponent.material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
				break;
			// Detect if a button was released this frame
			case ButtonState.Pressed when state.Buttons.A == ButtonState.Released:
				rendererComponent.material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
				break;
		}

		// Make the current object turn
		transform.localRotation *= Quaternion.Euler(0.0f, state.ThumbSticks.Left.X * 25.0f * Time.deltaTime, 0.0f);
	}

	private void OnGUI() {
		string text =
			"Use left stick to turn the cube, hold A to change color\n"
			+ $"IsConnected {state.IsConnected} Packet #{state.PacketNumber}\n"
			+ $"\tTriggers {state.Triggers.Left} {state.Triggers.Right}\n"
			+ $"\tD-Pad {state.DPad.Up} {state.DPad.Right} {state.DPad.Down} {state.DPad.Left}\n"
			+ $"\tButtons Start {state.Buttons.Start} Back {state.Buttons.Back} Guide {state.Buttons.Guide}\n"
			+ $"\tButtons LeftStick {state.Buttons.LeftStick} RightStick {state.Buttons.RightStick}\n"
			+ $"\tLeftShoulder {state.Buttons.LeftShoulder} RightShoulder {state.Buttons.RightShoulder}\n"
			+ $"\tButtons A {state.Buttons.A} B {state.Buttons.B} X {state.Buttons.X} Y {state.Buttons.Y}\n"
			+ $"\tSticks Left {state.ThumbSticks.Left.X} {state.ThumbSticks.Left.Y} Right {state.ThumbSticks.Right.X} {state.ThumbSticks.Right.Y}\n";
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), text);
	}
}