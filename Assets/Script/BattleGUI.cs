using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

namespace Footsies {
	/// <summary>
	/// Compute the fight area that is going to be on screen and update fighter sprites position
	/// Also update the debug display
	/// </summary>
	public class BattleGUI : MonoBehaviour {
		#region serialize field

		[SerializeField] private GameObject _battleCoreGameObject;

		[SerializeField] private GameObject fighter1ImageObject;

		[SerializeField] private GameObject fighter2ImageObject;

		[SerializeField] private GameObject hitEffectObject1;

		[SerializeField] private GameObject hitEffectObject2;

		[SerializeField] private float _battleBoxLineWidth = 2f;

		[SerializeField] private GUIStyle debugTextStyle;

		[SerializeField] private bool drawDebug;

		#endregion

		#region private field

		private BattleCore battleCore;

		private Vector2 battleAreaTopLeftPoint;
		private Vector2 battleAreaBottomRightPoint;

		private Vector2 fightPointToScreenScale;
		private float centerPoint;

		private RectTransform rectTransform;

		private Image fighter1Image;
		private Image fighter2Image;

		private Animator hitEffectAnimator1;
		private Animator hitEffectAnimator2;

		#endregion

		private void Awake() {
			rectTransform = gameObject.GetComponent<RectTransform>();

			if (_battleCoreGameObject != null) {
				battleCore = _battleCoreGameObject.GetComponent<BattleCore>();
				battleCore.damageHandler += OnDamageHandler;
			}

			if (fighter1ImageObject != null) {
				fighter1Image = fighter1ImageObject.GetComponent<Image>();
			}

			if (fighter2ImageObject != null) {
				fighter2Image = fighter2ImageObject.GetComponent<Image>();
			}

			if (hitEffectObject1 != null) {
				hitEffectAnimator1 = hitEffectObject1.GetComponent<Animator>();
			}

			if (hitEffectObject2 != null) {
				hitEffectAnimator2 = hitEffectObject2.GetComponent<Animator>();
			}
		}

		private void OnDestroy() {
			battleCore.damageHandler -= OnDamageHandler;
		}

		private void FixedUpdate() {
			if (Input.GetKeyDown(KeyCode.F12)) {
				drawDebug = !drawDebug;
			}

			CalculateBattleArea();
			CalculateFightPointToScreenScale();

			UpdateSprite();
		}

		private void OnGUI() {
			if (drawDebug) {
				battleCore.Fighters.ForEach((f) => DrawFighter(f));

				Rect labelRect = new Rect(
					Screen.width * 0.4f,
					Screen.height * 0.95f,
					Screen.width * 0.2f,
					Screen.height * 0.05f
				);
				debugTextStyle.alignment = TextAnchor.UpperCenter;
				GUI.Label(labelRect, "F1=Pause/Resume, F2=Frame Step, F12=Debug Draw", debugTextStyle);

				//DrawBox(new Rect(battleAreaTopLeftPoint.x,
				//    battleAreaTopLeftPoint.y,
				//    battleAreaBottomRightPoint.x - battleAreaTopLeftPoint.x,
				//    battleAreaBottomRightPoint.y - battleAreaTopLeftPoint.y),
				//    Color.gray, true);
			}
		}

		private void UpdateSprite() {
			if (fighter1Image != null) {
				Sprite sprite = battleCore.Fighter1.GetCurrentMotionSprite();
				if (sprite != null) {
					fighter1Image.sprite = sprite;
				}

				Vector3 position = fighter1Image.transform.position;
				position.x = TransformHorizontalFightPointToScreen(battleCore.Fighter1.Position.x)
				             + battleCore.Fighter1.SpriteShakePosition;
				fighter1Image.transform.position = position;
			}

			if (fighter2Image != null) {
				Sprite sprite = battleCore.Fighter2.GetCurrentMotionSprite();
				if (sprite != null) {
					fighter2Image.sprite = sprite;
				}

				Vector3 position = fighter2Image.transform.position;
				position.x = TransformHorizontalFightPointToScreen(battleCore.Fighter2.Position.x)
				             + battleCore.Fighter2.SpriteShakePosition;
				fighter2Image.transform.position = position;
			}
		}

		private void DrawFighter(Fighter fighter) {
			Rect labelRect = new Rect(0, Screen.height * 0.86f, Screen.width * 0.22f, 50);
			if (fighter.IsFaceRight) {
				labelRect.x = Screen.width * 0.01f;
				debugTextStyle.alignment = TextAnchor.UpperLeft;
			} else {
				labelRect.x = Screen.width * 0.77f;
				debugTextStyle.alignment = TextAnchor.UpperRight;
			}

			GUI.Label(labelRect, fighter.Position.ToString(), debugTextStyle);

			labelRect.y += Screen.height * 0.03f;
			int frameAdvantage = battleCore.GetFrameAdvantage(fighter.IsFaceRight);
			string frameAdvText = frameAdvantage > 0 ? $"+{frameAdvantage}" : frameAdvantage.ToString();
			GUI.Label(
				labelRect,
				$"Frame: {fighter.CurrentActionFrame}/{fighter.CurrentActionFrameCount}({frameAdvText})",
				debugTextStyle
			);

			labelRect.y += Screen.height * 0.03f;
			GUI.Label(labelRect, $"Stun: {fighter.CurrentHitStunFrame}", debugTextStyle);

			labelRect.y += Screen.height * 0.03f;
			GUI.Label(
				labelRect,
				$"Action: {fighter.CurrentActionID} {(CommonActionID) fighter.CurrentActionID}",
				debugTextStyle
			);

			foreach (Hurtbox hurtbox in fighter.Hurtboxes) {
				DrawFightBox(hurtbox.Rect, Color.yellow, true);
			}

			if (fighter.Pushbox != null) {
				DrawFightBox(fighter.Pushbox.Rect, Color.blue, true);
			}

			foreach (Hitbox hitbox in fighter.Hitboxes) {
				if (hitbox.Proximity) {
					DrawFightBox(hitbox.Rect, Color.gray, true);
				} else {
					DrawFightBox(hitbox.Rect, Color.red, true);
				}
			}
		}

		private void DrawFightBox(Rect fightPointRect, Color color, bool isFilled) {
			Rect screenRect = new Rect();
			screenRect.width = fightPointRect.width * fightPointToScreenScale.x;
			screenRect.height = fightPointRect.height * fightPointToScreenScale.y;
			screenRect.x = TransformHorizontalFightPointToScreen(fightPointRect.x) - screenRect.width / 2;
			screenRect.y = battleAreaBottomRightPoint.y
			               - fightPointRect.y * fightPointToScreenScale.y
			               - screenRect.height;

			DrawBox(screenRect, color, isFilled);
		}

		private void DrawBox(Rect rect, Color color, bool isFilled) {
			float startX = rect.x;
			float startY = rect.y;
			float width = rect.width;
			float height = rect.height;
			float endX = startX + width;
			float endY = startY + height;

			DrawUtil.DrawLine(new Vector2(startX, startY), new Vector2(endX, startY), color, _battleBoxLineWidth);
			DrawUtil.DrawLine(new Vector2(startX, startY), new Vector2(startX, endY), color, _battleBoxLineWidth);
			DrawUtil.DrawLine(new Vector2(endX, endY), new Vector2(endX, startY), color, _battleBoxLineWidth);
			DrawUtil.DrawLine(new Vector2(endX, endY), new Vector2(startX, endY), color, _battleBoxLineWidth);

			if (isFilled) {
				Color rectColor = color;
				rectColor.a = 0.25f;
				DrawUtil.DrawRect(new Rect(startX, startY, width, height), rectColor);
			}
		}

		private float TransformHorizontalFightPointToScreen(float x) {
			return x * fightPointToScreenScale.x + centerPoint;
		}

		private float TransformVerticalFightPointToScreen(float y) {
			return Screen.height - battleAreaBottomRightPoint.y + y * fightPointToScreenScale.y;
		}

		private void CalculateBattleArea() {
			Vector3[] v = new Vector3[4];
			rectTransform.GetWorldCorners(v);
			battleAreaTopLeftPoint = new Vector2(v[1].x, Screen.height - v[1].y);
			battleAreaBottomRightPoint = new Vector2(v[3].x, Screen.height - v[3].y);
		}

		private void CalculateFightPointToScreenScale() {
			fightPointToScreenScale.x =
				(battleAreaBottomRightPoint.x - battleAreaTopLeftPoint.x) / battleCore.BattleAreaWidth;
			fightPointToScreenScale.y = (battleAreaBottomRightPoint.y - battleAreaTopLeftPoint.y)
			                            / battleCore.BattleAreaMaxHeight;

			centerPoint = (battleAreaBottomRightPoint.x + battleAreaTopLeftPoint.x) / 2;
		}

		private void OnDamageHandler(Fighter damagedFighter, Vector2 damagedPos, DamageResult damageResult) {
			// Set attacker fighter to last sibling so that it will get draw last and be on the most front
			if (damagedFighter == battleCore.Fighter1) {
				fighter2Image.transform.SetAsLastSibling();

				RequestHitEffect(hitEffectAnimator1, damagedPos, damageResult);
			} else if (damagedFighter == battleCore.Fighter2) {
				fighter1Image.transform.SetAsLastSibling();

				RequestHitEffect(hitEffectAnimator2, damagedPos, damageResult);
			}
		}

		private void RequestHitEffect(Animator hitEffectAnimator, Vector2 damagedPos, DamageResult damageResult) {
			hitEffectAnimator.SetTrigger("Hit");
			Vector3 position = hitEffectAnimator2.transform.position;
			position.x = TransformHorizontalFightPointToScreen(damagedPos.x);
			position.y = TransformVerticalFightPointToScreen(damagedPos.y);
			hitEffectAnimator.transform.position = position;

			if (damageResult == DamageResult.GuardBreak) {
				hitEffectAnimator.transform.localScale = new Vector3(5, 5, 1);
			} else if (damageResult == DamageResult.Damage) {
				hitEffectAnimator.transform.localScale = new Vector3(2, 2, 1);
			} else if (damageResult == DamageResult.Guard) {
				hitEffectAnimator.transform.localScale = new Vector3(1, 1, 1);
			}

			hitEffectAnimator.transform.SetAsLastSibling();
		}
	}
}