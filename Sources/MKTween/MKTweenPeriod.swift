//
//  MKTweenPeriod.swift
//  MKTween
//
//  Created by Kevin Malkic on 25/01/2016.
//  Copyright © 2016 Malkic Kevin. All rights reserved.
//

import Foundation

open class MKTweenPeriod {
	
    open var loops: Int
	open var duration: TimeInterval
	open var delay: TimeInterval
	internal(set) open var startValue: Double
	internal(set) open var endValue: Double
	internal(set) open var progress: Double = 0
	
	internal var startTimeStamp: TimeInterval?
	internal var updatedTimeStamp: TimeInterval?
	
    public init(duration: TimeInterval, delay: TimeInterval = 0, loops: Int = 0, startValue: Double = 0, endValue: Double = 1) {
		
		self.duration = duration
		self.delay = delay
		self.startValue = startValue
		self.endValue = endValue
        self.loops = loops
	}
    
    public func Reset() {
        
    }
}
