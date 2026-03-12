# Enemy_ai_prototype
**Enemy AI behavior logic**



* Enemy do patrol all around map.
* Enemy's hearing range is grater than vision range.
* When enemy hear gun shoot or player footstep it move to the location the sound was generated and look around for small period of time and go back to patrol mode if player is moved away from enemy's hearing range.
* If enemy sees player it chase the player and do range attack and if player is to close it do melee attack.
* Enemy also have idle, run and melee attack animation.





**Key design decision**



* Unity's nav mesh is used for pathfinding and navigation for enemy ai movement mechanics.
* 
* AI behavior is structured around logical states such as Patrol, Investigate, Chase and Attack.
* 
* Instead of detecting the player instantly i used raycast so ai can only use angled perception to detect player in a particular distance.
* 
* A noise system is implemented so that enemies can react to sounds.





