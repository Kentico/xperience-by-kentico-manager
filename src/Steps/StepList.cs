namespace Xperience.Xman.Steps
{
    /// <summary>
    /// A collection of <see cref="IStep"/>s which can be navigated forward and back.
    /// </summary>
    public class StepList : List<IStep>
    {
        private int currentIndex = 0;


        /// <summary>
        /// The current step.
        /// </summary>
        public IStep Current => this[currentIndex];


        /// <summary>
        /// Navigates forward if there is a next element.
        /// </summary>
        /// <returns><c>False</c> if there is no next element.</returns>
        public bool Next()
        {
            if (currentIndex < Count - 1)
            {
                Step(1);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Navigates backward if there is a previous element.
        /// </summary>
        /// <returns><c>False</c> if there is no previous element.</returns>
        public bool Previous()
        {
            if (currentIndex > 0)
            {
                Step(-1);
                return true;
            }

            return false;
        }


        private void Step(int direction)
        {
            currentIndex += direction;
            if (currentIndex > Count - 1)
            {
                currentIndex = Count - 1;
            }

            if (currentIndex < 0)
            {
                currentIndex = 0;
            }
        }
    }
}
