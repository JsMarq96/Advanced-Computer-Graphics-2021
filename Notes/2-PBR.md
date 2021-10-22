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

Las ondas electromagneticas que podmeos observar son entre 400 y 700 namoetros.

### Ecuacion de Luz
La base de todo se basa en una mejor representacion que el modelo empirico de phong
Para eso existe la siguiente ecuacion que nos devuelve la luz reflejada en un objeto desde cualquier direccion
$$ L_0(\lambda, p, v) = L_e(\lambda, p, v) + \int_\Omega f(\lambda, p, l, v) L_i(\lambda, p, l)(n \cdot l)d\omega_i$$

Y si no tenemos en cuenta materiales emision, y no queremos representar explicitamtne una dependencia entre p y $\lambda$ tenemos, la cual es la Ecuacion de Reflectancia:
$$ L_0(v) = \int_\Omega f(l, v) L_i(l)(n \cdot l)d\omega_i$$
L integral integra $\Omega$, que es una hemisfera de unidad, en la cual se han proyectado todas las radiancias de ltodas las direcciones, que se vera relfejada en el vector v

- p es el punto del que calcular la luz reflejada
- $\lambda$ es la logitud de onda de la luz
- $l$ es la direccion de la luz actual
- v es la direccion del punto a el receptor (camara o ojo)
- n es la normal d ela superficie de p
- n dot l sirve para controlar la contribucion en funcion del angulo de visibilidad

- f es la funcion llamada BRDF (Bidirectional Reflectance Distribution Function), que define que parte de la luz que nos viene desde $l$ esta reflejada en la direccion v; y deve de cumplir la propidedad de reciprocidad (f(v,l) = f(l, v)) y cumplir con las layees de ocnservacion de la energia.
- $L_e$ es la funcion que represetna la emisividad de el objeto en el punto p
- $L_i$ es la intensidad de la luz entrante por i


### La luz
La interaccion de luz-materia esta descrita por un indice refractivo (que depende de la longitud de onda), qu puede ser escrito como un numero complejo.
La parte real mide como la materia afecta a la velocidad de la luz y la parte imaginaria define CUANTO se abosorve. Este indice varia en funcion de la longitud de onda d ela luz.

El indice refractivo es distitno para pcada material.

En la interficie de 2 materials (una superficie) se puede dar
-	Una reflexion (de la luz que no se aobsorve) (especular)
-	Una refraccion (la luz que se absorve) (difusa)

Sin embargo, cuando hablamos de materiales reales, esta superficie no es uniforme. Su rugosidad, incluso a un nivel microscopico, interviene en en estos resultados.

En funcion de la refraccion/relfexion, se pueden denominar 2 tipos de materiales:
- Metalicos: la relfexion es perfecta, por una abosorcion completa de los rayos que entran dentro del material, de modo que la unica luz que sale dle material es es la reflejada.
- Dieletricos: La relfeccion es imperfecta, por lo que algunos rayos de luz entras, y mediante el subsurface scatering, salen y se difuminan en varias direcciones (diffuse)

#### Propiedades de la luz
- Radiancia: Es definida como la energia por unidad de tiempo en un area unidad, proyectada sobre la normal de la superficie dado un angulo.
- Flujo radiante: es el flujo de la energia radiante por el tiempo, y se mide en Wattios
- Irradiancia (E): es la densidad del flujo radiante con respecto a el area
- itensidad Radiante es el flujo de la densidad con respecto a el angulo solido.

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
Una de las maneras mas simples y comunes de calcular la luz difusa es usando una difusion constante en toda la hemisfera aka Difusion lambertiana, que se da a partir de una constante $C_{diff}$ es el color diffuso de la superficie, o albedo difuso.
$$ f_{Lambert}(l, v) = C_{diff} / \pi$$

### BRDF Especular: $f_{specular}$
El modelado de la superficie se hace con microfacetas, imperfecciones a nivel microscopico.

Las refracciones especulares de estas microfacetas solo seran visibles cuando la normal este entre las direcciones de vsita y de incidencia. Este vector entremedias de los dos sera llamado el vector h. Conforma mas alineado este este vector h con la normal, mas se vera..?

A nivel microscopico todo material esta formado por microfacetas, y la BRDF especular esta modelado en funcion de esas microfacetas.
Cada una tiene su propia normal que es distitna que la normal de la superficie, y una manera que tenmos de saber si se reflejan es si su normal m, coincide con el half vector de la iluminacion y el view vector.

#### Formula general de la especular
$$ f_{pfacet}(l, v) = \frac{F(l, h) G(l, v, h)D(h)}{4(n \cdot l)(n \cdot v)} $$
- D(h) es la distribucion de las normales en las microfacteas
- G(l,v,h) es la fraccion de microfacetas que no estan ensombrecidas o en enmascaradas (?)
- F(l,h) es la reflectancia de Fresnel, que es la fraccion de la luz reflejada en una superficie puramente plana. Depende del angulo incidente y del indice refractivo (aka la longitud de onda) asi que lo representaremos usando 3 canales para simplifacr (RGB)
- El denomidaro es solametne normalizacion

#### Reflectancia de Fresnel F(l,n)
Representa la fraccion de luz que se relfeja desde una superficie plana.
Depende del angulo incidente y el indice refractivo del maerial (depende de la longitud de honde, es decir del color).
La podemos aproximas usando:
$$ F(l,n) \simeq F_0 + (1-F_0)(1-(l \cdot n))^5$$
$F_0$ es el color especular de la superficie y indica el resultado cuando el angulo de vision es 0 grados
En funcion del material este valor variara:
- metales tienen valores altos
- Semiconductores tiene valores intermedios
- No metales tien valores bajos

#### Funcion de Distribucion $D_p$
Funcion que mode la la distribucion estadistica de las normales de las microfacetas.
Ya que solo podemos ver cuyas normales son el vector half del view y la luz incidente, se puede evualuar como D(h) o D(m) que puede ser represetnado como la porporcion de microfacetas orientas en la direccion h/

La distribucion mas usada para esta funcion es la formula de Blinn-Phong:
$$D_p(m) = \frac{\alpha_p + 2}{2\pi}(n \cdot m)^{\alpha_p}$$
$\alpha_p$ es el paramtro de rugosidad (roughness), el cual tocan los artistas de texturas (de 0-1)
Generalmente esta formula se suele usar para la dsitribucion, que aunque sea un poco ams cara, da resutlados mas neutrales (epic games)
$$ D(m) = \frac{\alpha^2}{\pi((n \cdot m)^2 (\alpha^2 - 1) + 1)^2}$$
En la que el roughness/rugosidad $\alpha = roughnes^2$

#### Distribucion de Geometria
Describe las capacidades de "self-shadow" de las microfacetas; es decir, dice que proporcion de las microfacetas visibles estan ni ensombrecidas ni tapadas.

Si la superficie es muy rugosa, las microfacetas se pueden tapar unas a otras, reduciendo la cantidad de luz que se refleja

Se pueden ahcer varias cosas:
- Ponerlo a 1, el cual hace que el calculo sea implicito en la funcion de distribucion
- Usar la prpuesta de Cook-Torrance:

$$G_{tc}(l, v, h) = min(1, \frac{2(n \cdot h)(n \cdot v)}{v \cdot h}, \frac{2(n \cdot h)(n \cdot l)}{v \cdot h})$$
- Aproximas Cook torrance con Kelemen et al, para que sea menos cara
$$\frac{G_{tc}(l, v, h)}{(n \cdot l)(n \cdot v)} = \frac{1}{(l \cdot h)^2}$$
- Aproximacion de Epic
	$$ G(l,v,h) = G_1(l)G_1(v)$$
	$$ G_1(v) = \frac{n \cdot v}{(n \cdot v)(1 - k) + k} $$
En el que $k = \frac{(roughness+1)^2}{8}$

#### Resumen
D representa las microfactes cuya normal es suitable, G es la parte de esas que no esta tapada, y 

### Aproximaciones Numericas de la ecuacion de renderizado
$$ L_0(\lambda, p, v) = L_e(\lambda, p, v) + \int_\Omega f(\lambda, p, l, v) L_i(\lambda, p, l)(n \cdot l)d\omega_i$$
Para resolver la ecuacion de renderizado, devemos de ir resolviendo la de manera recursiva, (el L_i de uno, sera el L_0 de otro).
El acto de resolverlo se llama calcular la Globla Ilumination, donde raycasting/tracing se utilizaria

#### Aproximacion de Monte Carlo
Una manera de resolverlo seria tomar la eciacion de reflectancia y samplearlo aleatoriamente, y hacer la emdia del resultado.
Esto se conoce como metodos de montecarlo
$$ L_0(v) = \int_\Omega f(l, v) L_i(l)(n \cdot l)d\omega_i$$
$$ L_0(v) \approx \frac{1}{N} \sum_{j=1}^{N} f(l_j, v) L_i(l_j)(n \cdot l_j)$$
Otra aproximacion mas correcta es incluir la funcion de deistribucion de densidad de probabilidad, de la distribucion sampleada
$$ L_0(v) \approx \frac{1}{N} \sum_{j=1}^{N} \frac{f(l_j, v) L_i(l_j)(n \cdot l_j)}{p(l_j)}$$
Para que este metodo funcione bien, se tiene mucho cudiado ala hora de elegir el sampling, intentando usar una tecnica de importance sampling

#### Split Sum
Otra apxormiacion es dividir la anterior aproximacion en 2 sumarios, y es correcto siempre y cuando L es constante, y incluso sin esos puede funcionar bastnate bien en varios casos
$$ L_0(v) \approx (\frac{1}{N} \sum_{j=1}^{N} L_i(l_j))(\frac{1}{N} \sum_{j=1}^{N} \frac{ f(l_j, v) (n \cdot l_j)}{p(l_j)})$$