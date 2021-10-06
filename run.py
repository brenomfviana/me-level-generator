#!/bin/env python
import os
import platform
import random
import numpy as np

# --- Initialization

# Initialize the random seed
random.seed(0)

# Define the number of executions of each set of parameters
executions = range(10)

# Numbers of generations
generations = [100, 200, 300]

# Initial population sizes
populations = [15, 20, 25]

# Mutation rates
mutations = [5]

# Competitors
competitors = [3]


# --- Perform experiment

# Choose the executable
if platform.system() == 'Linux':
  executable = './bin/Debug/net5.0/publish/LevelGenerator '
elif platform.system() == 'Windows':
  executable = 'bin\\Debug\\net5.0\\publish\\LevelGenerator.exe '
else:
  print('This script is not able to run in this OS.')
  exit()


os.system('dotnet publish')


def run(ge, po, mu, co):
  # Generate a random seed
  rs = random.randint(0, np.iinfo(np.int32).max - 1)
  # Build the parameters
  parameters = ""
  for i in [rs, ge, po, mu, co]:
    parameters += str(i) + ' '
  parameters += '20 4 4 30 1.7'
  # Print parameters
  print('Parameters=[', parameters, ']')
  # Run algoritm for the current set of parameters
  os.system(executable + parameters)

# Variables to control the experiment progress
total = len(generations) * \
  len(populations) * \
  len(mutations) * \
  len(competitors) * \
  len(executions)
i = 1

for ge in generations:
  for po in populations:
    for mu in mutations:
      for co in competitors:
        for e in executions:
          # Run execuble
          run(ge, po, mu, co)
          # Print progress
          print("%.2f" % ((i / total) * 100))
          i += 1
