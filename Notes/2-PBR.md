### Que es?
Un sistema que modela la realidad de manera mas basada en la fisica, para conseguir imagenes fotorealistas

Se requieren 2 componentes
- Physcially based shading
- Ilmunation

## Physically based shading
Cunado la luz refleja en un objeto se pueden prdocuri varios fenomenos
- absorcion: la luz se convierte en calor
- scatering: la luz cambia de direccion cunado choca
- emision: energia (calor) se conviernte en luz

### Ecuacion de Luz
La base de todo se basa en una mejor representacion que el modelo empirico de phong
Para eso existe la siguiente ecuacion que nos devuelve la luz reflejada en un objeto desde cualquier direccion
$$ L_0(\lambda, p, v) = L_e(\lambda, p, v) + \int_\Omega f(\lambda, p, l, v) L_i(\lambda, p, l)(n \cdot l)d\omega_i$$

Y si no tenemos en cuenta materiales emision, y no queremos representar explicitamtne una dependencia entre p y $\lambda$ tenemos:
$$ L_0(v) = \int_\Omega f(l, v) L_i(l)(n \cdot l)d\omega_i$$
(Es una integral para tener en cuenta infintios puntos de luz posibles)

- p es el punto del que calcular la luz reflejada
- $\lambda$ es la logitud de onda de la luz
- $l$ es la direccion de la luz actual
- v es la direccion del punto a el receptor (camara o ojo)
- n es la normal d ela superficie de p

- f es la funcion llamada BRDF (Bidirectional Reflectance Distribution Function), que define que parte de la luz que nos viene desde $l$ esta reflejada en la direccion v; y deve de cumplir la propidedad de reciprocidad (f(v,l) = f(l, v)) y cumplir con las layees de ocnservacion de la energia.
- $L_e$ es la funcion que represetna la emisividad de el objeto en el punto p
- $L_i$ es la intensidad de la luz entrante por i


### La luz
La interaccion de luz-materia esta descrita por un indice refractivo (que depende de la longitud de onda), qu puede ser escrito como un numero complejo.
La parte real mide como la materia afecta a la velocidad de la luz y la parte imaginaria define CUANTO se abosorve.

El indice refractivo es distitno para pcada material.

En la interficie de 2 materials (una superficie) se puede dar
-	Una reflexion (de la luz que no se aobsorve)
-	Una refraccion (la luz que se absorve)

Sin embargo, cuando hablamos de materiales reales, esta superficie no es uniforme. Su rugosidad, incluso a un nivel microscopico, interviene en en estos resultados.

#### Radiancia
Es definida como la energia por unidad de tiempo en un area unidad, proyectada sobre la normal de la superficie dado un angulo.

### La eciacion de reflexion (refraccion?)
Modela una radiancia resultante $L_0$ en un punto p en una direccion; como resultado por todas las radianzas que llegan a p proyectadas sobre un hemisferio (porque??)

En la hemisfera se proyectan todos los resultados y se calcula la integral para conseguir el valor.

$$ L_0(v) = \int_\Omega f(l, v) L_i(l)(n \cdot l)d\omega_i$$

## BRDF
Para calcular un BRDF lo modelamos a partir de 2 funciones:
- Especular (inmperfecta)
- Diffusa
Y la suma de los 2 nos da el resultado que queremos

### Difusion Lambertiana
Una de las maneras mas simples y comunes de calcular la luz difusa es usando una difusion constante en toda la hemisfera aka Difusion lambertiana, que se da a partir de una constante
$$ f_{Lambert}(l, v) = C_{diff} / \pi$$
En el que $C_{diff}$ es el color diffuso de la superficie, o albedo difuso.

### BRDF Especular: $f_{specular}$
El modelado de la superficie se hace con microfacetas, imperfecciones a nivel microscopico.

Las refracciones especulares de estas microfacetas solo seran visibles cuando la normal este entre las direcciones de vsita y de incidencia. Este vector entremedias de los dos sera llamado el vector h. Conforma mas alineado este este vector h con la normal, mas se vera..?

#### Formula general de la especular
$$ f_{pfacet}(l, v) = \frac{F(l, h) G(l, v, h)D(h)}{4(n \cdot l)(n \cdot v)} $$
- D(h) es la distribucion de las normales en las microfacteas
- G(l,v,h) es la fraccion de microfacetas que no estan ensombrecidas o en enmascaradas (?)
- F(l,h) es la reflectancia de Fresnel, que es la fraccion de la luz reflejada en una superficie puramente plana. Depende del angulo incidente y del indice refractivo (aka la longitud de onda) asi que lo representaremos usando 3 canales para simplifacr (RGB)
- El denomidaro es solametne normalizacion

#### Reflectancia de Fresnel F(l,n)
$$ F(l,n) \simeq F_0 + (1-F_0)(1-(l \cdot n))^5$$
$F_0$ es el color especular de la superficie
En funcion del material este valor variara:
- metales tienen valores altos
- Semiconductores tiene valores intermedios
- No metales tien valores bajos
#### Fresnel
Conforme el angulo entre la superfice y el vector de vesta es mayor, mas se refleja.. ?

#### Funcion de Distribucion $D_p$
La distribucion mas usada para esta funcion es lla formula de Blinn-Phong:
$$D_p(m) = \frac{\alpha_p + 2}{2\pi}(n \cdot m)^{\alpha_p}$$
$\alpha_p$ es el paramtro de rugosidad (roughness), el cual tocan los artistas de texturas (de 0-1)
Generalmente esta formula se suele usar para la dsitribucion
$$ D(m) = \frac{\alpha^2}{\pi((n \cdot m)^2 (\alpha^2 - 1) + 1)^2}$$
En la que el roughness/rugosidad $\alpha = roughnes^2$

#### Distribucion de Geometria
Describe las capacidades de "self-shadow" de las microfacetas.
Si la superficie es muy rugosa, las microfacetas se pueden tapar unas a otras, reduciendo la cantidad de luz que se refleja
Se pueden ahcer 2 cosas:
- Ponerlo a 1
- Usar la prpuesta de Cook-Torrance:

$$G_{tc}(l, v, h) = min(1, \frac{2(n \cdot h)(n \cdot v)}{v \cdot h}, \frac{2(n \cdot h)(n \cdot l)}{v \cdot h})$$

Aproximaciones??