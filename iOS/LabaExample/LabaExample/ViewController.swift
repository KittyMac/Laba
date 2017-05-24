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
        
        
        Laba.shared.Animate(target: self.view, animationString: "f0|f1|d3f0|d3f1", onComplete: nil)
        
        
    }

}

