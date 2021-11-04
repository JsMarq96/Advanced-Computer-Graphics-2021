1
Es la ecuacion de renderizado de volumenes, la cual es una derivda de la radianza resultante en un punto, y dada una direccion, a lo largo del tamano transversal del objecto

El primer termino modela la obsorcion que sufre la radiancia cuando pasa por el medio, siendo L la raidiancia y \mu_a el termino de abosrcion
Sin ambargo no solametne se absorve la luz, tambien re felja en varias direcciones (scatter) perdiendo la intensidad en la direccion \Omega, lo cual se repreestna con el segunod termino y la -\mu_s, multiplicandola por la luz entrante

Sin ambargo, los cuerpos a veces pueden emitir luz propia, asi que se le anade la radianza que sale del punto x, en la direccion /Omega, lo cual se refelja el L_e

Y tambien puede absorver y reflejas la luz rellejada (scattered) por el entorno, asi que eso se calculara como el ultimo termino, volviendo a usar el factor de scatter, pero esta vez positivo; y una integral, cuyo objetivo es integrar todo el scaterring de la direccion \Omega
Esto lo hace inegrando la multiplicacion de la funcion f_p, que modela el refeljo bidireccional del scatering, por la luz que llega en la direccion que se integra.
Esto se integra sobre una circunferencia, para tener en cuenta las contribuciones dle scatter en todas las direcciones.

2
- Para simplificar la aecuacion de 1, solo tenemos en cuenta la aobsorcion y la emision, no calculamos el scattering
- $$L_{i+1} = L_i(1-A_i) + L_{ei}\Delta_i$$
- Delta_i es la diferencia entre el anterior ye l nuevo paso (s_1 - s_i+1)
	- L_i es la luz entrante en el punto s_i y L_ei es la luz que se le anade en el punto s_i
	- A_i es una maenra de escribit Delta_i * mu_a(s_i)
- En cada paso, se le va sustrayendo el componente difuso del resultado anterior, pero se le anede la contrinucion emisiva de ese punto
- Se basa en ir avanzando poco a poco, a lo largo de un rallo, desde la parte trasera (que la camara no ve) hasta la frontal, calculando las contribucnioes a cada paso de maenra discreta

3 Es el polinomio de talyor de orden 1, al rededor de 0 de e^x \approx 1 + x y si sustituimos x por ... da eso de resultado.
