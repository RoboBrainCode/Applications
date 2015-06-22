VEIL - Dataset 500
Verb-Environment-Instruction Library
**************************************************************

This folder contains sub-folders each referring to a single environment. Each environment sub-folder further has 10 variations of the environment which differ in objects used and also the values of the states of same objects. The sub-folder also has a pddl file which contains the rules which are applied to the given environment.

***************************************************************

Files :- 

DomainKnowledge.pddl
       Contains physics rules of the world encoded in pddl  
       Strips format.

ControllerInstruction.txt 
           
       Lists all controller actions used in the dataset.

EnvironmentXMLTree.txt

       Gives the idea of how the environment xml tree looks 
       like.

Objects.xml
      
       This file contains static properties of objects used in 
       the environment. Each object used in the dataset has a 
       unique name [across environments] which consists of two 
       terms - its class name like Mug, LongGlass etc. and a 
       unique number [which differentiates identitical looking
       members] like LongGlass1, Mug3 etc.


