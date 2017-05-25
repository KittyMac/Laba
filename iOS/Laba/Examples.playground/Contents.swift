import MKTween
import Laba
import PlaygroundSupport

let hostView = UIView(frame: CGRect(x: 0, y: 0, width: 240, height: 380))
hostView.layer.borderWidth = 1
hostView.layer.borderColor = UIColor.blue.cgColor
hostView.backgroundColor = UIColor.white
PlaygroundPage.current.liveView = hostView


let labaString = "s0.2|<10L5|<10l5"
Laba.shared.Animate(target: hostView, animationString: labaString, onComplete: nil)
Laba.shared.Describe(target: hostView, animationString: labaString)

