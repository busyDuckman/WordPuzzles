# Word Puzzles

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE.txt)
[![Latest release](https://img.shields.io/github/v/release/busyDuckman/WordPuzzles?include_prereleases)](https://github.com/busyDuckman/WordPuzzles/releases)
![Platform: Windows](https://img.shields.io/badge/platform-Windows-0078D6.svg)
![.NET Framework 4.7.2](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4.svg)
![Language: C#](https://img.shields.io/badge/language-C%23-239120.svg)
![Status: alpha](https://img.shields.io/badge/status-alpha-orange.svg)

Easy to use software to generate crossword puzzles.
This is still very much alpha software (totally unfinished).  This crossword puzzle generator is provided here for those who would like to kick the tyres and get a preview of some upcoming software.
It uses the [GNU Collaborative International Dictionary of English](http://gcide.gnu.org.ua/) as the source for it's english dictionary.

> [!NOTE]
> **This project is mothballed.** It is no longer being actively developed, and issues and pull requests are unlikely to be addressed. The code is left here as-is for anyone who finds it useful.
>
> Note: I left this to create a commercial product instead, using Rust: [TickWords](https://tickwords.com/).

# Screen Shot
![Screen Shot](Screenshots/wordselect.png?raw=true "Screen Shot")

# Download

  [Latest release 0.8-Alpha](https://github.com/busyDuckman/WordPuzzles/releases/download/v0.8_alpha/publish_wordpuzzles_0.8.zip)

# Building and Running
Requires Windows and the .NET Framework 4.7.2 Developer Pack (included with the Visual Studio ".NET desktop development" workload).

  - **Visual Studio:** open WordPuzzles.sln and press F5. 

  - **Command line:** from a Developer Command Prompt:

    msbuild WordPuzzles.sln -restore -p:RestorePackagesConfig=true
    bin\Debug\WordPuzzles.exe

# Getting Started
  - Press F5 for a new maze...

# Limitations:
  - Apha software (very incomplete)
  - Unimplemented menu items are greyed out..
  - No way to save a crossword yet.

# What is done:
  - Generates a valid cross word, generally quickly, but may stall for 20 seconds during generation.
  - Not a bug, my algorithm is not finished. If it works itself into a corner, it tries to think its way out instead of backing up a step.
  - Supports several generation rules.
  - “View Solution” toggle enabled
  - Gives basic clues (just a word definition at the moment)
  - Zooming in moving etc.

# Next Release
  - Mark squares that must/must not be used.
  - Dictionary management
  - Find spot for given word
  - Find word(s) for given spot.
  - Restrict to words of a certain readership / usage frequency.
  - Bitmap backgrounds.

# Road Map
  - Proper clues
  - Subject areas option for word selection
  - Support for output styles
  - Awesome-sauce graphics options
  - Artistic options
  - Use one dictionary across a series of crosswords.
  - Multiple languages


 