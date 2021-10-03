### Recursos para el curso
- Libro real Time Rendering (epic)
- El curso Y presentaciones de siggraph de PBR

### Basicos de shading
Iluminacion local con el modelo de Phong (no Blin-phong)
Se basa en la combinacion de varios valores, a saber:
- Luz ambiente: factor uniforme que intenta aproximar la iluminacion global que le llega, solo un numerito
- Especular: es la luz que se refleja y por lo tanto tiene el color de la luz. Them white splotches of light. Dependen de la superficie y de la perspectiva
- Difusa: Depende de la posicion del pto de la luz y coje el color de la luz y del material (

$$ I = k_a i_i + k_d(N \cdot L)i_i = k_s(R \cdot V)^n i_i$$
$i_i$ es la luz incidente
N es la normas
K$k_a$ es la constante de la luz ambiente
$k_d$ es la constante de la luz difusa
$k_s$ es la constante de la luz especular
Estos dependen del material