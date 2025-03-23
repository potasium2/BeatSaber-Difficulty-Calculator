# ReBeat Star Rating Tool

## What does this star rating tool evaluate?
- Stamina required to accurately keep up with a maps speed.
- Dexterity required to accurately hit certain angles and angle changes.
- Distance between notes as well as amount of circling required to accurately hit certain angles.

## How is Performance calculated?
Performance is calculated based on the evaluation of Stamina and Angles within a given map.
It also takes into account the following:
- The NJS of the given Beatmap
- The Modifiers active for a given play (Ghost Notes, Pro Mode, Faster Song)
- The length of a given Beatmap (Think Osu! length bonus)
- The Accuracy and Swing of a given score

## How are misses factored into Performance?
Misses are factored in based on the difficulty peak of where the miss occurs.
If a miss occurs at peak difficulty, meaning the hardest given section of a map, it will reduce performance value by 10%~20% for the given skill.

Misses are slightly weighted as well in order to reduce high misscount penalties. The base fall off for is a 94% miss penalty reduction per miss.
This means that the highest strain miss is weighted at 100%, the second highest at 94%, the third at 88.3%, etc.

## Dependencies
- Joshaparity

## How to use?
For now there is a simple console log that asks for a !BSR code and by inputting one it will download the map.
The strains for the beatmap will be calculated and saved, these can be viewed by running the included python script in draw strains/strain graph.py

I plan on implementing a GUI version of the calculator similar to that of osu!tools which will also you to recalculate scores on ScoreSaber and/or Beatleader.
Expect to see this implementation whenever I have this calculator in a state I'm actually happy with.

## Future Plans
- Massive Refactor so code is more maintainable
- Per map curve generation based on average strain difficulty
- Potential implementation to allow the tool to work with ReBeat
