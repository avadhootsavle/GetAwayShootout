# Player face assets

The user identified the first existing photo as Kavya, the person wearing glasses as Krishna, and supplied a third photo for Avadhoot. Original filenames are retained for compatibility.

Sources (preserved):
- `Assets/Sprites/avadhoot_face_cutout.png`
- `Assets/Sprites/friend_face_cutout.png`

Transparent head assets used in menus, HUD, and player bodies:
- Kavya: `Assets/Resources/Faces/Player1.png`
- Krishna: `Assets/Resources/Faces/Player2.png`
- Avadhoot: `Assets/Resources/Faces/Avadhoot.png`

Created with the built-in image-generation tool, using each original photo as the edit reference. Menus, lettering, scenery, and pixel bodies are editable Unity code, not generated menu images.

Prompt used for each photo:

> Use case: background-extraction. Edit target: the attached photograph. Create a clean photographic HEAD-ONLY cutout for a small 2D game character. Preserve this exact person's face, expression, hair, skin, and glasses if present; do not beautify, redraw, stylize, change age, or invent facial details. Remove the entire background, clothing, shoulders, and neck below the chin. Keep the full hair silhouette, ears and chin, with no oval mask or border. Center the entire isolated head in a tight portrait canvas with about 5 percent transparent padding on each side. Genuine alpha transparency, no shadows, no text, no extra objects. This is careful background removal, not a new portrait.

## Added Avadhoot

Source: user attachment `codex-clipboard-c6b4b0d5-4c53-41b7-b5fa-95e45f6cb9ba.jpg`. Prepared using the built-in image-generation tool, with alpha transparency preserved.

Prompt:

> Use case: background-extraction. Edit target: attached photograph of Avadhoot. Create a clean photographic HEAD-ONLY cutout for a small 2D game character. Preserve this exact person's face, expression, hair, skin, proportions and age; do not beautify, redraw or stylize. Remove the white background, clothing, shoulders and neck below the chin. Keep full hair silhouette, ears and chin, with no oval mask or border. Center the isolated head in a tight portrait canvas with about 5 percent transparent padding. Genuine alpha transparency. No shadows, text or extra objects. Careful background removal, not a new portrait.
