import os
import sys
from pathlib import Path
import copy
import json
import numpy as np
from pandas.core import base
import seaborn as sb
from pandas import DataFrame
import matplotlib.pyplot as plt


# List of indexes
exploration = ['0.5-0.6', '0.6-0.7', '0.7-0.8', '0.8-0.9', '0.9-1.0']
# List of columns
leniency = ['0.5-0.6', '0.4-0.5', '0.3-0.4', '0.2-0.3', '0.1-0.2']


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
    x = int(name[1]) # Leniency
    y = int(name[0]) # Exploration
    map[x, y] = json.loads(files[i])[attribute]
    # Uncomment to check if the levels are being placed in the right position
    # print(json.loads(files[i])['exploration'])
    # print(exploration[y])
    # print(json.loads(files[i])['leniency'])
    # print(leniency[x])
    # print()
  return map


# Plot and write the heatmap
def plot_heatmap(map, folder, filename, max, title):
  df = DataFrame(map, index=leniency, columns=exploration)
  color = sb.color_palette('viridis_r', as_cmap=True)
  ax = sb.heatmap(df, vmin=0, vmax=max, annot=True, cmap=color)
  ax.invert_yaxis()
  figname = folder + os.path.sep + filename + '.png'
  # plt.subplots_adjust(bottom=0.3)
  plt.title(title)
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
basename = sys.argv[1] + '-'   # maximum time
basename += sys.argv[2] + '-'  # initial population size
basename += sys.argv[3] + '-'  # mutation chance
basename += sys.argv[4] + '-'  # number of tournament competitors
basename += sys.argv[5] + '-'  # weight or not the enemy sparsity
basename += sys.argv[6] + '-'  # include or not empty rooms in enemy STD
basename += sys.argv[7] + '-'  # number of rooms
basename += sys.argv[8] + '-'  # number of keys
basename += sys.argv[9] + '-'  # number of locks
basename += sys.argv[10] + '-' # number of enemies
basename += sys.argv[11]       # linear coefficient

# Get the number of executions
executions = int(sys.argv[12])

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

  type = ''

  # Plot the 'fitness' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fitness')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if np.isnan(map_aux[l, e]):
        map_aux[l, e] = 0
  title = type + 'fitness'
  plot_heatmap(map, target_ex, 'fitness', np.max(map_aux), title)

  # Plot the 'generation' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'generation')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'generation'
  plot_heatmap(map, target_ex, 'generation', np.max(map_aux), title)

  # Plot the 'fitnessLeo' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fGoal')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Original do Leo'
  plot_heatmap(map, target_ex, 'fitnessLeo', np.max(map_aux), title)

  # Plot the 'fitnessRooms' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fRooms')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Rooms'
  plot_heatmap(map, target_ex, 'fitnessRooms', np.max(map_aux), title)

  # Plot the 'fitnessKeys' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fKeys')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Keys'
  plot_heatmap(map, target_ex, 'fitnessKeys', np.max(map_aux), title)

  # Plot the 'fitnessLocks' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fLocks')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Locks'
  plot_heatmap(map, target_ex, 'fitnessLocks', np.max(map_aux), title)

  # Plot the 'fitnessLinearCoefficient' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fLinearCoefficient')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Linear Coefficient'
  plot_heatmap(map, target_ex, 'fitnessLinearCoefficient', np.max(map_aux), title)

  # Plot the 'fitnessEnemySparsity' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fEnemySparsity')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Enemy Sparsity'
  plot_heatmap(map, target_ex, 'fitnessEnemySparsity', np.max(map_aux), title)

  # Plot the 'fitnessSTD' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fSTD')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Enemy Sparsity'
  plot_heatmap(map, target_ex, 'fitnessSTD', np.max(map_aux), title)

  # Plot the 'fitnessNeededRooms' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fNeededRooms')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Needed Rooms'
  plot_heatmap(map, target_ex, 'fitnessNeededRooms', np.max(map_aux), title)

  # Plot the 'fitnessNeededLocks' heatmap of the MAP-Elites
  map = to_map(files[1:], filenames[1:], 'fNeededLocks')
  map_aux = copy.deepcopy(map)
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if (np.isnan(map_aux[l, e])):
        map_aux[l, e] = 0
  title = type + 'Needed Locks'
  plot_heatmap(map, target_ex, 'fitnessNeededLocks', np.max(map_aux), title)

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