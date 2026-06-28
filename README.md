
# Simple External Unity Console

This program is a lightweight, standalone development tool designed to capture, stream, and analyse debug logs from built Unity games in real time. Using BepInEx, this tool modifies a Unity build on launch, allowing both the programs to communicate.

This tool operates exactly like Unity's built-in console. Selecting any log will display its stack trace. Additionally, you can toggle and filter logs by type (Log, Warning, Error). 


## Screenshots

![App Screenshot](https://lh3.googleusercontent.com/d/1eT3Ao5V8bleNnsmK6xYRCLl5jdZrVaog)
![App Screenshot](https://lh3.googleusercontent.com/d/1g04qMUVhePelkD3U4rOtGWVCXKPbDeXN)



## Installation

Before you begin, ensure you have the following installed on your machine:
* **Visual Studio 2022** (or later) with the **.NET Desktop Development** workload checked.
* **.NET 10 SDK** 

Run the following commands in your terminal or command prompt to clone the repository and move into the project directory:

```bash
   git clone https://github.com/MitchMacP/SimpleExternalUnityConsole.git
   cd SimpleExternalUnityConsole
```
    
## FAQ

#### Question 1 - Does this support all Unity Builds?

It should work on most modern games (**Unity 2020 and newer**). However, it might not work on games that require to be launched through **Steam**.

#### Question 2 - How do connect this tool to a build?

After launching the tool, click the **PLAY** button. A dialog window will appear, allowing you to select a Unity build. This will launch the game and connect it to the tool.