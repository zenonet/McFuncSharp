# McFuncSharp

McFuncSharp is a collection of tools and libraries written in C# designed to generate Minecraft Datapacks.<br>
The core library "FuncSharp" can generate simple Datapacks and add entrypoints.

## McFuncScript

McFuncScript is a programming language that transpiles to McFunction (Minecraft Datapack Language). It uses FuncSharp for Datapack Generation.

### Language Features

* Variables (statically typed)
* If Statements
* Loops (currently limited by maxCommandChainLength)
   * While loops 
   * For loops
* Functions (transpile to actual minecraft functions; no parameter nor return value support yet)
* Operators
    * Comparison (>, <, ==)
    * Mathematics (+, -, *, /)
    * Not (!)
    * Ternary Comparison Operator (condition?positiveOutput:negativeOutput)
* Commands recreated as functions:
  * summon
  * setBlock
  * kill
  * say
* Types
  * String
  * Number
  * Vector (3D)
  * Entity (reference using a tag)
  * EntityType
  * BlockType
* Advanced Minecraft specific functionality:
  * raycasting
