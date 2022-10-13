# PSO2-Aqua-Library  
A library for handling PSO2 Aqua formats, with a focus on models, functional in grabbing model data from the game's format.   

A number of programs are now included which perform a variety of tasks related to PSO2.  

Aqua Model Tool - A tool for editing PSO2 models, along with various other utilities.  
  -Allows for editing many of the various structures in a model directly  
  -Allows for importing and exporting from .fbx models and the game's format  
  -Allows for importing and exporting the game's .prm/.prx simple model format  
  -Allows for importing (but not yet exporting) animations to PSO2's format from .fbx (Experimental. Export is being worked on)  
  -Allows for transplanting shader struct metadata from one model to another  
  -Provides a utility for 'spirefying' models (Created to assist with NGS's gathering primarily)  
  -Provides a utility for updating mod animations, or others only in old pso2, to be compatible with NGS bodies  
  -Provides a utility for batch converting models to .fbx  
  -Provides a utility for batch dumping pso2 .set file data to plain text  
  -Provides a utility based on aqp2obj for taking in and exporting .obj data from PSO2 models (Not recommended for NGS)  
  -Allows for converting the game's .text to and from plain text  
  -Allows for patching PSO2 Global's translation onto PSO2 JP.  
  -Allows for generating file reference sheets based on the selected client  
  -Allows for parsing shader data from the game's various models for analysis  
  -Allows for parsing the game's older VTBF type files to text for analysis  
  -Allows for dumping PSO2 NGS maps to .fbx and .png files  
  
  As well as a few non PSO2 features:  
  -Allows for converting most Phantasy Star Nova models to pso2's format  
  -Allows for converting most Phantasy Star Portable/2/2 Infinity models to pso2's format  
  -Allows for converting PSO1 PC n.rel stage models to pso2's format  
  -Allows for converting PSO1 .xvm texture archives and .xvr textures to .dds textures  
  -Allows for converting From Software flvers to .fbx or pso2's format  
  -Allows for converting BluePoint games .cmdl/.cmsh to .fbx or pso2's format  
  
  The library allows for many more PSO2 specific features, however.  
  
  
CMX Patcher - A tool for patching PSO2 .cmx files in order to expand what can be modified.  
  -Patches .cmx files to make life easier for character part mods
  -Can 'jailbreak' the character creator benchmark, allowing access to all character parts from the current game
  
Weapon Installer - A tool for listing out existing weapon files for PSO2 and NGS as well as swapping them at a basic level. Weapon swaps between categories may work better for old PSO2.  
  
Aqua Auto Rig - A command line tool intended to replace the old AQP2OBJ tool that was made by the Japanese community. Not recommended for NGS.  
  
