No Loose Ends

Description:
No Loose Ends is a low-poly, high-octane FPS and hack-and-slash hybrid. Drawing inspiration from the innovative gameplay of SUPERHOT, the game blends time dilation, parry-based bullet deflection, and seamless transitions between gunplay and melee combat into a fast-paced experience.

This project is the culmination of our coursework in Game Programming 2.
No Loose Ends marks our first "real" 3D game project. Although we had prior experience in 3D modeling from our 3D Modeling and Still Images course, transitioning to game-ready assets was a significant challenge. Many early models featured incorrect topologies and excessive polygons, leading to performance issues and unnatural deformations.
Through trial and error, we learned to optimize our models for Unity, using FBX imports and reassigning materials. Ultimately, we chose a low-poly art style to accommodate our beginner-level modeling skills, focusing instead on delivering satisfying gameplay.

Core Gameplay Features
Time Dilation: A limited-duration mechanic allowing players to slow time for tactical dodging and parrying.
Parry-Based Combat: Players can deflect bullets with precise timing, accompanied by hitstop effects and satisfying audio cues.
Physics-Based Projectiles: Instead of hitscan bullets, we implemented physics-based projectiles to support parry mechanics. This required extensive troubleshooting to address issues with fast-moving objects and small hitboxes.
Weapon System: Guns cannot be reloaded. Players must pick up weapons dropped by enemies or rely on their melee skills. Weapons can be thrown to knock out enemies.
Bullet Trails: Line renderer trails allow players to track bullet paths in the 3D space.
Enemy AI: We implemented Unity’s NavMesh for pathfinding and designed enemy behaviors based on weapon type (sniper, shotgun, pistol, SMG), with varying detection ranges, accuracy, and attack patterns.

Challenges and Lessons Learned
Modeling for Games: Adapting rendering techniques to game-ready assets taught us the importance of topology and optimization.
Physics Troubleshooting: Small projectiles and fast movement in Unity often caused collisions to fail, requiring creative solutions and experimentation.
Pathfinding and AI: Implementing NavMesh and designing diverse enemy behaviors was our first real encounter with AI programming, which took significant amount of time.
Scope Management: While we had ambitious plans for additional features, our focus on perfecting the core mechanics resulted in several planned ideas being shelved.

Team Members
Rafael Mallari: UI and 3D Environment Artist
Sean Ramoya: Programmer and Animator
Darren Tabago: 3D Weapon Artist and Video Editor
