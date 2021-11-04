## Porque?
Generalmente en graficos, se suele tener mas en cuenta el renderizado de superficeis, y reflexion en supericies
Pero hay tareas que requieren la representacion de volumenes, como si la luz se requiere atravesar o no, por ejemplo en medicina

## La luz
La luz se peude absorvver, emitir y refractar en funcion del manterial

La radiancia es la energia por unidad de tiempo que se recive en una area unitaria en un angulo

## Macro Modelo
Cuando la luz atraviesa un medio, con una radiancia L, una direccion $\omega$, como podemos definar la radianzia que nos lelga al ojo, despues de atravesar el medio??

### Micro Modelo: Out-scaterring
Parte de la luz que entra en el cuerpo se pierde por el scattering, y sale en otras direcciones.
Se representa esta perdida como una derivada, en funcion de la logitud por por la que pasa por el cuerpo(dz); dado un punto dentro del objeto (dado un pto y una direccion, que parte de la luz se disemina):
$$\frac{dL}{dz} = -\mu_s(x)L(x, \omega)$$


### Micro Modelado: Absorcion
Representa la parte de la radianza que ha lelgado a x, en una distancia dz, que se ha obsorvido por el medio, y se definie con esta derivada:
$$ \frac{dL}{dz} = -\mu_a(x)L(x, \omega)$$

En estos dos terminos, tanto $\mu_a$ como $\mu_s$ son coeficientes de absorcion y out-scaterring respectivamente.

### Micro Modelo: Emision
No todo es sustraer, tambien hay sustencias que emiten luz que se incorpora a la radizncia

Dado un pto x, con una direccion $\omega$, con una funcion de emisividad Le, esto se puede representar como una derivada:

$$\frac{dL}{dz} = \mu_e(x) L_e(z, \omega)$$
OJO: a veces el coefieciente de emisivdada esta incluido en la funcion de emidivdidad.

### Micro Modelo: In-scaterring
La luz que se quita en algunas direccion con el uot-scattering, tiene que salir por algun lado. Se define con esta derivada:
$$\frac{dL}{dz} = \mu_s(x) L_s(x, \omega)$$

Pero en este caso, la funcion de L_s, es una integral que calcula toda la luz proveniente de todas las direcciones:
$$L_s(x, \omega) = \int f_p(x, \omega, \omega') L(x, \omega') d\omega'$$

La integral es sobre S^2 que es el area de una esfera completa y f_p es la funcion de scaterring bidireccional. (f_p calcula el scatter dado la luz que viene de la direccion omega' dado por L(x, \omega))

## Conclusion: Micro modelo completo
$$ \frac{dL}{dz} = -\mu_a(x) L(x, \omega) - \mu_s L(x, \omega) + \mu_e(x) L(x, \omega) + \mu_s(x) \int f_p(x, \omega, \omega') L(x, \omega') d\omega'$$
 Esta es la version de la ecuacion de transferencia de luz
 
 ## Macro
 La eciacion diferencial que hemos conseguido a aprtir del micro modelo igualmetne encesita ser resuelta
 
 Para simplificarlo, podemos solamente considerar los terminos de absorcion y emision, lo cula haria que se podria resolver analiticamente, con una inetegral por un rayo
 
 $$L(u) = L(s^0)e^{-\int^u_{s_0} \mu_a(t) dt} + \int^u_{s_0} L_e(s) e^{- \int^u_{s} \mu_a(t) dt} ds$$
 
 En esta integral s_0 es el punto de "emision" de la luz y la radiancia inicial L(s0) se atenia conforme la distanica es mayor... De ahi que sea una exponencial negativa de el area de s0 a u (el rallo por el que se integra..? la direccion??)
 
 ira decallendo confome aumente la distancia y dependera del facctor de absorcion
 
 El factor de decaimiento se conoce como $T(s_0, u)$ y se llama Optical thickness (grosor optico??)

Asi que se puede reescribir esto como:
$$ L(u) = L(s_0) T(s_0, u) + \int^u_{s_0} L_e(s) T(s, u) dw$$

Esa integral es para la propiedad emisiva, tiene su propio optical thinkess, ya que la luz que la nueva luz que atraviese el meido, tambien esperimentara un decay; y la emisividad se considera radiancia extra.


# Aproximacion numericas
## Back to front: Metodo de Euler
Se basa en calcular la radianza en pequenos intervalos, enpezando con desde la luz, y atravesando el objeto, hasta llegar a el frente.
El proceso es que por cada paso, se anade la luz y se atenua.
Y este proceso se repite hasta salir del cuerpo.. hasta el front

Incluso si tenemos el metodo, tenemos que hayar una manera de resolver las DE propuestas de manera iterativa, no continua.
Por eso, usamos el metodo de euler partiendo de la DE base:
$$\frac{dL(s)}{ds} = -\mu_a(s) L(s) + L_e(s) $$
E discretizamos el intervalo de $[s_0, u]$ (emision, ojo/camara..?) de manera que
	- $L_i \approx L(s_i)$
	- $\Delta_i = s_i - s_{i+1}$
	- $\mu_a(s_i) = \mu_i$
	- $L_e(s_i) = L_{ei}$
Entonces aproximamos:
$$\frac{L_{i+1} - L_i}{\Delta_i} \simeq -\mu_a(s_i)L_i + L_e(s_i)$$
Y conseguimos:
$$L_{i+1} - L_i = -(\mu_i L_i)\Delta_i + L_{ei}|Delta_i$$
Lo cual equivale a la aproximacion final:
$$L_{i+1} = L_i(1-A_i) + L_{ei}\Delta_i$$
$$L_0 = L(s_0)$$
En la que $A_i = \mu_i \Delta_i$

## Front to back: Compositing
El que mas se utiliza
Se empieza desde el frente (la camara) y se va moviendo en itnervalos de s $\Delta s$ en los que se va anadiendo las contribuciones tenuadas de cada punto, hasta que llegemos a la parte trasera.

Parteindo de la integral de renderizado de volumen:
 $$L(u) = \int^u_{s_0} L_e(s) e^{- \int^u_{s} \mu_a(t) dt} ds$$
La idea es aproximar la integral de renderizado:
$$L(u) \approx \sum_{i=0}^{N-1} L_{ei} \prod_{j=0}^{i-1}(e^{- \mu_j \Delta_j}) \Delta_i$$
 Y podemos aproximas la exponencial con los 2 primeros terminos del polinomio de taylor en 0: $e^{-\mu \Delta} \approx 1 - \mu \Delta$
 $$L(u) \approx \sum_{i=0}^{N-1} L_{ei}( \prod_{j=0}^{i-1}(1 - \mu_j \Delta_j)) \Delta_i$$
 En el que $L_{ei} = L_e(s_i)$, $\Delta_i = s_{i+1} - s_i$ y $\mu_i = \mu_a(s_i)$ siendo i la iteracion actual/punto que estamos calculando
 
 Lo cual puede ser calculado mediante una composicion:
 $$\prod_j = \prod_{j-1} (1 - \mu_j \Delta_j)$$
 $$\sum_j = \sum_{i-1} + L_{ei} \Delta_i \prod_i $$
 NOTA: cuando el acumulador empieza a ser muy bajo, se puede y debe parar el calculo.
 Tambien se suele usar con Sampling adaptivo
 
 # Rayscasting
 Lanzar rayos desde la camara para ver si colisinan con el objeto y lo transpasan
 Se tocara mas en los eminarios
 
 
 # Fuentes de informacion
 De donde sacamos el volumen??
 
 En medicina, las imagenes de rayos X son imagenes 2D,q ue resultan de la absorcion de rayos x dada una fuente
 
 - CT (computerized topography): son "slices" de muchas imagenes de rayos X, por lo que al usar muchas tomadas con distancias y precios adecauadas, se puede conseguir una imagen 3D
 
-  MRI: capta los resultados de la radiacion de los atomos cuando se exponenen a campos magenticos muy fuertes. Puede porducir datos tridimensionales.
-  Ultrasonidos: 3D
-  PET: 3D
-  Simulaciones: number crunching para producir resutlados que se enecesitan visualizar, como simulaciones dinamicas de fluidos. El grid usado para el sampling es generalmente usado tambien para la representacion!
 
 Los rayor x y los Ct muestras estrucutra y composincion, pero los MRI muestras procesos y funcionalidad; por lo que se suelen juntar para tener informacion adicional.
 
 Aqui tambine entran las ecografias, que se usan para obejtener imagenes en 2D y 3D
 
  # Discretizacion: Voxeles y grids
 La informacion obtenida ha de ser almacenada en alguna manera, en una forma de que ayude al analisis de esta:
 ## Voxeles
 Un voxel peude ser tanto una represetnacion de un volumen, como una representacion de puntos en 3D
 Es una representacion compatible con un grid uniforme; y generalmente se toamara samples de manera uniforme y se interpolara entre las samples para conseguir una represetnacion mcas copmleta entre las samples.
 
 Generlamente para representar en voxeles se empleara interpolacion trilineal.
 
 # Pipeline de renderizado de volumenes
 Pasos:
 1) Transversacion de datos (Data transversal): se definene que puntos se samplearan del modelo
 2) Interpolacion: para obetenr valores en los puntos adecuados
 3) Compositiong: calculo de la integral de renderizado de vulmen (y su renderizado)
 
 ## Metodos de renderizado
 - Basados en imagen (image order): el analisis de la  imagen y los puntos qu se samplean dependen directamten del plano de imagen (Raycasting)
 - Basados en el objeto (objct order): Se escanea el volumen en 3D y kuego eso se proyecto sobre el plano de imagene  (cunfiones de transferencia..?)
 
 ### Raycasting
 Configuracion:
 	Calcular la primera colision/interseccion entre la figura y el rayo
Bucle de travesia:
	Se samplea a lo largo del rayo
		1) Acceso de datos: despues de la interpolacion, calcula el color y la opacidad d ela   muestra
		2) Compositing: actualizar el color y la opacidad (no blending OJO)
		3) Avanzar posicion del rayo a el siguiente punto de sampleo
Terminacion dle rayo: determinar cuando el rallo ha salido del volumen
FIN

### Funciones de transferencia
Funciones que se encanrgan de asingar propiedades opticas a la informacion de base, de la cual se esta generando el voumen (ej, si el mri tiene x valor que se ponga de este color, para una mejor representacion), y pueden ser propiedades como abosrcion, emision...

Se pueden plantear de 2 maneras:
	- Mappeado pre interpolacion: se samplea las propiedades opticas de los datos volumetricos (la funcion de transferencia se aplico antes).
	- Mapeado pos interpolacion: interpola los datos volumetricos para obtener samples y luego se aplica la funcion de transferencia a esos datos.
	
## Interpolacion en Grids uniformes: LERP & Friends
Las interpolaciones mas comunes en computer grapchis son:
- Lineal: $f(p) \approx (1- x)f(v_i) + xf(v_{i+1})$
- Bilineal: $f(p) \approx (1-x)(1-y)f(v_{i,j}) + x(1-y)f(v_{i+1,j}) + (1-x)yf(v_{i,j+1}) + xyf(v_{i+1,j+1})$
- Trilinal: $f(p) \approx (1-x)(1-y)(1-z)f(v_{i,j,k}) + x(1-y)(1-z)f(v_{i+1,j,k}) + x(1-y)zf(v_{i+1,j,k+1}) + (1-x)y(1-z)f(v_{i,j+1,k}) +(1-x)yzf(v_{i,j+1,k+1}) + xyzf(v_{i+1,j+1,k+1})$
Para samplear e interpolar datos de varias dimensiones de manera correcta.
NOTA: tanto la bilinieal como la trilineal se puede pensar como una composicion de interpoalciones lineales (LERP), como:
		$$biLERP(quad, point) = LERP(LERP(quad.corner1, quad.corner2, point.x),  LERP(quad.corner3, quad.corner4, point.x), point.y)$$
## Interpolacion en Grids no uniformes: baricentricas
A veces el sampling no es uniforme, y depednecide de la forma del grid se pueden usar otras maneras de obtener samples, que se apadapten.

En el caso de figuras tetrahedrales, se usan las coordenadas barycentricas

- 2D: Una figura tetrahedral en 2D es un triangulo, y se interpola convertiendo los vertices en coordenads baricentricas (x,y) -> $(\lambda_1, \lambda_2, \lambda_3)$, siempre y cuando $\lambda_1 + \lambda_2 + \lambda_3 = 1$ y los extremos sean (0,0,1), (1,0,0), (0,1,0).
		$$f(p) = \lambda_1 f(v_1) + \lambda_2 f(v_2) + \lambda_3 f(v_3)$$
- 3D es similar, pero las coordenadas son 4 dimensiones $(\lambda_1, \lambda_2, \lambda_3, \lambda_4)$
		$$f(p) = \lambda_1 f(v_1) + \lambda_2 f(v_2) + \lambda_3 f(v_3) + \lambda_4 f(v_4)$$