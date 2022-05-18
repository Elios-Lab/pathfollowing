# Pathfollowing with Deep Reinforcement Learning

## Prerequisites:

* Unity: >= 2020.3.22f1 (tested on 2021.3.2f1)
* Python: >= 3.8.10
* ML-Agents: >= 0.25.1 (tested on 0.29.0)
* ML-Agents Unity package version: 2.0.0

To install all the requirements use the command:

```pip install mlagents```

or, alternatively:

```pip install -r requirements.txt```

## Useful links:

* [Unity](https://unity.com/download)
* [Unity documentation](https://docs.unity3d.com/Manual/index.html)
* [ML-Agents](https://github.com/Unity-Technologies/ml-agents)

The cars 3D models used in this project come from Sketchfab:

* [Chevrolet Corvette 1980 Different colours](https://sketchfab.com/3d-models/chevrolet-corvette-1980-different-colours-7e428bdb3ab54b4e9ac610e545fd9d03)

# How to open the project in Unity?

After cloning or downloading the repository, you have to follow these simple steps to open the project and work on it:

1. Open the Unity Hub 
2. Open an existing project
3. Select the folder in which the project has been saved and open it

Note that a Unity project is a collection of files and directories, rather than just one specific Unity Project file. To open a Project, you must select the main Project folder, rather than a specific file.

After opening the project you need to install the ML-Agents package vers. 2.0.0 from Window/Package Manager in unity editor.

# ML-Agents setup

## Installation

* [Installation](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Installation.md)

## Documentation

For further information it is better to consult the official documentation:
* [Documentation](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Readme.md) 

Here is a detailed description of all the features [ML-Agents Overview](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/ML-Agents-Overview.md). 


## Training 
Training can be performed in the Unity editor or in a built project.
To train inside the editor use the following command:

```mlagents-learn config/trainer_config.yaml --run-id=name```

 Otherwise, to train using a built project:

 ```mlagents-learn config/trainer_config.yaml --run-id=name --env=path/to/built/project```

Note that if a run id is already present, training will not start. To overwrite an existing run id add ```--force``` to the above command.
For more informations and features:
* [Training Documentation](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-ML-Agents.md)
