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
import MKTween

extension UIView {
    public func Animate(_ animationString:String) {
        Laba.shared.Animate(target: self, animationString: animationString, onComplete: nil)
    }
    public func Animate(_ animationString:String, _ onComplete:(()->Void)?) {
        Laba.shared.Animate(target: self, animationString: animationString, onComplete: onComplete)
    }
    public func Describe(_ animationString:String) {
        Laba.shared.Describe(target: self, animationString: animationString)
    }
}

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
    public var rawValue : Double
    public var operatorChar : Int8
    
    public var fromValue : Double
    public var toValue : Double
    
    public var target : UIView?
    public var performAction : PerformAction?
    public var describeAction : DescribeAction?
    public var initAction : InitAction?
    public var easingAction : EasingAction?
    public var easingName : String?
    
    
    public var userDouble_1 : Double
    public var userDouble_2 : Double
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
        
        userDouble_1 = 0
        userDouble_2 = 0
        userVector2_1 = CGPoint.zero
        userVector2_2 = CGPoint.zero
    }
    
    init(_ operatorChar:Int8, _ target:UIView, _ inverse:Bool, _ rawValue:Double, _ easing:@escaping EasingAction, _ easingName:String) {
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
        
        userDouble_1 = 0.0
        userDouble_2 = 0.0
        userVector2_1 = CGPoint.zero
        userVector2_2 = CGPoint.zero
        
        if(self.initAction != nil) {
            self.initAction?(&self)
        }
    }
    
    mutating func Reset() -> Bool {
        if (self.initAction != nil) {
            let tempAction = LabaAction (operatorChar, target!, inverse, rawValue, easingAction!, easingName!)
            self.fromValue = tempAction.fromValue
            self.toValue = tempAction.toValue
            self.userDouble_1 = tempAction.userDouble_1
            self.userDouble_2 = tempAction.userDouble_2
            self.userVector2_1 = tempAction.userVector2_1
            self.userVector2_2 = tempAction.userVector2_2
            return true
        }
        return false
    }
    
    public func Perform(_ v:Double) -> Bool {
        if performAction != nil {
            performAction? (target!, easingAction!(v, fromValue, toValue - fromValue, 1.0), self)
            return true
        }
        return false
    }
    
    public func Describe(_ sb:inout String) -> Bool {
        if describeAction != nil {
            describeAction? (&sb, self)
            return true
        }
        return false
    }
}

typealias EasingAction = MKTweenTimingFunction
typealias InitAction = ((inout LabaAction) -> Void)
typealias PerformAction = ((UIView, Double, LabaAction) -> Void)
typealias DescribeAction = ((inout String, LabaAction) -> Void)

public class Laba {
    
    public let labaDefaultValue : Double = Double.leastNormalMagnitude
    
    public var timeScale = 1.0
    
    private let kMaxPipes = 10
    private let kMaxActions = 10
    private let kDefaultDuration : Double = 0.87
    
    public static let shared = Laba()
    private init() {
        InitActions = [Int8: InitAction]()
        PerformActions = [Int8: PerformAction]()
        DescribeActions = [Int8: DescribeAction]()
        
        
        // *** LOOP ABSOLUTE ***
        RegisterOperation(
            "L",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = -1
                }
                action.fromValue = action.rawValue
                action.toValue = action.rawValue
        },
            { (view, v, action) in },
            { (description, action) in
        })
        // ************
        
        
        // *** LOOP RELATIVE ***
        RegisterOperation(
            "l",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = -1
                }
                action.fromValue = action.rawValue
                action.toValue = action.rawValue
        },
            { (view, v, action) in },
            { (description, action) in
        })
        // ************
        
        
        // *** DURATION ***
        RegisterOperation(
            "d",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = self.kDefaultDuration
                }
                action.fromValue = action.rawValue
                action.toValue = action.rawValue
        },
            { (view, v, action) in },
            { (description, action) in
        })
        // ************
        
        
        // *** STAGGERED DURATION ***
        RegisterOperation(
            "D",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = self.kDefaultDuration
                }
                action.fromValue = action.rawValue * Double((action.target!.superview?.subviews.index(of: action.target!))!)
                action.toValue = action.fromValue
        },
            { (view, v, action) in },
            { (description, action) in
        })
        // ************
        
        
        // *** MOVE LEFT ***
        RegisterOperation(
            "<",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = Double(action.target!.bounds.size.width)
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.frame.origin.x)
                    action.toValue = Double(action.target!.frame.origin.x) - action.rawValue
                }else{
                    action.fromValue = Double(action.target!.frame.origin.x) + action.rawValue
                    action.toValue = Double(action.target!.frame.origin.x)
                }
        },
            { (view, v, action) in
                action.target!.frame.origin.x = CGFloat(v)
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("move left \(action.rawValue) units, ")
                } else {
                    description.append("move in from left \(action.rawValue) units, ")
                }
        })
        // ************
        
        
        // *** MOVE RIGHT ***
        RegisterOperation(
            ">",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = Double(action.target!.bounds.size.width)
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.frame.origin.x)
                    action.toValue = Double(action.target!.frame.origin.x) + action.rawValue
                }else{
                    action.fromValue = Double(action.target!.frame.origin.x) - action.rawValue
                    action.toValue = Double(action.target!.frame.origin.x)
                }
        },
            { (view, v, action) in
                action.target!.frame.origin.x = CGFloat(v)
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("move right \(action.rawValue) units, ")
                } else {
                    description.append("move in from right \(action.rawValue) units, ")
                }
        })
        // ************
        
        
        // *** MOVE UP ***
        RegisterOperation(
            "^",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = Double(action.target!.bounds.size.height)
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.frame.origin.y)
                    action.toValue = Double(action.target!.frame.origin.y) - action.rawValue
                }else{
                    action.fromValue = Double(action.target!.frame.origin.y) + action.rawValue
                    action.toValue = Double(action.target!.frame.origin.y)
                }
        },
            { (view, v, action) in
                action.target!.frame.origin.y = CGFloat(v)
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("move up \(action.rawValue) units, ")
                } else {
                    description.append("move in from above \(action.rawValue) units, ")
                }
        })
        // ************
        
        
        // *** MOVE DOWN ***
        RegisterOperation(
            "v",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = Double(action.target!.bounds.size.height)
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.frame.origin.y)
                    action.toValue = Double(action.target!.frame.origin.y) + action.rawValue
                }else{
                    action.fromValue = Double(action.target!.frame.origin.y) - action.rawValue
                    action.toValue = Double(action.target!.frame.origin.y)
                }
        },
            { (view, v, action) in
                action.target!.frame.origin.y = CGFloat(v)
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("move down \(action.rawValue) units, ")
                } else {
                    description.append("move in from below \(action.rawValue) units, ")
                }
        })
        // ************
        
        
        // *** UNIFORM SCALE ***
        RegisterOperation(
            "s",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = 1.0
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.layer.transform.m11)
                    action.toValue = action.rawValue
                }else{
                    action.fromValue = (action.rawValue > 0.5 ? 0.0 : 1.0)
                    action.toValue = action.rawValue
                }
        },
            { (view, v, action) in
                action.target!.layer.transform.m11 = CGFloat(v)
                action.target!.layer.transform.m22 = CGFloat(v)
                action.target!.setNeedsDisplay()
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("scale to \(Int(action.rawValue*100))%, ")
                } else {
                    description.append("scale in from \(Int(action.rawValue*100))%, ")
                }
        })
        // ************
        
        
        // *** WIDTH ***
        RegisterOperation(
            "w",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = Double(action.target!.frame.size.width)
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.frame.size.width)
                    action.toValue = action.rawValue
                }else{
                    action.fromValue = action.rawValue
                    action.toValue = Double(action.target!.frame.size.width)
                }
        },
            { (view, v, action) in
                action.target!.frame.size.width = CGFloat(v)
                action.target!.setNeedsDisplay()
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("width to \(Int(action.rawValue*100))%, ")
                } else {
                    description.append("width in from \(Int(action.rawValue*100))%, ")
                }
        })
        // ************
        
        
        // *** HEIGHT ***
        RegisterOperation(
            "h",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = Double(action.target!.frame.size.height)
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.frame.size.height)
                    action.toValue = action.rawValue
                }else{
                    action.fromValue = action.rawValue
                    action.toValue = Double(action.target!.frame.size.height)
                }
        },
            { (view, v, action) in
                action.target!.frame.size.height = CGFloat(v)
                action.target!.setNeedsDisplay()
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("height to \(action.rawValue) units, ")
                } else {
                    description.append("height in from \(action.rawValue) units, ")
                }
        })
        // ************
        
        
        
        // *** ROLL ***
        RegisterOperation(
            "r",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = 0.0
                }
                
                let currentRotation = action.target!.value(forKeyPath: "layer.transform.rotation.z") as! Double
                if(action.inverse == false){
                    action.fromValue = currentRotation
                    action.toValue = currentRotation - self.degreesToRadians(action.rawValue)
                }else{
                    action.fromValue = currentRotation + self.degreesToRadians(action.rawValue)
                    action.toValue = currentRotation
                }
        },
            { (view, v, action) in
                action.target!.setValue(CGFloat(v), forKeyPath: "layer.transform.rotation.z")
                action.target!.setNeedsDisplay()
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("rotate around z by \(action.rawValue)°, ")
                } else {
                    description.append("rotate in from around z by \(action.rawValue)°, ")
                }
        })
        // ************
        
        // *** PITCH ***
        RegisterOperation(
            "p",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = 0.0
                }
                
                let currentRotation = action.target!.value(forKeyPath: "layer.transform.rotation.x") as! Double
                if(action.inverse == false){
                    action.fromValue = currentRotation
                    action.toValue = currentRotation - self.degreesToRadians(action.rawValue)
                }else{
                    action.fromValue = currentRotation + self.degreesToRadians(action.rawValue)
                    action.toValue = currentRotation
                }
        },
            { (view, v, action) in
                action.target!.setValue(CGFloat(v), forKeyPath: "layer.transform.rotation.x")
                action.target!.setNeedsDisplay()
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("rotate around x by \(action.rawValue)°, ")
                } else {
                    description.append("rotate in from around x by \(action.rawValue)°, ")
                }
        })
        // ************
        
        // *** YAW ***
        RegisterOperation(
            "y",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = 0.0
                }
                
                let currentRotation = action.target!.value(forKeyPath: "layer.transform.rotation.y") as! Double
                if(action.inverse == false){
                    action.fromValue = currentRotation
                    action.toValue = currentRotation - self.degreesToRadians(action.rawValue)
                }else{
                    action.fromValue = currentRotation + self.degreesToRadians(action.rawValue)
                    action.toValue = currentRotation
                }
        },
            { (view, v, action) in
                action.target!.setValue(CGFloat(v), forKeyPath: "layer.transform.rotation.y")
                action.target!.setNeedsDisplay()
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("rotate around y by \(action.rawValue)°, ")
                } else {
                    description.append("rotate in from around y by \(action.rawValue)°, ")
                }
        })
        // ************
        
        
        // *** FADE ***
        RegisterOperation(
            "f",
            { (action) in
                if (action.rawValue == self.labaDefaultValue) {
                    action.rawValue = 1.0
                }
                if(action.inverse == false){
                    action.fromValue = Double(action.target!.alpha)
                    action.toValue = action.rawValue
                }else{
                    action.fromValue = (action.rawValue > 0.5 ? 0.0 : 1.0)
                    action.toValue = action.rawValue
                }
        },
            { (view, v, action) in
                action.target!.alpha = CGFloat(v)
        },
            { (description, action) in
                if(action.inverse == false) {
                    description.append("fade to \(Int(action.rawValue*100))%, ")
                } else {
                    description.append("fade from \(Int(action.fromValue * 100))% to \(action.toValue * 100)%, ")
                }
        })
        // ************
    }
    
    
    
    
    private func ParseAnimationString(_ target:UIView, _ charString:[Int8], _ startIdx:Int, _ endIdx:Int) -> [[LabaAction]] {
        
        var idx : Int = startIdx
        
        let isOperator : ((Int8) -> Bool) = { (c) in
            // '|' or '!' or 'e'
            if (c == 124 || c == 33 || c == 101) {
                return true
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
        var easingAction : EasingAction = MKTweenTiming.CubicInOut
        var easingName = ""
        
        while (idx < endIdx) {
            var invertNextOperator = false
            var action : Int8 = 32 // ascii for ' '
            
            // find the next operator
            while (idx < endIdx) {
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
                        break
                    }
                }
                idx += 1
            }
            
            // skip anything not important
            while (idx < endIdx && isNumber (charString [idx]) == false && isOperator (charString [idx]) == false) {
                idx += 1
            }
            
            var value : Double = labaDefaultValue
            
            // if this is a number read it in
            if (idx < endIdx && isNumber (charString [idx])) {
                
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
                var fractionalValue : Double = 10.0
                while (idx < endIdx) {
                    let c = charString [idx]
                    if (isNumber (c)) {
                        if (c >= 48 && c <= 57) { // '0', '9'
                            if (fractionalPart) {
                                value = value + (Double)(c - 48) / fractionalValue // '0'
                                fractionalValue *= 10.0
                            } else {
                                value = value * 10 + (Double)(c - 48) // '0'
                            }
                        }
                        if (c == 46) { // '.'
                            fractionalPart = true
                        }
                    }
                    if (isOperator (c)) {
                        break
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
    
    
    
    
    
    
    private func AnimateOne(_ target:UIView, _ animationString:[Int8], _ startIdx:Int, _ endIdx:Int, _ onComplete:(()->Void)?) {
        
        sb = nil
        
        SharedProcessIndividualLabaString(target, animationString, startIdx, endIdx, onComplete, { (actionList, loopRelative, looping, pipeIdx, duration, onComplete) in
                var localActionList:[[LabaAction]] = actionList
            
                if (loopRelative) {
                    var lastV : Double = 1.0
                    
                    MKTween.shared.addTweenOperation(MKTweenOperation(period: MKTweenPeriod(duration: TimeInterval(duration * self.timeScale), loops:looping), updateBlock: { (period) -> () in
                        if (period.progress < lastV) {
                            for j in 0..<self.kMaxActions {
                                if !localActionList [pipeIdx][j].Reset() {
                                    break
                                }
                            }
                        }
                        lastV = period.progress
                        for i in 0..<self.kMaxActions {
                            if (!localActionList [pipeIdx][i].Perform(period.progress)) {
                                break
                            }
                        }
                    }, completeBlock: { () in
                        if onComplete != nil {
                            onComplete?()
                        }
                    }))
                    
                    // todo: .setLoopCount ((int)looping)
                } else {
                    for j in 0..<self.kMaxActions {
                        if !localActionList [pipeIdx][j].Reset() {
                            break
                        }
                    }
                    MKTween.shared.addTweenOperation(MKTweenOperation(period: MKTweenPeriod(duration: TimeInterval(duration * self.timeScale), loops:looping), updateBlock: { (period) -> () in
                        for i in 0..<self.kMaxActions {
                            if (!localActionList [pipeIdx][i].Perform(period.progress)) {
                                break
                            }
                        }
                    }, completeBlock: { () in
                        if onComplete != nil {
                            onComplete?()
                        }
                    }))
                    // todo: .setLoopCount ((int)looping)
                }
            }
        )
    }
    
    
    private var sb:String? = nil
    private func DescribeOne(_ target:UIView, _ animationString:[Int8], _ startIdx:Int, _ endIdx:Int, _ onComplete:(()->Void)?) {
        
        sb = ""
        
        SharedProcessIndividualLabaString(target, animationString, startIdx, endIdx, onComplete, { (actionList, loopRelative, looping, pipeIdx, duration, onComplete) in
                var localActionList:[[LabaAction]] = actionList
                for i in 0..<self.kMaxActions {
                    if !localActionList [pipeIdx][i].Describe (&self.sb!) {
                        break
                    }
                }
            }
        )
        
        print(sb!.replacingOccurrences(of: "  ", with: " ").replacingOccurrences(of: "  ", with: " ").replacingOccurrences(of: "  ", with: " "))
        
        sb = nil
    }
    
    
    private func SharedProcessIndividualLabaString(_ target:UIView, _ animationString:[Int8], _ startIdx:Int, _ endIdx:Int, _ onComplete:(()->Void)?, _ processOperation:@escaping (( [[LabaAction]],Bool,Int,Int,Double,(()->Void)?)->Void)) {
        var actionList = ParseAnimationString (target, animationString, startIdx, endIdx)
        let durationAction1 : Int8 = 100 // 'd'
        let durationAction2 : Int8 = 68 // 'D'
        let loopAction1 : Int8 = 76 // 'L'
        let loopAction2 : Int8 = 108 // 'l'
        
        var numOfPipes : Int = 0
        
        var duration : Double = 0.0
        var looping : Int = 1
        var loopingRelative = false
        for i in 0..<kMaxPipes {
            if (actionList [i][0].performAction != nil) {
                numOfPipes += 1
                
                var durationForPipe : Double = kDefaultDuration
                for j in 0..<kMaxActions {
                    if (actionList [i][j].operatorChar == durationAction1 || actionList [i][j].operatorChar == durationAction2) {
                        durationForPipe = actionList [i][j].fromValue
                    }
                    if (actionList [i][j].operatorChar == loopAction1) {
                        looping = Int(actionList [i][j].fromValue)
                    }
                    if (actionList [i][j].operatorChar == loopAction2) {
                        loopingRelative = true
                        looping = Int(actionList [i][j].fromValue)
                    }
                }
                duration += durationForPipe
            }
        }
        
        var oldDescriptionHash:Int = 0
        if sb != nil {
            oldDescriptionHash = sb!.hash
        }
        
        // having only a single pipe makes things much more efficient, so treat it separately
        if (numOfPipes == 1) {
            
            processOperation(actionList, loopingRelative, looping, 0, duration, onComplete)
            
            if sb != nil {
                
                if  looping > 1 {
                    sb!.append(" repeating \(looping) times, ")
                } else if (looping == -1) {
                    sb!.append(" repeating forever, ")
                }
                
                if oldDescriptionHash != sb?.hash {
                    sb!.append (" \(actionList [0][0].easingName!)  ")
                    if duration == 0 {
                        sb!.append (" instantly.")
                    }else{
                        sb!.append (" over \(duration * timeScale) seconds.")
                    }
                } else {
                    sb!.append (" wait for \(duration * timeScale) seconds.")
                }
            }
            
        } else {
            
            var nextAction : (()->Void)? = nil
            
            for pipeIdx in stride(from: numOfPipes-1, to: -1, by: -1) {
                
                var durationForPipe : Double = kDefaultDuration
                var loopingForPipe : Int = 1
                var loopingRelativeForPipe : Bool = false
                
                for j in 0..<kMaxActions {
                    if (actionList [pipeIdx][j].operatorChar == durationAction1 || actionList [pipeIdx][j].operatorChar == durationAction2) {
                        durationForPipe = actionList [pipeIdx][j].fromValue
                    }
                    if (actionList [pipeIdx][j].operatorChar == loopAction1) {
                        loopingForPipe = Int(actionList [pipeIdx][j].fromValue)
                    }
                    if (actionList [pipeIdx][j].operatorChar == loopAction2) {
                        loopingRelativeForPipe = true
                        loopingForPipe = Int(actionList [pipeIdx][j].fromValue)
                    }
                }
                
                var localNextAction = nextAction
                if (localNextAction == nil) {
                    localNextAction = onComplete
                }
                if (localNextAction == nil) {
                    localNextAction = { () in }
                }
                
                nextAction = { () in
                    processOperation(actionList, loopingRelativeForPipe, loopingForPipe, pipeIdx, durationForPipe, localNextAction)
                    
                    if self.sb != nil {
                        if  loopingForPipe > 1 {
                            self.sb!.append(" repeating \(loopingForPipe) times, ")
                        } else if (loopingForPipe == -1) {
                            self.sb!.append(" repeating forever, ")
                        }
                        
                        if oldDescriptionHash != self.sb?.hash {
                            self.sb!.append (" \(actionList [pipeIdx][0].easingName!)  ")
                            if durationForPipe == 0 {
                                self.sb!.append (" instantly.")
                            }else{
                                self.sb!.append (" over \(durationForPipe * self.timeScale) seconds.")
                            }
                        } else {
                            self.sb!.append (" wait for \(durationForPipe * self.timeScale) seconds.")
                        }
                        
                        if (pipeIdx + 1 < numOfPipes) {
                            self.sb!.append (" Once complete then  ")
                        }
                        
                        if localNextAction != nil {
                            localNextAction!()
                        }
                    }
                }
            }
            
            if nextAction != nil {
                nextAction? ()
            } else {
                if onComplete != nil {
                    onComplete? ()
                }
            }
        }
    }

    
    
    private func SharedAnimateProcessString(_ animationString:String, _ performActionOnLabaString: (([Int8],Int,Int)->Void)) {
        
        var animationAsciiString : [Int8] = animationString.asciiArray8
        var isMultipleAnimations = false
        
        for i in 0..<animationAsciiString.count {
            if animationAsciiString[i] == 91 { // '['
                isMultipleAnimations = true
                break
            }
        }
        
        if isMultipleAnimations {
            // replace all '[' with ' '.  for all ']', insert 0
            for i in 0..<animationAsciiString.count {
                if animationAsciiString[i] == 91 { // '['
                    animationAsciiString[i] = 32
                }
                if animationAsciiString[i] == 93 { // ']'
                    animationAsciiString[i] = 0
                }
            }
            
            // animate each part individiually
            for i in 0..<animationAsciiString.count {
                
                if i == 0 || animationAsciiString[i] == 0 {
                    
                    for j in 0..<animationAsciiString.count {
                        if animationAsciiString[j] == 0 {
                            performActionOnLabaString(animationAsciiString, i+1, j)
                        }
                    }
                    
                }
            }
            
        } else {
            performActionOnLabaString(animationAsciiString, 0, animationAsciiString.count)
        }
        
    }
    
    public func Animate(target:UIView, animationString:String, onComplete:(()->Void)?) {
        
        var localOnComplete = onComplete

        SharedAnimateProcessString(animationString, { (animationAsciiString, startIdx, endIdx) in
            AnimateOne(target, animationAsciiString, startIdx, endIdx, localOnComplete)
            localOnComplete = nil
        })
    }
    
    public func Describe(target:UIView, animationString:String) {
        
        SharedAnimateProcessString(animationString, { (animationAsciiString, startIdx, endIdx) in
            DescribeOne(target, animationAsciiString, startIdx, endIdx, nil)
        })
    }
    
    
    

    
    var InitActions:[Int8: InitAction]
    var PerformActions:[Int8: PerformAction]
    var DescribeActions:[Int8: DescribeAction]
    
    private func RegisterOperation(_ stringOperator:String, _ initAction:@escaping InitAction, _ performAction:@escaping PerformAction, _ describeAction:@escaping DescribeAction) {
        let charOperator:Int8 = stringOperator.utf8CString[0]
        InitActions [charOperator] = initAction
        PerformActions [charOperator] = performAction
        DescribeActions [charOperator] = describeAction
    }
    
    
    
    
    
    private var allEasings : [EasingAction] = [
        MKTweenTiming.Linear,
        MKTweenTiming.BackIn,
        MKTweenTiming.BackOut,
        MKTweenTiming.BackInOut,
        MKTweenTiming.BounceIn,
        MKTweenTiming.BounceOut,
        MKTweenTiming.BounceInOut,
        MKTweenTiming.CircleIn,
        MKTweenTiming.CircleOut,
        MKTweenTiming.CircleInOut,
        MKTweenTiming.CubicIn,
        MKTweenTiming.CubicOut,
        MKTweenTiming.CubicInOut,
        MKTweenTiming.ElasticIn,
        MKTweenTiming.ElasticOut,
        MKTweenTiming.ElasticInOut,
        MKTweenTiming.ExpoIn,
        MKTweenTiming.ExpoOut,
        MKTweenTiming.ExpoInOut,
        MKTweenTiming.QuadIn,
        MKTweenTiming.QuadOut,
        MKTweenTiming.QuadInOut,
        MKTweenTiming.QuartIn,
        MKTweenTiming.QuartOut,
        MKTweenTiming.QuartInOut,
        MKTweenTiming.QuintIn,
        MKTweenTiming.QuintOut,
        MKTweenTiming.QuintInOut,
        MKTweenTiming.SineIn,
        MKTweenTiming.SineOut,
        MKTweenTiming.SineInOut
    ]
    
    private var allEasingsByName : [String] = [
        "ease linear",
        "ease back in",
        "ease back out",
        "ease back in/out",
        "ease bounce in",
        "ease bounce out",
        "ease bounce in/out",
        "ease circle in",
        "ease circle out",
        "ease circle in/out",
        "ease cubic in",
        "ease cubic out",
        "ease cubic in/out",
        "ease elastic in",
        "ease elastic out",
        "ease elastic in/out",
        "ease expo in",
        "ease expo out",
        "ease expo in/out",
        "ease quad in",
        "ease quad out",
        "ease quad in/out",
        "ease quart in",
        "ease quart out",
        "ease quart in/out",
        "ease quint in",
        "ease quint out",
        "ease quint in/out",
        "ease sine in",
        "ease sine out",
        "ease sine in/out",
    ]
    
    
    func degreesToRadians(_ degrees: Double) -> Double {
        return degrees * Double.pi / 180
    }
    
    func radiansToDegress(_ radians: Double) -> Double {
        return radians * 180 / Double.pi
    }
}
