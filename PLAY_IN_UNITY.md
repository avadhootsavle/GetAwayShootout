# Play Getaway Shootout

Open **Assets/Scenes/Game.unity** in **Unity 6000.6.5f1**, then press Play.

Choose **1 Player** or **2 Players**, choose your characters, and press **Play**. The character arrows cycle through **Kavya, Krishna, Avadhoot, Agent, and Bones**. Two-player mode starts as a duel; turn **Bots On** for four racers.

| Action | Player 1 | Player 2 |
| --- | --- | --- |
| Lean left / jump left | Hold / release W | Hold / release I |
| Lean right / jump right | Hold / release E | Hold / release O |
| Use a pickup | R | P |

**Esc** pauses or resumes. Win three rounds to win the match. The final point requires finishing alone.

Menu shortcuts: **1 / 2** choose a mode; **A / D** and **J / L** change the characters; **B** toggles bots; **Space / Enter** continues; **Esc** goes back.

## Editing your game

- Menus: `Assets/Scripts/UI/CharacterSelectUI.cs`
- Buttons and colors: `Assets/Scripts/Presentation/PixelUI.cs`
- Pixel lettering: `Assets/Scripts/Presentation/PixelLabel.cs`
- Menu scenery: `Assets/Scripts/Presentation/ArcadeBackdrop.cs`
- In-game scenery: `Assets/Scripts/Presentation/CitySky.cs`
- Faces: `Assets/Resources/Faces/Player1.png`, `Player2.png`, and `Avadhoot.png`
- Pixel bodies and face proportions: `Assets/Scripts/Presentation/FighterArt.cs` and `Assets/Scripts/Player/PlayerBody.cs`
- Level 2: `Assets/Prefabs/Maps/TemplateMap2.prefab`

The menus are editable Unity UI, not flattened images. Enter Play mode to see them. The level 2 changes are saved in the prefab and can be edited in the Scene view.

## Level 2 changes

The opening wall is lower, an access step breaks up the initial climb, the middle landing is raised, the far roof is lower, and there is more headroom at the exit. Pickups and checkpoint waypoints are aligned to the repaired route. Levels now start at level 1 and cycle in order.

Movement uses filtered ground checks, a short jump-release grace period, consistent launch speed, continuous collision detection, and camera smoothing. Respawning clears old velocity. Pausing cancels held movement and weapon input.

## Level 3 changes

A wide intermediate landing breaks up the central gap. A broad step at the finish divides the final climb into two smaller jumps, with more overhead clearance. Checkpoints follow the repaired route. The player jump strength is unchanged.

## Validation

See `Documentation/VALIDATION.md`. Run the editor validation tools only in a disposable project copy: each batch runner enters Play mode and exits that Unity process.

```sh
/path/to/Unity -batchmode -projectPath /path/to/copy -executeMethod ArcadeValidation.Run -logFile /path/to/smoke.log
/path/to/Unity -batchmode -projectPath /path/to/copy -executeMethod TraversalValidation.Run -logFile /path/to/traversal.log
```

## Maps and sound

The three routes cycle in order: daytime city, sunset docks, and night rooftops. Level 1 is the warm-up; level 2 adds a low rooftop vent, and level 3 has two taller vents plus its stepped route. Safe intermediate landings are retained. Missed jumps below the map trigger respawn.

Pistol, SMG, shotgun, sniper, RPG, and teleport gun each have an original firing effect. Throwables have a short whoosh, and reloads have a mechanical click. Sound is shared across both players in split screen and pauses with the game. Files live in `Assets/Resources/Audio`; regenerate them with `python3 Tools/generate_weapon_audio.py`.

The home menu now has its own rendering camera to prevent Unity's “No cameras rendering” overlay. Stop and restart Play mode after an update.

## Character death recordings

In **two-player mode only**, a human-controlled **Kavya** plays `Kavya.wav` on a lethal weapon hit, and **Avadhoot** plays the updated `Avadhoot.wav` on a lethal weapon hit. The selection determines the voice, not the player number. Krishna, Agent, Bones, computer-controlled opponents, and all single-player deaths are silent. Falling off the map, nonlethal hits, round completion, and respawning do not trigger a recording. Being hit earlier and then falling also stays silent.

The voice sources live with the match manager, so the recording continues while the dead character is hidden. Both players can trigger their own recordings simultaneously. Pause also pauses voice playback; returning to the menu or loading a new scene clears it.

Recordings are in `Assets/Resources/Audio/DeathVoices/`. They were converted from the supplied OPUS files to mono, 48 kHz, 16-bit PCM WAV without changing the content or pitch. Unity plays these directly; FFmpeg is not required to run the game. The original files in Downloads are unchanged.

## Background music and college header

The home screen uses the same outlined pixel lettering for the college name and game title, with enlarged original college/SVKM logos. The controls hint was removed to give the main menu more space.

The supplied background track loops quietly across menus and rounds, without restarting on scene changes. Its normal volume is 12%; it lowers to 3.5% during character death recordings, and pauses with the game. The track is `Assets/Resources/Audio/BackgroundMusic.mp3`; volume settings are in `Assets/Scripts/Presentation/BackgroundMusic.cs`.

## Final victory sound

`Assets/Resources/Audio/Win.mp3` plays once when the final match winner is shown. It does not play for individual stage finishes. Playback does not loop or restart if the same result is shown again, and stops when returning to the menu or starting a rematch. Background music lowers while the win recording plays.
