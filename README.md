[![Carthage compatible](https://img.shields.io/badge/Carthage-compatible-4BC51D.svg?style=flat)](https://github.com/Carthage/Carthage)


# Laba

Labanotiation is a notation system designed to record and analyze human movement (its a written language for dance). Laba is a **minimalistic notation system designed for choreographing UI animations**. Laba allows you to express simple (or even complex) combinations and sequences of animations in a tightly defined string, which can then be parsed and driven by your applications animation engine of choice.

How you might choose to integrate Laba into your environment is your choice. As of this writing, this Laba repository supports the following:

* Unity3D (requires LeanTween)

## Syntax

Laba syntax is intentionally simple. It is a series of single character operators, and each operator may optionally be followed by a numerical argument. All whitespace and unrecognized characters are ignored, so if you need to artificially add spaces to provide visual clarity that is your choice.

Numerical arguments are always **OPTIONAL**. If no argument is supplied, a default value is used.

Laba is built with a set of built-in operators. Laba also provides easy machanisms for you to add your own operators.

What the animation described by your Laba notation applies to will be specific to your environment. In Unity, for example, your animations might affect a RectTransform of a GameObject. For the purposes of this document, we will refer to this as the **target** of the animation.

## Operators

`<` move left  
`>` move right  
`^` move up  
`v` move down  

`f` alpha fade  

`s` uniform scale  

`r` roll / z rotation  
`p` pitch / x rotation  
`y` yaw / y rotation  

`e` easing function (environment specific)  

`d` duration of sequence   

`D` staggaered duration based on sibling/child index

`L` loop (absolute) this segment (value is number of times to loop, -1 means loop infinitely)

`l` loop (relative) this segment (value is number of times to loop, -1 means loop infinitely)

`|` pipe animations into multiple sequences  

`!` invert the next operator  

`[]` concurrent Laba animations ( example: [>d2][!fd1] )


## Example 1

```e0<100f0d1```  

This Laba notation would move the target linearly left 100 units while fading its alpha to 0.  Let's break it up:

`e0` This specifies that the following Laba expessions should use the easing function with the id of 0. The mappings for easing functions are environment specific, so please look at the docs. In our example, easing 0 is a linear easing. Multiple easings can be defined.

`<100` This specifies the target should move left 100 units from its position at the start of the animation. "Units" is also a value which only means something specific in your chosen environment.

`f0` This says that the target should animate its alpha (or transparency) from whatever its current alpha is to 0.

`d1` This sets the duration of the animation to complete in 1 second. All animations defined in the same sequence execute concurrently for the entire duration.


## Example 2

```!>!f|s1.2d0.2|sd0.2```

This Laba notation will move the target in from the left and end at its current position while fading it in from 0 alpha to 1 alpha. Once this is complete, it will then scale up the target by 20% over 0.2s, and then scale it back to normal size over an additional 0.2s.

`!>` This specifies an inverted move to the right. Normally a movement will move from the current position to a new position some number of units away. The inverted movement operator will start the animation some number of units away and end the animation at the position it had prior to the animation starting. Single no numerical argument is supplied, it will use the default which for the move right operator is the width of the target.

`!f` Inverting the fade operator is similar to inverting the movement operator. This will cause the fade to animation starting at 0 and end at 1 (fully visible). Since no numerical argument is specified the default of 1 is used.

`|` The pipe operator specifies the ending of one sequence of operators and the start of a new sequence of operators. All previous operators will end their animation prior to starting the next sequence.

`s1.2` This performs a uniform scaling of the target, in this case it will scale to 120% in both its width and height.

`d0.2` This sets the duration of this sequence to finish in 0.2 seconds.

`|` Another pipe operator to specify another sequence

`s` This performs a uniform scaling of the target; the default value for uniform scale is 100%.

`d0.2` This sets the duration of this sequence to finish in 0.2 seconds.








## License

Laba is free software distributed under the terms of the MIT license, reproduced below. Laba may be used for any purpose, including commercial purposes, at absolutely no cost. No paperwork, no royalties, no GNU-like "copyleft" restrictions. Just download and enjoy.

Copyright (c) 2017 [Small Planet Digital, LLC](http://smallplanet.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.