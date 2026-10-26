# Art prompts (GPT)

Prompts are English (Sezgin's rule). One picture per unit or pest — never animation frames
(ADR 001).

## Where the pictures go

| What | File | Facing | Aspect (width : height) |
|---|---|---|---|
| Das Ei | `game/art/units/egg.png` | lying on its side, right | 90 : 62 |
| Käseblock | `game/art/units/cheese.png` | right | 150 : 56 |
| Toastbrot | `game/art/units/toast.png` | right | 92 : 84 |
| Gurken-Kanone | `game/art/units/pickle.png` | right, lying flat | 120 : 44 |
| Senfflasche | `game/art/units/mustard.png` | right, upright | 48 : 124 |
| Pfefferstreuer | `game/art/units/pepper.png` | right, upright | 42 : 70 |
| Kirschtomate | `game/art/units/tomato.png` | right | 1 : 1 |
| Ameise | `game/art/enemies/ant.png` | **left** | 1 : 1 |
| Fleischbällchen | `game/art/enemies/meatball.png` | **left** | 1 : 1 |
| Fruchtfliege | `game/art/enemies/fly.png` | **left** | 1 : 1 |
| Wackelpudding | `game/art/enemies/pudding.png` | **left** | 1 : 1 |

The file name is the `visual` key in `units.json` / `enemies.json`. The game fits the picture into
the unit's size (`width` × `height`), so **crop the transparent margin away** and keep the aspect
above — or change `width`/`height` in the JSON to match the picture. The picture should fill the
whole shape: the physics uses the shape, not the picture.

## Base style (paste first, every time)

> Cute cartoon game asset for a 2D mobile-style game, side view, bold thick dark-brown outline,
> flat bright colors with soft cel shading, big expressive eyes, simple readable silhouette,
> centered, the object fills the whole image edge to edge, transparent background, no ground
> shadow, no text, no frame. Same style as the rest of the series.

## Per unit (add after the base style)

- **egg:** "A raw white chicken egg lying on its side, nervous worried face looking right. Aspect ratio 3:2, wide."
- **cheese:** "A wide flat block of bare yellow cheese with holes on every side, no rind, no wax. Brave smiling face looking right. Very wide aspect ratio 8:3." (Every face sticks — no rind may suggest otherwise, ADR 003, 2026-10-26.)
- **cheese_triangle:** "An equilateral triangle of bare yellow cheese pointing up, with holes, no rind, no wax. Brave smiling face looking right. Aspect ratio 110:95." (The cheese block's other shape; every face sticks.)
- **toast:** "A thick square slice of toast bread with golden crust, calm smiling face looking right. Almost square."
- **pickle:** "A green pickle lying horizontally like a cannon barrel, determined face looking right. Wide aspect ratio 11:4."
- **mustard:** "A tall thin yellow mustard squeeze bottle with a red cap, cheeky face looking right. Tall aspect ratio 2:5."
- **pepper:** "A small glass pepper shaker with a metal cap with holes, grumpy face looking right. Tall aspect ratio 3:5."
- **tomato:** "A round shiny cherry tomato with a green stem, happy face looking right. Square."

## Per pest (add after the base style)

- **ant:** "A small black-brown ant walking, angry eyebrows, facing left. Square."
- **meatball:** "A round brown meatball with sauce spots, angry eyebrows, facing left. Square."
- **fly:** "A tiny fruit fly with big red eyes and transparent wings, mischievous, facing left. Square."
- **pudding:** "A big wobbly pink jelly pudding, translucent and shiny, angry eyebrows, facing left. Square."
