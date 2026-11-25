# Decisions log

What I asked the AI agent, what it got wrong, what I changed and why.

## Week 12 · Sound

2026-11-26  Asked the agent to generate all SFX and two music loops with a Python script, so every sound is my own and the README license is simple. Loops are rendered with the tail wrapped to the start, so the seam has no click.
2026-11-26  Agent's first AudioManager set a random pitch on ONE shared AudioSource before PlayOneShot. That also bends a sound that is still ringing. Changed to a pool of 6 SFX sources.
2026-11-26  Crystals have trigger colliders, and the enemy sight raycast stopped at them, so enemies were "blind" behind a pickup. Added `if (h.collider.isTrigger) continue;` in EnemyAI.
2026-11-26  Death reloads the scene at once, so a death sound on the player would be cut. All sounds play from the AudioManager (DontDestroyOnLoad); "lose" plays 0.35 s after "death".
2026-11-26  Old save.json has no volume fields. Switched Load to FromJsonOverwrite on a new SaveData, so defaults (music 0.8) survive. Reset progress now keeps the volume settings.
2026-11-26  Lecture crossfade is linear: quieter in the middle. Used an equal-power curve (cos/sin).
2026-11-26  Agent compared scenes by scene.handle as an int; Unity 6.6 made that conversion an error (CS0619). Compare the Scene structs directly instead.
