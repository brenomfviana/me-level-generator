using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace LevelGenerator
{
    /// This class holds the fitness operator.
    public class Mutation
    {
        private enum MutationOp
        {
            insertChild = 0,
            removeLeaf = 1
        };

        public static void Apply(
            ref Dungeon individual,
            ref Random rand
        ) {
            try
            {
                //Mutate keys, adding or removing a pair
                bool willMutate = rand.Next(101) <= Constants.MUTATION_RATE;
                MutationOp op;
                if (willMutate)
                {
                    op = rand.Next(101) <= Constants.MUTATION0_RATE ? MutationOp.insertChild : MutationOp.removeLeaf;
                    switch (op)
                    {
                        case MutationOp.insertChild:
                            individual.AddLockAndKey(ref rand);
                            break;
                        case MutationOp.removeLeaf:
                            individual.RemoveLockAndKey(ref rand);
                            break;  
                    }
                    individual.FixRoomList();
                }
                //foreach(Room room in individual.RoomList){
                /*for (int i = 0; i < individual.RoomList.Count; ++i)
                {
                    Room room = individual.RoomList[i];

                    willMutate = rand.Next(101) <= Constants.MUTATION_RATE;
                    if (willMutate)
                    {
                        op = rand.Next(101) <= Constants.MUTATION0_RATE ? MutationOp.insertChild : MutationOp.removeLeaf;
                        switch (op)
                        {
                            case MutationOp.insertChild:
                                if (room.LeftChild != null && room.BottomChild != null && room.RightChild != null) continue;
                                bool found = false;
                                Util.Direction dir;
                                do
                                {
                                    dir = (Util.Direction)rand.Next(3);
                                    if (dir == Util.Direction.left && room.LeftChild == null) found = true;
                                    else if (dir == Util.Direction.down && room.BottomChild == null) found = true;
                                    else if (dir == Util.Direction.right && room.RightChild == null) found = true;
                                } while (!found);

                                if (room.ValidateChild(dir, individual.roomGrid))
                                {
                                    //System.Console.WriteLine("Insertion!");
                                    Room child = new Room();
                                    room.InsertChild(dir, ref child, ref individual.roomGrid);
                                    child.ParentDirection = dir;
                                    individual.RoomList.Add(child);
                                    individual.roomGrid[child.X, child.Y] = child;
                                }
                                break;
                            case MutationOp.removeLeaf:
                                if (room.LeftChild == null && room.RightChild == null && room.BottomChild == null && room.Type == Type.normal) // It's a leaf node, remove it from the dungeon
                                {
                                    dir = room.ParentDirection;
                                    switch (dir)
                                    {
                                        case Util.Direction.right:
                                            room.Parent.RightChild = null;
                                            break;
                                        case Util.Direction.left:
                                            room.Parent.LeftChild = null;
                                            break;
                                        case Util.Direction.down:
                                            room.Parent.BottomChild = null;
                                            break;
                                    }
                                    individual.RemoveFromGrid(room);
                                    individual.RoomList.Remove(room);
                                }
                                break;
                        }
                    }
                }*/
            
            }
            catch (System.Exception e)
            {
                throw e;
            }



            //Mutation(ref individual.LeftChild);
            //Mutation(ref individual.BottomChild);
            //Mutation(ref individual.RightChild);
        }
    }
}