# Validation results — 14 October 2026

Tested in Unity 6000.6.5f1 using a disposable copy of the project.

## Level 2 keyboard traversal

`TraversalValidation.Run` passed. Both human players crossed the repaired second level after 10 jumps each. The test injects virtual keyboard events through the real PlayerInput bindings (E and O), holds to lean, then releases to jump. It does not teleport the players or directly change their movement velocities or rotations. Both players traversed the opening steps, central gap, far roof, and finish approach.

The test uses a long finish timer so both players can complete the route, and a trajectory planner to time key releases. This establishes that the route is physically traversable with the implemented controls; it is not a substitute for human difficulty testing.

## Runtime smoke tests

`ArcadeValidation.Run` passed with exit code 0:

- Solo versus bots, two-player duel, and two humans plus bots.
- Switching from duel back to solo.
- Duplicate finish protection and score handling.
- Countdown, pause and resume.
- Death and respawn.
- Three controlled round wins, victory screen, and rematch.

The round-win checks invoke finish actions programmatically; the separate traversal test exercises actual keyboard input and physics.

## Visual checks and limitations

Rendered menu, character selection, race, split-screen, and victory previews are saved in `Previews/`. The screenshot helper renders world cameras and composites the actual Unity UI because the batch-mode preview camera otherwise clears the world image.

The editor emitted an unrelated UnityEditor.Search indexing exception, excluded from game runtime error tracking. No game runtime exceptions were reported in the successful smoke run.

Manual playtesting, exhaustive weapon/bot balance, and a standalone player build remain unverified. Open `Assets/Scenes/Game.unity` and press Play to play in Unity.

## Character update

`ArcadeValidation.RunCharacters` passed in Unity 6000.6.5f1 (exit 0). It verifies Kavya/Krishna names, cycling to Avadhoot, loading his face/body/arm sprites, and spawning him in a two-player match. The runtime smoke sequence also completed respawn, victory, and rematch with the new selection retained. Character-selection and split-screen renders were inspected and updated in `Previews/`.

## Level 3 route repair

`TraversalValidation.RunThird` passed (Unity exit 0). Both players crossed the entire third level with real keyboard bindings, 11 jumps each, without teleporting or overriding player velocities. The tested prefab matches `Assets/Prefabs/Maps/TemplateMap3.prefab` in the working project.

The middle gap now has a wide landing at height 1. The finish has an intermediate step at height 0.75, dividing the rise from -0.5 to 2 into two smaller climbs. The overhead border is raised and waypoints/checkpoints follow the safe surfaces. Jump strength is unchanged. The same trajectory-planner and extended finish-timer limitations described above apply.

## Character death recordings — 15 October 2026

`DeathVoiceValidation.Run` passed in Unity 6000.6.5f1, exit code 0. The test triggers real Health.Die events and verifies Kavya/Avadhoot recording selection, swapped player slots, one playback per death despite duplicate death requests, simultaneous deaths, bot exclusion, other-character exclusion, normal respawning, pause/resume, silent round completion, audio host cleanup on returning to the menu, and silent single-player deaths. Imported PCM recordings were decoded and checked for nonzero audio samples in Unity.

The recordings preload and decompress on load; no runtime OPUS codec or FFmpeg dependency is needed. Both audio sources are hosted by the match manager rather than the disabled player objects. The existing UnityEditor.Search indexing exception was excluded from gameplay error tracking. No game runtime exceptions occurred in the successful test. Speaker/headphone listening was not verified by the automated test.

## Lethal-hit-only recording update — 15 October 2026

Replaced Avadhoot's WAV from the newly supplied `Downloads/Avadhoot.opus`. The death voice gate now checks the fatal damage cause, requiring positive weapon damage. Falling and prior nonfatal hits cannot trigger it. Respawn clears the cause flag.

`DeathVoiceValidation.Run` passed with exit code 0. Added checks for nonlethal-hit silence and the actual fall-trigger handler for both characters, including a fall after a prior hit. The existing two-player routing, duplicate-death protection, other-character/bot/single-player exclusions, respawn, pause, menu cleanup and round-end checks also passed. No game runtime errors were reported; the previously documented editor search-index exception is unrelated.

## College header and background music update — 15 October 2026

Rendered and inspected the revised home screen in Unity: pixel college lettering, larger logos, lower title/buttons, removed controls hint. Updated `Previews/lobby.png`.

Music validation passed with exit code 0: the supplied track loads and plays, loops at low volume, has one persistent instance through menu reloads, and uses exactly one active listener in menus and gameplay. Existing lethal-hit-only voice, pause/resume, and character/mode exclusion checks also passed. The unrelated UnityEditor.Search indexing exception remains excluded. Audio volume was checked programmatically; speaker listening was not performed.

## Final match victory recording — 15 October 2026

`WinAudioValidation.Run` passed in Unity 6000.6.5f1 (exit 0). The test completes three scored rounds, confirms intermediate stages do not create the victory audio source, verifies the final victory starts the supplied Win clip with looping disabled, checks repeated ShowVictory calls do not restart it, waits for natural completion, and confirms rematch is silent. Background music ducks for the recording duration. No game runtime exceptions occurred; the known editor search indexing exception was excluded.
