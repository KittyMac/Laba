/* The Labal notation is very minimalistic. Each command is a single, non numerical character (excluding +/-).
 * Each command can optionally be followed by a single numerical value, which makes sense only in the context of the command. For example,
 * "<120" would mean animate left 120 units.
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
 */

import Foundation

extension String {
    
    var asciiArray8: [Int8] {
        return unicodeScalars.filter{$0.isASCII}.map{Int8($0.value)}
    }
    
    init (_ asciiArray8 : [Int8]) {
        self.init()
        
        for x in asciiArray {
            self.append(Character(UnicodeScalar(UInt16(x))!))
        }
    }
    
    var asciiArray16: [Int16] {
        return unicodeScalars.filter{$0.isASCII}.map{Int16($0.value)}
    }
    
    init (_ asciiArray16 : [Int16]) {
        self.init()
        
        for x in asciiArray {
            self.append(Character(UnicodeScalar(UInt16(x))!))
        }
    }
    
    
    var asciiArray: [Int32] {
        return unicodeScalars.filter{$0.isASCII}.map{Int32($0.value)}
    }
    
    init (_ asciiArray : [Int32]) {
        self.init()
        
        for x in asciiArray {
            self.append(Character(UnicodeScalar(UInt32(x))!))
        }
    }
}

struct LabaAction {
    public var inverse : Bool
    public var rawValue : Float
    public var operatorChar : Int8
    
    public var fromValue : Float
    public var toValue : Float
    
    public var target : UIView?
    public var performAction : PerformAction?
    public var describeAction : DescribeAction?
    public var initAction : InitAction?
    public var easingAction : EasingAction?
    public var easingName : String?
    
    
    public var userFloat_1 : Float
    public var userFloat_2 : Float
    public var userVector2_1 : CGPoint
    public var userVector2_2 : CGPoint
    
    init() {
        inverse = false
        rawValue = 0
        operatorChar = 0
        
        target = nil
        fromValue = 0
        toValue = 0
        performAction = nil
        describeAction = nil
        initAction = nil
        easingAction = nil
        easingName = nil
        
        userFloat_1 = 0
        userFloat_2 = 0
        userVector2_1 = CGPoint.zero
        userVector2_2 = CGPoint.zero
    }
    
    init(_ operatorChar:Int8, _ target:UIView, _ inverse:Bool, _ rawValue:Float, _ easing:@escaping EasingAction, _ easingName:String) {
        self.operatorChar = operatorChar
        self.target = target
        self.inverse = inverse
        self.rawValue = rawValue
        self.easingAction = easing
        self.easingName = easingName
        
        self.performAction = Laba.shared.PerformActions[operatorChar]
        self.describeAction = Laba.shared.DescribeActions[operatorChar]
        self.initAction = Laba.shared.InitActions[operatorChar]
        
        if(inverse == false){
            fromValue = 0.0
            toValue = 1.0
        }else{
            fromValue = 1.0
            toValue = 0.0
        }
        
        userFloat_1 = 0.0
        userFloat_2 = 0.0
        userVector2_1 = CGPoint.zero
        userVector2_2 = CGPoint.zero
        
        if(self.initAction != nil) {
            self.initAction?(&self)
        }
    }
}

typealias EasingAction = ((Float, Float, Float) -> Float)
typealias InitAction = ((inout LabaAction) -> Void)
typealias PerformAction = ((UIView, Float, LabaAction) -> Void)
typealias DescribeAction = ((inout String, LabaAction) -> Void)

public class Laba {
    
    public let labaDefaultValue : Float = Float.leastNormalMagnitude;
    
    private let kMaxPipes = 10
    private let kMaxActions = 10
    private let kDefaultDuration = 0.87
    
    public static let shared = Laba()
    private init() {
        InitActions = [Int8: InitAction]();
        PerformActions = [Int8: PerformAction]();
        DescribeActions = [Int8: DescribeAction]();
        
        
        // *** FADE ***
        RegisterOperation(
            "f",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = 1.0;
                }
                if(action.inverse == false){
                    action.fromValue = (Float)(action.target!.alpha)
                    action.toValue = action.rawValue
                }else{
                    action.fromValue = (action.rawValue > 0.5 ? 0.0 : 1.0)
                    action.toValue = action.rawValue
                }
        },
            { (view, v, action) in
                action.target!.alpha = (CGFloat)(v)
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("fade to \(action.rawValue*100)%")
                } else {
                    description.append("fade from \(action.fromValue * 100)% to \(action.toValue * 100)%")
                }
        })
        // ************
    }
    
    
    
    
    private func ParseAnimationString(_ target:UIView, _ charString:[Int8]) -> [[LabaAction]] {
        
        var idx : Int = 0;
        
        let isOperator : ((Int8) -> Bool) = { (c) in
            // '|' or '!' or 'e'
            if (c == 124 || c == 33 || c == 101) {
                return true;
            }
            return self.InitActions[c] != nil
        }
        
        let isNumber : ((Int8) -> Bool) = { (c) in
            // yes, we have to do it this way instead of on one line because 
            // Swift is not capable of figuring it out in a reasonable amount of time.
            
            // '+' or '-' or '.' or '0' -> '9'
            if c >= 48 && c <= 57 {
                return true
            }
            return (c == 43 || c == 45 || c == 46)
        }
        
        var combinedActions : [[LabaAction]] = [[LabaAction]](repeating: [LabaAction](repeating: LabaAction(), count: kMaxActions), count: kMaxPipes)
        var currentPipeIdx : Int = 0
        var currentActionIdx: Int = 0
        var easingAction : EasingAction = { (from, to, v) in
            return (to - from) * v + from
        }
        var easingName = ""
        
        while (idx < charString.count) {
            var invertNextOperator = false
            var action : Int8 = 32 // ascii for ' '
            
            // find the next operator
            while (idx < charString.count) {
                let c = charString [idx]
                if (isOperator (c)) {
                    if (c == 33) { // c == '!'
                        invertNextOperator = true
                    } else if (c == 124) { // c == '|'
                        currentPipeIdx += 1
                        currentActionIdx = 0
                    } else {
                        action = c
                        idx += 1
                        break;
                    }
                }
                idx += 1
            }
            
            // skip anything not important
            while (idx < charString.count && isNumber (charString [idx]) == false && isOperator (charString [idx]) == false) {
                idx += 1
            }
            
            var value : Float = labaDefaultValue
            
            // if this is a number read it in
            if (idx < charString.count && isNumber (charString [idx])) {
                
                // read in numerical value (if it exists)
                var isNegativeNumber = false
                if (charString [idx] == 43) { // '+'
                    idx += 1
                } else if (charString [idx] == 45) { // '-'
                    isNegativeNumber = true
                    idx += 1
                }
                
                value = 0.0
                
                var fractionalPart = false
                var fractionalValue : Float = 10.0
                while (idx < charString.count) {
                    let c = charString [idx]
                    if (isNumber (c)) {
                        if (c >= 48 && c <= 57) { // '0', '9'
                            if (fractionalPart) {
                                value = value + (Float)(c - 48) / fractionalValue // '0'
                                fractionalValue *= 10.0
                            } else {
                                value = value * 10 + (Float)(c - 48) // '0'
                            }
                        }
                        if (c == 46) { // '.'
                            fractionalPart = true;
                        }
                    }
                    if (isOperator (c)) {
                        break;
                    }
                    idx += 1
                }
                
                if (isNegativeNumber) {
                    value *= -1.0
                }
            }
            
            
            
            // execute the action?
            if (action != 32) { // ' '
                if (InitActions[action] != nil) {
                    
                    combinedActions[currentPipeIdx][currentActionIdx] = LabaAction(action, target, invertNextOperator, value, easingAction, easingName)
                    //combinedActions [currentPipeIdx, currentActionIdx] = new LabaAction (action, rectTransform, invertNextOperator, value, easingAction, easingName)
                    currentActionIdx += 1
                } else {
                    if (action == 101) { // 'e'
                        let easingIdx : Int = Int(value)
                        if (easingIdx >= 0 && idx < allEasings.count) {
                            easingAction = allEasings [easingIdx]
                            easingName = allEasingsByName [easingIdx]
                        }
                    }
                }
            }
            
        }
        
        return combinedActions
    }
    
    
    
    
    
    private func AnimateOne(target:UIView, animationString:[Int8], onComplete:(()->Void)?) {
        let actionList = ParseAnimationString (target, animationString)
        
        print(actionList)
    }
    
    
    public func Animate(target:UIView, animationString:String, onComplete:(()->Void)?) {
        let animationAsciiString : [Int8] = animationString.asciiArray8
    
        AnimateOne(target:target, animationString:animationAsciiString, onComplete:onComplete)
        
        /*
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
        }*/
    }
    
    
    
    
    

    
    var InitActions:[Int8: InitAction];
    var PerformActions:[Int8: PerformAction];
    var DescribeActions:[Int8: DescribeAction];
    
    private func RegisterOperation(_ stringOperator:String, _ initAction:@escaping InitAction, _ performAction:@escaping PerformAction, _ describeAction:@escaping DescribeAction) {
        let charOperator:Int8 = stringOperator.utf8CString[0]
        InitActions [charOperator] = initAction;
        PerformActions [charOperator] = performAction;
        DescribeActions [charOperator] = describeAction;
    }
    
    
    
    
    
    private var allEasings : [EasingAction] = [
        easeLinear
    ]
    
    private var allEasingsByName : [String] = [
        "ease linear"
    ]
    
    private static var easeLinear : EasingAction = { (from, to, v) in
        return (to - from) * v + from
    }
}
