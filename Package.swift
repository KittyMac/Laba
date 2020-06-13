// swift-tools-version:5.0
import PackageDescription

let package = Package(
    name: "Laba",
    products: [
        .library(name: "Laba", targets: ["Laba"])
    ],
    dependencies: [
    ],
    targets: [
        .target(
            name: "Laba",
            dependencies: [
				"MKTween"
            ]
        ),
        .target(
            name: "MKTween"
        )
    ]
)
