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

weigths = ["True", "False"]

includes = ["True", "False"]

rooms = [20]

keys = [4]

locks = [4]

enemies = [30]

linear_coefficients = [1.7]


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


def get_parameters(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11):
  # Build the parameters
  parameters = ""
  for i in [p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11]:
    parameters += str(i) + ' '
  return parameters

def get_total():
  return len(times) * \
          len(populations) * \
          len(mutations) * \
          len(competitors) * \
          len(weigths) * \
          len(includes) * \
          len(rooms) * \
          len(keys) * \
          len(locks) * \
          len(enemies) * \
          len(linear_coefficients) * \
          len(executions)

def run(parameters):
  # Generate a random seed
  rs = random.randint(0, np.iinfo(np.int32).max - 1)
  parameters = str(rs) + ' ' + parameters
  # Print parameters
  print('Parameters=[ ' + parameters + ']')
  # Run algoritm for the current set of parameters
  os.system(executable + parameters)

# Variables to control the experiment progress
total = get_total()
i = 1

for p1 in times:
  for p2 in populations:
    for p3 in mutations:
      for p4 in competitors:
        for p5 in weigths:
          for p6 in includes:
            for p7 in rooms:
              for p8 in keys:
                for p9 in locks:
                  for p10 in enemies:
                    for p11 in linear_coefficients:
                      for e in executions:
                        # Run execuble
                        parameters = get_parameters(p1, p2, p3, p4, p5, p6,
                                                    p7, p8, p9, p10, p11)
                        run(parameters)
                        # Print progress
                        print("%.2f" % ((i / total) * 100))
                        i += 1


# --- Plot charts of the experiment results

def plot(parameters):
  os.system('python plot.py ' + parameters)

# Variables to control the plotting progress
total = get_total()
i = 1

# Plot charts for all sets of parameters
print('Plotting')
for p1 in times:
  for p2 in populations:
    for p3 in mutations:
      for p4 in competitors:
        for p5 in weigths:
          for p6 in includes:
            for p7 in rooms:
              for p8 in keys:
                for p9 in locks:
                  for p10 in enemies:
                    for p11 in linear_coefficients:
                      # Plot charts
                      parameters = get_parameters(p1, p2, p3, p4, p5, p6,
                                                  p7, p8, p9, p10, p11)
                      parameters += str(len(executions))
                      plot(parameters)
                      # Print progress
                      print("%.2f" % ((i / total) * 100))
                      i += 1