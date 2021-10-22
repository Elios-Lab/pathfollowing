# Pathfollowing

## Prerequisites:

* Unity: >= 2020.3.12f
* Python3: >= 3.8.10
* MLagents: 0.25.1 

To install all the requirements use the command:

<code>pip install -r /path/to/requirements.txt</code>

## Links:

* [Unity](https://unity.com/download)
* [Unity documentation](https://docs.unity3d.com/Manual/index.html)
* [MLAgents](https://github.com/Unity-Technologies/ml-agents)

Some of the 3D models used in this project are from Sketchfab:

* [Shrine of The Fury](https://sketchfab.com/3d-models/chevrolet-corvette-1980-different-colours-7e428bdb3ab54b4e9ac610e545fd9d03)

# How to open the project in Unity?

After cloning or downloading the repository, you have to follow these simple steps to open the project and work on it:

1. Open the Unity Hub 
2. In Projects click ADD 
3. Select the folder in which has been saved and open it

Note that a Unity project is a collection of files and directories, rather than just one specific Unity Project file. To open a Project, you must select the main Project folder, rather than a specific file.

# ML-Agents setup

## Installation

* [Installation](https://github.com/Unity-Technologies/ml-agents/blob/release_18_docs/docs/Installation.md)

## Documentation

For further information it is better to consult the official documentation:
* [Documentation](https://github.com/Unity-Technologies/ml-agents/blob/release_18_docs/docs/Readme.md) 

Here is a detailed description of all the features [ML-Agents Overview](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/ML-Agents-Overview.md). 


## Training 

Go in the project folder and use the following command:

<code>mlagents-learn config/trainer_config.yaml --run-id=name</code>

For more informations and features:
* [Training Documentation](https://github.com/Unity-Technologies/ml-agents/blob/release_18_docs/docs/Training-ML-Agents.md)
