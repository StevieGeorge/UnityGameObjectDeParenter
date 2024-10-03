# UnityGameObjectDeParenter
A Unity Editor script to improve performance by moving nested gameobjects out of empty parents with default scale

-----
Developers have found through testing that GameObjects which are nested beneath other GameObjects perform dramatically worse than those in the root of the scene. Even a single static parent can have a severe impact. 
However, nesting game objects can be extremely useful for organizing a scene. This script allows you to preserve a scene hierarchy while alleviating the performance impact.

This is accomplished by executing whenever the game is built or run in-editor (IProcessSceneWithReport).
The script digs through all of the active scene's GameObjects and checks to see if they have only a Transform with no other components, a default scale, and any child objects nested beneath it. If it does, it moves all of the children to the scene root.

This script should have no permanent effect on your project's hierarchy, only the build. However Unity bugs do occur, so back up your project first.

-----
INSTALLATION:

Simply copy the contents of the Assets folder into the Assets folder of your project. There is only one script.
By default, the script only operates on children of objects which are active and have the "Untagged" tag, due to the high likelihood than specially-tagged empty objects might make use of their hierarchy. You can customize this behavior by changing those variables in the script. You may choose to block or exclusively allow any number of tags.
