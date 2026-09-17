# Brushes para Terrain de Unity — RPG

16 máscaras originales de intensidad de 1024 × 1024, PNG de 8 bits en escala de grises. Creadas matemáticamente para esta entrega; no copiadas del catálogo de Unity. No se afirma que ningún otro paquete tenga formas similares.

## Instalación (flujo documentado para Unity 6)
1. Copia la carpeta Assets/RPG_Terrain_Brushes a Assets de tu proyecto.
2. Selecciona los PNG de Masks: Texture Type = Default, sRGB desactivado, Wrap Mode = Clamp, Filter Mode = Bilinear, Max Size = 1024, Compression = None; Apply. Para estas máscaras de pincel puedes desactivar Generate Mip Maps. No necesitan Read/Write.
3. En Project: clic derecho > Create > Brush. Asigna un PNG a Mask Texture. También puedes usar New Brush en el Inspector del Terrain. Repite para los que vayas a usar; importar PNG por sí solo no crea assets Brush.
4. Falloff: curva constante a valor 1, Radius Scale = 1, Remap = 0–1, sin invertir. Las máscaras ya incluyen desvanecimiento; otra curva puede atenuarlas o recortarlas más.
5. Selecciona Terrain > Paint Texture o Raise/Lower Terrain y el brush. En Paint Texture selecciona también el Terrain Layer que quieres pintar.

Blanco = efecto máximo; negro = sin efecto; gris = intensidad parcial. Unity usa el canal rojo de una máscara multicanal. Usa los PNG de Masks para pinceles, NO los de Decal_Alpha: estos últimos tienen RGB blanco y la forma en alpha.

Los porcentajes siguientes son puntos de partida orientativos: el tamaño del Terrain, la herramienta y la versión afectan el resultado. Para bajar terreno usa la acción de bajar de tu herramienta (Shift en el Raise/Lower clásico). Los brushes de relieve son máscaras de influencia: no heightmaps de elevación absoluta ni simulaciones físicas de erosión.

## Catálogo

| Archivo | Uso sugerido |
|---|---|
| 01_sendero_desgastado.png | Paint Texture: tierra, 10–25%; arrastrar siguiendo el eje largo. Rotar al cambiar la dirección. |
| 02_roderas_carro.png | Paint Texture o bajar altura: dos surcos de camino. Alinear antes de arrastrar; altura 1–3%. |
| 03_grava_dispersa.png | Paint Texture: grava en bordes de caminos, 10–20%. La máscara no crea piedras 3D. |
| 04_barro_fragmentado.png | Paint Texture: barro irregular en pantanos, 10–25%. |
| 05_musgo_islas.png | Paint Texture: manchas de musgo sobre suelo rocoso, 5–20%. |
| 06_hojarasca.png | Paint Texture con material de hojarasca, 10–20%. Son agrupaciones de máscara, no hojas con textura. |
| 07_orilla_erosionada.png | Paint Texture: arena o limo junto al agua, 10–20%; orientar siguiendo la orilla. |
| 08_claro_campamento.png | Paint Texture: tierra bajo campamentos, plazas rurales y entradas, 15–30%. |
| 09_escombros.png | Paint Texture: piedra entre ruinas, 10–25%; relieve ligero solo si el heightmap tiene resolución suficiente. |
| 10_grietas.png | Preferible decal para detalle fino; Paint Texture con tierra oscura para grietas grandes, 10–20%. |
| 11_cresta_montana.png | Raise/Lower Terrain: elevar a 1–5% en varias pasadas. Girar y variar tamaño para formar cordilleras. |
| 12_ladera_surcos.png | Raise/Lower Terrain: 1–3% sobre una ladera existente; orientar en la dirección de la pendiente. |
| 13_meseta_irregular.png | Raise Terrain: elevar suavemente. Aplanar después con Set Height si necesitas una superficie jugable perfectamente plana. |
| 14_cuenca_lago.png | Lower Terrain: bajar al 1–5%. Añadir agua por separado; esta máscara no crea agua. |
| 15_cauce_serpenteante.png | Lower Terrain: bajar al 1–4% por tramos, solapando extremos y suavizando uniones. Añadir agua por separado. |
| 16_borde_crater.png | Raise Terrain: elevar el borde a 1–3%; bajar el centro por separado con Cuenca de lago a menor tamaño. |

## ¿Terrain Layers transparentes como decals?
Con el shader estándar de Terrain, el alpha de la textura de color no equivale a transparencia convencional. Según shader/pipeline y Mask Map puede representar suavidad o densidad. La mezcla pintada entre Terrain Layers la controla la splatmap.

Para tierra que se funde con césped: aplica césped como base, añade un layer de tierra y pinta este último con los brushes a baja intensidad. La intensidad controla cuánto cambias los pesos en cada pasada; no es una opacidad fija de capa, y varias pasadas acumulan el efecto.

URP/HDRP ofrecen Opacity as Density en determinadas configuraciones: asignar Diffuse y Mask Map, y desactivar Enable Height-based Blend en el material Terrain Lit. Sirve para mezcla por umbral/densidad, no para convertir una capa en una pegatina de alpha convencional. No lo actives suponiendo que dará el mismo resultado que un decal.

Para marcas localizadas con fondo transparente: usa decals. En URP añade Decal Renderer Feature al renderer activo; crea un material con Shader Graphs/Decal (o un Decal Shader Graph), conecta/asigna textura y alpha, y asígnalo a un Decal Projector. Orienta el proyector hacia el suelo y ajusta su volumen hasta que interseque el Terrain. Si solo quieres color, desactiva aportes de normal/MAOS cuando tu shader lo permita. El terreno receptor debe usar un shader compatible con decals.

En HDRP utiliza su Decal Projector y material de decal, verificando que el material receptor admite decals. Built-in no utiliza la Renderer Feature de URP: requiere su flujo de Projector con material compatible o una solución específica. Los nombres de menú dependen de versión y pipeline.

## Extras: alpha para decals
Extras/Decal_Alpha contiene 16 versiones RGBA: RGB blanco uniforme y A igual a la máscara. Son siluetas teñibles para un material decal; no incluyen albedo fotográfico, normales, materiales ni prefabs. Cópialas a Assets si las necesitas. Importa como Default, Alpha Source = Input Texture Alpha, Wrap = Clamp. En un Decal Shader Graph conecta alpha de la textura a Alpha y multiplica RGB por el color deseado para Base Color. El RGB blanco permite teñir barro, musgo o marcas sin halos negros. Para relieve usa las máscaras de Terrain; un decal de color no deforma geometría.

## Resolución y verificación
El tamaño del PNG no aumenta la resolución del Terrain. Los trazos finos dependen de la resolución de alphamap al pintar y heightmap al esculpir: si desaparecen, aumenta su tamaño en mundo o ajusta prudentemente la resolución del Terrain. No son texturas tileables. Suaviza uniones de cauces y evita arrastrar sellos como el cráter.

Comprobado: 32 PNG de 1024 × 1024, bordes negros en máscaras, alpha equivalente en extras y ZIP íntegro. No se ha ejecutado el paquete dentro de Unity; faltan tu versión y pipeline para verificar esa integración. No hay scripts de Editor ni dependencias de paquetes.

Puedes usar y modificar estas máscaras en proyectos personales y comerciales, sin atribución obligatoria.

Fuentes oficiales consultadas:
- https://docs.unity3d.com/6000.0/Documentation/Manual/class-TerrainLayer.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/class-Brush.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-feature-decal.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/decal-shader.html
