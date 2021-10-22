Nos centramos en la prte de $L_i$ de la ecuacion
Es decir, de la radaizacion que entra del entorno

Tecnica de global ilumnation para calcular la radianza del entorno; perp a diferencia de raytraciong y otras, su coste no es muy elevado, ya que es derivar la luz del entorno de una imagen de alta fidelidad y de alto rango dinamico.

utiliza projection mapping para asociar partes del HDRi a la textura

## Pasos
1) Generar la light probe, con la iluminacion d ela imangen, capturando iluminacion del mundo real mediante un light probe image
2) Modelas la geometria y reflectancia de la imagen usando los amteriales apropiados (PBS)
3) Mapper la luz probe a una superficie emisiva que envuelve la imagen
4) Renderizar la escena iluminada por esta superifice emisiva
5) Tonemap las imagenes renderizadas

## Light probe
Usas imagenes HDR para capturas la iluminacion de una escena en fotos con 360 grados

Se pueden juntar varias fotos a SDR (standart dynamic range) con distitnas exposionces, y ser convertidas a una sola imagen HDR

## Enviorment maps
Una iamgen esferica puede ser mapeada como un cubemap o un mapa de longitud-latitud
Se suele usar  cubemap maps, porque tiene soporte de hardware

Y se toman con camaras de 360 grados
 Una vez este almacenadoen un cubamp por ejemplo, la textrua se visualiza com si embolviese al objeto, en un cubo, y se usasen las direcciones de salida del objeto para conseguir los dsitintos valores de la textura.
 Tambien se puede variar en cuncion del angulo de vision y relfjejarlo en la normla, para tener reflejos del entorno

## Implementacion practica del SPlit sum de Rendering
Una manera de aproximar practicamente la ecuacion de renderizado es
$$ L_0(v) \approx (\frac{1}{N} \sum^N_{j=1}L_i(j))(\frac{1}{N} \sum^N_{j=1} \frac{f(l_j,v)(n \cdot l_j)}{p(l_j)})$$
La primera suma pued eser precalculada a disintos niveles de rugosidad de las relfexiones, para que el cualculo de eso sea solamente un texture lookup.

Para el siguiente termino, se puede sustituir por una BRDF precalculada, pero cakculando aparte del fresnel

El primer termino es el PREM (Prefiltered Radiance Enviorment Mapping) y el segundo es una BRDF precalculada.
Me cachis eso es una BIG BOI integral

## Tone Mapping
Aplicar un filtro para que las tonalidades de la iamgen generada sean mas adecuadas a la realidad (o al estilo que se prefiere).

## Pathtracing
Raytracing pero solo con 1 bote

