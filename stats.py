import os
from pathlib import Path
import json
import numpy as np
from pandas import DataFrame


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

shape = (len(leniency), len(exploration))
mean_map = np.zeros(shape)
std_map = np.zeros(shape)

maps = []

files = []
filenames = []
for i in range(10):
  path = 'results' + os.path.sep + '60-20-5-3' + os.path.sep + str(i)
  print(i)
  for p in Path(path).glob('*.json'):
    with p.open() as f:
      if p.name == 'data.json':
        continue
      files.append(f.read())
      filenames.append(p.name)

  # Convert the read files into a map of fitness
  # and add them to the list of maps
  maps.append(to_map(files[1:], filenames[1:], 'fitness'))
  # maps.append(to_map(files[1:], filenames[1:], 'fGoal'))
  # maps.append(to_map(files[1:], filenames[1:], 'fSTD'))
  # maps.append(to_map(files[1:], filenames[1:], 'fEnemySparsity'))


for map in maps:
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if not np.isnan(map[l, e]):
        mean_map[l, e] += map[l, e]

mean_map = mean_map / 10

# Uncomment to debug the mean map
# for l in range(len(leniency)):
#   for e in range(len(exploration)):
#     print('%3.2f' % mean_map[l, e], end=' ')
#   print()

for map in maps:
  for l in range(len(leniency)):
    for e in range(len(exploration)):
      if not np.isnan(map[l, e]):
        std_map[l, e] += pow(map[l, e] - mean_map[l, e], 2)
std_map = std_map / 10
std_map = np.sqrt(std_map)

# Uncomment to debug the std map
# for l in range(len(leniency)):
#   for e in range(len(exploration)):
#     print('%3.2f' % std_map[l, e], end=' ')
#   print()

# Merge the mean and std maps
fmap = [ [ '' for e in range(len(exploration)) ] for l in range(len(leniency)) ]
for l in range(len(leniency)):
  for e in range(len(exploration)):
    fmap[l][e] = '{:.2f}+-{:.2f}'.format(mean_map[l, e], std_map[l, e])

# Uncomment to debug the merged map
# for l in range(len(leniency)):
#   for e in range(len(exploration)):
#     print(fmap[l][e], end=' ')
#   print()

# Print the resulting table
df = DataFrame(fmap, index=leniency, columns=exploration)
print(df)

# Uncomment to write a CSV file with the resulting table
# filename = 'std_atual.csv'
# df.to_csv(filename)