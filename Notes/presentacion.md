In order to render a whole scene, in a eficient way, we need to evaluate an SDF of the whole scene.
ANd sicne we need wor the scene to be, a bit interesting, we would like for it to have complex shapes; built by convinibg the already disclosed primitives

And the solucion to dat, is Operations.
More concretlye, its a set of functions that operate on teh produced distance of an SDF, making them shape agnostic.
Moslty the operations that we are going to see merges 2 sdfs into a new distance function; or are just applied to one.

This can result in a hierarchical representation of the scene, since the Scene SDF can be viewed as a nest of merged SDFs via differnet operations.

Union operation
-	Basic ones
-	Goal is to create distnace function that represents both SDFs
-	Is very simple to do, since a SDF gives you teh closest distance to a point, so by taking the minum of both distances, you represent, and thereofre render, two different shapes with only one function.

We are going to talk with a bit more of detail about the union operations, that is one of the more basic ones; wich end goal is to show both SDFs on the result.

This works since the distance obtained by a SDF is the closest distance a surface; so this funcion will retunr the closest distance between the two sdfs; and since it is evaluated for all the pixels of the screen, the result is returning the values of boths SDFS.

Now this is one of the most importat qualities of distance functions; the felxibility that we have for manipulation.
For example, if we interpolate between the two distances, with a smoothing step, we find a really easy (and inexpensive way) of merging both shapes, based on proximity.

And that is really powrfull



## Demo TIme
Now for the demo, we used as a template the framework of teh course's labs with a few modifications.
Instead of rendering the volume to a cube, we render it to a full screen quad; since we want to render a whole scene.
Due to this change, we also needed to create a rudymentary camera system insice of the shader.
Also we used the spheretracing algorithm that we talked about, instead of raymarching.

We also aded some IMGUI contronsl and some infrastrucutre for it to run on teh shader.

We reused the phong code and the gradient calculation methods, for a better visualization


Here also are the resources, we divided them by theory resources, and code resources, if you want to check them out.

Thanks you for your time, is there any questions??