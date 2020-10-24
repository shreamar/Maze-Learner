using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Maze_Learner
{
    /// <summary>
    /// Amar Shrestha - 10/19/2020
    /// Class for each cells of maze
    /// </summary>
    public class MazeCell
    {
        #region Private Fields
        private bool _Top;
        private bool _Right;
        private bool _Bottom;
        private bool _Left;
        #endregion

        #region Constructor
        public MazeCell()
        {
            Top = true;
            Right = true;
            Bottom = true;
            Left = true;
        }
        #endregion

        #region Public Properties
        public bool Top
        {
            get { return _Top; }
            set { _Top = value; }
        }

        public bool Right
        {
            get { return _Right; }
            set { _Right = value; }
        }

        public bool Bottom
        {
            get { return _Bottom; }
            set { _Bottom = value; }
        }

        public bool Left
        {
            get { return _Left; }
            set { _Left = value; }
        }
        #endregion
    }
}
