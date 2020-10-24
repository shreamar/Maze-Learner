using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Maze_Learner
{
    /// <summary>
    /// Amar Shrestha - 10/19/2020
    /// Maze data structure
    /// </summary>
    public class Maze
    {
        #region Private Fields
        private ushort _Rows;
        private ushort _Columns;
        private bool[,,] _Matrix;
        private List<uint>[] _Paths;
        private List<uint> _DestroyableWalls;
        private bool _PathExists;
        #endregion

        #region Constructor
        public Maze(ushort rows, ushort cols)
        {
            Reset_Maze(rows, cols);
        }
        #endregion

        #region Public Properties
        public ushort Rows
        {
            get { return _Rows; }
        }

        public ushort Columns
        {
            get { return _Columns; }
        }

        /// <summary>
        /// [Row position, column position, 4 walls of the cell]
        /// False: Wall not knocked down
        /// True: Wall is knocked down
        /// </summary>
        public bool[,,] Matrix
        {
            get { return _Matrix; }
        }

        public List<uint>[] Paths
        {
            get { return _Paths; }
        }

        /// <summary>
        /// Number of walls that can be knocked down
        /// Excludes the outer wall
        /// </summary>
        public uint TotalDestroyableWalls
        {
            get
            {
                return ((uint)((Rows - 1) * Columns + (Columns - 1) * Rows));
            }
        }

        /// <summary>
        /// Total number of cells in the maze
        /// </summary>
        public uint TotalCells
        {
            get
            {
                return (uint)(Rows * Columns);
            }
        }

        /// <summary>
        /// Returns if path from Entry to Exit cell exists in the maze
        /// </summary>
        public bool PathExists
        {
            get { return _PathExists; }
        }

        /// <summary>
        /// Cell number of entry cell
        /// </summary>
        public uint Entry
        {
            get { return 0; }
        }

        /// <summary>
        /// Cell number of exit cell
        /// </summary>
        public uint Exit
        {
            get { return TotalCells - 1; }
        }

        /// <summary>
        /// Total number of walls that has been destroyed
        /// </summary>
        public uint TotalDestroyedWalls
        {
            get { return TotalDestroyableWalls - RemainingDestroyableWalls; }
        }

        /// <summary>
        /// Remaining number of destroyable walls
        /// </summary>
        public uint RemainingDestroyableWalls
        {
            get { return (uint)DestroyableWalls.Count; }
        }

        /// <summary>
        /// List of walls that are destroyable
        /// </summary>
        public List<uint> DestroyableWalls
        {
            get { return _DestroyableWalls; }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Resets the whole maze
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        private void Reset_Maze(ushort rows, ushort cols)
        {
            if (rows < 1) rows = 1;
            if (cols < 1) cols = 1;

            if (rows > cols) rows = cols;

            if (rows > 300) rows = 300;
            if (cols > 500) rows = 500;

            if (cols == 1 && rows == 1)
            {
                rows = 2;
                cols = 2;
            }

            _Rows = rows;
            _Columns = cols;
            _DestroyableWalls = new List<uint>();

            for (uint i = 0; i < TotalDestroyableWalls; i++)
            {
                DestroyableWalls.Add(i);
            }

            _Matrix = new bool[Rows, Columns, 4];
            _Paths = new List<uint>[TotalCells];

            for (int i = 0; i < Paths.Length; i++)
            {
                Paths[i] = new List<uint>();
            }

            //Create entry and exit door
            CreateEntryAndExit();
        }

        /// <summary>
        /// Amar Shrestha - 10/19/2020
        /// Knocks down wall of cell at position (x,y)
        /// wall - 0:Top, 1:Right, 2:Bottom, 3:Left
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="wall"></param>
        /// <returns></returns>
        public bool KnockDownWall(ushort x, ushort y, byte wall)
        {
            //invalid entries
            if (x >= Rows || y >= Columns || wall > 3)
            {
                return false;
            }

            //outer most walls cannot be knocked down
            if ((x == 0 && wall == 0) || (x == Matrix.GetUpperBound(0) && wall == 2) || (y == 0 && wall == 3) || (y == Matrix.GetUpperBound(1) && wall == 1))
            {
                return false;
            }

            //top wall of the given cell is bottom wall of cell above it
            if (wall == 0 && !Matrix[x, y, 0])
            {
                return MutualWallDependency_DestroyAction(x, y, (ushort)(x - 1), y, 0, 2);
            }
            //right wall of the given cell is the left wall of cell on the right
            else if (wall == 1 && !Matrix[x, y, 1])
            {
                return MutualWallDependency_DestroyAction(x, y, x, (ushort)(y + 1), 1, 3);
            }
            //bottom wall of the given cell is the top wall of the cell below
            else if (wall == 2 && !Matrix[x, y, 2])
            {
                return MutualWallDependency_DestroyAction(x, y, (ushort)(x + 1), y, 2, 0);
            }
            //left wall of the given cell is right wall of the cell on the left
            else if (wall == 3 && !Matrix[x, y, 3])
            {
                return MutualWallDependency_DestroyAction(x, y, x, (ushort)(y - 1), 3, 1);
            }

            return false;
        }

        /// <summary>
        /// Maintains property of mutual walls when walls are destroyed.
        /// Returns if the action was successful
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="w"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        private bool MutualWallDependency_DestroyAction(ushort x, ushort y, ushort a, ushort b, byte w, byte v)
        {
            if (GetDestroyableWallID(x, y, w) != TotalDestroyableWalls)
            {
                Matrix[x, y, w] = true;
                Matrix[a, b, v] = true;

                //Update paths
                //Paths are two ways so both cells paths are updated
                Paths[CellID(x, y)].Add(CellID(a, b));
                Paths[CellID(a, b)].Add(CellID(x, y));

                //Remove the given wall from list of undestroyed walls                
                DestroyableWalls.RemoveAt(DestroyableWalls.IndexOf(GetDestroyableWallID(x, y, w)));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates entry and exit door
        /// Knocks down top and left wall of top-left cell
        /// Knocks down right and bottom wall of bottom-right cell
        /// </summary>
        private void CreateEntryAndExit()
        {
            //Create Entry
            Matrix[0, 0, 0] = true;
            Matrix[0, 0, 3] = true;

            //Create Exit
            Matrix[Rows - 1, Columns - 1, 1] = true;
            Matrix[Rows - 1, Columns - 1, 2] = true;
        }

        /// <summary>
        /// Generates single dimensional cell ID based on row and column value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public uint CellID(ushort x, ushort y)
        {
            return (uint)(x * Columns + y);
        }

        /// <summary>
        /// Randomly and repeatedly destroys walls until path is found. 
        /// Returns the array of previous nodes
        /// </summary>
        /// <returns></returns>
        public List<uint> CreatePath_RandomWallDestroy()
        {
            //initial batch size is 33% of all the destroyable walls 
            uint batchSize = (TotalDestroyableWalls / 5);

            //Randomly knock down 10% of the walls to create paths
            RandomlyKnockWalls(batchSize);

            int[] prev = BreadthFirstSearch(Entry);
            List<uint> path = TracePath_BFS(Entry, Exit, prev);

            while (path.Count < 2)
            {
                //one percent of all the walls
                batchSize = ((RemainingDestroyableWalls) / 12);

                RandomlyKnockWalls(batchSize);

                prev = BreadthFirstSearch(Entry);
                path = TracePath_BFS(Entry, Exit, prev);
            }
            //flag that path exits from Entry to Exit
            _PathExists = true;

            return path; ;
        }

        /// <summary>
        /// Blindly destroys walls to goto to neighboring cell and keeps repeating until it Exit is found
        /// </summary>
        /// <returns></returns>
        public List<uint> CreatePath_BlindTunnelDigging()
        {
            List<uint> path = new List<uint>();
            path.Add(Entry);

            //Iterate until exit is found
            while (path[path.Count - 1] != Exit)
            {
                List<uint> neighbors = FindSurroundingNeighborCells(path[path.Count - 1], 1, false);

                Random random = new Random((int)(DateTime.Now.Ticks + path[path.Count - 1]));
                int index = random.Next(neighbors.Count);
                uint cellID = neighbors[index];

                DigTunnel(path[path.Count - 1], cellID);

                //add to path
                path.Add(cellID);
            }

            //flag path exists
            _PathExists = true;
            return TracePath_BFS(Entry, Exit, BreadthFirstSearch(Entry));
        }

        /// <summary> 
        /// Destroys walls until all cells are reachable from all other cells
        /// Returns path from Entry cell to Exit cell. 
        /// </summary>
        /// <returns></returns>
        public List<uint> CreateOmnireachPaths()
        {
            if (PathExists)
            {
                //find array of prevous nodes from entry to exit first
                int[] prev = BreadthFirstSearch(Entry);

                //list of all cells that are reachable
                List<uint> reachableCells = TracePath_BFS(Entry, Exit, prev);

                //Iterates through prev array. Elements with -1 value are unreacbable cells
                for (uint cellIndex = (uint)(prev.Length-1); cellIndex >=1 ; cellIndex--)
                {
                    //skips the cells that are already reachable
                    if (prev[cellIndex] != -1 || reachableCells.Contains(cellIndex)) continue;

                    bool pathFound = false;

                    for (ushort searchLevel = 0; searchLevel < (Rows > Columns ? Rows : Columns) && !pathFound; searchLevel++)
                    {
                        List<uint> cells = new List<uint>();

                        if (searchLevel == 0)
                        {
                            cells.Add(cellIndex);
                        }
                        else
                        {
                            List<uint> neighbors = FindSurroundingNeighborCells(cellIndex, searchLevel);

                            foreach (uint cell in neighbors)
                            {
                                if (!cells.Contains(cell))
                                {
                                    cells.Add(cell);
                                }
                            }
                        }

                        //Iterates through all the cells in the surrounding until path is found to the cell
                        while (cells.Count != 0 && !pathFound)
                        {
                            Random random = new Random((int)(DateTime.Now.Ticks + cellIndex * 100));

                            int currentCellIndex = random.Next(cells.Count);
                            List<uint> destroyableWalls = DestroyableWallsForGivenCellID(cells[currentCellIndex]);
                            //MessageBox.Show(cells[currentCellIndex].ToString());

                            //Iterate through all the remaining destroyable walls
                            while (destroyableWalls.Count != 0 && !pathFound)
                            {
                                Random rand = new Random((int)(DateTime.Now.Ticks + cellIndex + 999));

                                uint wall = destroyableWalls[rand.Next(destroyableWalls.Count)];

                                //Knock down the selected wall
                                KnockDownWall(GetRowFromDestroyableWallID(wall), GetColumnFromDestroyableWallID(wall), GetWallFromDestroyableWallID(wall));

                                //Remove that wall from the list
                                destroyableWalls.Remove(wall);

                                prev = BreadthFirstSearch(Entry);

                                if (prev[cellIndex] != -1)
                                {
                                    pathFound = true;

                                    List<uint> path = TracePath_BFS(Entry, cellIndex, prev);

                                    foreach (uint p in path)
                                    {
                                        if (!reachableCells.Contains(p))
                                        {
                                            reachableCells.Add(p);
                                        }
                                    }
                                    break;
                                }
                            }
                            //Remove the cell from list when all its walls are destroyed
                            cells.RemoveAt(currentCellIndex);
                        }
                    }

                }
                return TracePath_BFS(Entry, Exit, BreadthFirstSearch(Entry));
            }
            return new List<uint>();
        }

        /// <summary>
        /// Reconstructs the whole maze with only path from Entry to Exit cell. 
        /// Returns if the reconstruction was successful. If failed does nothing
        /// </summary>
        /// <param name="path"></param>
        public bool ReconstructMaze_PathOnly(List<uint> path)
        {
            if (path[0] == Entry && path[path.Count - 1] == Exit)
            {
                //Reset maze
                Reset_Maze(Rows, Columns);

                uint counter = 0;
                //Dig tunnel through the cells in path
                for (int index = 0; index < path.Count - 1; index++)
                {
                    counter = DigTunnel(path[index], path[index + 1]) ? counter + 1 : counter;
                }

                if (counter == path.Count) return true;
            }
            return false;
        }

        /// <summary>
        /// Destroys corresponding wall to create path between two adjacent cells
        /// </summary>
        /// <param name="cellA"></param>
        /// <param name="cellB"></param>
        /// <returns></returns>
        private bool DigTunnel(uint cellA, uint cellB)
        {
            List<uint> neighbors = FindSurroundingNeighborCells(cellA, 1, false);

            if (neighbors.Contains(cellB))
            {
                //if the neighbor is below
                if (GetRowFromCellID(cellA) + 1 == GetRowFromCellID(cellB))
                {
                    KnockDownWall(GetRowFromCellID(cellB), GetColumnFromCellID(cellB), 0);
                }
                //if the neighbor is above
                else if (GetRowFromCellID(cellA) - 1 == GetRowFromCellID(cellB))
                {
                    KnockDownWall(GetRowFromCellID(cellB), GetColumnFromCellID(cellB), 2);
                }
                //if the neighbor is right
                else if (GetColumnFromCellID(cellA) + 1 == GetColumnFromCellID(cellB))
                {
                    KnockDownWall(GetRowFromCellID(cellB), GetColumnFromCellID(cellB), 3);
                }
                //if the neighbor is left
                else if (GetColumnFromCellID(cellA) - 1 == GetColumnFromCellID(cellB))
                {
                    KnockDownWall(GetRowFromCellID(cellB), GetColumnFromCellID(cellB), 1);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the list of wall ids of all the remaining destroyable walls for a given cell id
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
        private List<uint> DestroyableWallsForGivenCellID(uint cellID)
        {
            List<uint> walls = new List<uint>();

            ushort row = GetRowFromCellID(cellID);
            ushort col = GetColumnFromCellID(cellID);

            for (byte i = 0; i < 4; i++)
            {
                //Total Destroyable walls value for destroyable ID means invalid wall
                if (GetDestroyableWallID(row, col, i) != TotalDestroyableWalls)
                {
                    walls.Add(GetDestroyableWallID(row, col, i));
                }
            }
            return walls;
        }

        /// <summary>
        /// Returns the list of all the cells that are surrounding a given cell at given level
        /// </summary>
        /// <param name="cellID"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<uint> FindSurroundingNeighborCells(uint cellID, ushort level, bool includeCorner = true)
        {
            List<uint> neighbors = new List<uint>();

            if (level < 1) return neighbors;

            if (level > (Rows > Columns ? Rows : Columns) - 1) return neighbors;

            int row = GetRowFromCellID(cellID);
            int col = GetColumnFromCellID(cellID);

            //Left and right columns include al cells in that level of row
            //except corner cells
            for (int i = -(level - 1); i <= (level - 1); i++)
            {
                //skip if out of range
                if (!(col - level < 0 || row + i < 0 || row + i >= Rows))
                {
                    //left column
                    neighbors.Add(CellID((ushort)(row + i), (ushort)(col - level)));
                }

                if (!(col + level >= Columns || row + i < 0 || row + i >= Rows))
                {
                    //right column
                    neighbors.Add(CellID((ushort)(row + i), (ushort)(col + level)));
                }
            }

            //Top and bottom row includes cells the cells in that level of column 
            //except corner cells
            for (int i = -(level - 1); i <= (level - 1); i++)
            {
                //skip if out of range
                if (!(row - level < 0 || col + i < 0 || col + i >= Columns))
                {
                    //top row
                    neighbors.Add(CellID((ushort)(row - level), (ushort)(col + i)));
                }

                if (!(row + level >= Rows || col + i < 0 || col + i >= Columns))
                {
                    //bottom row
                    neighbors.Add(CellID((ushort)(row + level), (ushort)(col + i)));
                }
            }

            //Corner cells
            if (includeCorner)
            {
                for (int i = -level; i <= level; i += level * 2)
                {
                    for (int j = -level; j <= level; j += level * 2)
                    {
                        if (row + i < 0 || row + i >= Rows) continue;

                        if (col + j < 0 || col + j >= Columns) continue;

                        neighbors.Add(CellID((ushort)(row + i), (ushort)(col + j)));
                    }
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Randomly knocks down walls in batches to generate paths
        /// </summary>
        /// <param name="batchSize"></param>
        public void RandomlyKnockWalls(uint batchSize)
        {
            if (batchSize < 1) batchSize = 1;
            if (batchSize > RemainingDestroyableWalls) batchSize = RemainingDestroyableWalls;

            for (uint counter = 0; counter < batchSize; counter += 0)
            {
                Random random = new Random((int)(DateTime.Now.Ticks + counter));
                int index = random.Next(DestroyableWalls.Count);
                uint wallID = DestroyableWalls[index];

                ushort x = GetRowFromDestroyableWallID(wallID);
                ushort y = GetColumnFromDestroyableWallID(wallID);
                byte w = GetWallFromDestroyableWallID(wallID);

                counter = KnockDownWall(x, y, w) ? counter + 1 : counter;
            }
        }

        /// <summary>
        /// Returns the array of previous nodes using breadth first search from a given starting cell
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public int[] BreadthFirstSearch(uint start)
        {
            //adds the starting node in the queue
            Queue<uint> queue = new Queue<uint>();
            queue.Enqueue(start);

            bool[] visited = new bool[TotalCells];
            visited[start] = true;

            //array to store the link parent nodes
            //-1 is considered no path so the path
            int[] previousNodes = new int[TotalCells];

            //initialize all previous nodes to be unreachable i.e. -1
            for (int i = 0; i < TotalCells; i++)
            {
                previousNodes[i] = -1;
            }

            //iterate the process until the queue is empty
            while (!queue.isEmpty())
            {
                //pop the last element on the queue
                uint node = queue.Dequeue();

                //get neighboring nodes of the given node
                List<uint> path = Paths[node];

                //iterate through all neighbors of the given nodes
                foreach (uint door in path)
                {
                    //if the given neighbor is not visited then add it to queue
                    if (!visited[door])
                    {
                        queue.Enqueue(door);
                        visited[door] = true;
                        previousNodes[door] = (int)node;
                    }
                }
            }

            return previousNodes;
        }

        /// <summary>
        /// Returns the list of paths from start cell to end cell using the previous nodes array. 
        /// Returns empty list if there is no path found.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="previousNodes"></param>
        /// <returns></returns>
        public List<uint> TracePath_BFS(uint start, uint end, int[] previousNodes)
        {
            List<uint> path = new List<uint>();

            //start from end and trace back and iterate using previousNodes
            //until -1 is reached because the parent of start node is -1
            for (int i = (int)end; i != -1; i = previousNodes[i])
            {
                //A path cannot be longer than the total number of vertices
                //So this means the path is invalid so return 
                if (path.Count >= TotalCells)
                {
                    path.Clear();
                    return path;
                }

                //previous nodes have value = actual value + 1
                path.Add((uint)(i));
            }

            //reverse the traced path so that start is at the beginning
            path.Reverse();

            //return the traced path only if the first element in path is start node
            if (path[0] == start)
            {
                return path;
            }

            //Otherwise return empty list
            path.Clear();
            return path;
        }

        /// <summary>
        /// Returns the row position for a given cell ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ushort GetRowFromCellID(uint id)
        {
            return (ushort)(id / Columns);
        }

        /// <summary>
        /// Returns the column position for a given cell ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ushort GetColumnFromCellID(uint id)
        {
            return (ushort)(id - GetRowFromCellID(id) * Columns);
        }

        /// <summary>
        /// Returns column position of the cell in which the wall belongs to. 
        /// The cell at which the wall belongs to is based on right or bottom positions
        /// </summary>
        /// <param name="wallID"></param>
        /// <returns></returns>
        private ushort GetColumnFromDestroyableWallID(uint wallID)
        {
            if (wallID < Rows * (Columns - 1))
            {
                return ((ushort)(wallID / Rows));
            }
            else
            {
                return (ushort)(wallID - GetRowFromDestroyableWallID(wallID) * Columns - (Rows * (Columns - 1)));
            }
        }

        /// <summary>
        /// Returns row position of the cell in which the wall belongs to. 
        /// The cell at which the wall belongs to is based on right or bottom positions
        /// </summary>
        /// <param name="wallID"></param>
        /// <returns></returns>
        private ushort GetRowFromDestroyableWallID(uint wallID)
        {
            if (wallID < Rows * (Columns - 1))
            {
                return ((ushort)(wallID - GetColumnFromDestroyableWallID(wallID) * Rows));
            }
            else
            {
                return ((ushort)(uint)((wallID - Rows * (Columns - 1)) / Columns));
            }
        }

        /// <summary>
        /// Returns the numerical value equivalent to the orientationn of the given destroyable wall from wall ID. 
        /// Top: 0, Right: 1, Bottom: 2, Left: 3
        /// </summary>
        /// <param name="wallID"></param>
        /// <returns></returns>
        private byte GetWallFromDestroyableWallID(uint wallID)
        {
            if (wallID < Rows * (Columns - 1))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        /// <summary>
        /// Returns the wall ID for when the row position, column position and wall number is given
        /// Wall ID starts from vertical walls (left) and then continues to horizontal walls (bottom)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        private uint GetDestroyableWallID(ushort x, ushort y, byte w)
        {
            //top wall is bottom wall of cell above
            if (w == 0 && x != 0)
            {
                //wallID for bottom wall  = x*col+y+ row*(col-1)
                return (uint)((x - 1) * Columns + y + (Rows * (Columns - 1)));
            }
            //last row doesn't have destroyable bottom wall
            else if (w == 2 && x != Rows - 1)
            {
                //wallID for bottom wall  = x*col+y+ row*(col-1)
                return (uint)((x * Columns + y + (Rows * (Columns - 1))));
            }
            //last column doesn't have destroyable right wall
            else if (w == 1 && y != Columns - 1)
            {
                return (uint)(y * Rows + x);
            }
            //right wall is the left wall of cell to the right
            else if (w == 3 && y != 0)
            {
                return (uint)((y - 1) * Rows + x);
            }
            else
            {
                //return maximum valid wall id +1 
                //which is Number of destroyable walls
                return TotalDestroyableWalls;
            }
        }
        #endregion
    }
}
