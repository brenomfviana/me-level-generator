#!/bin/env python
import os
import sys
import platform
import random
import numpy as np



# --- Initialization

# Initialize the random seed
seed = random.randrange(sys.maxsize)
random.seed(seed)
print("Seed:", seed)
# random.seed(0)

# Define the number of executions of each set of parameters
executions = range(1)



# --- Define the set of parameters

# Maximum times
times = [60] # [60, 300, 600, 3600]

# Initial population sizes
populations = [20]

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


# Compile project
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
total = len(times) * \
  len(populations) * \
  len(mutations) * \
  len(competitors) * \
  len(executions)
i = 1

for ge in times:
  for po in populations:
    for mu in mutations:
      for co in competitors:
        for e in executions:
          # Run execuble
          run(ge, po, mu, co)
          # Print progress
          print("%.2f" % ((i / total) * 100))
          i += 1


def plot(ge, po, mu, co, ex):
  # Plot the charts for the current set of parameters
  parameters = ''
  for i in [ge, po, mu, co, ex]:
    parameters += str(i) + ' '
  os.system('python plot.py ' + parameters)

# Variables to control the experiment progress
total = len(times) * \
  len(populations) * \
  len(mutations) * \
  len(competitors) * \
  len(executions)
i = 1

# Plot all the results
print('Plotting')
for ge in times:
  for po in populations:
    for mu in mutations:
        for co in competitors:
          # Plot charts
          plot(ge, po, mu, co, len(executions))
          # Print progress
          print("%.2f" % ((i / total) * 100))
          i += 1