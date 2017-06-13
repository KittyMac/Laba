using UnityEngine;

/* The Labal langauge is very minimalistic. Each command is a single, non numerical character (excluding +/-). 
 * Each command can optionally be followed by a single numerical value, which makes sense only in the context of the command. For example,
 * "<120" would mean animate left 120 units.
 * 
 * x move to x position
 * y move to y position
 * 
 * < move left
 * > move right
 * ^ move up
 * v move down
 * 
 * f alpha fade
 * 
 * s uniform scale
 * 
 * r roll
 * p pitch
 * y yaw
 * 
 * d duration for current pipe
 * 
 * D staggaered duration based on sibling/child index
 * 
 * L loop (absolute) this segment (value is number of times to loop, -1 means loop infinitely)
 * 
 * l loop (relative) this segment (value is number of times to loop, -1 means loop infinitely)
 * 
 * e easing (we allow e# for shorthand or full easeInOutQuad)
 * 
 * | pipe animations (chain)
 * 
 * ! invert an action (instead of move left, its move to current position from the right)
 * 
 * [] concurrent Laba animations ( example: [>d2][!fd1] )
 * 
 * * means a choreographed routine; the * is followed by a series of operators which represent the preprogrammed actions
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

public class Laba : MonoBehaviour {

	public string labaNotation;
	public string initialLabaNotation;
	public bool loop;

	public void Start() {
		if (transform is RectTransform && labaNotation != null) {

			if (initialLabaNotation != null) {
				Laba.Animate (transform as RectTransform, initialLabaNotation, () => {
					initialLabaNotation = null;
					Start ();
				});
				//Debug.LogFormat ("{0}\n{1}", initialLabaNotation, Laba.Describe (transform as RectTransform, initialLabaNotation));
			} else {
				Laba.Animate (transform as RectTransform, labaNotation, () => {
					if (loop) {
						Start ();
					}
				});
				//Debug.LogFormat ("{0}\n{1}", labaNotation, Laba.Describe (transform as RectTransform, labaNotation));
			}
		}
	}





	public static float timeScale = 1.0f;




	public delegate float EasingAction(float fromValue, float toValue, float easing);
	public delegate LabaAction InitAction(LabaAction newAction);
	public delegate void PerformAction(RectTransform rt, float v, LabaAction action);
	public delegate void DescribeAction(StringBuilder sb, LabaAction action);

	public struct LabaAction {
		public bool inverse;
		public float rawValue;
		public char operatorChar;

		public RectTransform target;
		public float fromValue;
		public float toValue;
		public PerformAction action;
		public DescribeAction describe;
		public InitAction init;
		public EasingAction easing;
		public string easingName;


		public float userFloat_1;
		public float userFloat_2;
		public Vector2 userVector2_1;
		public Vector2 userVector2_2;
		public Vector3 userVector3_1;
		public Vector3 userVector3_2;
		public Vector4 userVector4_1;
		public Vector4 userVector4_2;

		public LabaAction(char operatorChar, RectTransform target, bool inverse, float rawValue, EasingAction easing, string easingName) {
			this.operatorChar = operatorChar;
			this.target = target;
			this.inverse = inverse;
			this.rawValue = rawValue;
			this.easing = easing;
			this.easingName = easingName;

			this.action = PerformActions[operatorChar];
			this.describe = DescribeActions[operatorChar];
			this.init = InitActions[operatorChar];

			if(this.inverse == false){
				this.fromValue = 0.0f;
				this.toValue = 1.0f;
			}else{
				this.fromValue = 1.0f;
				this.toValue = 0.0f;
			}

			userFloat_1 = 0.0f;
			userFloat_2 = 0.0f;
			userVector2_1 = Vector2.zero;
			userVector2_2 = Vector2.zero;
			userVector3_1 = Vector3.zero;
			userVector3_2 = Vector3.zero;
			userVector4_1 = Vector4.zero;
			userVector4_2 = Vector4.zero;

			if(this.init != null){
				this = this.init(this);
			}
		}

		public bool Init() {
			if (init != null) {
				LabaAction tempAction = new LabaAction (operatorChar, target, inverse, rawValue, easing, easingName);
				this.fromValue = tempAction.fromValue;
				this.toValue = tempAction.toValue;
				this.userFloat_1 = tempAction.userFloat_1;
				this.userFloat_2 = tempAction.userFloat_2;
				this.userVector2_1 = tempAction.userVector2_1;
				this.userVector2_2 = tempAction.userVector2_2;
				this.userVector3_1 = tempAction.userVector3_1;
				this.userVector3_2 = tempAction.userVector3_2;
				this.userVector4_1 = tempAction.userVector4_1;
				this.userVector4_2 = tempAction.userVector4_2;
				return true;
			}
			return false;
		}

		public bool Perform(float v) {
			if (action != null) {
				action (target, easing(fromValue, toValue, v), this);
				return true;
			}
			return false;
		}

		public bool Describe(StringBuilder sb) {
			if (action != null) {
				describe (sb, this);
				return true;
			}
			return false;
		}
	}


	// these are here for convenience; use autocomplete to quickly look up the number of your easing function
	public const int linear = 0;
	public const int easeOutQuad = 1;
	public const int easeInQuad = 2;
	public const int easeInOutQuad = 3;
	public const int easeInCubic = 4;
	public const int easeOutCubic = 5;
	public const int easeInOutCubic = 6;
	public const int easeInQuart = 7;
	public const int easeOutQuart = 8;
	public const int easeInOutQuart = 9;
	public const int easeInQuint = 10;
	public const int easeOutQuint = 11;
	public const int easeInOutQuint = 12;
	public const int easeInSine = 13;
	public const int easeOutSine = 14;
	public const int easeInOutSine = 15;
	public const int easeInExpo = 16;
	public const int easeOutExpo = 17;
	public const int easeInOutExpo = 18;
	public const int easeInCirc = 19;
	public const int easeOutCirc = 20;
	public const int easeInOutCirc = 21;
	public const int easeInBounce = 22;
	public const int easeOutBounce = 23;
	public const int easeInOutBounce = 24;

	private static EasingAction[] allEasings = new EasingAction[] {
		LeanTween.linear, 
		LeanTween.easeOutQuad, 
		LeanTween.easeInQuad, 
		LeanTween.easeInOutQuad, 
		LeanTween.easeInCubic, 
		LeanTween.easeOutCubic, 
		LeanTween.easeInOutCubic, 
		LeanTween.easeInQuart, 
		LeanTween.easeOutQuart, 
		LeanTween.easeInOutQuart, 
		LeanTween.easeInQuint, 
		LeanTween.easeOutQuint, 
		LeanTween.easeInOutQuint, 
		LeanTween.easeInSine, 
		LeanTween.easeOutSine, 
		LeanTween.easeInOutSine, 
		LeanTween.easeInExpo, 
		LeanTween.easeOutExpo, 
		LeanTween.easeInOutExpo, 
		LeanTween.easeInCirc, 
		LeanTween.easeOutCirc, 
		LeanTween.easeInOutCirc, 
		LeanTween.easeInBounce, 
		LeanTween.easeOutBounce, 
		LeanTween.easeInOutBounce
	};

	private static string[] allEasingsByName = new string[] {
		"ease linear", "ease out quad", "ease in quad", "ease in/out quad", "ease in cubic", "ease out cubic", "ease in/out cubic", "ease in quart", "ease out quart", "ease in/out quart", 
		"ease in quint", "ease out quint", "ease in/out quint", "ease in sine", "eas out sine", "ease in/out sine", "ease in expo", "ease out expo", "ease in out expo", "ease in circ", "ease out circ", "ease in/out circ", 
		"ease in bounce", "ease out bounce", "ease in/out bounce"
	};

	public static float LabaDefaultValue = float.MinValue;

	private static Dictionary<char,InitAction> InitActions;
	private static Dictionary<char,PerformAction> PerformActions;
	private static Dictionary<char,DescribeAction> DescribeActions;

	private static int kMaxPipes = 10;
	private static int kMaxActions = 10;
	private static float kDefaultDuration = 0.87f;

	private static LabaAction[,] ParseAnimationString(RectTransform rectTransform, string animationString) {
		int idx = 0;
		char[] charString = animationString.ToCharArray ();

		Func<char,bool> isOperator = (c) => {
			if (c == '|' || c == '!' || c == 'e') {
				return true;
			}
			return InitActions.ContainsKey (c);
		};
		Func<char,bool> isNumber = (c) => {
			return (c == '+' || c == '-' || c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9' || c == '.');
		};

		LabaAction[,] combinedActions = new LabaAction[kMaxPipes, kMaxActions];
		int currentPipeIdx = 0;
		int currentActionIdx = 0;
		EasingAction easingAction = LeanTween.easeInOutCubic;
		string easingName = "";

		while (idx < charString.Length) {

			bool invertNextOperator = false;
			char action = ' ';

			// find the next operator
			while (idx < charString.Length) {
				char c = charString [idx];
				if (isOperator (c)) {
					if (c == '!') {
						invertNextOperator = true;
					} else if (c == '|') {
						currentPipeIdx++;
						currentActionIdx = 0;
					} else {
						action = c;
						idx++;
						break;
					}
				}
				idx++;
			}

			// skip anything not important
			while (idx < charString.Length && isNumber (charString [idx]) == false && isOperator (charString [idx]) == false) {
				idx++;
			}

			float value = LabaDefaultValue;

			// if this is a number read it in
			if (idx < charString.Length && isNumber (charString [idx])) {
				
				// read in numerical value (if it exists)
				bool isNegativeNumber = false;
				if (charString [idx] == '+') {
					idx++;
				} else if (charString [idx] == '-') {
					isNegativeNumber = true;
					idx++;
				}

				value = 0.0f;

				bool fractionalPart = false;
				float fractionalValue = 10.0f;
				while (idx < charString.Length) {
					char c = charString [idx];
					if (isNumber (c)) {
						if (c >= '0' && c <= '9') {
							if (fractionalPart) {
								value = value + (c - '0') / fractionalValue;
								fractionalValue *= 10.0f;
							} else {
								value = value * 10 + (c - '0');
							}
						}
						if (c == '.') {
							fractionalPart = true;
						}
					}
					if (isOperator (c)) {
						break;
					}
					idx++;
				}

				if (isNegativeNumber) {
					value *= -1.0f;
				}
			}


			// execute the action?
			if (action != ' ') {
				if (InitActions.ContainsKey (action)) {
					//Debug.LogFormat (" [{3},{4}]   action: {0}   value: {1}   inverted: {2}", action, value, invertNextOperator, currentPipeIdx, currentActionIdx);
					combinedActions [currentPipeIdx, currentActionIdx] = new LabaAction (action, rectTransform, invertNextOperator, value, easingAction, easingName);
					currentActionIdx++;
				} else {
					if (action == 'e') {
						int easingIdx = Mathf.FloorToInt (value);
						if (easingIdx >= 0 && idx < allEasings.Length) {
							easingAction = allEasings [easingIdx];
							easingName = allEasingsByName [easingIdx];
						}
					}
				}
			}

		}

		return combinedActions;
	}





	private static void AnimateOne(RectTransform rectTransform, string animationString, Action onComplete = null, StringBuilder describe = null) {
		LabaAction[,] actionList = ParseAnimationString (rectTransform, animationString);
		PerformAction durationAction1 = PerformActions ['d'];
		PerformAction durationAction2 = PerformActions ['D'];
		PerformAction loopAction1 = PerformActions ['L'];
		PerformAction loopAction2 = PerformActions ['l'];

		int numOfPipes = 0;

		float duration = 0.0f;
		float looping = 1.0f;
		bool loopingRelative = false;
		for (int i = 0; i < kMaxPipes; i++) {
			if (actionList [i, 0].action != null) {
				numOfPipes++;

				float durationForPipe = kDefaultDuration;
				for (int j = 0; j < kMaxActions; j++) {
					if (actionList [i, j].action == durationAction1 || actionList [i, j].action == durationAction2) {
						durationForPipe = actionList [i, j].fromValue;
					}
					if (actionList [i, j].action == loopAction1) {
						looping = actionList [i, j].fromValue;
					}
					if (actionList [i, j].action == loopAction2) {
						loopingRelative = true;
						looping = actionList [i, j].fromValue;
					}
				}
				duration += durationForPipe;
			}
		}

		// having only a single pipe makes things much more efficient, so treat it separately
		if (numOfPipes == 1) {

			if (loopingRelative) {
				float lastV = 1.0f;
				LeanTween.value (rectTransform.gameObject, (v) => {
					if (v < lastV) {
						for (int j = 0; j < kMaxActions; j++) {
							if (!actionList [0, j].Init ()) {
								break;
							}
						}
					}
					lastV = v;
					for (int i = 0; i < kMaxActions; i++) {
						if (!actionList [0, i].Perform (v)) {
							break;
						}
					}
				}, 0.0f, 1.0f, duration).setOnComplete (onComplete).setLoopCount ((int)looping);
			} else {
				for (int j = 0; j < kMaxActions; j++) {
					if (!actionList [0, j].Init ()) {
						break;
					}
				}
				LeanTween.value (rectTransform.gameObject, (v) => {
					for (int i = 0; i < kMaxActions; i++) {
						if (!actionList [0, i].Perform (v)) {
							break;
						}
					}
				}, 0.0f, 1.0f, duration * timeScale).setOnComplete (onComplete).setLoopCount ((int)looping);
			}
		} else {

			// for multiple pipes, the only mechanism leantween provides for this in onComplete actions
			// unfortunately, this means we need to create an Action for each pipe


			Action nextAction = null;
			for (int pipeIdx = numOfPipes - 1; pipeIdx >= 0; pipeIdx--) {

				float durationForPipe = kDefaultDuration;
				float loopingForPipe = 1.0f;
				bool loopingRelativeForPipe = false;
				for (int j = 0; j < kMaxActions; j++) {
					if (actionList [pipeIdx, j].action == durationAction1 || actionList [pipeIdx, j].action == durationAction2) {
						durationForPipe = actionList [pipeIdx, j].fromValue;
					}
					if (actionList [pipeIdx, j].action == loopAction1) {
						loopingForPipe = actionList [pipeIdx, j].fromValue;
					}
					if (actionList [pipeIdx, j].action == loopAction2) {
						loopingRelativeForPipe = true;
						loopingForPipe = actionList [pipeIdx, j].fromValue;
					}
				}

				int idx = pipeIdx;
				Action localNextAction = nextAction;
				if (localNextAction == null) {
					localNextAction = onComplete;
				}
				if (localNextAction == null) {
					localNextAction = () => {};
				}
				nextAction = () => {

					if (loopingRelativeForPipe) {
						float lastV = 1.0f;
						LeanTween.value (rectTransform.gameObject, (v) => {
							if (v < lastV) {
								for (int j = 0; j < kMaxActions; j++) {
									if (!actionList [idx, j].Init ()) {
										break;
									}
								}
							}
							lastV = v;
							for (int j = 0; j < kMaxActions; j++) {
								if (!actionList [idx, j].Perform (v)) {
									break;
								}
							}
						}, 0.0f, 1.0f, durationForPipe).setOnComplete (localNextAction).setLoopCount ((int)loopingForPipe);
					} else {
						for (int j = 0; j < kMaxActions; j++) {
							if (!actionList [idx, j].Init ()) {
								break;
							}
						}
						LeanTween.value (rectTransform.gameObject, (v) => {
							for (int j = 0; j < kMaxActions; j++) {
								if (!actionList [idx, j].Perform (v)) {
									break;
								}
							}
						}, 0.0f, 1.0f, durationForPipe * timeScale).setOnComplete (localNextAction).setLoopCount ((int)loopingForPipe);
					}

				};
			}

			if (nextAction != null) {
				nextAction ();
			} else {
				if (onComplete != null) {
					onComplete ();
				}
			}

		}
	}

	public static void Animate(RectTransform rectTransform, string animationString, Action onComplete = null) {

		if (animationString.Contains ("[")) {
			string[] parts = animationString.Replace ('[', ' ').Split (']');
			foreach (string part in parts) {
				if (part.Length > 0) {
					AnimateOne (rectTransform, part, onComplete);
					onComplete = null;
				}
			}
		} else {
			AnimateOne (rectTransform, animationString, onComplete);
			onComplete = null;
		}

		LeanTween.update ();
	}




	private static void DescribeOne(RectTransform rt, string animationString, StringBuilder sb) {
		LabaAction[,] actionList = ParseAnimationString (rt, animationString);
		PerformAction durationAction1 = PerformActions ['d'];
		PerformAction durationAction2 = PerformActions ['D'];
		PerformAction loopingAction1 = PerformActions ['L'];
		PerformAction loopingAction2 = PerformActions ['l'];

		int numOfPipes = 0;

		float duration = 0.0f;
		int looping = 1;
		string loopingRelative = "absolute";
		for (int i = 0; i < kMaxPipes; i++) {
			if (actionList [i, 0].action != null) {
				numOfPipes++;

				float durationForPipe = kDefaultDuration;
				for (int j = 0; j < kMaxActions; j++) {
					if (actionList [i, j].action == durationAction1 || actionList [i, j].action == durationAction2) {
						durationForPipe = actionList [i, j].fromValue;
					}
					if (actionList [i, j].action == loopingAction1) {
						looping = (int)actionList [i, j].fromValue;
					}
					if (actionList [i, j].action == loopingAction2) {
						looping = (int)actionList [i, j].fromValue;
						loopingRelative = "relative";
					}
				}
				duration += durationForPipe;
			}
		}

		// having only a single pipe makes things much more efficient, so treat it separately
		if (numOfPipes == 1) {
			int stringLengthBefore = sb.Length;

			for (int i = 0; i < kMaxActions; i++) {
				if (!actionList [0, i].Describe (sb)) {
					break;
				}
			}


			if (looping > 1) {
				sb.AppendFormat (" {1} repeating {0} times, ", looping, loopingRelative);
			} else if (looping == -1) {
				sb.AppendFormat (" {0} repeating forever, ", loopingRelative);
			}
				
			if (stringLengthBefore != sb.Length) {
				sb.AppendFormat (" {0}  ", actionList [0, 0].easingName);

				sb.Length = sb.Length - 2;
				if (duration == 0.0f) {
					sb.AppendFormat (" instantly.");
				} else {
					sb.AppendFormat (" over {0} seconds.", duration * timeScale);
				}
			} else {
				if (sb.Length > 2) {
					sb.Length = sb.Length - 2;
				}
				sb.AppendFormat (" wait for {0} seconds.", duration * timeScale);
			}

		} else {
			
			for (int pipeIdx = 0; pipeIdx < numOfPipes; pipeIdx++) {
				int stringLengthBefore = sb.Length;

				float durationForPipe = kDefaultDuration;
				int loopingForPipe = 1;
				string loopingRelativeForPipe = "absolute";
				for (int j = 0; j < kMaxActions; j++) {
					if (actionList [pipeIdx, j].action == durationAction1 || actionList [pipeIdx, j].action == durationAction2) {
						durationForPipe = actionList [pipeIdx, j].fromValue;
					}
					if (actionList [pipeIdx, j].action == loopingAction1) {
						loopingForPipe = (int)actionList [pipeIdx, j].fromValue;
					}
					if (actionList [pipeIdx, j].action == loopingAction2) {
						loopingForPipe = (int)actionList [pipeIdx, j].fromValue;
						loopingRelativeForPipe = "relative";
					}
				}

				int idx = pipeIdx;
				for (int j = 0; j < kMaxActions; j++) {
					if (!actionList [idx, j].Init ()) {
						break;
					}
				}

				for (int j = 0; j < kMaxActions; j++) {
					if (!actionList [idx, j].Describe (sb)) {
						break;
					}
				}

				if (loopingForPipe > 1) {
					sb.AppendFormat (" {1} repeating {0} times, ", loopingForPipe, loopingRelativeForPipe);
				} else if (loopingForPipe == -1) {
					sb.AppendFormat (" {0} repeating forever, ", loopingRelativeForPipe);
				}

				if (stringLengthBefore != sb.Length) {
					sb.AppendFormat (" {0}  ", actionList [idx, 0].easingName);

					sb.Length = sb.Length - 2;
					if (durationForPipe == 0.0f) {
						sb.AppendFormat (" instantly.");
					} else {
						sb.AppendFormat (" over {0} seconds.", durationForPipe * timeScale);
					}
				} else {
					sb.AppendFormat (" wait for {0} seconds.", durationForPipe * timeScale);
				}

				if (pipeIdx + 1 < numOfPipes) {
					sb.AppendFormat (" Once complete then  ");
				}
			}
		}
	}

	public static string Describe(RectTransform rt, string animationString) {

		if (animationString == null || animationString.Length == 0) {
			return "do nothing";
		}

		StringBuilder sb = new StringBuilder ();

		if (animationString.Contains ("[")) {
			string[] parts = animationString.Replace ('[', ' ').Split (']');
			int animNumber = 0;
			sb.AppendFormat ("Perform a series of animations at the same time.\n");
			foreach (string part in parts) {
				if (part.Length > 0) {
					sb.AppendFormat ("Animation #{0} will ", animNumber+1);
					DescribeOne (rt, part, sb);
					sb.AppendFormat ("\n");
					animNumber++;
				}
			}
		} else {
			DescribeOne (rt, animationString, sb);
		}
			
		if (sb.Length > 0) {
			// upper case the starting letter
			sb.Insert (0, sb.ToString ().Substring (0, 1).ToUpper ());
			sb.Remove (1, 1);

			sb.Replace ("  ", " ");
		}

		return sb.ToString ();
	}




	static Laba() {
		InitActions = new Dictionary<char, InitAction> ();
		PerformActions = new Dictionary<char, PerformAction> ();
		DescribeActions = new Dictionary<char, DescribeAction> ();

		#region LOOP ABSOLUTE
		RegisterOperation(
			'L',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = -1;
				}
				newAction.fromValue = newAction.toValue = newAction.rawValue;
				return newAction;
			},
			(rt, v, action) => { },
			(sb, action) => { }
		);
		#endregion

		#region LOOP RELATIVE
		RegisterOperation(
			'l',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = -1;
				}
				newAction.fromValue = newAction.toValue = newAction.rawValue;
				return newAction;
			},
			(rt, v, action) => { },
			(sb, action) => { }
		);
		#endregion

		#region DURATION
		RegisterOperation(
			'd',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = kDefaultDuration;
				}
				newAction.fromValue = newAction.toValue = newAction.rawValue;
				return newAction;
			},
			(rt, v, action) => { },
			(sb, action) => { }
		);
		#endregion

		#region STAGGERED DURATION
		RegisterOperation(
			'D',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = kDefaultDuration;
				}
				newAction.fromValue = newAction.toValue = newAction.rawValue * newAction.target.GetSiblingIndex();
				return newAction;
			},
			(rt, v, action) => { },
			(sb, action) => { }
		);
		#endregion


		#region MOVE TO X POSITION
		RegisterOperation(
			'x',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 0;
				}

				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.anchoredPosition.x;
					newAction.toValue = newAction.rawValue;
				}else{
					newAction.fromValue = newAction.rawValue;
					newAction.toValue = newAction.target.anchoredPosition.x;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.anchoredPosition = new Vector2 (v, rt.anchoredPosition.y);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("move to {0} x pos, ", action.rawValue); 
				} else {
					sb.AppendFormat("move from {0} x pos, ", action.rawValue); 
				}
			}
		);
		#endregion

		#region MOVE TO Y POSITION
		RegisterOperation(
			'y',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 0;
				}

				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.anchoredPosition.y;
					newAction.toValue = newAction.rawValue;
				}else{
					newAction.fromValue = newAction.rawValue;
					newAction.toValue = newAction.target.anchoredPosition.y;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.anchoredPosition = new Vector2 (rt.anchoredPosition.x, v);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("move to {0} y pos, ", action.rawValue); 
				} else {
					sb.AppendFormat("move from {0} y pos, ", action.rawValue); 
				}
			}
		);
		#endregion



		#region MOVE LEFT
		RegisterOperation(
			'<',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = newAction.target.rect.width;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.anchoredPosition.x;
					newAction.toValue = newAction.target.anchoredPosition.x - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.anchoredPosition.x + newAction.rawValue;
					newAction.toValue = newAction.target.anchoredPosition.x;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.anchoredPosition = new Vector2 (v, rt.anchoredPosition.y);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("move left {0} units, ", action.rawValue); 
				} else {
					sb.AppendFormat("move in from left {0} units, ", action.rawValue); 
				}
			}
		);
		#endregion

		#region MOVE RIGHT
		RegisterOperation(
			'>',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = newAction.target.rect.width;
				}

				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.anchoredPosition.x;
					newAction.toValue = newAction.target.anchoredPosition.x + newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.anchoredPosition.x - newAction.rawValue;
					newAction.toValue = newAction.target.anchoredPosition.x;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.anchoredPosition = new Vector2 (v, rt.anchoredPosition.y);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("move right {0} units, ", action.rawValue); 
				} else {
					sb.AppendFormat("move in from right {0} units, ", action.rawValue); 
				}
			}
		);
		#endregion

		#region MOVE UP
		RegisterOperation(
			'^',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = newAction.target.rect.height;
				}

				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.anchoredPosition.y;
					newAction.toValue = newAction.target.anchoredPosition.y + newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.anchoredPosition.y - newAction.rawValue;
					newAction.toValue = newAction.target.anchoredPosition.y;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.anchoredPosition = new Vector2 (rt.anchoredPosition.x, v);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("move up {0} units, ", action.rawValue); 
				} else {
					sb.AppendFormat("move in from above {0} units, ", action.rawValue); 
				}
			}
		);
		#endregion

		#region MOVE DOWN
		RegisterOperation(
			'v',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = newAction.target.rect.height;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.anchoredPosition.y;
					newAction.toValue = newAction.target.anchoredPosition.y - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.anchoredPosition.y + newAction.rawValue;
					newAction.toValue = newAction.target.anchoredPosition.y ;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.anchoredPosition = new Vector2 (rt.anchoredPosition.x, v);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("move down {0} units, ", action.rawValue); 
				} else {
					sb.AppendFormat("move in from below {0} units, ", action.rawValue); 
				}
			}
		);
		#endregion

		#region MOVE ALONG Z AXIS
		RegisterOperation(
			'z',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = (newAction.target.rect.height + newAction.target.rect.width) * 0.5f;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.anchoredPosition3D.z;
					newAction.toValue = newAction.target.anchoredPosition3D.z - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.anchoredPosition3D.z + newAction.rawValue;
					newAction.toValue = newAction.target.anchoredPosition3D.z ;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.anchoredPosition3D = new Vector3 (rt.anchoredPosition3D.x, rt.anchoredPosition3D.y, v);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("move along z axis {0} units, ", action.rawValue); 
				} else {
					sb.AppendFormat("move in from z axis {0} units, ", action.rawValue); 
				}
			}
		);
		#endregion


		#region UNIFORM SCALE
		RegisterOperation(
			's',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 1.0f;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.localScale.x;
					newAction.toValue = newAction.rawValue;
				}else{
					newAction.fromValue = (newAction.rawValue > 0.5f ? 0.0f : 1.0f);
					newAction.toValue = newAction.rawValue;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.localScale = new Vector3 (v, v, 1.0f);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("scale to {0}%, ", Mathf.FloorToInt(action.rawValue * 100.0f));
				} else {
					sb.AppendFormat("scale in from {0}%, ", Mathf.FloorToInt(action.rawValue * 100.0f));
				}
			}
		);
		#endregion

		#region ROLL
		RegisterOperation(
			'r',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 0.0f;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.localRotation.eulerAngles.z;
					newAction.toValue = newAction.target.localRotation.eulerAngles.z - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.localRotation.eulerAngles.z + newAction.rawValue;
					newAction.toValue = newAction.target.localRotation.eulerAngles.z;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.localRotation = Quaternion.Euler(rt.localRotation.eulerAngles.x, rt.localRotation.eulerAngles.y, v);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("rotate around z by {0}°, ", action.rawValue);
				} else {
					sb.AppendFormat("rotate in from around z by {0}°, ", action.rawValue);
				}
			}
		);
		#endregion

		#region PITCH
		RegisterOperation(
			'p',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 0.0f;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.localRotation.eulerAngles.x;
					newAction.toValue = newAction.target.localRotation.eulerAngles.x - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.localRotation.eulerAngles.x + newAction.rawValue;
					newAction.toValue = newAction.target.localRotation.eulerAngles.x;
				}

				newAction.userFloat_1 = newAction.target.localRotation.eulerAngles.y;
				newAction.userFloat_2 = newAction.target.localRotation.eulerAngles.z;

				return newAction;
			},
			(rt, v, action) => {
				rt.localRotation = Quaternion.Euler(v, action.userFloat_1, action.userFloat_2);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("rotate around x by {0}°, ", action.rawValue);
				} else {
					sb.AppendFormat("rotate in from around x by {0}°, ", action.rawValue);
				}
			}
		);
		#endregion


		#region YAW
		RegisterOperation(
			'y',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 0.0f;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.localRotation.eulerAngles.y;
					newAction.toValue = newAction.target.localRotation.eulerAngles.y - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.localRotation.eulerAngles.y + newAction.rawValue;
					newAction.toValue = newAction.target.localRotation.eulerAngles.y;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.localRotation = Quaternion.Euler(rt.localRotation.eulerAngles.x, v, rt.localRotation.eulerAngles.z);
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("rotate around y by {0}°, ", action.rawValue);
				} else {
					sb.AppendFormat("rotate in from around y by {0}°, ", action.rawValue);
				}
			}
		);
		#endregion

		#region FADE

		RegisterOperation(
			'f', 
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 1.0f;
				}
				CanvasGroup canvasGroup = newAction.target.gameObject.GetComponent<CanvasGroup>();
				if(canvasGroup == null){
					canvasGroup = newAction.target.gameObject.AddComponent<CanvasGroup>();
				}
				if(newAction.inverse == false){
					newAction.fromValue = canvasGroup.alpha;
					newAction.toValue = newAction.rawValue;
				}else{
					newAction.fromValue = (newAction.rawValue > 0.5f ? 0.0f : 1.0f);
					newAction.toValue = newAction.rawValue;
				}
				return newAction;
			},
			(rt, v, action) => {
				CanvasGroup canvasGroup = rt.gameObject.GetComponent<CanvasGroup>();
				canvasGroup.alpha = v;
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("fade to {0}%, ", Mathf.FloorToInt(action.rawValue * 100.0f));
				} else {
					sb.AppendFormat("fade from {0}% to {1}%, ", Mathf.FloorToInt(action.fromValue * 100.0f), Mathf.FloorToInt(action.toValue * 100.0f));
				}
			}
		);

		#endregion
	}


	static public void RegisterOperation(char charOperator, InitAction init, PerformAction perform, DescribeAction describe){
		InitActions [charOperator] = init;
		PerformActions [charOperator] = perform;
		DescribeActions [charOperator] = describe;
	}
}