import copy
import os
import sys
from pathlib import Path
import json
import numpy as np
from pandas.core import base
import seaborn as sb
from pandas import DataFrame
import matplotlib.pyplot as plt


# List of indexes
exploration = ['0.5-0.6', '0.6-0.7', '0.7-0.8', '0.8-0.9', '0.9-1.0']
# List of columns
leniency = ['0.4-0.5', '0.3-0.4', '0.2-0.3', '0.1-0.2', '0.0-0.1']


# Convert the list of files to a map
def to_map(files, filenames, attribute):
  shape = (len(leniency), len(exploration))
  map = np.zeros(shape)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      map[l, e] = None
  for i in range(len(files)):
    name = filenames[i].replace('level-', '')
    name = name.replace('.json', '')
    name = name.split('-')
    x = int(name[1])
    y = int(name[0])
    map[x, y] = json.loads(files[i])[attribute]
  return map


# Plot and write the heatmap
def plot_heatmap(map, folder, filename, max):
  df = DataFrame(map, index=leniency, columns=exploration)
  color = sb.color_palette('viridis_r', as_cmap=True)
  ax = sb.heatmap(df, vmin=0, vmax=max, annot=True, cmap=color)
  ax.invert_yaxis()
  figname = folder + os.path.sep + filename + '.png'
  # plt.subplots_adjust(bottom=0.3)
  plt.savefig(figname)
  plt.close()


# The folder that stores the results
RESULTS_FOLDER = 'results'
results = RESULTS_FOLDER + os.path.sep

# Create the folder to store all charts
CHART_FOLDER = 'charts'
if not os.path.isdir(CHART_FOLDER):
  os.mkdir(CHART_FOLDER)

# Calculate the basename
basename = sys.argv[1] + '-'  # Number of generations
basename += sys.argv[2] + '-' # Initial population size
basename += sys.argv[3] + '-' # Mutation chance
basename += sys.argv[4]       # Number of competitors

# Get the number of executions
executions = int(sys.argv[5])

# Calculate the mean duration
duration = []


# Create the folder to store the charts for the entered parameters
target = CHART_FOLDER + os.path.sep + basename
if not os.path.isdir(target):
  os.mkdir(target)

# Read all the JSON files for the entered parameters
for ex in range(executions):
  # Read all the json files generated in execution `ex`
  path = results + basename + os.path.sep + str(ex)
  files = []
  filenames = []
  for p in Path(path).glob('*.json'):
    with p.open() as f:
      files.append(f.read())
      filenames.append(p.name)

  # Create the folder for the execution `ex`
  target_ex = target + os.path.sep + str(ex)
  if not os.path.isdir(target_ex):
    os.mkdir(target_ex)

  # Plot the fitness heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fitness')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  plot_heatmap(map, target_ex, 'fitness', np.max(map_aux))

  # Plot the generation heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'generation')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  plot_heatmap(map, target_ex, 'generation', np.max(map_aux))

  # Calculate the mean duration
  obj = json.loads(files[0])
  duration.append(obj['duration'])

dict = {}
dict['mean'] = np.mean(duration)
dict['std'] = np.std(duration)

jsonString = json.dumps(dict)
jsonFile = open(target + os.path.sep + 'duration.json', 'w')
jsonFile.write(jsonString)
jsonFile.close()

