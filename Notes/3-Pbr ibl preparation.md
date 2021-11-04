## Preparacion para el examen:

3 
This function represents the diffuse factor of a BRDF, more concretly the Lambertian aproximation of the diffuse, that is based on a Base color divided by pi/ with is the area of the unit hemisphere.

A brdf is based on two base functions, and each one calculates a different feature.
- The specular one, that models the reflected light
- A diffuse one, that models the refracted light
The presented function is a diffuse function of a BRDF, more accuratly a Lambert aproximacion to the diffuse funcion, that returns a constant color for the surface (the c_diff is callled a diffuse color or diffuse albedo). This is said to be more thatn enough for modeling teh diffuse part, and ther is no need 



4 
That ecuation represents a part of a BRDF, Bi directional reflection function, wich is used to model the specular reflections of a material.

F(l,h) is the calculation of Fresnel, wich representas a fraction of light that is reflected depended on the inciding vision angle.
G(l,v,h) is teh Geometry distribution function, that model how many of teh microfacets that are visible (the normal is aligned) are not occluded, via thers microfacets.

D(h) is the distribution function, that models how wuch of the surface's microfacets are aligned (the microfacet's normal is ecual to the half vector of the view and light), and thereofre facing out way, contrbuting the the specular reflection.
The bottom part is anormalization factor

l is the light vector, tha comes from the lightsource position the the actual point
v is the view vector, the direction from the camera position to the point that we are rednerinf
h is the half vectors, is the sum between the view vector and teh light vector
n is the surface norla vector

5 
Since the montecarlo method is based on random sampling, and averange it, so we are not going to calculate the whole hemisphere.

Instead, we are going to calculate N samples, of the hemisphere, living us with an average sum.

Now in order to increase the accuracy, we divide the elemtns of the sum with the probabliity function, of the samples, including it.

One important part is that he sampling shlud not be random, since this greatly impacts theaccuracy of the model, so using some kind of priority or importance probablity funcion is required.



If the system is lenar then you can get this things in constant time???

It is NOT time invariant