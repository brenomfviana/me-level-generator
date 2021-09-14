using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the crossover operator.
    public class Crossover
    {
        /// Choose a random room to switch between the parents and arrange
        /// every aspect of the room needed after the change. Including the
        /// grid, and also the exceptions where the new nodes overlap the old
        /// ones.
        public static Individual[] Apply(
            Individual _parent1,
            Individual _parent2,
            ref Random _rand
        ) {
            // Initialize the two new individuals
            Individual[] individuals = new Individual[2];

            Dungeon ind1, ind2;
            //The root of the branch that will be traded
            Room roomCut1, roomCut2;
            //List of rooms that were the root of the branch and led to an impossible crossover (Tabu List)
            List<Room> failedRooms;
            //List of special rooms in the branche to be traded of each parent
            List<int> specialRooms1 = new List<int>(), specialRooms2 = new List<int>();
            //List of special rooms in the traded brach after the crossover
            List<int> newSpecial1 = new List<int>(), newSpecial2 = new List<int>();
            //Total number of rooms in each branch that will be traded
            int nRooms1 = 0, nRooms2 = 0;
            //Answers if the trade is possible or not
            bool isImpossible = false;

            do
            {
                ind1 = _parent1.dungeon.Clone();
                ind2 = _parent2.dungeon.Clone();
                //Get a random node from the parent, find the number of keys, locks and rooms and add it to the list of future failed rooms
                roomCut1 = ind1.RoomList[_rand.Next(1, ind1.RoomList.Count)];
                FindNKLR(ref nRooms1, ref specialRooms1, roomCut1);
                failedRooms = new List<Room>();

                //While the number of Keys and Locks from a branch is greater than the number of rooms of the other branch,
                //Redraw the cut point (root of the branch).
                //System.Console.WriteLine("STARTFINDINGCUT");
                do
                {
                    do
                    {
                        roomCut2 = ind2.RoomList[_rand.Next(1, ind2.RoomList.Count)];
                    } while (failedRooms.Contains(roomCut2));
                    failedRooms.Add(roomCut2);
                    if (failedRooms.Count == ind2.RoomList.Count - 1)
                        isImpossible = true;
                    FindNKLR(ref nRooms2, ref specialRooms2, roomCut2);
                } while ((specialRooms2.Count > nRooms1 || specialRooms1.Count > nRooms2) && !isImpossible);

                //Changes the children of the parent's and neighbor's nodes to the node of the other branch if it is not an impossible trade
                if (!isImpossible)
                {
                    ChangeChildren(ref roomCut1, ref roomCut2);
                    ChangeChildren(ref roomCut2, ref roomCut1);

                    //Change the parent of each node
                    Room auxRoom;
                    //Changes the parents of the chosen nodes
                    auxRoom = roomCut1.Parent;
                    roomCut1.Parent = roomCut2.Parent;
                    roomCut2.Parent = auxRoom;

                    //Remove the node and their children from the grid of the old dungeon
                    ind1.RemoveFromGrid(roomCut1);
                    ind2.RemoveFromGrid(roomCut2);

                    //Update the position, parent's direction and rotation of both nodes that are switched
                    int x = roomCut1.X;
                    int y = roomCut1.Y;
                    Util.Direction dir = roomCut1.ParentDirection;
                    int rotation = roomCut1.Rotation;
                    roomCut1.X = roomCut2.X;
                    roomCut1.Y = roomCut2.Y;
                    roomCut1.ParentDirection = roomCut2.ParentDirection;
                    roomCut1.Rotation = roomCut2.Rotation;
                    roomCut2.X = x;
                    roomCut2.Y = y;
                    roomCut2.ParentDirection = dir;
                    roomCut2.Rotation = rotation;

                    //Updates the grid with all the new nodes. If any conflicts arise, handle them as in the child creation.
                    //That is, any overlap will make the node and its children cease to exist
                    ind1.RefreshGrid(ref roomCut2);
                    ind2.RefreshGrid(ref roomCut1);

                    //Find the number of keys, locks and rooms in the newly switched branches
                    newSpecial1 = new List<int>();
                    newSpecial2 = new List<int>();
                    FindNKLR(ref nRooms2, ref newSpecial2, roomCut2);
                    FindNKLR(ref nRooms1, ref newSpecial1, roomCut1);
                }
                //If in the new branches there are special rooms missing or the number of special rooms is greater then the number of total rooms, retry
            } while ((newSpecial1.Count != specialRooms1.Count || newSpecial2.Count != specialRooms2.Count || specialRooms1.Count > nRooms2 || specialRooms2.Count > nRooms1) && !isImpossible);

            //If the crossover can be done, do it. If not, don't.
            //System.Console.WriteLine("Fixing");
            if (!isImpossible)
            {
                // Replace locks and keys in the new branches
                roomCut2.FixBranch(specialRooms1, ref _rand);
                roomCut1.FixBranch(specialRooms2, ref _rand);
                // Fix the list of rooms
                ind1.FixRoomList();
                ind2.FixRoomList();
            }

            individuals[0] = new Individual(ind1);
            individuals[1] = new Individual(ind1);

            return individuals;
        }

        /// Search the tree of rooms to find the number of special rooms. The
        /// key room is saved in the list with its positive ID, while the
        /// locked room with its negative value of the ID.
        private static void FindNKLR(
            ref int _nRooms,
            ref List<int> _specialRooms,
            Room _root
        ) {
            // Initialize the list of special rooms
            _specialRooms = new List<int>();
            _nRooms = 0;
            // Search for the special rooms in the dungeon
            Queue<Room> toVisit = new Queue<Room>();
            toVisit.Enqueue(_root);
            while (toVisit.Count > 0)
            {
                _nRooms++;
                Room actualRoom = toVisit.Dequeue() as Room;
                RoomType type;
                type = actualRoom.RoomType;
                if (type == RoomType.key)
                {
                    _specialRooms.Add(actualRoom.KeyToOpen);
                }
                else if (type == RoomType.locked)
                {
                    _specialRooms.Add(-actualRoom.KeyToOpen);
                }
                Room[] rooms = new Room[] {
                    actualRoom.LeftChild,
                    actualRoom.BottomChild,
                    actualRoom.RightChild
                };
                foreach (Room room in rooms)
                {
                    if (room != null)
                    {
                        toVisit.Enqueue(room);
                    }
                }
            }
        }

        /// Change the selected rooms between the parent dungeons. To do so,
        /// change the parent who is their child to the corresponding node.
        private static void ChangeChildren(
            ref Room _room1,
            ref Room _room2
        ) {
            // No room involved in this operation can be null
            Debug.Assert(
                _room1 != null && _room2 != null && _room1.Parent != null,
                "There is something wrong with the dungeon representation."
            );
            // Set `_room2` as a child of the parent of `_room1`
            switch (_room1.ParentDirection)
            {
                case Util.Direction.Right:
                    _room1.Parent.RightChild = _room2;
                    break;
                case Util.Direction.Down:
                    _room1.Parent.BottomChild = _room2;
                    break;
                case Util.Direction.Left:
                    _room1.Parent.LeftChild = _room2;
                    break;
            }
        }
    }
}