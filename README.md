# SAAT - Stardew Valley Audio API &amp; Toolkit

Proposed Audio API for SMAPI, without any utilization of HarmonyLib.

## Current Functionality / Work in Progress:
- Allow brand new music and sound effects to be added into Stardew Valley
- Audio tracks that are defined as `music` should appear on the Jukebox, once the player character has heard them.

## To Do List:
- Allow modification to existing music and sound effect, including their RPC Variables
- Streaming functionality, where audio can stream in portion by portion rather than retaining everything in memory (Specifically targetting large music tracks) 
- Packed Audio Format that allows for loading, unloading, and reloading.
- Tool to allow content creators to pack audio content into said audio format above.

