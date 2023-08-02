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

![Starter Layer Generation](https://github.com/whoJake/Project-V/assets/37589250/e2eece4f-f8e9-4791-91ef-047b64874a6e)
The underlying density texture generation is left to a single compute shader for each layer (in this example, everything visible is one layer). This particular layer can be found at [Assets/Resources/Compute/Layers/StarterTerrain.compute](Assets/Resources/Compute/Layers/StarterTerrain.compute). These compute shaders kind of perform like one giant Signed Distance Function which I find quite interesting to think about.

![Generation Seperated Into Chunks](https://github.com/whoJake/Project-V/assets/37589250/e232fcc5-692b-48b1-8589-682a6f2bec5f)
![ChunkLoading](https://github.com/whoJake/Project-V/assets/37589250/f826c4eb-ae99-4cc1-9875-917028d2dcbb)

Here you can see each chunk loading, one chunk per frame, when in play mode.

Since the terrain is split into chunks, I can edit them pretty much on command and then recalculate the mesh to see the result. Here are some edit actions that I've already implemented

![DynamicChunkEditing](https://github.com/whoJake/Project-V/assets/37589250/c0fe6d24-d54b-40c9-aa4d-7058c4893cb0)

-----------------------------------------------

## Geometry Shader Grass
Only recently have I got to understanding the workings of the geometry shader pass but since the results can look quite pleasing I might aswell show the results of them here. Points get randomized, placed on the pre-baked heightmap of terrain then frustum culled. They are then sent to the material as vertices where they each act as a base for the grass mesh which is built up inside the geometry shader.

![grassFrustumCulling](https://github.com/whoJake/Project-V/assets/37589250/ba844924-e4f0-42f6-ac33-f202da50dc99)
Example of the frustum culling

...

...
![alternateShape1](https://github.com/whoJake/Project-V/assets/37589250/14724b65-5f34-40ac-af02-b685882e7e3b)
Since the mesh is produced procedurally, the parameters for the grass shape can be inputted as a material property and updated in real time

...

...
![alternateColors3](https://github.com/whoJake/Project-V/assets/37589250/267753d4-8c53-4891-b51f-06f93df4c150)
The same applys to the colour of the blades ( just a simple gradient based on height in current implementation )

...

The current implmentation is far from performant but being able to mess with this part of the pipeline makes it easy to produce some good looking results.

---------------------------------------------
Theres other things I've worked on but I'll save adding them to here until they are a bit more presentable
