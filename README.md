# Project-V (Passion Project)

Being my passion project, I have huge plans that are likely far too big for me to ever complete but the process is the reason I keep at it and I have a lot of really interesting tech that I've implemented for it
and feel proud to show off. The project is made in Unity, but a lot of the parts I've created are custom, despite having Unity counterparts.
For example I have my own [custom camera controlling script](Assets/Scripts/Camera/ThirdPersonController.cs). Things like this happened simply because I was interested in it and decided to write my own implementation for my needs so they're not always feature complete.

--------------------------------------------------

## Terrain Generation
A lot of the focus so far has been into getting the framework right for terrain generation. I have lots of plans involving the terrain that have big incorporations into gameplay so having terrain that I can generate procedurally and regenerate with changes is a must.
This is why so far I've chosen to implement the [Marching Cubes](http://paulbourke.net/geometry/polygonise/) algorithm as once set up, you can change the underlying density texture and regenerate the mesh to reflect changes. Another feature is that the world is grouped
into chunks that make handling this huge amount of data a little bit easier. Most GPUs have a limit on texture size aswell so splitting up the generation into chunks was a must for a procedural world.

--------------------------------------------------

![Starter Layer Generation](https://github.com/whoJake/Project-V/assets/37589250/94fb3883-de53-4aa7-9d30-e1f7394357b9) 
The underlying density texture generation is left to a single compute shader for each layer (in this example, everything visible is one layer). This particular layer can be found at [Assets/Resources/Compute/Layers/StarterTerrain.compute](Assets/Resources/Compute/Layers/StarterTerrain.compute)


![Generation Seperated Into Chunks](https://github.com/whoJake/Project-V/assets/37589250/29c59b56-6f5d-43ac-839c-b4690c349786)
![ChunkLoading](https://github.com/whoJake/Project-V/assets/37589250/46b606b4-8001-411d-a958-d11f647a3ed5) 

Here you can see each chunk loading, one chunk per frame, when in play mode.

Since the terrain is split into chunks, I can edit them pretty much on command and then recalculate the mesh to see the result. Here are some edit actions that I've already implemented
![DynamicChunkEditing](https://github.com/whoJake/Project-V/assets/37589250/40fcf363-d3f9-46df-b8f8-d89d0ab551e4)

-----------------------------------------------

## Geometry Shader Grass
Only recently have I got to understanding the workings of the geometry shader pass but since the results can look quite pleasing I might aswell show the results of them here. Points randomized and sent to the material as vertices, then each of these vertices acts as a base for the grass mesh
which is built up inside the geometry shader.
![Geometry Shader Grass 2](https://github.com/whoJake/Project-V/assets/37589250/f5453483-7ee6-4864-a6c7-b96cfb60d374)
![GeometryShaderGrassChanging](https://github.com/whoJake/Project-V/assets/37589250/0002c87f-b1ec-4eb7-ae13-7cff69ec4650)

---------------------------------------------
Theres other things I've worked on but I'll save adding them to here until they are a bit more presentable
