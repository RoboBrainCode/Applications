# Applications
This repository contains applications built over roboBrain using raquel to query the roboBrain

## Instruction for installing setup:
### Dependencies:
#### Openrave:
Planit requires openrave to be installed for loading environments and planning paths. ( An existing installation of ffmpeg may result in a segmentation fault in loading openrave, reason unknown)
Openrave can be installed from the instructions mentioned in the following tutorial:

http://www.aizac.info/installing-openrave0-9-on-ubuntu-trusty-14-04-64bit/

#### C# compiler:
Tell Me Dave is written in C#. Hence the application requires the compiler for C# to be installed on the machine. C# complier can be installed on Ubuntu as: 

`sudo apt-get install mono-devel`

#### Stanford Parser:
TellMe Dave requires stanford parser for parsing natural language instructions.Stanford Parser can be downloaded from : 

https://drive.google.com/folderview?id=0B1j3R7CmuYG-fkZrb3NQOF8yeDZJZ1ZLaWpjWk5nd2NCYzdPbEVjdGlRSTdqS1BORUppaTA&usp=sharing

and must be copied within the src folder of tellmedave.

### Individual component compilation
Compile tellmedave files by running  

	`./tellmedave/compile.sh` 
	
from the main folder. 

### Running the application:
The application can be run as : 

	`python text_to_traject.py` 

User can set the following parameters in the application. The parameters can be set in the file text_to_traject.py 
* envPath : The path to the environment dae file. 
* contextGraph : The context file which specifies the activities taking place in the environment 
* trajectorySaveLocation : the location where to store the final trajectories obtained via planit 
* inputStr : The natural language instruction given as input 

## Running the application on aws machine
* ssh to the aws machine:
	ssh -i conf/www/pem ubuntu@52.25.65.189
* cd downloads/Applications
* python text_to_traject.py
* in order to see trajectories, use -X during ssh and uncomment the following line

`PathPlanner.playTrajFromFile(envPath,trajectorySaveLocation)` 

in test_to_traject.py file
