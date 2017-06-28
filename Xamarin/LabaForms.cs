/* The Labal langauge is very minimalistic. Each command is a single, non numerical character (excluding +/-). 
 * Each command can optionally be followed by a single numerical value, which makes sense only in the context of the command. For example,
 * "<120" would mean animate left 120 units.
 * 
 * NOTE: Implied values for <v>^ are currently squirrely for Xamarin Forms and won't work prior to the views being laid out (as width/height are -1)
 * 
 * < move left
 * > move right
 * ^ move up
 * v move down
 * 
 * x move to x position
 * y move to y position
 * 
 * f alpha fade
 * 
 * s uniform scale
 * 
 * r roll
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
using Xamarin.Forms;

public class LabaForms : Object {

	public string labaNotation;
	public string initialLabaNotation;
	public bool loop;

	public static double timeScale = 1.0f;


	public delegate LabaAction InitAction(LabaAction newAction);
	public delegate void PerformAction(View rt, double v, LabaAction action);
	public delegate void DescribeAction(StringBuilder sb, LabaAction action);

	public struct LabaAction {
		public bool inverse;
		public double rawValue;
		public char operatorChar;

		public View target;
		public double fromValue;
		public double toValue;
		public PerformAction action;
		public DescribeAction describe;
		public InitAction init;
		public Easing easing;
		public string easingName;


		public double userFloat_1;
		public double userFloat_2;

		public LabaAction(char operatorChar, View target, bool inverse, double rawValue, Easing easing, string easingName) {
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
				return true;
			}
			return false;
		}

		public bool Perform(double v) {
			if (action != null) {
				action (target, fromValue + (toValue - fromValue) * easing.Ease(v), this);
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
	public const int easeInCubic = 1;
	public const int easeOutCubic = 2;
	public const int easeInOutCubic = 3;
	public const int easeInSine = 4;
	public const int easeOutSine = 5;
	public const int easeInOutSine = 6;
	public const int easeInBounce = 7;
	public const int easeOutBounce = 8;
	public const int easeInSpring = 9;
	public const int easeOutSpring = 10;


	private static Easing[] allEasings = new Easing[] {
        Easing.Linear, Easing.CubicIn, Easing.CubicOut, Easing.CubicInOut, Easing.SinIn, Easing.SinOut, Easing.SinInOut, Easing.BounceIn, Easing.BounceOut, Easing.SpringIn, Easing.SpringOut
	};

    private static string[] allEasingsByName = new string[] {
        "ease linear", "ease cubic in","ease cubic out","ease cubic in out","ease sin in","ease sin out","ease sin in out","ease bounce in","ease bounce out","ease spring in","sping out"
    };

	public static double LabaDefaultValue = double.MinValue;

	private static Dictionary<char,InitAction> InitActions;
	private static Dictionary<char,PerformAction> PerformActions;
	private static Dictionary<char,DescribeAction> DescribeActions;

	private static int kMaxPipes = 10;
	private static int kMaxActions = 10;
	private static double kDefaultDuration = 0.87f;

	private static LabaAction[,] ParseAnimationString(View rectTransform, string animationString) {
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
		Easing easingAction = Easing.CubicInOut;
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

			double value = LabaDefaultValue;

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
				double fractionalValue = 10.0f;
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
                        int easingIdx = (int)Math.Floor (value);
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





	private static void AnimateOne(View rectTransform, string animationString, Action onComplete = null, StringBuilder describe = null) {
		LabaAction[,] actionList = ParseAnimationString (rectTransform, animationString);
		PerformAction durationAction1 = PerformActions ['d'];
		PerformAction durationAction2 = PerformActions ['D'];
		PerformAction loopAction1 = PerformActions ['L'];
		PerformAction loopAction2 = PerformActions ['l'];

		int numOfPipes = 0;

		double duration = 0.0f;
		double looping = 1.0f;
		bool loopingRelative = false;
		for (int i = 0; i < kMaxPipes; i++) {
			if (actionList [i, 0].action != null) {
				numOfPipes++;

				double durationForPipe = kDefaultDuration;
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
				double lastV = 1.0f;

                int localLoops = (int)looping;
                new Animation((v) =>
                {
                    if (v < lastV)
                    {
                        for (int j = 0; j < kMaxActions; j++)
                        {
                            if (!actionList[0, j].Init())
                            {
                                break;
                            }
                        }
                    }
                    lastV = v;
                    for (int i = 0; i < kMaxActions; i++)
                    {
                        if (!actionList[0, i].Perform(v))
                        {
                            break;
                        }
                    }
                }, 0.0f, 1.0f, Easing.Linear).Commit(rectTransform, "LabaAnimation", 16, (uint)(duration * timeScale * 1000), Easing.Linear, (x, y) =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                }, () => {
                    if (localLoops < 0)
                        return true;
                    localLoops--;
                    return localLoops > 0;
                });
			} else {
				for (int j = 0; j < kMaxActions; j++) {
					if (!actionList [0, j].Init ()) {
						break;
					}
				}

                int localLoops = (int)looping;
				new Animation((v) =>
				{
					for (int i = 0; i < kMaxActions; i++)
					{
						if (!actionList[0, i].Perform(v))
						{
							break;
						}
					}
                }, 0.0f, 1.0f, Easing.Linear).Commit(rectTransform, "LabaAnimation", 16, (uint)(duration * timeScale * 1000), Easing.Linear, (x, y) =>
				{
					if (onComplete != null)
					{
						onComplete();
					}
				}, () =>
				{
					if (localLoops < 0)
						return true; 
					localLoops--;
					return localLoops > 0;
				});
			}
		} else {

			// for multiple pipes, the only mechanism leantween provides for this in onComplete actions
			// unfortunately, this means we need to create an Action for each pipe


			Action nextAction = null;
			for (int pipeIdx = numOfPipes - 1; pipeIdx >= 0; pipeIdx--) {

				double durationForPipe = kDefaultDuration;
				double loopingForPipe = 1.0f;
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
						double lastV = 1.0f;

                        int localLoops = (int)loopingForPipe;
                        new Animation((v) =>
						{
							if (v < lastV)
							{
								for (int j = 0; j < kMaxActions; j++)
								{
									if (!actionList[idx, j].Init())
									{
										break;
									}
								}
							}
							lastV = v;
							for (int j = 0; j < kMaxActions; j++)
							{
								if (!actionList[idx, j].Perform(v))
								{
									break;
								}
							}
						}, 0.0f, 1.0f, Easing.Linear).Commit(rectTransform, "LabaAnimation", 16, (uint)(durationForPipe * timeScale * 1000), Easing.Linear, (x, y) =>
						{
							if (localNextAction != null)
							{
								localNextAction();
							}
						}, () =>
						{
							localLoops--;
							return localLoops > 0;
						});
					} else {
						for (int j = 0; j < kMaxActions; j++) {
							if (!actionList [idx, j].Init ()) {
								break;
							}
						}

                        int localLoops = (int)loopingForPipe;
						new Animation((v) =>
						{
							for (int j = 0; j < kMaxActions; j++)
							{
								if (!actionList[idx, j].Perform(v))
								{
									break;
								}
							}
						}, 0.0f, 1.0f, Easing.Linear).Commit(rectTransform, "LabaAnimation", 16, (uint)(durationForPipe * timeScale * 1000), Easing.Linear, (x, y) =>
						{
							if (localNextAction != null)
							{
								localNextAction();
							}
						}, () =>
						{
							localLoops--;
							return localLoops > 0;
						});
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

	public static void Animate(View view, string animationString, Action onComplete = null) {

        LabaSharedInit();

		if (animationString.Contains ("[")) {
			string[] parts = animationString.Replace ('[', ' ').Split (']');
			foreach (string part in parts) {
				if (part.Length > 0) {
					AnimateOne (view, part, onComplete);
					onComplete = null;
				}
			}
		} else {
			AnimateOne (view, animationString, onComplete);
			onComplete = null;
		}
	}


	private static void DescribeOne(View rt, string animationString, StringBuilder sb) {
		LabaAction[,] actionList = ParseAnimationString (rt, animationString);
		PerformAction durationAction1 = PerformActions ['d'];
		PerformAction durationAction2 = PerformActions ['D'];
		PerformAction loopingAction1 = PerformActions ['L'];
		PerformAction loopingAction2 = PerformActions ['l'];

		int numOfPipes = 0;

		double duration = 0.0f;
		int looping = 1;
		string loopingRelative = "absolute";
		for (int i = 0; i < kMaxPipes; i++) {
			if (actionList [i, 0].action != null) {
				numOfPipes++;

				double durationForPipe = kDefaultDuration;
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

				double durationForPipe = kDefaultDuration;
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

	public static string Describe(View rt, string animationString) {

		LabaSharedInit();

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




	static void LabaSharedInit() {
        if(InitActions != null) {
            return;
        }

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

                // find the ancestral layout?
                int siblingIndex = 0;

				// TODO: how to get in forms? This seems pretty heavy handed
				ILayoutController parent = newAction.target.Parent as ILayoutController;
	            if(parent != null) {
	                for (int i = 0; i < parent.Children.Count; i++){
	                    if(newAction.target == parent.Children[i]){
                            siblingIndex = i;
	                    }
	                }
	            }
                

                newAction.fromValue = newAction.toValue = newAction.rawValue * siblingIndex;
				return newAction;
			},
			(rt, v, action) => { },
			(sb, action) => { }
		);
		#endregion

		#region MOVE LEFT
		RegisterOperation(
			'<',
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
                    newAction.rawValue = newAction.target.Width;
				}
				if(newAction.inverse == false){
                    newAction.fromValue = newAction.target.TranslationX;
					newAction.toValue = newAction.target.TranslationX - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.TranslationX + newAction.rawValue;
					newAction.toValue = newAction.target.TranslationX;
				}
				return newAction;
			},
			(rt, v, action) => {
                rt.TranslationX = v;
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
                    newAction.rawValue = newAction.target.Width;
				}

				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.TranslationX;
					newAction.toValue = newAction.target.TranslationX + newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.TranslationX - newAction.rawValue;
					newAction.toValue = newAction.target.TranslationX;
				}
				return newAction;
			},
			(rt, v, action) => {
                rt.TranslationX = v;
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
                    newAction.rawValue = newAction.target.Height;
				}

				if(newAction.inverse == false){
                    newAction.fromValue = newAction.target.TranslationY;
					newAction.toValue = newAction.target.TranslationY + newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.TranslationY - newAction.rawValue;
					newAction.toValue = newAction.target.TranslationY;
				}
				return newAction;
			},
			(rt, v, action) => {
                rt.TranslationY = v;
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
                    newAction.rawValue = newAction.target.Height;
				}
				if(newAction.inverse == false){
					newAction.fromValue = newAction.target.TranslationY;
					newAction.toValue = newAction.target.TranslationY - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.TranslationY + newAction.rawValue;
					newAction.toValue = newAction.target.TranslationY ;
				}
				return newAction;
			},
			(rt, v, action) => {
				rt.TranslationY = v;
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

		#region MOVE X
		RegisterOperation(
			'x',
			(newAction) =>
			{
				if (newAction.rawValue == LabaDefaultValue)
				{
					newAction.rawValue = newAction.target.Width;
				}

				if (newAction.inverse == false)
				{
					newAction.fromValue = newAction.target.TranslationX;
					newAction.toValue = newAction.rawValue;
				}
				else
				{
					newAction.fromValue = newAction.rawValue;
					newAction.toValue = newAction.target.TranslationX;
				}
				return newAction;
			},
			(rt, v, action) =>
			{
				rt.TranslationX = v;
			},
			(sb, action) =>
			{
				if (action.inverse == false)
				{
					sb.AppendFormat("move to x position {0}, ", action.rawValue);
				}
				else
				{
					sb.AppendFormat("move in from x position {0}, ", action.rawValue);
				}
			}
		);
		#endregion

		#region MOVE Y
		RegisterOperation(
			'y',
			(newAction) =>
			{
				if (newAction.rawValue == LabaDefaultValue)
				{
					newAction.rawValue = newAction.target.Height;
				}
				if (newAction.inverse == false)
				{
					newAction.fromValue = newAction.target.TranslationY;
					newAction.toValue = newAction.rawValue;
				}
				else
				{
					newAction.fromValue = newAction.rawValue;
					newAction.toValue = newAction.target.TranslationY;
				}
				return newAction;
			},
			(rt, v, action) =>
			{
				rt.TranslationY = v;
			},
			(sb, action) =>
			{
				if (action.inverse == false)
				{
					sb.AppendFormat("move to y position {0}, ", action.rawValue);
				}
				else
				{
					sb.AppendFormat("move in from y position {0}, ", action.rawValue);
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
                    newAction.fromValue = newAction.target.Scale;
					newAction.toValue = newAction.rawValue;
				}else{
					newAction.fromValue = (newAction.rawValue > 0.5f ? 0.0f : 1.0f);
					newAction.toValue = newAction.rawValue;
				}
				return newAction;
			},
			(rt, v, action) => {
                rt.Scale = v;
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("scale to {0}%, ", (int)(action.rawValue * 100.0f));
				} else {
					sb.AppendFormat("scale in from {0}%, ", (int)(action.rawValue * 100.0f));
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
                    newAction.fromValue = newAction.target.Rotation;
					newAction.toValue = newAction.target.Rotation - newAction.rawValue;
				}else{
					newAction.fromValue = newAction.target.Rotation + newAction.rawValue;
					newAction.toValue = newAction.target.Rotation;
				}
				return newAction;
			},
			(rt, v, action) => {
                rt.Rotation = v;
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

		#region FADE

		RegisterOperation(
			'f', 
			(newAction) => {
				if (newAction.rawValue == LabaDefaultValue) {
					newAction.rawValue = 1.0f;
				}
				if(newAction.inverse == false){
                    newAction.fromValue = newAction.target.Opacity;
					newAction.toValue = newAction.rawValue;
				}else{
					newAction.fromValue = (newAction.rawValue > 0.5f ? 0.0f : 1.0f);
					newAction.toValue = newAction.rawValue;
				}
				return newAction;
			},
			(rt, v, action) => {
                rt.Opacity = v;
			},
			(sb, action) => { 
				if(action.inverse == false) {
					sb.AppendFormat("fade to {0}%, ", (int)(action.rawValue * 100.0f));
				} else {
					sb.AppendFormat("fade from {0}% to {1}%, ", (int)(action.fromValue * 100.0f), (int)(action.toValue * 100.0f));
				}
			}
		);

		#endregion
	}


	static public void RegisterOperation(char charOperator, InitAction init, PerformAction perform, DescribeAction describe){
        LabaSharedInit();
		InitActions [charOperator] = init;
		PerformActions [charOperator] = perform;
		DescribeActions [charOperator] = describe;
	}
}