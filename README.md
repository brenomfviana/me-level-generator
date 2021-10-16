# MAP-Elites Level Generator

## Description

This program is a Level Generator that evolves levels via a MAP-Elites Genetic
Algorithm.
This algorithm is an extension of the level generator introduced by [Pereira
et al.][1].
The output of this program is a set of levels written in JSON files.

## Program arguments

This level generator receives seven arguments:
- a random seed;
- the maximum time;
- the initial population size;
- the mutation chance;
- the number of tournament competitors;
- the number of rooms;
- the number of keys;
- the number of locks;
- the number of enemies, and;
- the linear coefficient.

## Author

- Breno M. F. Viana

## References

[1]: Pereira, Leonardo Tortoro, et al. "Procedural generation of dungeons' maps
and locked-door missions through an evolutionary algorithm validated with
players." Expert Systems with Applications 180 (2021): 115009.