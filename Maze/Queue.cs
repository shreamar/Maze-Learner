using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze_Learner
{
    /// <summary>
    /// Amar Shrestha - 10/4/2020
    /// Simple queue class that queues data in sequential manner
    /// </summary>
    public class Queue<T>
    {
        #region Private Fields
        private List<T> _Stack;
        #endregion

        #region Constructor
        public Queue()
        {
            _Stack = new List<T>();
        }
        #endregion

        #region Public Property

        //public List<T> Stack
        //{
        //    get
        //    {
        //        return _Stack;
        //    }
        //    set
        //    {
        //        _Stack = value;
        //    }
        //}
        #endregion

        #region Methods
        /// <summary>
        /// Enqueues the given element in the queue
        /// </summary>
        /// <param name="element"></param>
        public void Enqueue(T element)
        {
            bool duplicate = false;
            foreach (T item in _Stack)
            {
                if (item.Equals(element))
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
            {
                _Stack.Add(element);
            }
        }

        /// <summary>
        /// Pops out the last element from the queue
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            T item = _Stack[0];
            _Stack.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Specifies if the queue is empty or not
        /// </summary>
        /// <returns></returns>
        public bool isEmpty()
        {
            return (_Stack.Count > 0 ? false : true);
        }
        #endregion
    }
}
