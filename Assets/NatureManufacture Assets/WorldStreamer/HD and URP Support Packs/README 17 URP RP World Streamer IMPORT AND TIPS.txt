
BEFORE YOU START:
- you need Unity 6+
- you need URP SRP pipline 17

Step 1 You can improve FPS amount by 30% if you change rendering path from forward to deferred at  PC Renderer setting
BUT!!  at initial unity 6 version we notice water doesnt show up at deferred and screen space ambient occlusion turned on at the same time. 
Looks like near/far clip planes are bugged at that engine version and it send wrong depth data.

Step 2 Setup Shadows and other render setups. Find File "PC_RPAsset" 
        - Change shadow distance to 150 or higer
	- Turn on "Opaque Texture" this will fix water translucency and distortion if its turned off
	- Turn on "Depth Texture" this will fix water visibility at playmode if its turned off
	- Optionaly use 1k or 2k shadow resolution. We used 2k.
	- Turn on HDR if its turned off

Step 3 Go to project settings: 
    - Player and set:  Color Space to Linear
    - Quality settings: Go to quality settings and: 
	     * use ultra level 
	     * turn turn off vsync
		 * lod bias should be around 1.5-2 and 1 for low end devices.
                        

Step 4 Find "Game Streamer Scene" and open it.

Step 5 - Add scenes from world streamer into build settings

Step 6 - HIT PLAY!:)


