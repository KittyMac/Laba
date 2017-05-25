//
//  ViewController.swift
//  LabaExample
//
//  Created by Rocco Bowling on 5/22/17.
//  Copyright © 2017 Rocco Bowling. All rights reserved.
//

import UIKit
import Laba

class ViewController: UIViewController {

    override func viewDidLoad() {
        super.viewDidLoad()
        
        
        let labaString = "s0.2|f0|f1|<100|v100|>100|^100|y360|p360|r360"
        
        Laba.shared.Animate(target: self.view, animationString: labaString, onComplete: nil)
        
        Laba.shared.Describe(target: self.view, animationString: labaString)
        
    }

}

