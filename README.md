# Pinnacle Coding Convention
This project is about to ensure and automate some coding conventions in the Pinnacle solution. 
It helps developers to avoid some tedious NW in code review, e.g. A-Z order, missing regions, regions not matched...

![Demo](Images/Demo.gif)

## Build
[![Build Status](https://dev.azure.com/khanhthevu/PinnacleCodingConvention/_apis/build/status/PinnacleCodingConvention?branchName=master)](https://dev.azure.com/khanhthevu/PinnacleCodingConvention/_build/latest?definitionId=2&branchName=master)

## Release 1.1.2
Add ability to Run Code Cleanup (Profile) feature from Visual Studio 2019. Be able to config which Profile to execute through option: Tools > Options > Pinnacle Coding Convention > General > Profile

Some functionalities from Cleanup Profile:
- Sort usings
- Remove Unnecessary usings
- Apply implicit/explicit preferences
- Remove unused variable

![Config Code Cleanup](Images/Configure_Code_Cleanup.png)